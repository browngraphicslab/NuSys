using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft;
using NusysIntermediate;

namespace NusysServer
{
    public class SqlSelectQueryArgs
    {
        public Constants.SQLTableType TableType { set; get; }
        public Message SelectProperties { get; set; }
        public Constants.Operator GroupOperator { get; set; }
        public IEnumerable<string> ColumnsToGet { get; set; }
    }
}