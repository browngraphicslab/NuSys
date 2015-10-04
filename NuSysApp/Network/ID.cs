using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class ID
    {
        private string _creator;
        private string _timestamp;
        public ID(string id)
        {
            Value = id;
        }
        public string Value { get; }

        public string Creator
        {
            get
            {
                if (_creator == null)
                {
                    SetParts();
                }
                return _creator;
            }
        }

        public string TimeStamp
        {
            get
            {
                if (_timestamp == null)
                {
                    SetParts();
                }
                return _timestamp;
            }
        }

        private void SetParts()
        {
            var index = Value.IndexOf("#");
            _creator = Value.Substring(0, index);
            _timestamp = Value.Substring(index + 1);
        }
    }
}
