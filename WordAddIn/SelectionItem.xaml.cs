
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
using System.Collections.ObjectModel;
using System.Windows.Documents;

namespace WordAddIn
{
    /// <summary>
    /// Interaction logic for SelectionItem.xaml
    /// </summary>
    public partial class SelectionItem : UserControl
    {
		private Boolean _isExported;
        private Comment _comment;
        private ScaleTransform _renderTransform;
        private Range _range;
        private string _text;
        private string _rtfContent;
		
        public SelectionItem()
        {
            InitializeComponent();
            _renderTransform = new ScaleTransform(1, 1);
            parseRtf();			
            DataContext = this;
        }

        private void StackPanel_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!(sender is SelectionItem))
                return;

            var selectionItem = (SelectionItem)sender;
            selectionItem.Range.Select();
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            ObservableCollection<SelectionItem> CheckedSelections = Globals.ThisAddIn.SidePane.CheckedSelections;
            if (!CheckedSelections.Contains(this))
            {
                CheckedSelections.Add(this);
            } 
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            ObservableCollection<SelectionItem> CheckedSelections = Globals.ThisAddIn.SidePane.CheckedSelections;
            if (CheckedSelections.Contains(this))
            {
                CheckedSelections.Remove(this);
            }
        }

        public void parseRtf()
        {
            rtb.Paste();
            TextRange textRange = new TextRange(
                rtb.Document.ContentStart,
                rtb.Document.ContentEnd
            );

            using (MemoryStream ms = new MemoryStream())
            {
                textRange.Save(ms, DataFormats.Rtf);
                ms.Seek(0, SeekOrigin.Begin);
                using (StreamReader sr = new StreamReader(ms))
                {
                    RtfContent = sr.ReadToEnd();
                }
            }

            StringBuilder tempText = new StringBuilder();
            var lines = textRange.Text.Split(Environment.NewLine.ToCharArray()).ToArray();
            foreach (var line in lines)
            {
                if (line!=String.Empty && line != " ")
                {
                    tempText.Append(" " +line);
                }
            }
            Text = tempText.ToString();

            foreach (Block block in rtb.Document.Blocks)
            {
                if (block is System.Windows.Documents.Paragraph)
                {
                    System.Windows.Documents.Paragraph paragraph = (System.Windows.Documents.Paragraph)block;
                    foreach (Inline inline in paragraph.Inlines)
                    {
                        if (inline is InlineUIContainer)
                        {
                            InlineUIContainer uiContainer = (InlineUIContainer)inline;
                            if (uiContainer.Child is System.Windows.Controls.Image)
                            {
                                img.Source = ((System.Windows.Controls.Image)uiContainer.Child).Source;
                                img.Visibility = Visibility.Visible;
                                imgBorder.Visibility = Visibility.Visible;
                                return;
                            }
                        }
                    }
                }
                else if (block is BlockUIContainer)
                {
                    var container = (BlockUIContainer)block;
                    if (container.Child is System.Windows.Controls.Image)
                    {
                        img.Source = ((System.Windows.Controls.Image)container.Child).Source;
                        img.Visibility = Visibility.Visible;
                        imgBorder.Visibility = Visibility.Visible;
                        return;
                    }
                }
            }
        }

		public Boolean IsExported
		{
			get { return _isExported; }
			set { _isExported = value; }
		}

        public string Text
        {
            get { return _text; }
            set { _text = value; }
        }

        public string RtfContent
        {
            get { return _rtfContent; }
            set { _rtfContent = value; }
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
    }
}

