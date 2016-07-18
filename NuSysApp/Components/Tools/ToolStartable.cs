using System;
using System.Collections.Generic;
using Windows.UI.Xaml;

namespace NuSysApp.Tools
{
    public interface ToolStartable
    {
        /// <summary>
        /// Returns the list of output library ids
        /// </summary>
        HashSet<string> GetOutputLibraryIds();

        event EventHandler<HashSet<string>> OutputLibraryIdsChanged;
        event EventHandler<string> Disposed;

        /// <summary>
        /// Returns the toolStartable id
        /// </summary>
        string GetID();

        /// <summary>
        /// Returns the ids of all its parents
        /// </summary>
        HashSet<string> GetParentIds();



    }
}