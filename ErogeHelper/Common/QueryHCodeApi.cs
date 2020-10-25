using log4net;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ErogeHelper.Common
{
    public class QueryHCodeApi
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(QueryHCodeApi));

        static readonly string GameQuery = "http://vnr.aniclan.com/connection.php?go=game_query";

		public static async Task<string> QueryCode(string md5)
        {
			string param = $"md5={md5}";
			byte[] bs = Encoding.ASCII.GetBytes(param);
			HttpWebRequest req = (HttpWebRequest)WebRequest.Create(GameQuery);
			req.Method = "POST";
			req.ContentType = "application/x-www-form-urlencoded";
			req.ContentLength = bs.Length;
			using (Stream reqStream = req.GetRequestStream())
			{
				reqStream.Write(bs, 0, bs.Length);
			}
            using WebResponse wr = await req.GetResponseAsync();
            using StreamReader sr = new StreamReader(wr.GetResponseStream());
            string xmlString = sr.ReadToEnd();

            var xDoc = XDocument.Parse(xmlString);
            var game = xDoc.Element("grimoire").Element("games").Element("game");
            if (game.Element("hook") != null)
            {
                return game.Element("hook").Value;
            }
            else
            {
                return "";
            }
        }
    }
}
