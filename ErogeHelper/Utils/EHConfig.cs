using ErogeHelper.Models;
using System.Xml.Linq;

namespace ErogeHelper.Utils
{
    static class EHConfig
    {
        public static void WriteConfig(string writeTo, EHProfile pro)
        {
            var baseNode = new XElement("Profile",
                                      new XAttribute("Name", pro.Name),
                                      new XElement("MD5", pro.MD5.ToUpper()),
                                      new XElement("HookCode", pro.HCode),
                                      new XElement("HookThreadNumber", pro.HookThread)
                             );

            var tree = new XElement("EHConfig", baseNode);

            try
            {
                tree.Save(writeTo);
            }
            catch
            {
            }
        }
    }
}
