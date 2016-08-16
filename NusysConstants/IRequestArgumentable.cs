using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NusysIntermediate;

namespace NusysIntermediate
{
    public interface IRequestArgumentable
    {
        /// <summary>
        /// This should return a message with all the data in it using the request keys in the nusysconstants class.
        /// </summary>
        /// <returns></returns>
        Message PackToRequestKeys();
    }
}
