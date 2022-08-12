using DotNetCommons.CheckDigits;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace DotNetCommons.Test.CheckDigits
{
    [TestClass]
    public class LuhnCheckDigitsTests
    {
        private ICheckDigits _luhn = null!;

        [TestInitialize]
        public void Setup()
        {
            _luhn = new LuhnCheckDigits();
        }

        [TestMethod]
        public void TestCalculate()
        {
            Assert.AreEqual('3', _luhn.Calculate("1234567890"));
            Assert.AreEqual('3', _luhn.Calculate("FC-1234-5678-90-"));
            Assert.AreEqual('2', _luhn.Calculate("909"));
        }

        [TestMethod]
        public void TestAppend()
        {
            Assert.AreEqual("12345678903", _luhn.Append("1234567890"));
            Assert.AreEqual("FC-1234-5678-90-3", _luhn.Append("FC-1234-5678-90-"));
            Assert.AreEqual("9092", _luhn.Append("909"));
        }

        [TestMethod]
        public void TestValidate()
        {
            Assert.IsTrue(_luhn.Validate("49927398716"));
            Assert.IsFalse(_luhn.Validate("49927398717"));
            Assert.IsFalse(_luhn.Validate("1234567812345678"));
            Assert.IsTrue(_luhn.Validate("1234567812345670"));
        }

        [TestMethod, ExpectedException(typeof(InvalidOperationException))]
        public void TestCalculate_NoDigits() => _luhn.Calculate("FC-");

        [TestMethod, ExpectedException(typeof(InvalidOperationException))]
        public void TestCalculate_Empty() => _luhn.Calculate("");
    }
}
