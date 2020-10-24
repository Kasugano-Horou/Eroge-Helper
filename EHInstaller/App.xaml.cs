using System;
using System.Windows;
using System.Security.Principal;
using System.Linq.Expressions;
using System.ComponentModel;

namespace EHInstaller
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            if (principal.IsInRole(WindowsBuiltInRole.Administrator))
            {
                Console.WriteLine("Program run in admin mode");
            }

            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
                {
                    FileName = "ServerRegistrationManager.exe",
                    Arguments = "install EHShellMenuHandler.dll -codebase"
                });
            }
            catch(Win32Exception ex)
            {
                MessageBox.Show("找不到文件ServerRegistrationManager.exe", "EHInstaller");
                throw ex;
            }
            MessageBox.Show("已将EH注册到右键菜单! 右键游戏.\n" +
                            "移动文件夹会自动取消", "EHInstaller");

            // Uninstall
            //Process.Start(new ProcessStartInfo()
            //{
            //FileName = "ServerRegistrationManager.exe",
            //Arguments = "uninstall EHShellMenuHandler.dll"
            //});
        }
    }
}
