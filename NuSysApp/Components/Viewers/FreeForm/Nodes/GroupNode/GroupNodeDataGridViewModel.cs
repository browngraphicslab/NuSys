using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

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

        private void ControllerOnDisposed(object source)
        {
            AtomDataList.Clear();
            _infoDict = null;
            AtomViewList.CollectionChanged -= AtomViewListOnCollectionChanged;
            Controller.Disposed -= ControllerOnDisposed;
        }

        private async void AtomViewListOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (var atom in e.NewItems)
                {
                    

                    var atomTest = (FrameworkElement)atom;
                    var vm = (ElementViewModel)atomTest.DataContext; //access viewmodel
                    if (vm is LinkViewModel)
                        continue;
                    
                    _nodeModel = (ElementModel)vm.Model; // access model
                    Controller.LibraryElementController.AddMetadata(new MetadataEntry("creator", _nodeModel.CreatorId, true));

                    var id = _nodeModel.Id;
                    var timeStamp = Controller.LibraryElementController.GetMetadata("node_creation_date");
                    var creator = Controller.LibraryElementController.GetMetadata("creator");
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