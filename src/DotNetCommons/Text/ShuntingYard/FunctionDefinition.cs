namespace DotNetCommons.Text.ShuntingYard;

public delegate double Function(params double[] args);
public record FunctionDefinition(string Name, int Arity, Function FunctionCallback);
