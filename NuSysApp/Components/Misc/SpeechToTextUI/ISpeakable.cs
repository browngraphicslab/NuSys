using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public interface ISpeakable
    {
        /// <summary>
        /// Sets the text in the current element to the text paramater in SetData
        /// </summary>
        /// <param name="text"></param>
        void SetData(string text);
    }
}
