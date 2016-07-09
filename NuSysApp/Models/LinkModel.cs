using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NuSysApp.Components.Nodes;
using NuSysApp.Nodes.AudioNode;
using NuSysApp.Viewers;
using Windows.UI.Xaml.Controls;

namespace NuSysApp
{
    public class LinkModel
    {
        public string LibraryId { get; private set; }
        public LinkModel(string id = null)
        {
            Id = id ?? SessionController.Instance.GenerateId();
        }
        public string Id { get; private set; }
        public bool IsPresentationLink { get; set; }

        public string InAtomId { get; set; }

        public string OutAtomId { get; set; }

        //public LinkedTimeBlockModel InFineGrain { get; set; }

        //TODO: public RegionView
        
        public RectangleViewModel RectangleModel { get; set; }

        public void SetLibraryId(string libraryElementId)
        {
            LibraryId = libraryElementId;
        }
    }
}