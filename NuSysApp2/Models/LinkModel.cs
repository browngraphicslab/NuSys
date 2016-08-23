using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace NuSysApp2
{
    public class LinkModel
    {
        public string LibraryId { get; private set; }
        public LinkModel(string id = null)
        {
            Id = id ?? SessionController.Instance.GenerateId();
        }
        public string Id { get; private set; }

        public string InAtomId { get; set; }

        public string OutAtomId { get; set; }
        //TODO: public RegionView

        public void SetLibraryId(string libraryElementId)
        {
            LibraryId = libraryElementId;
        }
    }
}