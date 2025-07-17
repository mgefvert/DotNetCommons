using DotNetCommons.Text.ShuntingYard;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetCommons.Test.Text.ShuntingYard;

[TestClass]
public class ShuntingYardEvaluatorTests
{
    private ShuntingYardEvaluator Evaluator => ShuntingYardEvaluator.Default;

    [TestMethod]
    public void Test_SimpleAddition()
    {
        Evaluator.Evaluate("1 + 2").Should().Be(3.0);
    }

    [TestMethod]
    public void Test_OperatorPrecedence()
    {
        Evaluator.Evaluate("2 + 3 * 4").Should().Be(14.0);
    }

    [TestMethod]
    public void Test_ParenthesesOverridePrecedence()
    {
        Evaluator.Evaluate("(2 + 3) * 4").Should().Be(20.0);
    }

    [TestMethod]
    public void Test_NegativeNumberAtStart()
    {
        Evaluator.Evaluate("-4 + 6").Should().Be(2.0);
    }

    [TestMethod]
    public void Test_NegativeNumberAfterOperator()
    {
        Evaluator.Evaluate("3 * -2").Should().Be(-6.0);
    }

    [TestMethod]
    public void Test_Division()
    {
        Evaluator.Evaluate("8 / 2").Should().Be(4.0);
    }

    [TestMethod]
    public void Test_Exponentiation()
    {
        Evaluator.Evaluate("2 ^ 3").Should().Be(8.0);
        Evaluator.Evaluate("2 ^ 3 ^ 2").Should().Be(512.0);  // 2 ^ (3 ^ 2)
        Evaluator.Evaluate("(2 ^ 3) ^ 2").Should().Be(64.0); // (2 ^ 3) ^ 2 = 8 ^ 2
    }

    [TestMethod]
    public void Test_ComplexExpression()
    {
        Evaluator.Evaluate("3 + 4 * 2 / (1 - 5)").Should().Be(1.0);
    }

    [TestMethod]
    public void Test_UnaryPlusIsIgnored()
    {
        Evaluator.Evaluate("+5").Should().Be(5.0);
        Evaluator.Evaluate("+3 + +2").Should().Be(5.0);
    }

    [TestMethod]
    public void Test_InvalidNumberThrows()
    {
        Assert.ThrowsException<InvalidOperationException>(() =>
        {
            Evaluator.Evaluate("12.3.4 + 5");
        });
    }

    [TestMethod]
    public void Test_MismatchedParenthesesThrows()
    {
        Assert.ThrowsException<InvalidOperationException>(() =>
        {
            Evaluator.Evaluate("(2 + 3");
        });

        Assert.ThrowsException<InvalidOperationException>(() =>
        {
            Evaluator.Evaluate("2 + 3)");
        });
    }

    [TestMethod]
    public void Test_NotEnoughOperandsThrows()
    {
        Assert.ThrowsException<InvalidOperationException>(() =>
        {
            Evaluator.Evaluate("1 +");
        });
    }

    [TestMethod]
    public void Test_EmptyExpressionThrows()
    {
        Assert.ThrowsException<InvalidOperationException>(() =>
        {
            Evaluator.Evaluate("");
        });
    }

    [TestMethod]
    public void Test_Constants()
    {
        Evaluator.Evaluate("pi").Should().Be(Math.PI);
        Evaluator.Evaluate("e").Should().Be(Math.E);
    }

    [TestMethod]
    public void Test_ConstantsInExpressions()
    {
        Evaluator.Evaluate("2 * pi").Should().Be(2 * Math.PI);
        Evaluator.Evaluate("e ^ 2").Should().Be(Math.Pow(Math.E, 2));
        Evaluator.Evaluate("pi + e").Should().Be(Math.PI + Math.E);
    }

    [TestMethod]
    public void Test_ConstantsCaseInsensitive()
    {
        Evaluator.Evaluate("PI").Should().Be(Math.PI);
        Evaluator.Evaluate("E").Should().Be(Math.E);
        Evaluator.Evaluate("Pi").Should().Be(Math.PI);
    }

    [TestMethod]
    public void Test_UnknownConstantThrows()
    {
        Assert.ThrowsException<InvalidOperationException>(() =>
        {
            Evaluator.Evaluate("unknown + 5");
        });
    }

    [TestMethod]
    public void Test_Functions()
    {
        Evaluator.Evaluate("sqrt(4)").Should().Be(2.0);
        Evaluator.Evaluate("abs(-5)").Should().Be(5.0);
        Evaluator.Evaluate("sin(0)").Should().Be(0.0);
        Evaluator.Evaluate("cos(0)").Should().Be(1.0);
        Evaluator.Evaluate("tan(0)").Should().Be(0.0);
        Evaluator.Evaluate("log(100)").Should().Be(2.0);
        Evaluator.Evaluate("ln(e)").Should().Be(1.0);
    }

    [TestMethod]
    public void Test_FunctionsInExpressions()
    {
        Evaluator.Evaluate("2 * sqrt(9)").Should().Be(6.0);
        Evaluator.Evaluate("sqrt(9) + abs(-3)").Should().Be(6.0);
        Evaluator.Evaluate("sin(pi/2)").Should().BeApproximately(1.0, 0.0000001);
        Evaluator.Evaluate("3 + sqrt(4 * 9)").Should().Be(9.0);
    }

    [TestMethod]
    public void Test_FunctionsCaseInsensitive()
    {
        Evaluator.Evaluate("SQRT(4)").Should().Be(2.0);
        Evaluator.Evaluate("Sqrt(4)").Should().Be(2.0);
        Evaluator.Evaluate("ABS(-3)").Should().Be(3.0);
    }

    [TestMethod]
    public void Test_NestedFunctions()
    {
        Evaluator.Evaluate("sqrt(abs(-16))").Should().Be(4.0);
        Evaluator.Evaluate("abs(sin(pi))").Should().BeApproximately(0.0, 0.0000001);
    }

    [TestMethod]
    public void Test_FunctionWithMissingArgumentThrows()
    {
        Assert.ThrowsException<InvalidOperationException>(() =>
        {
            Evaluator.Evaluate("sqrt()");
        });
    }

    [TestMethod]
    public void Test_UnknownFunctionThrows()
    {
        Assert.ThrowsException<InvalidOperationException>(() =>
        {
            Evaluator.Evaluate("unknown(5)");
        });
    }

    [TestMethod]
    public void Test_CustomConstants()
    {
        // Create a new evaluator instance (not the default one)
        var evaluator = new ShuntingYardEvaluator();

        // Add custom constants
        evaluator.AddConstant("golden", 1.618033988749895);
        evaluator.AddConstant("tau", Math.PI * 2);

        // Test the custom constants
        evaluator.Evaluate("golden").Should().Be(1.618033988749895);
        evaluator.Evaluate("tau").Should().Be(Math.PI * 2);
        evaluator.Evaluate("tau / 2").Should().Be(Math.PI);

        // Custom constants should be case-insensitive
        evaluator.Evaluate("GOLDEN").Should().Be(1.618033988749895);
        evaluator.Evaluate("Tau").Should().Be(Math.PI * 2);

        // Default constants should still work
        evaluator.Evaluate("pi").Should().Be(Math.PI);
        evaluator.Evaluate("e").Should().Be(Math.E);
    }

    [TestMethod]
    public void Test_CustomConstantsOverrideDefaults()
    {
        var evaluator = new ShuntingYardEvaluator();

        // Override built-in constant
        evaluator.AddConstant("pi", 3.0); // Not the real value

        evaluator.Evaluate("pi").Should().Be(3.0);
        evaluator.Evaluate("PI").Should().Be(3.0);
    }

    [TestMethod]
    public void Test_CannotModifyDefaultEvaluatorConstants()
    {
        Assert.ThrowsException<InvalidOperationException>(() =>
        {
            ShuntingYardEvaluator.Default.AddConstant("tau", Math.PI * 2);
        });
    }

    [TestMethod]
    public void Test_CustomFunctions()
    {
        var evaluator = new ShuntingYardEvaluator();

        // Add custom functions
        evaluator.AddFunction("square", x => x * x);
        evaluator.AddFunction("cube", x => x * x * x);
        evaluator.AddFunction("double", x => x * 2);
        evaluator.AddFunction("half", x => x / 2);

        // Test the custom functions
        evaluator.Evaluate("square(4)").Should().Be(16.0);
        evaluator.Evaluate("cube(3)").Should().Be(27.0);
        evaluator.Evaluate("double(5)").Should().Be(10.0);
        evaluator.Evaluate("half(8)").Should().Be(4.0);

        // Custom functions should be case-insensitive
        evaluator.Evaluate("SQUARE(4)").Should().Be(16.0);
        evaluator.Evaluate("Cube(3)").Should().Be(27.0);

        // Default functions should still work
        evaluator.Evaluate("sqrt(16)").Should().Be(4.0);
        evaluator.Evaluate("sin(0)").Should().Be(0.0);
    }

    [TestMethod]
    public void Test_CustomFunctionsInExpressions()
    {
        var evaluator = new ShuntingYardEvaluator();

        evaluator.AddFunction("square", x => x * x);
        evaluator.AddFunction("increment", x => x + 1);

        evaluator.Evaluate("square(3) + 2").Should().Be(11.0);
        evaluator.Evaluate("square(sqrt(16))").Should().Be(16.0);
        evaluator.Evaluate("increment(square(3))").Should().Be(10.0);
        evaluator.Evaluate("square(increment(3))").Should().Be(16.0);
    }

    [TestMethod]
    public void Test_CustomFunctionsOverrideDefaults()
    {
        var evaluator = new ShuntingYardEvaluator();

        // Override built-in function
        evaluator.AddFunction("sqrt", x => x); // Identity function instead of square root

        evaluator.Evaluate("sqrt(16)").Should().Be(16.0); // Not 4.0
        evaluator.Evaluate("SQRT(16)").Should().Be(16.0);
    }

    [TestMethod]
    public void Test_CannotModifyDefaultEvaluatorFunctions()
    {
        Assert.ThrowsException<InvalidOperationException>(() =>
        {
            ShuntingYardEvaluator.Default.AddFunction("square", x => x * x);
        });
    }

    [TestMethod]
    public void Test_CustomConstantsAndFunctionsTogether()
    {
        var evaluator = new ShuntingYardEvaluator();

        evaluator.AddConstant("c", 299792458); // Speed of light in m/s
        evaluator.AddFunction("square", x => x * x);

        // E = mc²
        evaluator.Evaluate("10 * square(c)").Should().Be(10 * 299792458.0 * 299792458.0);
    }
}