using DotNetCommons.CheckDigits;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetCommons.Test.CheckDigits
{
    [TestClass]
    public class AbaCheckDigitTests
    {
        private CheckDigit _checkDigit = null!;

        [TestInitialize]
        public void Setup()
        {
            _checkDigit = new AbaCheckDigit();
        }

        [TestMethod]
        public void TestCalculate()
        {
            Assert.AreEqual('8', _checkDigit.Calculate("01120060"));
            Assert.AreEqual('8', _checkDigit.Calculate("FC-01120060-"));
        }

        [TestMethod]
        public void TestAppend()
        {
            Assert.AreEqual("011200608", _checkDigit.Append("01120060"));
            Assert.AreEqual("FC-01120060-8", _checkDigit.Append("FC-01120060-"));
        }

        [TestMethod]
        public void TestValidate()
        {
            Assert.IsTrue(_checkDigit.Validate("011200608"));
            Assert.IsFalse(_checkDigit.Validate("011200609"));
            Assert.IsTrue(_checkDigit.Validate("111000025"));
            Assert.IsFalse(_checkDigit.Validate("111000024"));
        }

        [TestMethod, ExpectedException(typeof(InvalidOperationException))]
        public void TestCalculate_NoDigits() => _checkDigit.Calculate("FC-");

        [TestMethod, ExpectedException(typeof(InvalidOperationException))]
        public void TestCalculate_Empty() => _checkDigit.Calculate("");
    }
}