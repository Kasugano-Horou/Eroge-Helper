using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Media.Imaging;

namespace ErogeHelper.Common
{
    static class Utils
    {
        public static string GetMD5(string filePath)
        {
            FileStream file = File.OpenRead(filePath);
            var md5 = new MD5CryptoServiceProvider();
            byte[] retVal = md5.ComputeHash(file);
            file.Close();

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < retVal.Length; i++)
            {
                sb.Append(retVal[i].ToString("x2"));
            }
            return sb.ToString();
        }

        /// <summary>
        /// <para>查看一个List&lt;Process&gt;集合中是否存在MainWindowHandle</para>
        /// <para>若存在，返回其所在Process，否则返回null</para>
        /// </summary>
        /// <param name="procList"></param>
        /// <returns></returns>
        public static Process FindHWndProc(List<Process> procList)
        {
            foreach (var p in procList)
            {
                if (p.MainWindowHandle != IntPtr.Zero)
                    return p;
            }
            return null;
        }

        /// <summary>
        /// Load a resource WPF-BitmapImage (png, bmp, ...) from embedded resource defined as 'Resource' not as 'Embedded resource'.
        /// </summary>
        /// <param name="pathInApplication">Path without starting slash</param>
        /// <param name="assembly">Usually 'Assembly.GetExecutingAssembly()'. If not mentionned, I will use the calling assembly</param>
        /// <returns></returns>
        public static BitmapImage LoadBitmapFromResource(string pathInApplication, Assembly assembly = null)
        {
            if (assembly == null)
            {
                assembly = Assembly.GetCallingAssembly();
            }

            if (pathInApplication[0] == '/')
            {
                pathInApplication = pathInApplication.Substring(1);
            }
            return new BitmapImage(new Uri(@"pack://application:,,,/" + assembly.GetName().Name + ";component/" + pathInApplication, UriKind.Absolute));
        }
    }
}
