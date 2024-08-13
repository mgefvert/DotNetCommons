using DotNetCommons.CheckDigits;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetCommons.Test.CheckDigits
{
    [TestClass]
    public class AbaCheckDigitsTests
    {
        private ICheckDigits _checkDigits = null!;

        [TestInitialize]
        public void Setup()
        {
            _checkDigits = new AbaCheckDigits();
        }

        [TestMethod]
        public void TestCalculate()
        {
            Assert.AreEqual('8', _checkDigits.Calculate("01120060"));
            Assert.AreEqual('8', _checkDigits.Calculate("FC-01120060-"));
        }

        [TestMethod]
        public void TestAppend()
        {
            Assert.AreEqual("011200608", _checkDigits.Append("01120060"));
            Assert.AreEqual("FC-01120060-8", _checkDigits.Append("FC-01120060-"));
        }

        [TestMethod]
        public void TestValidate()
        {
            Assert.IsTrue(_checkDigits.Validate("011200608"));
            Assert.IsFalse(_checkDigits.Validate("011200609"));
            Assert.IsTrue(_checkDigits.Validate("111000025"));
            Assert.IsFalse(_checkDigits.Validate("111000024"));
        }

        [TestMethod, ExpectedException(typeof(InvalidOperationException))]
        public void TestCalculate_NoDigits() => _checkDigits.Calculate("FC-");

        [TestMethod, ExpectedException(typeof(InvalidOperationException))]
        public void TestCalculate_Empty() => _checkDigits.Calculate("");
    }
}