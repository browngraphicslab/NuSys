using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NusysIntermediate;

namespace NusysServer
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
        /// The full column title which column 2 should be equal to.
        /// </summary>
        public string Column1 { get; set; }

        /// <summary>
        /// The full column title  which column 1 should be equal to.
        /// </summary>
        public string Column2 { get; set; }
    }
}