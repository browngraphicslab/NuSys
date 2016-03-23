namespace NuSysApp
{
    public class GroupNodeDataGridInfo
    {
        private string _timeStamp;
        private string _creator;
        private string _nodetype;
        
        public GroupNodeDataGridInfo(string id,string time, string name, string nodetype)
        {
            Id = id;
            this._timeStamp = time;
            this._creator = name;
            this._nodetype = nodetype;
        }

        public string TimeStamp
        {
            get { return _timeStamp; }
            set { _timeStamp = value; }
        }

        public string Creator
        {
            get { return _creator; }
            set { _creator = value; }
        }

        public string NodeType
        {
            get { return _nodetype; }
            set { _nodetype = value; }
        }

        public string Id { get; set; }
    }
}