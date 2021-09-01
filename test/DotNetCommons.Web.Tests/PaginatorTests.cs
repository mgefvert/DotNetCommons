using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetCommons.Web.Tests
{
    [TestClass]
    public class PaginatorTests
    {
        [TestMethod]
        public void ItWorks()
        {
            var paginator = Paginator.FromPages(0, 3, 0);
            Assert.AreEqual(3, paginator.Count);
            Assert.AreEqual(0, paginator.Current);
            Assert.AreEqual(2, paginator.Max);
            Assert.AreEqual(0, paginator.Mid);
            Assert.AreEqual(1, paginator.Next);
            Assert.IsNull(paginator.Previous);
        }
    }
}
