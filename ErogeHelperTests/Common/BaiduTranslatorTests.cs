using Microsoft.VisualStudio.TestTools.UnitTesting;
using ErogeHelper.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErogeHelper.Common.Tests
{
    [TestClass()]
    public class BaiduTranslatorTests
    {
        [TestMethod()]
        public async Task TranslateTest()
        {
            string source = "悠真くんを攻略すれば２１０円か。なるほどなぁ…";

            string appid = "";
            string pass = "";

            var t = new BaiduTranslator();
            var result = await t.Translate(source, Language.Japenese, Language.ChineseSimplified, appid, pass);

            string expect = "攻略悠真的话是210日元吧。原来如此啊…";
            Assert.AreEqual(expect, result);
        }
    }
}