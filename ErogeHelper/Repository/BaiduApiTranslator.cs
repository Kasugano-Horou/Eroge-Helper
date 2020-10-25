using ErogeHelper.Common;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace ErogeHelper.Repository
{
    class BaiduApiTranslator : ITranslator
    {
        // 语言简写列表 https://api.fanyi.baidu.com/product/113
        private static readonly ILog log = LogManager.GetLogger(typeof(BaiduApiTranslator));

        private static CancellationTokenSource _cancelToken = new CancellationTokenSource();

        private string errorInfo;

        public string GetLastError()
        {
            return errorInfo;
        }

        public string GetTranslatorName()
        {
            return "BaiduApi";
        }

        public async Task<string> Translate(string sourceText, Language srcLang, Language desLang)
        {
            #region SetCancelToken
            _cancelToken.Cancel();
            var CancelToken = new CancellationTokenSource();
            _cancelToken = CancelToken;
            #endregion

            #region Define Support Language
            string from = "";
            string to = "";
            switch (srcLang)
            {
                case Language.Japenese:
                    from = "jp";
                    break;
            }
            switch (desLang)
            {
                case Language.ChineseSimplified:
                    to = "zh";
                    break;
            }
            #endregion

            HttpResponseMessage resp = null;
            try
            {
                // TODO: 有密匙的接口 需要检查Praivate 里设置状态，如果那边通过这边才能读取 具体检查哪儿？
                string appId = Properties.Settings.Default.BaiduAppId;
                string secretKey = Properties.Settings.Default.BaiduSecretKey;
                string query = sourceText;

                Random rd = new Random();
                string salt = rd.Next(100000).ToString();
                string sign = EncryptString(appId + query + salt + secretKey);
                string url = "http://api.fanyi.baidu.com/api/trans/vip/translate?";
                url += "q=" + HttpUtility.UrlEncode(query);
                url += "&from=" + from;
                url += "&to=" + to;
                url += "&appid=" + appId;
                url += "&salt=" + salt;
                url += "&sign=" + sign;
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "GET";
                request.ContentType = "text/html;charset=UTF-8";
                request.UserAgent = null;
                request.Timeout = 6000;

                var client = Utils.GetHttpClient();
                resp = await client.GetAsync(url); // 网络8不行 TaskCanceledException
                resp.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex)
            {
                errorInfo = ex.Message + "可能是程序无法连接到互联网";
                return null;
            }
            catch (TaskCanceledException ex)
            {
                errorInfo = ex.Message + "可能是网络状态不太好";
                return null;
            }

            #region Insert CancelAssert Before Any Return
            if (CancelToken.Token.IsCancellationRequested)
            {
                return string.Empty;
            }
            #endregion

            BaiduTransOutInfo obj = await resp.Content.ReadAsAsync<BaiduTransOutInfo>();

            if (obj.error_code == null || obj.error_code == "52000")
            {
                //得到翻译结果
                if (obj.trans_result.Count == 1)
                {
                    return obj.trans_result[0].dst;
                }
                else
                {
                    errorInfo = "未知错误";
                    return null;
                }
            }
            else
            {
                errorInfo = "Error ID:" + obj.error_code;
                return null;
            }
        }

        // 计算MD5值
        private string EncryptString(string str)
        {
            MD5 md5 = MD5.Create();
            // 将字符串转换成字节数组
            byte[] byteOld = Encoding.UTF8.GetBytes(str);
            // 调用加密方法
            byte[] byteNew = md5.ComputeHash(byteOld);
            // 将加密结果转换为字符串
            StringBuilder sb = new StringBuilder();
            foreach (byte b in byteNew)
            {
                // 将字节转换成16进制表示的字符串，
                sb.Append(b.ToString("x2"));
            }
            // 返回加密的字符串
            return sb.ToString();
        }

        class BaiduTransOutInfo
        {
            public string from { get; set; }
            public string to { get; set; }
            public List<BaiduTransResult> trans_result { get; set; }
            public string error_code { get; set; }

            public class BaiduTransResult
            {
                public string src { get; set; }
                public string dst { get; set; }
            }
        }
    }
}
