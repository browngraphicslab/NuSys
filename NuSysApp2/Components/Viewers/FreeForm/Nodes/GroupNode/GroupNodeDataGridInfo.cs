﻿using System;

namespace NuSysApp
{
    public class GroupNodeDataGridInfo
    {
        private string _timeStamp;
        private string _creator;
        private string _nodetype;
        private string _title;
        
        public GroupNodeDataGridInfo(string id,string time, string name, string nodetype, string title)
        {
            Id = id;

            this._timeStamp = time;
            this._creator = name;
            this._nodetype = nodetype;
            this._title = title;
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

        public string Title
        {
            get { return _title; }
            set { _title = value; }
        }

        public string Id { get; set; }
    }
}