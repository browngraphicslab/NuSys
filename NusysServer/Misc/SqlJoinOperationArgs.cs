using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NusysIntermediate;

namespace NusysServer.Misc
{
    public class SqlJoinOperationArgs
    {
        public ITableRepresentable LeftTable { get; set; }
        public ITableRepresentable RightTable { get; set; }
        public Constants.JoinedType JoinOperator { get; set; }
        public Constants.SQLTableType Column1TableSource { get; set; }
        public Constants.SQLTableType Column2TableSource { get; set; }
        public NusysConstants.SqlColumns Column1 { get; set; }
        public NusysConstants.SqlColumns Column2 { get; set; }
    }
}