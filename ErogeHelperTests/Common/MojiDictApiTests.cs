using Microsoft.VisualStudio.TestTools.UnitTesting;
using ErogeHelper.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace ErogeHelper.Common.Tests
{
    [TestClass()]
    public class MojiDictApiTests
    {
        [TestMethod()]
        public async Task MojiDictApiTest()
        {
            var query = "私";
            var result = await new MojiDictApi().RequestAsync(query);
            Assert.AreEqual(query, result.result.word.spell);
        }
    }
}