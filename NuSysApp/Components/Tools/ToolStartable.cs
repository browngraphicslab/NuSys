using System;
using System.Collections.Generic;
using Windows.UI.Xaml;

namespace NuSysApp.Tools
{
    public interface ToolStartable
    {
        /// <summary>
        /// Returns the list of output library ids
        /// If recursively refresh is true, then we will reload the output library ids starting from the parent.
        /// If it is false, it just returns the output library ids it had before.
        /// </summary>
        HashSet<string> GetOutputLibraryIds();

        /// <summary>
        /// the list of all the library ids from all of the startable's parents, if any.
        /// </summary>
        /// <returns></returns>
        IEnumerable<string> GetUpdatedDataList();

        event EventHandler<HashSet<string>> OutputLibraryIdsChanged;
        event EventHandler<string> Disposed;

        /// <summary>
        /// This is listend to by all children so that when you change from metadata to basic or basic to metadata,
        /// The children can create a new link to the new tool and re add the new tool as a parent (since the old tool was disposed of). (The reason why the link can't listen to this is the link takes in an element controller.) It
        /// takes an element controller beause it already has all the anchor functionality.
        /// </summary>
        event EventHandler<ToolViewModel> FilterTypeAllMetadataChanged;


        /// <summary>
        /// Returns the toolStartable id
        /// </summary>
        string GetID();

        /// <summary>
        /// Returns the ids of all its parents
        /// </summary>
        HashSet<string> GetParentIds();

        /// <summary>
        /// This should refresh the entire tool chain no matter which tool controller calls it
        /// </summary>
        void RefreshFromTopOfChain();
    }
}