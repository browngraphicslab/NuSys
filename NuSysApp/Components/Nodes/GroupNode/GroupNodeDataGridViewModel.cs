using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace NuSysApp
{
    public class GroupNodeDataGridViewModel : NodeContainerViewModel
    {
        private List<GroupNodeDataGridInfo> _atomDataList;
        private AtomModel _nodeModel;

        public GroupNodeDataGridViewModel(NodeContainerModel model) : base(model)
        {
            AtomViewList.CollectionChanged += AtomViewListOnCollectionChanged;
            _atomDataList = new List<GroupNodeDataGridInfo>();
        }

        private async void AtomViewListOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems == null)
                return;
            
            foreach (var atom in e.NewItems)
            {
                Debug.WriteLine("item added!");
                var atomTest = (FrameworkElement) atom;
                var vm = (AtomViewModel)atomTest.DataContext; //access viewmodel
                _nodeModel = (AtomModel)vm.Model; // access model

                _nodeModel.SetMetaData("timestamp", "10/29/1992");
                _nodeModel.SetMetaData("creator", "Junsu Choi");

                string timeStamp = _nodeModel.GetMetaData("timestamp").ToString();
                string creator = _nodeModel.GetMetaData("creator").ToString();
                GroupNodeDataGridInfo atomData = new GroupNodeDataGridInfo(timeStamp, creator);
                _atomDataList.Add(atomData);
            }
        }

        public List<GroupNodeDataGridInfo> AtomDataList 
        {
            get { return _atomDataList; }
            set { _atomDataList = value; }
        }
    }
}