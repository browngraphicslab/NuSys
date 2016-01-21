namespace NuSysApp
{
    public class GroupNodeDataGridInfo
    {
        private string _timeStamp;
        private string _creator;

        public GroupNodeDataGridInfo(string time, string name)
        {
            this._timeStamp = time;
            this._creator = name;
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
    }
}