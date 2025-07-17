using DotNetCommons.Text.Tokenizer;
using System.Globalization;

namespace DotNetCommons.Text.ShuntingYard;

public class ShuntingYardEvaluator
{
    private readonly StringTokenizer<ShuntingYardToken> _tokenizer = new(Definitions);
    private static readonly object DefaultLock = new();
    private static ShuntingYardEvaluator? _default;

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

    private static readonly Dictionary<string, double> Constants = new()
    {
        ["pi"] = Math.PI,
        ["e"] = Math.E
    };

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

        // Process identifiers (constants)
        foreach (var token in result.Where(x => x.ID == ShuntingYardToken.Identifier))
        {
            var constantName = token.Text?.ToLowerInvariant();
            if (Constants.TryGetValue(constantName!, out var value))
            {
                token.ID = ShuntingYardToken.Number;
                token.Tag = value;
            }
            else
                throw new InvalidOperationException($"Unknown constant '{token.Text}' at {token.Line}:{token.Column}");
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

    public static double Evaluate(string text)
    {
        var evaluator = Default;
        var tokens = evaluator.Tokenize(text);
        tokens = evaluator.ToPostfix(tokens);
        return evaluator.Evaluate(tokens);
    }
}