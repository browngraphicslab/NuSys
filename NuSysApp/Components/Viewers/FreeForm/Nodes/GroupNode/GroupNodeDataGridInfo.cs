﻿using System;

namespace NuSysApp
{
    public class GroupNodeDataGridInfo : BaseINPC
    {
        private string _timeStamp;
        private string _creator;
        private string _nodetype;
        private string _title;
        
        public GroupNodeDataGridInfo(string id,string time, string name, string nodetype, string title)
        {
            Id = id;

            _timeStamp = time;
            _creator = name; 
            _nodetype = nodetype;
            _title = title;

            // The list item needs to update live as the title of the item is changed elsewhere.
            var itemController = SessionController.Instance.ElementModelIdToElementController[id].LibraryElementController;
            itemController.TitleChanged += ItemControllerOnTitleChanged;
        }

        private void ItemControllerOnTitleChanged(object sender, string title)
        {
            Title = title;
            RaisePropertyChanged("Title");
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

        public void Dispose()
        {
            _timeStamp = null;
            _creator = null;
            _nodetype = null;
            _title = null;

            // Remove the title changed event handler
            var itemController = SessionController.Instance.ElementModelIdToElementController[Id].LibraryElementController;
            itemController.TitleChanged -= ItemControllerOnTitleChanged;

        }
    }
}