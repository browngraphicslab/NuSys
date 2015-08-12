
using System.Windows.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Office.Interop.PowerPoint;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Input;

namespace PowerPointAddIn
{
    /// <summary>
    /// Interaction logic for SelectionItem.xaml
    /// </summary>
    public partial class SelectionItem : UserControl
    {
        private string _content;
        private Comment _comment;
        private Slide _slide;
        private int _slideNumber;
        private ScaleTransform _renderTransform;
        private ImageSource _thumbnail;

        public SelectionItem()
        {
            InitializeComponent();
            _renderTransform = new ScaleTransform(1, 1);
            DataContext = this;
        }

        private void StackPanel_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!(sender is SelectionItem))
                return;

            var selectionItem = (SelectionItem)sender;
            Globals.ThisAddIn.Application.ActiveWindow.View.GotoSlide(selectionItem.SlideNumber);
        }

        public string Content
        {
            get { return _content; }
            set { _content = value; }
        }

        public ImageSource Thumbnail
        {
            get { return _thumbnail; }
            set { _thumbnail = value;            }
        }

        public ScaleTransform RenderTransform
        {
            get { return _renderTransform; }
            set { _renderTransform = value; }
        }


        public Comment Comment
        {
            get { return _comment; }
            set { _comment = value; }
        }

        public Slide Slide
        {
            get { return _slide; }
            set { _slide = value; }
        }
        public int SlideNumber
        {
            get { return _slideNumber; }
            set { _slideNumber = value; }
        }
    }
}

