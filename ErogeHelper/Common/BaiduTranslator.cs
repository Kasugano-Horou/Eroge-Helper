using Jurassic;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace ErogeHelper.Common
{
    public class BaiduTranslator : ITranslator
    {
        // 语言简写列表 https://api.fanyi.baidu.com/product/113
        private static readonly ILog log = LogManager.GetLogger(typeof(BaiduTranslator));

        private const string baseUrl = @"https://www.baidu.com";
        private const string transUrl = @"https://fanyi.baidu.com";
        private const string serviceUrl = @"https://fanyi.baidu.com/v2transapi";

        public async Task<string> Translate(string query, Language srcLang, Language desLang, params string[] list)
        {
            string appId = Properties.Settings.Default.BaiduAppId;
            string secretKey = Properties.Settings.Default.SettingsKey;

            if (list.Length != 0)
            {
                appId = list[0];
                secretKey = list[1];
            }

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

            if (appId != null && appId != "")
            {
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
                var resp = await client.GetAsync(url);
                resp.EnsureSuccessStatusCode();

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
                        return "UnknownError";
                    }
                }
                else
                {
                    return "ErrorID:" + obj.error_code;
                }
            }
            else
            {
                // Call web api
                #region 创建HttpClientHandler以先行获取cookie
                var uri = new Uri(baseUrl);
                var client = Utils.GetHttpClient();

                // Do request to get cookies
                lock (this)
                {
                    if (sb_cookie.ToString() == "")
                    {
                        // May cause error?
                        client.GetAsync(uri).GetAwaiter().GetResult();
                        List<Cookie> cookies = Utils.cookieContainer.GetCookies(uri).Cast<Cookie>().ToList();
                        foreach (var item in cookies)
                        {
                            sb_cookie.Append(item.Name);
                            sb_cookie.Append("=");
                            sb_cookie.Append(item.Value);
                            sb_cookie.Append(";");
                        }
                    }
                }
                #endregion

                string gtk = "";
                string token = "";
                string content = "";
                lock (this)
                {
                    content = client.GetStringAsync(transUrl).GetAwaiter().GetResult();
                }
                var tokenMatch = Regex.Match(content, "token: '(.*?)',");
                var gtkMatch = Regex.Match(content, "window.gtk = '(.*?)';");
                if (gtkMatch.Success && gtkMatch.Groups.Count > 1)
                    gtk = gtkMatch.Groups[1].Value;
                if (tokenMatch.Success && tokenMatch.Groups.Count > 1)
                    token = tokenMatch.Groups[1].Value;
                jsEngine.Evaluate(jsBaiduToken);
                string sign = jsEngine.CallGlobalFunction<string>("token", query, gtk);

                var dict = new Dictionary<string, string>
                {
                    {"from", from},
                    {"to", to},
                    {"query", query},
                    {"transtype", "translang"},
                    {"simple_means_flag", "3"},
                    {"sign", sign},
                    {"token", token}
                };
                var data = new FormUrlEncodedContent(dict);

                var resp = await client.PostAsync(serviceUrl, data);

                BaiduWebApiResponce obj = await resp.Content.ReadAsAsync<BaiduWebApiResponce>();

                if (obj.liju_result == null || obj.trans_result == null || obj.logid == 0)
                    return "未知错误";

                return obj.trans_result.data[0].dst;
                // TODO: Check Error!
            }
        }

        private StringBuilder sb_cookie = new StringBuilder();
        private static ScriptEngine jsEngine = new ScriptEngine();
        private static readonly string jsBaiduToken = @"
function a(r, o) {
    for (var t = 0; t < o.length - 2; t += 3) {
        var a = o.charAt(t + 2);
        a = a >= 'a' ? a.charCodeAt(0) - 87 : Number(a),
        a = '+' === o.charAt(t + 1) ? r >>> a : r << a,
        r = '+' === o.charAt(t) ? r + a & 4294967295 : r ^ a
	}
    return r
}
var C = null;
var token = function( r, _gtk ) {
    var o = r.length;
	o > 30 && (r = '' + r.substr(0, 10) + r.substr(Math.floor(o / 2) - 5, 10) + r.substring(r.length, r.length - 10));
    var t = void 0,
    t = null !== C ? C: (C = _gtk || '') || '';
    for (var e = t.split('.'), h = Number( e[0]) || 0, i = Number( e[1]) || 0, d = [], f = 0, g = 0; g<r.length; g++) {
        var m = r.charCodeAt( g );
        128 > m ? d[f++] = m : (2048 > m ? d[f++] = m >> 6 | 192 : (55296 === (64512 & m) && g + 1 < r.length && 56320 === (64512 & r.charCodeAt(g + 1)) ? (m = 65536 + ((1023 & m) << 10) + (1023 & r.charCodeAt(++g)), d[f++] = m >> 18 | 240, d[f++] = m >> 12 & 63 | 128) : d[f++] = m >> 12 | 224, d[f++] = m >> 6 & 63 | 128), d[f++] = 63 & m | 128)
    }
    for (var S = h, u = '+-a^+6', l = '+-3^+b+-f', s = 0; s<d.length; s++)
		S += d[s], S = a( S, u);
    return S = a( S, l),
		S ^= i,
		0 > S && (S = (2147483647 & S) + 2147483648),
		S %= 1e6,
		S.toString() + '.' + (S ^ h)
}
";

        // 计算MD5值
        public string EncryptString(string str)
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

        public class BaiduWebApiResponce
        {
            public TransResult trans_result { get; set; }
            public LijuResult liju_result { get; set; }
            public long logid { get; set; }

            public class TransResult
            {
                public List<Datum> data { get; set; }
                public string from { get; set; }
                public int status { get; set; }
                public string to { get; set; }
                public int type { get; set; }
                public List<Phonetic> phonetic { get; set; }

                public class Datum
                {
                    public string dst { get; set; }
                    public int prefixWrap { get; set; }
                    public List<List<object>> result { get; set; }
                    public string src { get; set; }
                }

                public class Phonetic
                {
                    public string src_str { get; set; }
                    public string trg_str { get; set; }
                }
            }

            public class LijuResult
            {
                public string @double { get; set; }
                public string single { get; set; }
            }
        }
    }
}