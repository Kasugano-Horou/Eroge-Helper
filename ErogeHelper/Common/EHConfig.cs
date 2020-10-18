using ErogeHelper.Model;
using log4net;
using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace ErogeHelper.Common
{
    static class EHConfig
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(EHConfig));

        private static string Path = GalaSoft.MvvmLight.Ioc.SimpleIoc.Default.GetInstance<GameInfo>().ConfigPath;

        public static void WriteConfig(string writeTo, EHProfile pro)
        {
            var baseNode = new XElement("Profile",
                new XAttribute("Name", value: pro.Name),
                new XElement("MD5", content: pro.MD5.ToUpper()),
                new XElement("HookCode", content: pro.HookCode),
                new XElement("ThreadContext", content: pro.ThreadContext),
                new XElement("SubThreadContext", content: pro.SubThreadContext),
                new XElement("Regexp", content: pro.Regexp)
            );

            var tree = new XElement("EHConfig", baseNode);
            tree.Save(writeTo);
            log.Info("Write config file succeed");
        }

        public static void SetValue(string Node, string value)
        {

        }

        public static string GetValue(string Node)
        {
            return null;
        }

        internal static void NewWriteConfig(IEnumerable<XElement> nodeList)
        {
            var baseNode = new XElement("Profile", nodeList);
            var tree = new XElement("EHConfig", baseNode);
            tree.Save(Path);
            log.Info("Update config file");
        }
    }

    public struct EHProfile
    {
        public string Name;
        public string MD5;
        public string HookCode;
        public long ThreadContext;
        public long SubThreadContext;
        public string Regexp;
    }
}
