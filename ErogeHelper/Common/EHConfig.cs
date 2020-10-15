using ErogeHelper.Model;
using log4net;
using System.Xml.Linq;

namespace ErogeHelper.Common
{
    static class EHConfig
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(EHConfig));

        public static void WriteConfig(string writeTo, EHProfile pro)
        {
            var baseNode = new XElement("Profile",
                                      new XAttribute("Name", value: pro.Name),
                                      new XElement("MD5", content: pro.MD5.ToUpper()),
                                      new XElement("HookCode", content: pro.HookCode),
                                      new XElement("ThreadContext", content: pro.ThreadContext),
                                      new XElement("SubThreadContext", content: pro.SubThreadContext)
                             );

            var tree = new XElement("EHConfig", baseNode);
            tree.Save(writeTo);
            log.Info("Write config file succeed");
        }
    }
}
