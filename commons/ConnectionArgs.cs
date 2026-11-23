using System.ComponentModel.DataAnnotations;
using DotNetCommons.Sys;

namespace commons;

public class ConnectionArgs
{
    [Required, CommandLineOption('c', "connection", "mysql.cnf login-path argument")]
    public string? Connection { get; set; }
}
