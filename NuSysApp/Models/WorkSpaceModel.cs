using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class WorkSpaceModel
    {

        Node _selectedNode;
        Dictionary<int, Node> _nodeDict;
        private int _currentID;
        private Factory _factory;
        public WorkSpaceModel()
        {
            _nodeDict = new Dictionary<int, Node>();
            _currentID = 0;
           // _factory = new Factory(this);
        }

        public void createNewTextNode(string data)
        {
            //_nodeDict.Add(CurrentID, _factory.createNewTextNode(data));
            //CurrentID++;
        }
        public int CurrentID
        {
            get { return _currentID; }
            set { if(value >= _currentID)//decreasing the current ID doesn't make sense
                {
                    _currentID = value;
                }
            }
        }
    }
}
