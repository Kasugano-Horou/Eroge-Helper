using ErogeHelper.Common;
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
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace ErogeHelper.Repository
{
    public class BaiduWebTranslator : ITranslator
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(BaiduWebTranslator));

        private static CancellationTokenSource _cancelToken = new CancellationTokenSource();

        private string errorInfo;

        public string GetLastError()
        {
            return errorInfo;
        }

        public string GetTranslatorName()
        {
            return "BaiduWeb";
        }

        private const string baseUrl = @"https://www.baidu.com";
        private const string transUrl = @"https://fanyi.baidu.com";
        private const string serviceUrl = @"https://fanyi.baidu.com/v2transapi";

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
                // 创建HttpClientHandler以先行获取cookie
                var uri = new Uri(baseUrl);
                var client = Utils.GetHttpClient();

                // Do request to get cookies
                if (sb_cookie.ToString() == "")
                {
                    // May cause error? 网不行的时候先报TaskCanceledException
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

                string gtk = "";
                string token = "";
                string content = client.GetStringAsync(transUrl).GetAwaiter().GetResult();
                var tokenMatch = Regex.Match(content, "token: '(.*?)',");
                var gtkMatch = Regex.Match(content, "window.gtk = '(.*?)';");
                if (gtkMatch.Success && gtkMatch.Groups.Count > 1)
                    gtk = gtkMatch.Groups[1].Value;
                if (tokenMatch.Success && tokenMatch.Groups.Count > 1)
                    token = tokenMatch.Groups[1].Value;
                jsEngine.Evaluate(jsBaiduToken);
                string sign = jsEngine.CallGlobalFunction<string>("token", sourceText, gtk);

                var dict = new Dictionary<string, string>
                {
                    {"from", from},
                    {"to", to},
                    {"query", sourceText},
                    {"transtype", "translang"},
                    {"simple_means_flag", "3"},
                    {"sign", sign},
                    {"token", token}
                };
                var data = new FormUrlEncodedContent(dict);

                resp = await client.PostAsync(serviceUrl, data);
                resp.EnsureSuccessStatusCode(); // HttpRequestException
            }
            catch(HttpRequestException ex)
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

            BaiduWebApiResponce obj = await resp.Content.ReadAsAsync<BaiduWebApiResponce>();

            if (obj.liju_result == null || obj.trans_result == null || obj.logid == 0)
            {
                errorInfo = "未知错误";
                return null;
            }
            return obj.trans_result.data[0].dst;
        }

        private static StringBuilder sb_cookie = new StringBuilder();
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