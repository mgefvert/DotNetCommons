using DotNetCommons.Text.Tokenizer;
using System.Globalization;

namespace DotNetCommons.Text.ShuntingYard;

/// <summary>
/// Represents an evaluator based on the Shunting-Yard algorithm for parsing and evaluating mathematical expressions.
/// Provides methods to tokenize expressions, convert them to postfix notation, and evaluate the resulting expressions.
/// </summary>
public class ShuntingYardEvaluator
{
    private readonly StringTokenizer<ShuntingYardToken> _tokenizer = new(Definitions);
    private static readonly object DefaultLock = new();
    private static ShuntingYardEvaluator? _default;

    public delegate double Function0();
    public delegate double Function1(double arg);

    /// <summary>
    /// Provides a default, singleton instance of the <see cref="ShuntingYardEvaluator"/> class.
    /// This instance is lazily instantiated and thread-safe, ensuring a single global instance
    /// of the evaluator that can be reused for parsing and evaluating mathematical expressions.
    /// </summary>
    public static ShuntingYardEvaluator Default
    {
        get
        {
            if (_default != null)
                return _default;

            lock (DefaultLock)
            {
                return _default ??= new ShuntingYardEvaluator();
            }
        }
    }

    private static readonly Dictionary<string, (int precedence, bool rightAssociative)> Operators = new()
    {
        ["+"] = (1, false),
        ["-"] = (1, false),
        ["*"] = (2, false),
        ["/"] = (2, false),
        ["^"] = (3, true)
    };

    private static readonly Dictionary<string, double> DefaultConstants = new()
    {
        ["pi"] = Math.PI,
        ["e"] = Math.E
    };

    private static readonly Dictionary<string, Function1> DefaultFunctions1 = new()
    {
        // Roots and exponents, logarithms
        ["sqrt"] = Math.Sqrt,
        ["exp"]  = Math.Exp,
        ["log"]  = Math.Log10,
        ["ln"]   = Math.Log,

        // Rounding and utility
        ["abs"]   = Math.Abs,
        ["ceil"]  = Math.Ceiling,
        ["floor"] = Math.Floor,
        ["round"] = Math.Round,
        ["trunc"] = Math.Truncate,

        // Trigonometric functions
        ["sin"]   = Math.Sin,
        ["asin"]  = Math.Asin,
        ["sinh"]  = Math.Sinh,
        ["asinh"] = Math.Asinh,
        ["cos"]   = Math.Cos,
        ["acos"]  = Math.Acos,
        ["cosh"]  = Math.Cosh,
        ["acosh"] = Math.Acosh,
        ["tan"]   = Math.Tan,
        ["tanh"]  = Math.Tanh,
        ["atan"]  = Math.Atan,
        ["atanh"] = Math.Atanh,
    };

    private static readonly Dictionary<string, Function0> DefaultFunctions0 = new()
    {
        // Random number generation
        ["rnd"] = () => new Random().NextDouble(),
    };

    private readonly Dictionary<string, double> _constants;
    private readonly Dictionary<string, Function0> _functions0;
    private readonly Dictionary<string, Function1> _functions1;

    private static readonly Definition<ShuntingYardToken>[] Definitions =
    [
        new Characters<ShuntingYardToken>(ShuntingYardToken.Number, false).Add(TokenMode.Digit).AddSpecific("."),
        new Characters<ShuntingYardToken>(ShuntingYardToken.Whitespace, true).Add(TokenMode.Whitespace).Add(TokenMode.EndOfLine),
        new Characters<ShuntingYardToken>(ShuntingYardToken.Identifier, false).Add(TokenMode.Letter).Add(TokenMode.Digit),
        new Strings<ShuntingYardToken>(ShuntingYardToken.Operator, "+", false),
        new Strings<ShuntingYardToken>(ShuntingYardToken.Operator, "-", false),
        new Strings<ShuntingYardToken>(ShuntingYardToken.Operator, "/", false),
        new Strings<ShuntingYardToken>(ShuntingYardToken.Operator, "*", false),
        new Strings<ShuntingYardToken>(ShuntingYardToken.Operator, "^", false),
        new Strings<ShuntingYardToken>(ShuntingYardToken.LeftParen, "(", false),
        new Strings<ShuntingYardToken>(ShuntingYardToken.RightParen, ")", false),
        new Strings<ShuntingYardToken>(ShuntingYardToken.Comma, ",", false)
    ];

    /// <summary>
    /// Represents an evaluator based on the Shunting-Yard algorithm to evaluate mathematical expressions.
    /// Provides functionality to tokenize, convert to postfix notation, and evaluate expressions.
    /// </summary>
    public ShuntingYardEvaluator()
    {
        _constants = DefaultConstants.ToDictionary(x => x.Key, x => x.Value);
        _functions0 = DefaultFunctions0.ToDictionary(x => x.Key, x => x.Value);
        _functions1 = DefaultFunctions1.ToDictionary(x => x.Key, x => x.Value);
    }

    /// <summary>
    /// Adds a constant with the specified name and value to the evaluator.
    /// </summary>
    /// <param name="name">The name of the constant to add.</param>
    /// <param name="value">The value of the constant to add.</param>
    public void AddConstant(string name, double value)
    {
        if (this == Default)
            throw new InvalidOperationException("Cannot add constants to the default instance of the evaluator");

        _constants[name] = value;
    }

    /// <summary>
    /// Adds a custom zero-arity function with the specified name and implementation to the evaluator.
    /// These functions take no arguments and return a double value.
    /// </summary>
    /// <param name="name">The name of the function to add.</param>
    /// <param name="function">The delegate representing the function's implementation that takes no arguments and
    /// returns a double.</param>
    public void AddFunction(string name, Function0 function)
    {
        if (this == Default)
            throw new InvalidOperationException("Cannot add functions to the default instance of the evaluator");

        _functions0[name] = function;
    }

    /// <summary>
    /// Adds a custom function with the specified name and implementation to the evaluator.
    /// </summary>
    /// <param name="name">The name of the function to add.</param>
    /// <param name="function">The delegate representing the function's implementation that takes a single argument and
    /// returns a double.</param>
    public void AddFunction(string name, Function1 function)
    {
        if (this == Default)
            throw new InvalidOperationException("Cannot add functions to the default instance of the evaluator");

        _functions1[name] = function;
    }

    /// <summary>
    /// Tokenizes a mathematical expression into a list of categorized tokens based on the Shunting-Yard algorithm.
    /// Validates and processes numerical tokens, identifiers, constants, and functions during tokenization.
    /// </summary>
    /// <param name="source">The input string containing the mathematical expression to tokenize.</param>
    /// <returns>A <see cref="TokenList{ShuntingYardToken}"/> containing the categorized tokens of the input expression.</returns>
    /// <exception cref="InvalidOperationException">Thrown when an invalid number, unknown identifier, or unknown constant is
    /// encountered during tokenization.</exception>
    public TokenList<ShuntingYardToken> Tokenize(string source)
    {
        var result = _tokenizer.Tokenize(source);

        // Parse and validate all numbers
        foreach (var token in result.Where(x => x.ID == ShuntingYardToken.Number))
        {
            if (double.TryParse(token.Text, CultureInfo.InvariantCulture, out var value))
                token.Tag = value;
            else
                throw new InvalidOperationException($"Invalid number '{token.Text}' at {token.Line}:{token.Column}");
        }

        // Process identifiers (constants and functions)
        foreach (var token in result.Where(x => x.ID == ShuntingYardToken.Identifier))
        {
            var name = token.Text?.ToLowerInvariant();

            // Check if it's a constant
            if (_constants.TryGetValue(name!, out var value))
            {
                token.ID = ShuntingYardToken.Number;
                token.Tag = value;
            }
            // Check if it's a function with one argument
            else if (_functions1.TryGetValue(name!, out var function))
            {
                token.ID = ShuntingYardToken.Function1;
                token.Tag = function;
            }
            // Check if it's a zero-arity function
            else if (_functions0.TryGetValue(name!, out var functionZero))
            {
                token.ID = ShuntingYardToken.Function0;
                token.Tag = functionZero;
            }
            else
                throw new InvalidOperationException($"Unknown identifier '{token.Text}' at {token.Line}:{token.Column}");
        }

        // Process identifiers (constants)
        foreach (var token in result.Where(x => x.ID == ShuntingYardToken.Identifier))
        {
            var constantName = token.Text?.ToLowerInvariant();
            if (DefaultConstants.TryGetValue(constantName!, out var value))
            {
                token.ID = ShuntingYardToken.Number;
                token.Tag = value;
            }
            else
                throw new InvalidOperationException($"Unknown identifier '{token.Text}' at {token.Line}:{token.Column}");
        }

        // Rewrite token stream to handle special cases
        for (var i = 0; i < result.Count; )
        {
            if (result.IsToken(i, ShuntingYardToken.Operator, "-") && result.IsToken(i + 1, ShuntingYardToken.Number))
            {
                // Must be preceded by either by an operator, a left paren, or the start of the expression
                if (i == 0 || result.IsToken(i - 1, ShuntingYardToken.Operator) || result.IsToken(i - 1, ShuntingYardToken.LeftParen))
                {
                    result.RemoveAt(i);
                    result[i].Tag = -(double)result[i].Tag;
                    continue;
                }
            }

            // Rewrite unary plus (just remove it)
            if (result.IsToken(i, ShuntingYardToken.Operator, "+") && result.IsToken(i + 1, ShuntingYardToken.Number))
            {
                if (i == 0 || result.IsToken(i - 1, ShuntingYardToken.Operator) || result.IsToken(i - 1, ShuntingYardToken.LeftParen))
                {
                    result.RemoveAt(i); // Drop the unary plus
                    continue;
                }
            }

            i++;
        }

        return result;
    }

    /// <summary>
    /// Converts a given list of tokens into postfix (Reverse Polish) notation using the Shunting-Yard algorithm.
    /// </summary>
    /// <param name="tokens">The list of tokens representing the mathematical expression in infix notation.</param>
    /// <returns>A token list representing the mathematical expression in postfix notation.</returns>
    /// <exception cref="InvalidOperationException">Thrown when mismatched parentheses or unexpected tokens are encountered
    /// in the input.</exception>
    public TokenList<ShuntingYardToken> ToPostfix(TokenList<ShuntingYardToken> tokens)
    {
        var output = new TokenList<ShuntingYardToken>();
        var stack  = new Stack<Token<ShuntingYardToken>>();

        foreach (var token in tokens)
        {
            switch (token.ID)
            {
                case ShuntingYardToken.Number:
                    output.Add(token);
                    break;

                case ShuntingYardToken.Function0:
                    // For zero-arity functions, we can add them directly to the output
                    // because they don't need to wait for arguments
                    output.Add(token);
                    break;

                case ShuntingYardToken.Function1:
                    stack.Push(token);
                    break;

                case ShuntingYardToken.Operator:
                    while (stack.Count > 0 && stack.Peek().ID == ShuntingYardToken.Operator)
                    {
                        var top      = stack.Peek();
                        var currPrec = Operators[token.Text!].precedence;
                        var topPrec  = Operators[top.Text!].precedence;
                        var isRight  = Operators[token.Text!].rightAssociative;

                        if ((isRight && currPrec < topPrec) || (!isRight && currPrec <= topPrec))
                            output.Add(stack.Pop());
                        else
                            break;
                    }

                    stack.Push(token);
                    break;

                case ShuntingYardToken.LeftParen:
                    stack.Push(token);
                    break;

                case ShuntingYardToken.RightParen:
                    while (stack.Count > 0 && stack.Peek().ID != ShuntingYardToken.LeftParen)
                        output.Add(stack.Pop());

                    if (stack.Count == 0 || stack.Peek().ID != ShuntingYardToken.LeftParen)
                        throw new InvalidOperationException("Mismatched parentheses");

                    stack.Pop(); // discard the left paren

                    // If the token at the top of the stack is a function token, pop it onto the output queue
                    if (stack.Count > 0 && stack.Peek().ID == ShuntingYardToken.Function1)
                        output.Add(stack.Pop());
                    break;

                default:
                    throw new InvalidOperationException($"Unexpected token '{token.ID}' at {token.Line}:{token.Column}");
            }
        }

        while (stack.Count > 0)
        {
            if (stack.Peek().ID is ShuntingYardToken.LeftParen or ShuntingYardToken.RightParen)
            {
                var token = stack.Pop();
                throw new InvalidOperationException($"Mismatched parentheses at {token.Line}:{token.Column}");
            }

            output.Add(stack.Pop());
        }

        return output;
    }

    /// <summary>
    /// Evaluates a mathematical expression represented in postfix notation and calculates its result.
    /// </summary>
    /// <param name="postfix">The postfix notation tokens representing the mathematical expression to evaluate.</param>
    /// <returns>The result of the evaluated mathematical expression as a double.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the expression is invalid, contains insufficient or unexpected operands,
    /// or an unknown operator is encountered during evaluation.
    /// </exception>
    public double Evaluate(TokenList<ShuntingYardToken> postfix)
    {
        var stack = new Stack<double>();

        foreach (var token in postfix)
        {
            switch (token.ID)
            {
                case ShuntingYardToken.Number:
                    stack.Push((double)token.Tag);
                    break;

                case ShuntingYardToken.Function0:
                    var funcZero = (Function0)token.Tag;
                    stack.Push(funcZero());
                    break;

                case ShuntingYardToken.Function1:
                    if (stack.Count < 1)
                        throw new InvalidOperationException($"Not enough operands for function at {token.Line}:{token.Column}");

                    var arg = stack.Pop();
                    var func = (Function1)token.Tag;
                    stack.Push(func(arg));
                    break;

                case ShuntingYardToken.Operator:
                    if (stack.Count < 2)
                        throw new InvalidOperationException($"Not enough operands at {token.Line}:{token.Column}");

                    var b = stack.Pop();
                    var a = stack.Pop();

                    stack.Push(token.Text switch
                    {
                        "+" => a + b,
                        "-" => a - b,
                        "*" => a * b,
                        "/" => a / b,
                        "^" => Math.Pow(a, b),
                        _   => throw new InvalidOperationException($"Unknown operator '{token.Text}' at {token.Line}:{token.Column}")
                    });
                    break;

                default:
                    throw new InvalidOperationException($"Unexpected token '{token.ID}' at {token.Line}:{token.Column}");
            }
        }

        if (stack.Count != 1)
            throw new InvalidOperationException("Invalid expression");

        return stack.Pop();
    }

    /// <summary>
    /// Evaluates a mathematical expression represented as a string by tokenizing, converting to postfix notation, and calculating
    /// the result.
    /// </summary>
    /// <param name="text">The mathematical expression in string format to be evaluated.</param>
    /// <returns>The result of the evaluated mathematical expression as a double.</returns>
    public double Evaluate(string text)
    {
        var tokens = Tokenize(text);
        tokens = ToPostfix(tokens);
        return Evaluate(tokens);
    }
}