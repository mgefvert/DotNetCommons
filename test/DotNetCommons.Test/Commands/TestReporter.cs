namespace DotNetCommons.Test.Commands;

public class TestReporter : List<string>
{
    public string Text => string.Join(";", this);
}
