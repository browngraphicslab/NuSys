using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace NuSysApp
{
    public class GroupNodeDataGridViewModel : NodeContainerViewModel
    {
        private ObservableCollection<GroupNodeDataGridInfo> _atomDataList;
        private AtomModel _nodeModel;

        public GroupNodeDataGridViewModel(NodeContainerModel model) : base(model)
        {
            _atomDataList = new ObservableCollection<GroupNodeDataGridInfo>();
            AtomViewList.CollectionChanged += AtomViewListOnCollectionChanged;
        }

        private async void AtomViewListOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems == null)
                return;
            
            foreach (var atom in e.NewItems)
            {
                var atomTest = (FrameworkElement) atom;
                var vm = (AtomViewModel)atomTest.DataContext; //access viewmodel
                _nodeModel = (AtomModel)vm.Model; // access model

                _nodeModel.SetMetaData("creator", "Dummy Data");

                string timeStamp = _nodeModel.GetMetaData("node_creation_date").ToString();
                string creator = _nodeModel.GetMetaData("creator").ToString();
                string nodeType = _nodeModel.GetMetaData("node_type").ToString();
                GroupNodeDataGridInfo atomData = new GroupNodeDataGridInfo(timeStamp, creator, nodeType);
                _atomDataList.Add(atomData);
            }
        }

        public ObservableCollection<GroupNodeDataGridInfo> AtomDataList 
        {
            get { return _atomDataList; }
            set { _atomDataList = value; }
        }
    }
}