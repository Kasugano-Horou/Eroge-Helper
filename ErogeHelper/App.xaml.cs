using ErogeHelper.Models;
using ErogeHelper.Utils;
using ErogeHelper.Views;
using log4net;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Xml;

namespace ErogeHelper
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(App));

        protected override void OnStartup(StartupEventArgs e)
        {
            Directory.SetCurrentDirectory(Path.GetDirectoryName(this.GetType().Assembly.Location));
            log4net.Config.XmlConfigurator.Configure();
            log.Info("        =============  Started Logging  =============        ");
            log.Info($"Enviroment directory: {Directory.GetCurrentDirectory()}");

            base.OnStartup(e);

            if (e.Args.Length == 0)
            {
                MessageBox.Show("Please run ErogeHelperInstaller to install me \n\r" +
                                "then just right click game to use Eroge Helper",
                                "ErogeHelper");
                Current.Shutdown();
            }

            GameInfo gameInfo = GameInfo.Instance;
            gameInfo.Path = e.Args[0];
            gameInfo.ConfigPath = gameInfo.Path + ".eh.config";
            gameInfo.Dir = gameInfo.Path.Substring(0, gameInfo.Path.LastIndexOf('\\'));
            gameInfo.ProcessName = Path.GetFileNameWithoutExtension(gameInfo.Path);
            gameInfo.MD5 = MD5.GetMD5(gameInfo.Path);

            log.Info($"Game's path: {e.Args[0]}");
            log.Info($"Game's MD5 code: {gameInfo.MD5}");
            log.Info($"Locate Emulator status: {e.Args.Contains("/le")}");

            if (e.Args.Contains("/le"))
            {
                // Use Locate Emulator
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = Directory.GetCurrentDirectory() + @"\libs\x86\LEProc.exe";
                startInfo.UseShellExecute = false;

                startInfo.Arguments = File.Exists(gameInfo.Path + ".le.config") ?
                    $"-run \"{gameInfo.Path}\"" :
                    $"\"{gameInfo.Path}\"";

                var res = Process.Start(startInfo);
                res.WaitForInputIdle(7000);
            }
            else
            {
                // Direct start
                var res = Process.Start(gameInfo.Path);
                res.WaitForInputIdle(7000);
            }

            log.Info("Game Process Start over.");

            gameInfo.ProcList = Process.GetProcessesByName(gameInfo.ProcessName);
            log.Info($"{gameInfo.ProcList.Length} Process(es) detected.");
            if (gameInfo.ProcList.Length != 0)
            {
                // Cheak if there is eh.config file
                if (File.Exists(gameInfo.ConfigPath))
                {
                    // read xml file
                    XmlDocument config = new XmlDocument();
                    config.Load(gameInfo.ConfigPath);
                    XmlNodeList nodeList = config.GetElementsByTagName("HookCode");
                    GameInfo.Instance.HookCode = nodeList[0].InnerText;
                    XmlNodeList threadList = config.GetElementsByTagName("HookThreadNumber");
                    GameInfo.Instance.HookThread = int.Parse(threadList[0].InnerText);

                    log.Info($"Get HCode {GameInfo.Instance.HookCode} from file {gameInfo.ProcessName}.eh.config");
                    // Display text window
                    new GameWindow().Show();
                }
                else
                {
                    log.Info("No xml file, open config panel.");
                    // Open config panel
                    new ConfigWindow().Show();
                }

                Textractor.Init();
                return;
            }

            MessageBox.Show("Don't find Game Process!", "ErogeHelper");
            Current.Shutdown();
        }
    }
}
