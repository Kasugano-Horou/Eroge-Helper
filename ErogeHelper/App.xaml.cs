using ErogeHelper.Common;
using ErogeHelper.Model.Singleton;
using ErogeHelper.View;
using ErogeHelper.ViewModel;
using GalaSoft.MvvmLight.Threading;
using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Windows;
using System.Xml.Linq;

namespace ErogeHelper
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(App));

        private readonly GameInfo gameInfo = GameInfo.Instance;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Directory.SetCurrentDirectory(Path.GetDirectoryName(GetType().Assembly.Location));
            DispatcherHelper.Initialize();
            log4net.Config.XmlConfigurator.Configure();
            
            log.Info("Started Logging");
            log.Info($"Enviroment directory: {Directory.GetCurrentDirectory()}");
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            if (principal.IsInRole(WindowsBuiltInRole.Administrator))
            {
                log.Info("Program run in Admin mode");
            }

            if (e.Args.Length == 0)
            {
                MessageBox.Show("请使用 ErogeHelperInstaller 安装我> < \n\r" +
                                "如果你已经安装了直接右键游戏选择Eroge Helper启动就好了~",
                                "ErogeHelper");
                Current.Shutdown();
                return;
            }

            gameInfo.Path = e.Args[0];
            gameInfo.ConfigPath = gameInfo.Path + ".eh.config";
            gameInfo.Dir = gameInfo.Path.Substring(0, gameInfo.Path.LastIndexOf('\\'));
            gameInfo.ProcessName = Path.GetFileNameWithoutExtension(gameInfo.Path);
            gameInfo.MD5 = Utils.GetMD5(gameInfo.Path);

            log.Info($"Game's path: {e.Args[0]}");
            log.Info($"Locate Emulator status: {e.Args.Contains("/le")}");

            if (e.Args.Contains("/le"))
            {
                // Use Locate Emulator
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = Directory.GetCurrentDirectory() + @"\libs\x86\LEProc.exe",
                    UseShellExecute = false,

                    Arguments = File.Exists(gameInfo.Path + ".le.config") ?
                    $"-run \"{gameInfo.Path}\"" :
                    $"\"{gameInfo.Path}\""
                };

                Process.Start(startInfo);
            }
            else
            {
                // Direct start
                Process.Start(gameInfo.Path);
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
                        if (p.WaitForInputIdle(500) == false) // 500 延迟随意写的，正常启动一般在100~200范围
                        {
                            log.Info($"Procces {p.Id} maybe stuck");
                        }

                        newProcFind = true;
                    }
                }
                // 进程找完却没有得到hWnd的可能也是存在的，所以以hWnd为主
                gameInfo.HWndProc = Utils.FindHWndProc(gameInfo.ProcList);

                if (totalTime.Elapsed.TotalSeconds > 7 && gameInfo.HWndProc.MainWindowHandle == IntPtr.Zero)
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
                    // Read xml file
                    var profile = XElement.Load(gameInfo.ConfigPath).Element("Profile");

                    // TODO: 试探MD5是否与配置文件相同。若不同，弹窗提醒exe程序有变动，可能需要重新选取hook以读取文本
                    //if ( !gameInfo.MD5.Equals(profile.Element("MD5").Value) )

                    gameInfo.HookCode = profile.Element("HookCode").Value;
                    gameInfo.ThreadContext = long.Parse(profile.Element("ThreadContext").Value);

                    log.Info($"Get HCode {gameInfo.HookCode} from file {gameInfo.ProcessName}.exe.eh.config");
                    // Display text window
                    new GameView().Show();
                }
                else
                {
                    log.Info("No xml config file, open hook panel.");
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
