using DotNetCommons.CheckDigits;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetCommons.Test.CheckDigits
{
    [TestClass]
    public class LuhnCheckDigitsTests
    {
        private ICheckDigits _checkDigits = null!;

        [TestInitialize]
        public void Setup()
        {
            _checkDigits = new LuhnCheckDigits();
        }

        [TestMethod]
        public void TestCalculate()
        {
            Assert.AreEqual('3', _checkDigits.Calculate("1234567890"));
            Assert.AreEqual('3', _checkDigits.Calculate("FC-1234-5678-90-"));
            Assert.AreEqual('2', _checkDigits.Calculate("909"));
        }

        [TestMethod]
        public void TestAppend()
        {
            Assert.AreEqual("12345678903", _checkDigits.Append("1234567890"));
            Assert.AreEqual("FC-1234-5678-90-3", _checkDigits.Append("FC-1234-5678-90-"));
            Assert.AreEqual("9092", _checkDigits.Append("909"));
        }

        [TestMethod]
        public void TestValidate()
        {
            Assert.IsTrue(_checkDigits.Validate("49927398716"));
            Assert.IsFalse(_checkDigits.Validate("49927398717"));
            Assert.IsFalse(_checkDigits.Validate("1234567812345678"));
            Assert.IsTrue(_checkDigits.Validate("1234567812345670"));
        }

        [TestMethod, ExpectedException(typeof(InvalidOperationException))]
        public void TestCalculate_NoDigits() => _checkDigits.Calculate("FC-");

        [TestMethod, ExpectedException(typeof(InvalidOperationException))]
        public void TestCalculate_Empty() => _checkDigits.Calculate("");
    }
}