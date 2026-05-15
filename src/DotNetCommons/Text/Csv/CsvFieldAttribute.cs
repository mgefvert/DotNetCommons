namespace DotNetCommons.Text.Csv;

public class CsvFieldAttribute : Attribute
{
    public string Name { get; }
    public string? Format { get; set; }
    
    public CsvFieldAttribute(string name)
    {
        Name = name;
    }
}
