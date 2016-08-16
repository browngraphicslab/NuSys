using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public static class SqlStringSettings
    {
        public static string CleanString(string command)
        {
            command.Replace("'", "''");
            command.Replace("\"", "&quot;");
            return command;

        }
    }
}
