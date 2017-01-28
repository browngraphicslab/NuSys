using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NusysIntermediate
{
    /// <summary>
    /// Serializable object representing where a certain library element came from
    /// </summary>
    public class LibraryElementOrigin
    {
        public enum OriginType
        {
            LibraryImport, 
            Copy,
            Region
        }

        /// <summary>
        /// This will be the origin LibraryElementID if there exists one.
        /// More specifically, this will be the Id of the thing this was region'ed from, 
        /// The original library element it was copied from, or null if it was simply imported.
        /// </summary>
        public string OriginId { get; set; }

        /// <summary>
        /// the type of origin this represents
        /// </summary>
        public OriginType Type { get; set; }
    }
}
