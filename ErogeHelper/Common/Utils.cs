using ErogeHelper.Repository;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Media.Imaging;

namespace ErogeHelper.Common
{
    static class Utils
    {
        /// <summary>
        /// Get MD5 hash by file
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
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

        static HttpClient HC;
        static public CookieContainer cookieContainer;
        /// <summary>
        /// 获得HttpClinet单例，第一次调用自动初始化
        /// </summary>
        public static HttpClient GetHttpClient()
        {
            if (HC == null)
            {
                lock (typeof(Utils))
                {
                    if (HC == null)
                    {
                        cookieContainer = new CookieContainer();
                        var handel = new HttpClientHandler()
                        {
                            AutomaticDecompression = DecompressionMethods.GZip,
                            CookieContainer = cookieContainer
                        };
                        HC = new HttpClient(handel)
                        { 
                            Timeout = TimeSpan.FromSeconds(6)
                        };
                        var headers = HC.DefaultRequestHeaders;
                        headers.UserAgent.ParseAdd("Eroge-Helper");
                        headers.Add("ContentType", "text/html;charset=UTF-8");
                        headers.AcceptEncoding.ParseAdd("gzip");
                        headers.Connection.ParseAdd("keep-alive");
                        ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
                    }
                }
            }
            return HC;
        }

        public static void AddEnvironmentPaths(IEnumerable<string> paths)
        {
            var path = new[] { Environment.GetEnvironmentVariable("PATH") ?? string.Empty };
            string newPath = string.Join(Path.PathSeparator.ToString(), path.Concat(paths));
            Environment.SetEnvironmentVariable("PATH", newPath);   // 这种方式只会修改当前进程的环境变量
        }

        public static List<ITranslator> GetTranslatorList()
        {
            var ret = new List<ITranslator>();
            ret.Add(new BaiduWebTranslator());
            return ret;
        }
    }
}
