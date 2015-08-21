using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml;
using Windows.UI;
using Windows.UI.Xaml.Media;

namespace NuSysApp
{
    public class TextNodeViewModel : NodeViewModel
    {
        #region Private Members

        #endregion Private Members
        public TextNodeViewModel(TextNode model, WorkspaceViewModel workSpaceViewModel, string text, string id) : base(model, workSpaceViewModel, id)
        {
            this.Model.PropertyChanged += (s, e) => { Update(e); };
            this.View = new TextNodeView2(this);  
            this.Transform = new MatrixTransform();
            this.Width = Constants.DefaultNodeSize; //width set in /MISC/Constants.cs
            this.Height = Constants.DefaultNodeSize; //height set in /MISC/Constants.cs
            this.IsSelected = false;
            this.IsEditing = false;
            this.IsEditingInk = false;
            this.NodeType = NodeType.Text;
            this.Color = new SolidColorBrush(Windows.UI.Color.FromArgb(175, 255, 235, 205));
            this.View = new TextNodeView2(this);//TODO < whut is this? <IDK duuuude
            this.WorkSpaceViewModel.Model.OnDeletion += DeletionHappend;            
        }

        public void DeletionHappend(object source, DeleteEventArgs e)
        {
            if((Node)source == (Node)this.Model)
            {
                this.WorkSpaceViewModel.DeleteNode(this);
            };
        }

        private void Update(PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Model_Width":
                    this.Width = ((Node)this.Model).Width;
                    break;
                case "Model_Height":
                    this.Height = ((Node)this.Model).Height;
                    break;
                case "Model_X":
                    this.SetPosition(((Node)this.Model).X, ((Node)this.Model).Y);
                    //this.WorkSpaceViewModel.PositionNode(this, ((Node)this.Model).X, this.Y);
                    break;
                case "Model_Y":
                    this.SetPosition(((Node)this.Model).X, ((Node)this.Model).Y);
                    //this.WorkSpaceViewModel.PositionNode(this, this.X, ((Node)this.Model).Y);
                    break;
                case "Model_Text":
                    this.Data = ((TextNode) this.Model).Text;
                    break;
                case "Model_CanEdit":
                    this.CanEdit = ((TextNode) this.Model).CanEdit;
                    break;
            }
        }
        
        #region Public Properties

        private string _data;
        /// <summary>
        /// data contained by text node
        /// </summary>
        public string Data
        {
            get { return _data; }
            set
            {
                _data = value;
                ((TextNode) this.Model).Text = value;
                RaisePropertyChanged("Data");
            }
        }

        #endregion Public Properties
    }
}