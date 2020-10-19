using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace ErogeHelper.Common.Tests
{
    [TestClass()]
    public class QueryHCodeApiTests
    {
        [TestMethod()]
        public async Task QueryCodeTest()
        {
            var md5 = "8EA49EB5857E5B29ACD2D44FBA1A289B";
            var result = await QueryHCodeApi.QueryCode(md5);

            string hcode = "/HS4:8@3A740:anipani.exe";
            Assert.AreEqual(hcode, result);

        }
    }
}