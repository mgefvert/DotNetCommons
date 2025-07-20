namespace DotNetCommons.Text.ShuntingYard;

public enum ShuntingYardToken
{
    Number,
    Operator,
    LeftParen,
    RightParen,
    Identifier,
    Function,
    Comma,
    Whitespace
}
