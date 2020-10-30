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

        private static readonly string Path = GalaSoft.MvvmLight.Ioc.SimpleIoc.Default.GetInstance<GameInfo>().ConfigPath;

        public static void FirstTimeWriteConfig(EHProfile pro)
        {
            var baseNode = new XElement("Profile",
                new XAttribute("Name", value: pro.Name),
                new XElement("MD5", content: pro.MD5.ToUpper()),
                new XElement("IsUserHook", content: pro.IsUserHook),
                new XElement("HookCode", content: pro.HookCode),
                new XElement("ThreadContext", content: pro.ThreadContext),
                new XElement("SubThreadContext", content: pro.SubThreadContext),
                new XElement("Regexp", content: pro.Regexp),
                new XElement("NoFocus", content: pro.NoFocus)
            );

            var tree = new XElement("EHConfig", baseNode);
            tree.Save(Path);
            log.Info("Write config file succeed");
        }

        public static void SetValue(EHNode node, string value)
        {
            var doc = XDocument.Load(Path);
            var profile = doc.Element(EHNode.EHConfig.Name).Element(EHNode.Profile.Name);

            var oldNode = profile.Element(node.Name);
            if (oldNode != null) 
            {
                oldNode.Value = value;
            }
            else
            {
                profile.Add(new XElement(node.Name)
                {
                    Value = value
                });
            }

            doc.Save(Path);
        }

        public static string GetValue(EHNode node)
        {
            try
            {
                var doc = XElement.Load(Path);
                var profile = doc.Element("Profile");

                return profile.Element(node.Name).Value;
            }
            // should not be happend
            catch (NullReferenceException)
            {
                throw new NullReferenceException($"致命错误: 可能是配置文件中{node.Name}节点不存在");
            }
        }
    }

    public struct EHProfile
    {
        public string Name;
        public string MD5;
        public string IsUserHook;
        public string HookCode;
        public long ThreadContext;
        public long SubThreadContext;
        public string Regexp;
        public string NoFocus;
    }

    public class EHNode
    {
        private EHNode(string value) { Name = value; }

        public string Name { get; set; }

        public static EHNode EHConfig { get { return new EHNode("EHConfig"); } }
        public static EHNode Profile { get { return new EHNode("Profile"); } }

        public static EHNode IsUserHook { get { return new EHNode("IsUserHook"); } }
        public static EHNode HookCode { get { return new EHNode("HookCode"); } }
        public static EHNode ThreadContext { get { return new EHNode("ThreadContext"); } }
        public static EHNode SubThreadContext { get { return new EHNode("SubThreadContext"); } }
        public static EHNode Regexp { get { return new EHNode("Regexp"); } }
        public static EHNode NoFocus { get { return new EHNode("NoFocus"); } }
    }
}
