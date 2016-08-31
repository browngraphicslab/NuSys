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

        /// <summary>
        /// the list of all the library ids from all of the startable's parents, if any.
        /// </summary>
        /// <returns></returns>
        IEnumerable<string> GetUpdatedDataList(bool recursiveRefresh = false);

        event EventHandler<HashSet<string>> OutputLibraryIdsChanged;
        event EventHandler<string> Disposed;
        event EventHandler<ToolViewModel> FilterTypeAllMetadataChanged;


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