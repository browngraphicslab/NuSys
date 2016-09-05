﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp2
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
            MetadataKeys,
            MetadataValues
        }

        public enum ParentOperatorType
        {
            And,
            Or
        }

        public ParentOperatorType ParentOperator { get; private set; }

        public HashSet<string> LibraryIds { get; private set; }
        public HashSet<string> ParentIds { get; private set; }
        public bool Selected { get; private set; }
        public string Id { get; set; }
        public ToolModel()
        {
            Id = SessionController.Instance.GenerateId();
            ParentIds = new HashSet<string>();
            LibraryIds = new HashSet<string>();
            ParentOperator = ParentOperatorType.Or;
        }

        public void SetParentOperator(ParentOperatorType parentOperator)
        {
            ParentOperator = parentOperator;
        }

        public void SetLibraryIds(HashSet<string> libraryIds)
        {
            LibraryIds = libraryIds;
            LibraryIds = LibraryIds ?? new HashSet<string>();
        }

        public bool AddLibraryId(string libraryId)
        {
            LibraryIds = LibraryIds ?? new HashSet<string>();
            if (libraryId != null && LibraryIds.Contains(libraryId))
            {
                return false;
            }
            LibraryIds.Add(libraryId);
            return true;
        }
        public bool RemoveLibraryId(string libraryId)
        {
            LibraryIds = LibraryIds ?? new HashSet<string>();
            if (libraryId != null && LibraryIds.Contains(libraryId))
            {
                LibraryIds.Remove(libraryId);
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