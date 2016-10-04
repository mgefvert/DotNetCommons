﻿using System;
using System.Collections.Generic;

namespace CommonNetTools
{
    internal class LogFileDaily : ILogFileNaming
    {
        public IEnumerable<string> GetAllowedFiles(string name, string extension, int rotations, DateTime? date = null)
        {
            var dt = date ?? DateTime.Today;

            for (var i = 0; i <= rotations; i++)
            {
                var filename = GetCurrentFileName(name, extension, dt);
                yield return filename;
                yield return filename + ".gz";

                dt = dt.AddDays(-1);
            }
        }

        public string GetCurrentFileName(string name, string extension, DateTime? date)
        {
            return name + "-" + (date ?? DateTime.Today).ToString("yyyy-MM-dd") + extension;
        }

        public string GetFileSpec(string name, string extension)
        {
            return name + "-????-??-??" + extension;
        }
    }
}