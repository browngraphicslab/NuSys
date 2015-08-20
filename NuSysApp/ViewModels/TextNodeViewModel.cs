using System;
using System.Collections.Generic;
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
        
        public TextNodeViewModel(WorkspaceViewModel workSpaceViewModel, string text, int id) : base(workSpaceViewModel, id)
        {
            this.Model = new TextNode(text ?? "Enter text here", id);
            this.View = new TextNodeView2(this);  
            this.Transform = new MatrixTransform();
            this.Width = Constants.DefaultNodeSize; //width set in /MISC/Constants.cs
            this.Height = Constants.DefaultNodeSize; //height set in /MISC/Constants.cs
            this.IsSelected = false;
            this.IsEditing = false;
            this.IsEditingInk = false;
            this.NodeType = Constants.NodeType.Text;
            this.Color = new SolidColorBrush(Windows.UI.Color.FromArgb(175, 255, 235, 205));
            this.View = new TextNodeView2(this);
        }

        #region Public Properties

        /// <summary>
        /// data contained by text node
        /// </summary>
        public string Data
        {
            get { return ((TextNode)this.Model).Text; }
            set
            {
                ((TextNode)this.Model).Text = value;
                RaisePropertyChanged("Data");
            }
        }

        #endregion Public Properties
    }
}