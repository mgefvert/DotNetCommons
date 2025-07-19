namespace DotNetCommons.Text.ShuntingYard;

public enum ShuntingYardToken
{
    Number,
    Operator,
    LeftParen,
    RightParen,
    Identifier,
    Function0,
    Function1,
    Comma,
    Whitespace
}
