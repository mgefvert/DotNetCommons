using System;
using System.Collections.Generic;

namespace DotNetCommons
{
    internal interface ILogFileNaming
    {
        IEnumerable<string> GetAllowedFiles(string name, string extension, int rotations, DateTime? date = null);
        string GetCurrentFileName(string name, string extension, DateTime? date = null);
        string GetFileSpec(string name, string extension);
    }
}
