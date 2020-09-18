using ErogeHelper.Models;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace ErogeHelper.Utils
{
    class Translate
    {
        static HttpClient client = new HttpClient();

        static string host = "https://api.mojidict.com";
        static string path = "/parse/functions/search_v3";

        public static async Task<WordInfo> Search(string query)
        {
            if (client.BaseAddress == null)
            {
                client.BaseAddress = new Uri(host);
            }

            MojiResponse respMsg = null;

            try
            {
                Payload payload = new Payload
                {
                    needWords = "true",
                    searchText = query,
                    _ApplicationId = "E62VyFVLMiW7kvbtVq3p",
                };

                respMsg = await RequestApi(payload);
                if (respMsg.result.words.Length == 0)
                {
                    return new WordInfo
                    {
                        Original = respMsg.result.originalSearchText,
                        Pron = null,
                        Trans = "没有找到"};
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return new WordInfo
            {
                Original = respMsg.result.originalSearchText,
                Pron = respMsg.result.words[0].pron,
                Trans = respMsg.result.words[0].excerpt
            };
        }

        private static async Task<MojiResponse> RequestApi(Payload payload)
        {
            MojiResponse result = null;

            HttpResponseMessage response = await client.PostAsJsonAsync(
                path, payload);
            response.EnsureSuccessStatusCode();

            result = await response.Content.ReadAsAsync<MojiResponse>();

            return result;
        }
    }

    public class Payload
    {
        public string langEnv { get; set; }
        public string needWords { get; set; }
        public string searchText { get; set; }
        public string _ApplicationId { get; set; }
        public string _ClientVersion { get; set; }
        public string _InstallationId { get; set; }
        public string _SessionToken { get; set; }
    }
}
