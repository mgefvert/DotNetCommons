using DotNetCommons.CheckDigits;

namespace DotNetCommonTests.CheckDigits
{
    [TestClass]
    public class LuhnCheckDigitTests
    {
        private CheckDigit _checkDigit = null!;

        [TestInitialize]
        public void Setup()
        {
            _checkDigit = new LuhnCheckDigit();
        }

        [TestMethod]
        public void TestCalculate()
        {
            Assert.AreEqual('3', _checkDigit.Calculate("1234567890"));
            Assert.AreEqual('3', _checkDigit.Calculate("FC-1234-5678-90-"));
            Assert.AreEqual('2', _checkDigit.Calculate("909"));
        }

        [TestMethod]
        public void TestAppend()
        {
            Assert.AreEqual("12345678903", _checkDigit.Append("1234567890"));
            Assert.AreEqual("FC-1234-5678-90-3", _checkDigit.Append("FC-1234-5678-90-"));
            Assert.AreEqual("9092", _checkDigit.Append("909"));
        }

        [TestMethod]
        public void TestValidate()
        {
            Assert.IsTrue(_checkDigit.Validate("49927398716"));
            Assert.IsFalse(_checkDigit.Validate("49927398717"));
            Assert.IsFalse(_checkDigit.Validate("1234567812345678"));
            Assert.IsTrue(_checkDigit.Validate("1234567812345670"));
        }

        [TestMethod]
        public void TestCalculate_NoDigits() => Assert.ThrowsExactly<InvalidOperationException>(() => _checkDigit.Calculate("FC-"));

        [TestMethod]
        public void TestCalculate_Empty() => Assert.ThrowsExactly<InvalidOperationException>(() => _checkDigit.Calculate(""));
    }
}