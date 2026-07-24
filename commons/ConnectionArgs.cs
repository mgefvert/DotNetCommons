using DotNetCommons.Sys;

namespace commons;

public class ConnectionArgs
{
    [CommandLineOption('c', "connection", "mysql.cnf login-path argument")]
    public string? Connection { get; set; }
}
