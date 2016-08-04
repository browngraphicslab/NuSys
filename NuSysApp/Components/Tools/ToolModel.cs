using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public abstract class ToolModel
    {
        public enum ToolFilterTypeTitle
        {
            Title,
            Type,
            Creator,
            Date,
            LastEditedDate,
            AllMetadata,
            MetadataKeys
        }

        public enum ParentOperatorType
        {
            And,
            Or
        }

        public ParentOperatorType ParentOperator { get; private set; }

        /// <summary>
        /// The output library IDs represent the list of elements that should be included in its children tools.
        /// </summary>
        public HashSet<string> OutputLibraryIds { get; private set; }
        public HashSet<string> ParentIds { get; private set; }
        public bool Selected { get; private set; }
        public string Id { get; set; }
        public ToolModel()
        {
            Id = SessionController.Instance.GenerateId();
            ParentIds = new HashSet<string>();
            OutputLibraryIds = new HashSet<string>();
            ParentOperator = ParentOperatorType.Or;
        }

        public void SetParentOperator(ParentOperatorType parentOperator)
        {
            ParentOperator = parentOperator;
        }

        public void SetOutputLibraryIds(HashSet<string> libraryIds)
        {
            OutputLibraryIds = libraryIds;
            OutputLibraryIds = OutputLibraryIds ?? new HashSet<string>();
        }

        public bool AddOutputLibraryId(string libraryId)
        {
            OutputLibraryIds = OutputLibraryIds ?? new HashSet<string>();
            if (libraryId != null && OutputLibraryIds.Contains(libraryId))
            {
                return false;
            }
            OutputLibraryIds.Add(libraryId);
            return true;
        }
        public bool RemoveOutputLibraryId(string libraryId)
        {
            OutputLibraryIds = OutputLibraryIds ?? new HashSet<string>();
            if (libraryId != null && OutputLibraryIds.Contains(libraryId))
            {
                OutputLibraryIds.Remove(libraryId);
                return true;
            }
            return false;
        }
        public void SetParentIds(HashSet<string> parentIds)
        {
            ParentIds = parentIds;
            ParentIds = ParentIds ?? new HashSet<string>();
        }

        public bool AddParentId(string parentId)
        {
            ParentIds = ParentIds ?? new HashSet<string>();
            if (ParentIds.Contains(parentId))
            {
                return false;
            }
            ParentIds.Add(parentId);
            return true;
        }
        public bool RemoveParentId(string parentId)
        {
            ParentIds = ParentIds ?? new HashSet<string>();
            if (parentId != null && ParentIds.Contains(parentId))
            {
                ParentIds.Remove(parentId);
                return true;
            }
            return false;
        }

        public void SetSelected(bool selected)
        {
            Selected = selected;
        }
    }
}
