﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using NusysIntermediate;

namespace NuSysApp
{
    public class GroupNodeDataGridViewModel : ElementCollectionViewModel
    {

        private ElementModel _nodeModel;
        private Dictionary<FrameworkElement, GroupNodeDataGridInfo> _infoDict;
        public GroupNodeDataGridViewModel(ElementCollectionController controller) : base(controller)
        {
            AtomDataList = new ObservableCollection<GroupNodeDataGridInfo>();
            _infoDict = new Dictionary<FrameworkElement, GroupNodeDataGridInfo>();
            AtomViewList.CollectionChanged += AtomViewListOnCollectionChanged;
            controller.Disposed += ControllerOnDisposed;
        }

        private void ControllerOnDisposed(object source, object args)
        {
            // We need to dispose of the title changed handler for all of the list items
            if (AtomDataList != null)
            {
                foreach (var item in AtomDataList)
                {
                    item?.Dispose();
                }
            }
            AtomDataList?.Clear();
            _infoDict = null;
            if (AtomViewList != null)
            {
                AtomViewList.CollectionChanged -= AtomViewListOnCollectionChanged;
            }
            if (Controller != null)
            {
                Controller.Disposed -= ControllerOnDisposed;
            }
        }

        private async void AtomViewListOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (var atom in e.NewItems)
                {
                    

                    var atomTest = (FrameworkElement)atom;
                    if (atomTest == null)
                    {
                        return;
                    }
                    var vm = (ElementViewModel)atomTest.DataContext; //access viewmodel
                    if (vm is LinkViewModel)
                        continue;
                    
                    _nodeModel = vm.Model; // access model
                    //LibraryElementController.LibraryElementController.AddMetadata(new MetadataEntry("creator", new List<string> {_nodeModel.CreatorId}, MetadataMutability.MUTABLE));

                    var id = _nodeModel.Id;
                    var timeStamp = Controller.LibraryElementController.GetMetadata("node_creation_date")?.First();
                    var creator =
                        SessionController.Instance.NuSysNetworkSession.GetDisplayNameFromUserId(vm.Model.CreatorId) ??
                        "Unknown Author";
                    var nodeType = _nodeModel.ElementType.ToString();
                    var title = _nodeModel.Title;
                    
                    var atomData = new GroupNodeDataGridInfo(id, timeStamp, creator, nodeType, title);
                    
                    AtomDataList.Add(atomData);
                    _infoDict[atomTest] = atomData;
                }
            }

            if (e.OldItems != null)
            {
                foreach (var atom in e.OldItems)
                {
                    var atomTest = (FrameworkElement)atom;
                    if (!_infoDict.ContainsKey(atomTest))
                        continue;
                    
                    if (AtomDataList.Contains(_infoDict[atomTest]))
                    {
                        AtomDataList.Remove(_infoDict[atomTest]);
                    }
                }

            }
        }

        

        private void OnTitleChanged(object sender, string newTitle)
        {
            var s = sender;
            //AtomDataList.Where()
        }

        public ObservableCollection<GroupNodeDataGridInfo> AtomDataList { get; set; }
        
    }
}