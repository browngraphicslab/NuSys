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
    public class GroupNodeDataGridViewModel : ElementCollectionViewModel
    {
        private ObservableCollection<GroupNodeDataGridInfo> _atomDataList;
        private ElementModel _nodeModel;
        private Dictionary<FrameworkElement, GroupNodeDataGridInfo> _infoDict;
        public GroupNodeDataGridViewModel(ElementCollectionController controller) : base(controller)
        {
            _atomDataList = new ObservableCollection<GroupNodeDataGridInfo>();
            _infoDict = new Dictionary<FrameworkElement, GroupNodeDataGridInfo>();
            AtomViewList.CollectionChanged += AtomViewListOnCollectionChanged;
        }

        private async void AtomViewListOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {

            if (e.NewItems != null)
            {
                foreach (var atom in e.NewItems)
                {
                    var atomTest = (FrameworkElement)atom;
                    var vm = (ElementViewModel)atomTest.DataContext; //access viewmodel
                    _nodeModel = (ElementModel)vm.Model; // access model

                    _nodeModel.SetMetaData("creator", "Dummy Data");

                    string id = _nodeModel.Id;
                    string timeStamp = _nodeModel.GetMetaData("node_creation_date")?.ToString();
                    string creator = _nodeModel.GetMetaData("creator")?.ToString();
                    string nodeType = _nodeModel.ElementType.ToString();
                    var title = _nodeModel.Title;
                    GroupNodeDataGridInfo atomData = new GroupNodeDataGridInfo(id, timeStamp, creator, nodeType, title);
                    _atomDataList.Add(atomData);
                    _infoDict[atomTest] = atomData;
                }
            }

            if (e.OldItems != null)
            {
                foreach (var atom in e.OldItems)
                {
                    var atomTest = (FrameworkElement)atom;
                    if (AtomDataList.Contains(_infoDict[atomTest]))
                    {
                        AtomDataList.Remove(_infoDict[atomTest]);
                    }

                }

            }
        }

        public ObservableCollection<GroupNodeDataGridInfo> AtomDataList 
        {
            get { return _atomDataList; }
            set { _atomDataList = value; }
        }



      
        
    }
}