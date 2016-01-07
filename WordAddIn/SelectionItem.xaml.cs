
using System.Windows.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Input;
using Microsoft.Office.Interop.Word;

namespace WordAddIn
{
    /// <summary>
    /// Interaction logic for SelectionItem.xaml
    /// </summary>
    public partial class SelectionItem : UserControl
    {
        private string _content;
		private Boolean _isExported;
        private Comment _comment;
        //private Document _slide;
        private int _slideNumber;
        private ScaleTransform _renderTransform;
        private ImageSource _thumbnail;
        private Range _range;
		
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
            selectionItem.Range.Select();

            // Globals.ThisAddIn.Application.ActiveWindow.View.GotoSlide(selectionItem.SlideNumber);
        }

        public string Content
        {
            get { return _content; }
            set {
                _content = value;
                SetRtf(rtb, value);
                img.Visibility = Visibility.Collapsed;
                rtb.Visibility = Visibility.Visible;

                if (_content.IndexOf("goalw") > -1)
                {
                    Debug.WriteLine(_content.Substring(_content.IndexOf("goalw"),10));
                }
            }
        }

		public Boolean IsExported
		{
			get { return _isExported; }
			set { _isExported = value; }
		}
		
        public ImageSource Thumbnail
        {
            get { return _thumbnail; }
            set { _thumbnail = value;
                img.Visibility = Visibility.Visible;
                rtb.Visibility = Visibility.Collapsed;
            }
        }

        public Range Range
        {
            get { return _range; }
            set { _range = value; }
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

        /*public Document Document
        {
            get { return _slide; }
            set { _slide = value; }
        }*/
        public int SlideNumber
        {
            get { return _slideNumber; }
            set { _slideNumber = value; }
        }


        public void SetRtf(RichTextBox rtb, string document)
        {
            var documentBytes = Encoding.UTF8.GetBytes(document);
            using (var reader = new MemoryStream(documentBytes))
            {
                reader.Position = 0;
                rtb.SelectAll();
                rtb.Selection.Load(reader, DataFormats.Rtf);
            }
        }
    }
}

