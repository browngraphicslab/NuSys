using System.Collections.Generic;

namespace NuSysApp
{
    public class WorkSpaceModel
    {

        //Node _selectedNode;
        Dictionary<int, Node> _nodeDict;
        private int _currentId;
        //private Factory _factory;
        public WorkSpaceModel()
        {
            _nodeDict = new Dictionary<int, Node>();
            _currentId = 0;
           // _factory = new Factory(this);
        }

        public void CreateNewTextNode(string data)
        {
            //_nodeDict.Add(CurrentID, _factory.createNewTextNode(data));
            //CurrentID++;
        }
        public int CurrentId
        {
            get { return _currentId; }
            set { if(value >= _currentId)//decreasing the current ID doesn't make sense
                {
                    _currentId = value;
                }
            }
        }
    }
}
