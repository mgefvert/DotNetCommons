using DotNetCommons.Text.ShuntingYard;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetCommons.Test.Text.ShuntingYard;

[TestClass]
public class ShuntingYardEvaluatorTests
{
    [TestMethod]
    public void Test_SimpleAddition()
    {
        ShuntingYardEvaluator.Evaluate("1 + 2").Should().Be(3.0);
    }

    [TestMethod]
    public void Test_OperatorPrecedence()
    {
        ShuntingYardEvaluator.Evaluate("2 + 3 * 4").Should().Be(14.0);
    }

    [TestMethod]
    public void Test_ParenthesesOverridePrecedence()
    {
        ShuntingYardEvaluator.Evaluate("(2 + 3) * 4").Should().Be(20.0);
    }

    [TestMethod]
    public void Test_NegativeNumberAtStart()
    {
        ShuntingYardEvaluator.Evaluate("-4 + 6").Should().Be(2.0);
    }

    [TestMethod]
    public void Test_NegativeNumberAfterOperator()
    {
        ShuntingYardEvaluator.Evaluate("3 * -2").Should().Be(-6.0);
    }

    [TestMethod]
    public void Test_Division()
    {
        ShuntingYardEvaluator.Evaluate("8 / 2").Should().Be(4.0);
    }

    [TestMethod]
    public void Test_Exponentiation()
    {
        ShuntingYardEvaluator.Evaluate("2 ^ 3").Should().Be(8.0);
        ShuntingYardEvaluator.Evaluate("2 ^ 3 ^ 2").Should().Be(512.0);  // 2 ^ (3 ^ 2)
        ShuntingYardEvaluator.Evaluate("(2 ^ 3) ^ 2").Should().Be(64.0); // (2 ^ 3) ^ 2 = 8 ^ 2
    }

    [TestMethod]
    public void Test_ComplexExpression()
    {
        ShuntingYardEvaluator.Evaluate("3 + 4 * 2 / (1 - 5)").Should().Be(1.0);
    }

    [TestMethod]
    public void Test_UnaryPlusIsIgnored()
    {
        ShuntingYardEvaluator.Evaluate("+5").Should().Be(5.0);
        ShuntingYardEvaluator.Evaluate("+3 + +2").Should().Be(5.0);
    }

    [TestMethod]
    public void Test_InvalidNumberThrows()
    {
        Assert.ThrowsException<InvalidOperationException>(() =>
        {
            ShuntingYardEvaluator.Evaluate("12.3.4 + 5");
        });
    }

    [TestMethod]
    public void Test_MismatchedParenthesesThrows()
    {
        Assert.ThrowsException<InvalidOperationException>(() =>
        {
            ShuntingYardEvaluator.Evaluate("(2 + 3");
        });

        Assert.ThrowsException<InvalidOperationException>(() =>
        {
            ShuntingYardEvaluator.Evaluate("2 + 3)");
        });
    }

    [TestMethod]
    public void Test_NotEnoughOperandsThrows()
    {
        Assert.ThrowsException<InvalidOperationException>(() =>
        {
            ShuntingYardEvaluator.Evaluate("1 +");
        });
    }

    [TestMethod]
    public void Test_EmptyExpressionThrows()
    {
        Assert.ThrowsException<InvalidOperationException>(() =>
        {
            ShuntingYardEvaluator.Evaluate("");
        });
    }

    [TestMethod]
    public void Test_Constants()
    {
        ShuntingYardEvaluator.Evaluate("pi").Should().Be(Math.PI);
        ShuntingYardEvaluator.Evaluate("e").Should().Be(Math.E);
    }

    [TestMethod]
    public void Test_ConstantsInExpressions()
    {
        ShuntingYardEvaluator.Evaluate("2 * pi").Should().Be(2 * Math.PI);
        ShuntingYardEvaluator.Evaluate("e ^ 2").Should().Be(Math.Pow(Math.E, 2));
        ShuntingYardEvaluator.Evaluate("pi + e").Should().Be(Math.PI + Math.E);
    }

    [TestMethod]
    public void Test_ConstantsCaseInsensitive()
    {
        ShuntingYardEvaluator.Evaluate("PI").Should().Be(Math.PI);
        ShuntingYardEvaluator.Evaluate("E").Should().Be(Math.E);
        ShuntingYardEvaluator.Evaluate("Pi").Should().Be(Math.PI);
    }

    [TestMethod]
    public void Test_UnknownConstantThrows()
    {
        Assert.ThrowsException<InvalidOperationException>(() =>
        {
            ShuntingYardEvaluator.Evaluate("unknown + 5");
        });
    }
}