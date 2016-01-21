using System;
using System.Collections.Generic;

namespace NuSysApp
{
    public class GroupNodeDataGridViewModel : GroupNodeViewModel
    {
        private List<GroupNodeDataGridInfo> _atomDataList;
        private AtomModel _nodeModel;

        public GroupNodeDataGridViewModel(NodeContainerModel model) : base(model)
        {   
            foreach (var atom in AtomViewList)
            {
                var vm = (AtomViewModel)atom.DataContext; //access viewmodel
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