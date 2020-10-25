using log4net;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;

namespace ErogeHelper.Common
{
    public class MojiDictApi
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(MojiDictApi));

        string BaseUrl = "https://api.mojidict.com";

        string searchApi = "/parse/functions/search_v3";
        string fetchApi = "/parse/functions/fetchWord_v2";

        private HttpClient client;
        public MojiDictApi()
        {
            // lower than 100ms
            client = new HttpClient();
            client.BaseAddress = new Uri(BaseUrl);
        }

        public async Task<MojiFetchResponse> RequestAsync(string query)
        {
            MojiSearchPayload searchPayload = new MojiSearchPayload
            {
                //langEnv = "zh-CN_ja",
                needWords = "true",
                searchText = query,
                _ApplicationId = "E62VyFVLMiW7kvbtVq3p",
                //_ClientVersion = "",
                //_InstallationId = "",
                //_SessionToken = ""
            };

            // ? System.Net.Http.HttpRequestException:“发送请求时出错。”
            // WebException: 未能解析此远程名称: 'api.mojidict.com'
            try
            {
                HttpResponseMessage resMsg = await client.PostAsJsonAsync(searchApi, searchPayload);
                resMsg.EnsureSuccessStatusCode();

                var searchResponse = await resMsg.Content.ReadAsAsync<MojiSearchResponse>();

                if (searchResponse.result.words.Length != 0)
                {
                    MojiFetchPayload fetchPayload = new MojiFetchPayload
                    {
                        wordId = searchResponse.result.searchResults[0].tarId,
                        _ApplicationId = "E62VyFVLMiW7kvbtVq3p",
                    };

                    resMsg = await client.PostAsJsonAsync(fetchApi, fetchPayload);
                    resMsg.EnsureSuccessStatusCode();

                    return await resMsg.Content.ReadAsAsync<MojiFetchResponse>();
                }
                else
                {
                    return new MojiFetchResponse
                    {
                        result = null
                    };
                }
            }
            catch(HttpRequestException ex)
            {
                log.Error(ex.Message);
                return new MojiFetchResponse
                {
                    result = null
                };
            }
        }
    }

    #region MojiFetchPayload
    internal class MojiFetchPayload
    {
        public string wordId { get; set; }
        public string _ApplicationId { get; set; }
        public string _ClientVersion { get; set; }
        public string _InstallationId { get; set; }
        public string _SessionToken { get; set; }
    }
    #endregion

    #region MojiSearchPayload
    class MojiSearchPayload
    {
        public string langEnv { get; set; }
        public string needWords { get; set; }
        public string searchText { get; set; }
        public string _ApplicationId { get; set; }
        public string _ClientVersion { get; set; }
        public string _InstallationId { get; set; }
        public string _SessionToken { get; set; }
    }
    #endregion

    #region MojiSearchResponse
    /*
     * Search搜索不到时result状态
     * 
     * originalSearchText: "ドバサ"
     * searchResults: [{searchText: "ドバサ", count: 1,…}] 包含一个其他网站的索引
     * words: []
     * **/
    public class MojiSearchResponse
    {
        public Result result { get; set; }

        public class Result
        {
            public string originalSearchText { get; set; } // same as query
            public Searchresult[] searchResults { get; set; } // 与query最相近的索引列表，以及别个网站的跳转
            public Word[] words { get; set; } // 与 searchResults 索引对应，不一定完全一致，所以只使用index[0]号
        }

        public class Searchresult
        {
            public string searchText { get; set; }
            public string tarId { get; set; }    // 对应单词url后缀
            public int type { get; set; }
            public int count { get; set; }
            public string title { get; set; }
            public DateTime createdAt { get; set; }
            public DateTime updatedAt { get; set; }
            public string objectId { get; set; } // BoVutOlaPR
        }

        public class Word
        {
            public string excerpt { get; set; }  // [惯用语] 主动承担。（自分からすすんで引き受ける。） 
            public string spell { get; set; }    // 買う
            public string accent { get; set; }   // ?
            public string pron { get; set; }     // かう 
            public string romaji { get; set; }
            public DateTime createdAt { get; set; }
            public DateTime updatedAt { get; set; }
            public string updatedBy { get; set; }
            public string objectId { get; set; }  // 对应tarId
        }
    }
    #endregion

    #region MojiFetchResponse
    public class MojiFetchResponse
    {
        public Result result { get; set; }

        public class Result
        {
            public Word word { get; set; }
            public Detail[] details { get; set; } // details[0].title aka Shinhi
            public Subdetail[] subdetails { get; set; }
            public Example[] examples { get; set; } // 与subdetails相对应，可能null
        }

        public class Word
        {
            public string excerpt { get; set; } // details[0].title + subdetails[0]
            /// <summary>
            /// Surface
            /// </summary>
            public string spell { get; set; }   
            public string accent { get; set; }
            /// <summary>
            /// Hirakana
            /// </summary>
            public string pron { get; set; }
            public string romaji { get; set; }
            public DateTime createdAt { get; set; }
            public DateTime updatedAt { get; set; }
            public string updatedBy { get; set; }
            public string objectId { get; set; }
        }

        public class Detail
        {
            public string title { get; set; }
            public int index { get; set; }
            public DateTime createdAt { get; set; }
            public DateTime updatedAt { get; set; }
            public string wordId { get; set; }
            public string updatedBy { get; set; }
            public bool converted { get; set; }
            public string objectId { get; set; }
        }

        public class Subdetail
        {
            public string title { get; set; }
            public int index { get; set; }
            public DateTime createdAt { get; set; }
            public DateTime updatedAt { get; set; }
            public string wordId { get; set; }
            public string detailsId { get; set; }
            public string updatedBy { get; set; }
            public bool converted { get; set; }
            public string objectId { get; set; }
        }

        public class Example
        {
            public string title { get; set; }
            public int index { get; set; }
            public string trans { get; set; }
            public DateTime createdAt { get; set; }
            public DateTime updatedAt { get; set; }
            public string wordId { get; set; }
            public string subdetailsId { get; set; }
            public string updatedBy { get; set; }
            public bool converted { get; set; }
            public string objectId { get; set; }
        }
    }
    #endregion
}
