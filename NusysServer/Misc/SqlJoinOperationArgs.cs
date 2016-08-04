using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NusysIntermediate;

namespace NusysServer.Misc
{
    public class SqlJoinOperationArgs
    {
        /// <summary>
        /// The left table to join
        /// </summary>
        public ITableRepresentable LeftTable { get; set; }

        /// <summary>
        /// The right table to join
        /// </summary>
        public ITableRepresentable RightTable { get; set; }

        /// <summary>
        /// The type of join
        /// </summary>
        public Constants.JoinedType JoinOperator { get; set; }

        /// <summary>
        /// The source table of colummn 1
        /// </summary>
        public Constants.SQLTableType Column1TableSource { get; set; }

        /// <summary>
        /// The source table of column 2
        /// </summary>
        public Constants.SQLTableType Column2TableSource { get; set; }

        /// <summary>
        /// The column which column 2 should be equal to.
        /// </summary>
        public NusysConstants.SqlColumns Column1 { get; set; }

        /// <summary>
        /// The column which column 1 should be equal to.
        /// </summary>
        public NusysConstants.SqlColumns Column2 { get; set; }
    }
}