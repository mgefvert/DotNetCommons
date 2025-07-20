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
        ["||"] = (0, false),
        ["&&"] = (1, false),
        ["=="] = (2, false),
        ["!="] = (2, false),
        ["<>"] = (2, false),
        ["<"]  = (3, false),
        ["<="] = (3, false),
        [">"]  = (3, false),
        [">="] = (3, false),
        ["+"]  = (4, false),
        ["-"]  = (4, false),
        ["*"]  = (5, false),
        ["/"]  = (5, false),
        ["^"]  = (6, true),
        ["!"]  = (7, true)
    };

    private static readonly Dictionary<string, double> DefaultConstants = new()
    {
        ["pi"] = Math.PI,
        ["e"]  = Math.E
    };

    private static readonly FunctionDefinition[] DefaultFunctions =
    [
        // Roots and exponents, logarithms
        new("sqrt", 1, args => Math.Sqrt(args[0])),
        new("exp", 1, args => Math.Exp(args[0])),
        new("log", 1, args => Math.Log10(args[0])),
        new("ln", 1, args => Math.Log(args[0])),

        // Rounding and utility
        new("abs", 1, args => Math.Abs(args[0])),
        new("ceil", 1, args => Math.Ceiling(args[0])),
        new("floor", 1, args => Math.Floor(args[0])),
        new("round", 1, args => Math.Round(args[0])),
        new("trunc", 1, args => Math.Truncate(args[0])),

        // Trigonometric functions
        new("sin", 1, args => Math.Sin(args[0])),
        new("asin", 1, args => Math.Asin(args[0])),
        new("sinh", 1, args => Math.Sinh(args[0])),
        new("asinh", 1, args => Math.Asinh(args[0])),
        new("cos", 1, args => Math.Cos(args[0])),
        new("acos", 1, args => Math.Acos(args[0])),
        new("cosh", 1, args => Math.Cosh(args[0])),
        new("acosh", 1, args => Math.Acosh(args[0])),
        new("tan", 1, args => Math.Tan(args[0])),
        new("tanh", 1, args => Math.Tanh(args[0])),
        new("atan", 1, args => Math.Atan(args[0])),
        new("atanh", 1, args => Math.Atanh(args[0])),

        // Min/Max functions
        new("max", 2, args => Math.Max(args[0], args[1])),
        new("min", 2, args => Math.Min(args[0], args[1])),

        // Random number generation
        new("rnd", 0, _ => new Random().NextDouble()),
    ];

    private readonly Dictionary<string, double> _constants;
    private readonly Dictionary<string, FunctionDefinition> _functions;

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
        new Strings<ShuntingYardToken>(ShuntingYardToken.Operator, "&&", false),
        new Strings<ShuntingYardToken>(ShuntingYardToken.Operator, "||", false),
        new Strings<ShuntingYardToken>(ShuntingYardToken.Operator, "<", false),
        new Strings<ShuntingYardToken>(ShuntingYardToken.Operator, "<=", false),
        new Strings<ShuntingYardToken>(ShuntingYardToken.Operator, ">", false),
        new Strings<ShuntingYardToken>(ShuntingYardToken.Operator, ">=", false),
        new Strings<ShuntingYardToken>(ShuntingYardToken.Operator, "==", false),
        new Strings<ShuntingYardToken>(ShuntingYardToken.Operator, "!=", false),
        new Strings<ShuntingYardToken>(ShuntingYardToken.Operator, "<>", false),
        new Strings<ShuntingYardToken>(ShuntingYardToken.Operator, "!", false),
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
        _functions = DefaultFunctions.ToDictionary(x => x.Name);
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
    /// <param name="function">The function definition to add.</param>
    public void AddFunction(FunctionDefinition function)
    {
        if (this == Default)
            throw new InvalidOperationException("Cannot add functions to the default instance of the evaluator");

        _functions[function.Name] = function;
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
                token.ID  = ShuntingYardToken.Number;
                token.Tag = value;
            }
            // Check if it's a function
            else if (_functions.TryGetValue(name!, out var function))
            {
                token.ID  = ShuntingYardToken.Function;
                token.Tag = function;
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
                token.ID  = ShuntingYardToken.Number;
                token.Tag = value;
            }
            else
                throw new InvalidOperationException($"Unknown identifier '{token.Text}' at {token.Line}:{token.Column}");
        }

        // Rewrite the token stream to handle special cases
        for (var i = 0; i < result.Count;)
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

            // Verify that any function must always be followed by a LeftParen
            if (result.IsToken(i, ShuntingYardToken.Function) && !result.IsToken(i + 1, ShuntingYardToken.LeftParen))
            {
                var name = ((FunctionDefinition)result[i].Tag).Name;
                throw new InvalidOperationException($"Function '{name}' must be followed by '()' at {result[i].Line}:{result[i].Column}");
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
        var output        = new TokenList<ShuntingYardToken>();
        var stack         = new Stack<Token<ShuntingYardToken>>();
        var argumentCount = new Stack<int>(); // Track number of arguments for functions

        foreach (var token in tokens)
        {
            switch (token.ID)
            {
                case ShuntingYardToken.Number:
                    output.Add(token);
                    break;

                case ShuntingYardToken.Function:
                    stack.Push(token);
                    argumentCount.Push(0); // Initialize argument counter for this function
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

                case ShuntingYardToken.Comma:
                    while (stack.Count > 0 && stack.Peek().ID != ShuntingYardToken.LeftParen)
                        output.Add(stack.Pop());

                    if (stack.Count == 0)
                        throw new InvalidOperationException("Mismatched parentheses");

                    if (argumentCount.Count > 0)
                        argumentCount.Push(argumentCount.Pop() + 1);
                    break;

                case ShuntingYardToken.RightParen:
                    while (stack.Count > 0 && stack.Peek().ID != ShuntingYardToken.LeftParen)
                        output.Add(stack.Pop());

                    if (stack.Count == 0 || stack.Peek().ID != ShuntingYardToken.LeftParen)
                        throw new InvalidOperationException("Mismatched parentheses");

                    stack.Pop(); // discard the left paren

                    // If the token at the top of the stack is a function token, pop it onto the output queue
                    if (stack.Count > 0 && stack.Peek().ID == ShuntingYardToken.Function)
                    {
                        var func    = stack.Pop();
                        var funcDef = (FunctionDefinition)func.Tag;
                        var args    = funcDef.Arity == 0 ? 0 : argumentCount.Pop() + 1; // Only add 1 for non-zero arity functions

                        if (args != funcDef.Arity)
                            throw new InvalidOperationException(
                                $"Function {funcDef.Name} expects {funcDef.Arity} arguments but got {args}");

                        output.Add(func);
                    }

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

                case ShuntingYardToken.Function:
                    var funcDef = (FunctionDefinition)token.Tag;
                    if (stack.Count < funcDef.Arity)
                        throw new InvalidOperationException($"Not enough operands for function at {token.Line}:{token.Column}");

                    var args = new double[funcDef.Arity];
                    for (int i = funcDef.Arity - 1; i >= 0; i--)
                        args[i] = stack.Pop();

                    stack.Push(funcDef.FunctionCallback(args));
                    break;

                case ShuntingYardToken.Operator:
                    if (token.Text == "!")
                    {
                        if (stack.Count < 1)
                            throw new InvalidOperationException($"Not enough operands at {token.Line}:{token.Column}");
                        var a = stack.Pop();
                        stack.Push(IsTruthy(a) ? 0.0 : 1.0);
                    }
                    else
                    {
                        if (stack.Count < 2)
                            throw new InvalidOperationException($"Not enough operands at {token.Line}:{token.Column}");

                        var b = stack.Pop();
                        var a = stack.Pop();

                        stack.Push(token.Text switch
                        {
                            "+"  => a + b,
                            "-"  => a - b,
                            "*"  => a * b,
                            "/"  => a / b,
                            "^"  => Math.Pow(a, b),
                            "&&" => IsTruthy(a) && IsTruthy(b) ? 1.0 : 0.0,
                            "||" => IsTruthy(a) || IsTruthy(b) ? 1.0 : 0.0,
                            "<"  => a < b ? 1.0 : 0.0,
                            "<=" => a <= b ? 1.0 : 0.0,
                            ">"  => a > b ? 1.0 : 0.0,
                            ">=" => a >= b ? 1.0 : 0.0,
                            "==" => Math.Abs(a - b) < 1e-10 ? 1.0 : 0.0,
                            "!=" => Math.Abs(a - b) >= 1e-10 ? 1.0 : 0.0,
                            "<>" => Math.Abs(a - b) >= 1e-10 ? 1.0 : 0.0,
                            _    => throw new InvalidOperationException($"Unknown operator '{token.Text}' at {token.Line}:{token.Column}")
                        });
                    }
                    break;

                default:
                    throw new InvalidOperationException($"Unexpected token '{token.ID}' at {token.Line}:{token.Column}");
            }
        }

        if (stack.Count != 1)
            throw new InvalidOperationException("Invalid expression");

        return stack.Pop();
    }

    private static bool IsTruthy(double value) => Math.Abs(value) > 1e-10;

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