using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class ToolModel
    {
        public enum FilterTitle
        {
            Title,
            Type,
            Creator,
            Date,
            MetadataKeys,
            MetadataValues
        }
        public FilterTitle Filter { get; private set; }
        public List<string> LibraryIds { get; private set; }
        public string Selection { get; private set; }

        public void SetFilter(FilterTitle filter)
        {
            Filter = filter;
        }

        public void SetLibraryIds(List<string> libraryIds)
        {
            LibraryIds = libraryIds;
        }

        public bool AddLibraryId(string libraryId)
        {
            if (LibraryIds.Contains(libraryId))
            {
                return false;
            }
            LibraryIds.Add(libraryId);
            return true;
        }
        public bool RemoveLibraryId(string libraryId)
        {
            if (LibraryIds.Contains(libraryId))
            {
                LibraryIds.Remove(libraryId);
                return true;
            }
            return false;
        }

        public void SetSelection(string selection)
        {
            Selection = selection;
        }
    }
}
