using ErogeHelper.Common;
using ErogeHelper.Model;
using ErogeHelper.Model.Singleton;
using ErogeHelper.View;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Threading;
using Hardcodet.Wpf.TaskbarNotification;
using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;

namespace ErogeHelper
{


    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(App));

        private TaskbarIcon notifyIcon;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var currentDirectory = Path.GetDirectoryName(GetType().Assembly.Location);
            Directory.SetCurrentDirectory(currentDirectory);
            Utils.AddEnvironmentPaths((currentDirectory + @"\libs").Split());
            DispatcherHelper.Initialize();
            SimpleIoc.Default.Register<GameInfo>();
            SimpleIoc.Default.Register<AppSetting>();
            //new TaskbarView();
            notifyIcon = (TaskbarIcon)FindResource("NotifyIcon");
            log4net.Config.XmlConfigurator.Configure();
            // AppDomain.CurrentDomain.UnhandledException += GlobalErrorHandle 非ui线程
            DispatcherUnhandledException += (s, eventArgs) => {
                log.Error(eventArgs.Exception);
                MessageBox.Show(eventArgs.Exception.ToString(), "Eroge Helper");
                // TODO: 复制粘贴板转到github. Friendly error message
            };
            log.Info("Started Logging");
            log.Info($"Enviroment directory: {Directory.GetCurrentDirectory()}");

            if (e.Args.Length == 0)
            {
                MessageBox.Show("请使用 EHInstaller 安装我> < \n\r" +
                                "如果你已经安装了直接右键游戏选择Eroge Helper启动就好了~",
                                "ErogeHelper");
                Current.Shutdown();
                return;
            }

            GameInfo gameInfo = SimpleIoc.Default.GetInstance(typeof(GameInfo)) as GameInfo;
            gameInfo.Path = e.Args[0];
            gameInfo.ConfigPath = gameInfo.Path + ".eh.config";
            gameInfo.Dir = gameInfo.Path.Substring(0, gameInfo.Path.LastIndexOf('\\'));
            gameInfo.ProcessName = Path.GetFileNameWithoutExtension(gameInfo.Path);
            gameInfo.MD5 = Utils.GetMD5(gameInfo.Path);

            log.Info($"Game's path: {e.Args[0]}");
            log.Info($"Locate Emulator statu: {e.Args.Contains("/le")}");

            if (e.Args.Contains("/le"))
            {
                // Use Locate Emulator
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = Directory.GetCurrentDirectory() + @"\libs\x86\LEProc.exe",
                        UseShellExecute = false,

                        Arguments = File.Exists(gameInfo.Path + ".le.config") 
                                               ? $"-run \"{gameInfo.Path}\""
                                               : $"\"{gameInfo.Path}\""
                    });
                }
                // XXX: 捕获不到，7秒超时
                catch(AccessViolationException)
                {
                    throw new AccessViolationException("LE执行出现内存错误，这可能是游戏进程还未退出，问题不大请重新尝试用LE启动游戏~");
                }
            }
            else
            {
                // Direct start
                Process.Start(new ProcessStartInfo 
                {
                    FileName = gameInfo.Path,
                    UseShellExecute = false,
                    WorkingDirectory = gameInfo.Dir
                });
            }

            bool newProcFind;
            // Pid标记
            List<int> procMark = new List<int>();
            // tmpProcList 每次循环 Process.GetProcessesByName() 命中的进程
            List<Process> tmpProcList = new List<Process>();
            var totalTime = new Stopwatch();
            totalTime.Start();
            do
            {
                newProcFind = false;
                gameInfo.ProcList.Clear();
                tmpProcList.Clear();
                #region Collect Processes To tmpProcList
                foreach (Process p in Process.GetProcessesByName(gameInfo.ProcessName))
                {
                    tmpProcList.Add(p);
                }
                foreach (Process p in Process.GetProcessesByName(gameInfo.ProcessName + ".log"))
                {
                    tmpProcList.Add(p);
                }
                #endregion
                foreach (Process p in tmpProcList)
                {
                    gameInfo.ProcList.Add(p);
                    if (!procMark.Contains(p.Id))
                    {
                        procMark.Add(p.Id);
                        // May occurrent System.InvalidOperationException
                        try
                        {
                            if (p.WaitForInputIdle(500) == false) // 500 延迟随意写的，正常启动一般在100~200范围
                            {
                                log.Info($"Procces {p.Id} maybe stuck");
                            }
                        }
                        catch (InvalidOperationException ex)
                        {
                            // skip no effect error
                            log.Error(ex.Message);
                        }

                        newProcFind = true;
                    }
                }
                // 进程找完却没有得到hWnd的可能也是存在的，所以以hWnd为主
                gameInfo.HWndProc = Utils.FindHWndProc(gameInfo.ProcList);

                // timeout
                if (totalTime.Elapsed.TotalSeconds > 7 && gameInfo.HWndProc == null)
                {
                    log.Info("Timeout! Find MainWindowHandle Faied");
                    MessageBox.Show("(超时)没能找到游戏窗口！", "ErogeHelper");
                    Current.Shutdown();
                    return;
                }
            } while (newProcFind || (gameInfo.HWndProc == null));
            totalTime.Stop();

            log.Info($"{gameInfo.ProcList.Count} Process(es) and window handle Found. Spend time {totalTime.Elapsed.TotalSeconds}");
            if (gameInfo.ProcList.Count != 0)
            {
                // Cheak if there is eh.config file
                if (File.Exists(gameInfo.ConfigPath))
                {
                    gameInfo.HookCode = EHConfig.GetValue(EHNode.HookCode);
                    gameInfo.ThreadContext = long.Parse(EHConfig.GetValue(EHNode.ThreadContext));
                    gameInfo.SubThreadContext = long.Parse(EHConfig.GetValue(EHNode.SubThreadContext));
                    gameInfo.Regexp = EHConfig.GetValue(EHNode.Regexp);

                    log.Info($"Get HCode {gameInfo.HookCode} from file {gameInfo.ProcessName}.exe.eh.config");
                    // Display text window
                    new GameView().Show();
                }
                else
                {
                    log.Info("Not find xml config file, open hook panel.");
                    new HookConfigView().Show();
                }

                Textractor.Init();
            }
            else
            {
                MessageBox.Show("没有找到游戏进程！", "ErogeHelper");
                Current.Shutdown();
            }
        }
    }
}
