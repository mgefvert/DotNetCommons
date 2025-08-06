namespace DotNetCommonTests.Commands;

public class TestReporter : List<string>
{
    public string Text => string.Join(";", this);
}
