using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    /// <summary>
    /// Wrapper class used to store the tab info
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ITabType<T> where T : IEqualityComparer<T>
    {
        /// <summary>
        /// The title to display on the tab
        /// </summary>
        string Title { get; set; }

        /// <summary>
        /// The data that the tab contains, generic so that tabs can be created for anything
        /// but must be comparable, and only one tab can exist for any two items which are considered
        /// equal in the comparison.
        /// </summary>
        T Data { get; set; }

    }
}
