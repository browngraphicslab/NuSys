﻿
using Microsoft.Office.Interop.Word;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WordAddIn
{
    /// <summary>
    /// Interaction logic for SelectionItem.xaml
    /// </summary>
    public partial class SelectionItem : UserControl, INotifyPropertyChanged
    {
		private Boolean _isExported;
        private Comment _comment;
        private ScaleTransform _renderTransform;
        private Range _range;
        private string _text;
        private string _rtfContent;
        private MemoryStream _ms;
        private List<Bitmap> _imageContent;
        private Bookmark _bookmark;
        private String _dateTimeExported;
        private double _dropShadowOpac;

        public event PropertyChangedEventHandler PropertyChanged;

        public SelectionItem()
        {
            InitializeComponent();
            _renderTransform = new ScaleTransform(1, 1);
            DropShadowOpac = 0;
            DataContext = this;
        }

        public SelectionItemView GetView()
        {
            String path = null;
            if (Globals.ThisAddIn.Application.ActiveDocument != null && !String.IsNullOrEmpty(Globals.ThisAddIn.Application.ActiveDocument.FullName))
            {
                path = Globals.ThisAddIn.Application.ActiveDocument.FullName;
            }

            List<string> ImageNames = new List<string>();
            foreach (Bitmap img in ImageContent)
            {
                ImageNames.Add(string.Format(@"{0}", Guid.NewGuid()) + ".png");
            }

            return new SelectionItemView(this.Bookmark.Name, IsExported, RtfContent, path, ImageNames, DateTimeExported, Globals.ThisAddIn._fileToken);
        }

        public SelectionItemIdView GetIdView()
        {
            return new SelectionItemIdView(this.Bookmark.Name, IsExported, DateTimeExported);
        }

        private void SelectionItem_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!(sender is SelectionItem))
                return;

            var selectionItem = (SelectionItem)sender;
            selectionItem.Range.Select();

            //this.DropShadowOpac = 1.0;
            DropShadow.Opacity = 1.0;

            if (Globals.ThisAddIn.SidePane.SelectedSelection != null && Globals.ThisAddIn.SidePane.SelectedSelection != this)
            {
                Globals.ThisAddIn.SidePane.SelectedSelection.DropShadow.Opacity = 0.0;
            }

            Globals.ThisAddIn.SidePane.SelectedSelection = this;
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

        public void AddSelection()
        {
            ImageContent = new List<Bitmap>();

            if (this.Range.ShapeRange.Count > 0)
            {
                foreach (Shape shape in this.Range.ShapeRange)
                {
                    shape.Select();
                    Selection selection = Globals.ThisAddIn.Application.ActiveWindow.Selection;
                    selection.Copy();
                    parseImg();
                }

                if (this.Range.Text != null)
                {
                    Range textRange = Globals.ThisAddIn.Application.ActiveDocument.Range(this.Range.Start, this.Range.End);
                    textRange.Select();
                    Selection selection = Globals.ThisAddIn.Application.ActiveWindow.Selection;
                    selection.Copy();
                    fromClipboard();
                }
            }
            else
            {
                this.Range.Select();
                Selection selection = Globals.ThisAddIn.Application.ActiveWindow.Selection;
                selection.Copy();
                fromClipboard();
            }

            if (ImageContent.Count > 0)
            {
                setPreviewImage();
            }
        }

        public void fromClipboard()
        {
            if (Clipboard.ContainsData(System.Windows.DataFormats.Rtf))
            {
                parseRtf();
            }
            else if (Clipboard.ContainsData(System.Windows.Forms.DataFormats.Html))
            {
                parseHtml();
            }
            else if (Clipboard.ContainsData(System.Windows.Forms.DataFormats.Bitmap))
            {
                parseImg();
            }
        }

        public void parseHtml()
        {
            string html = (string)Clipboard.GetData(System.Windows.Forms.DataFormats.Html);
            var converter = new SautinSoft.HtmlToRtf();
            var RtfContent = converter.ConvertString(html);

            using (var reader = new MemoryStream(Encoding.UTF8.GetBytes(RtfContent)))
            {
                reader.Position = 0;
                rtb.SelectAll();
                rtb.Selection.Load(reader, DataFormats.Rtf);
            }

            getImgFromRtb();

            StringBuilder tempText = new StringBuilder();
            var lines = RtfContent.Split(Environment.NewLine.ToCharArray()).ToArray();
            foreach (var line in lines)
            {
                if (line != String.Empty && line != " ")
                {
                    tempText.Append(" " + line);
                }
            }

            Text = tempText.ToString();
        }

        public void parseImg()
        {
            Ms = new MemoryStream();

            System.Windows.Forms.IDataObject data = System.Windows.Forms.Clipboard.GetDataObject();
            Bitmap bitmapImg = (data.GetData(DataFormats.Bitmap, true) as Bitmap);

            ImageContent.Add(bitmapImg);
        }

        public void parseRtf()
        {
            rtb.Paste();
            System.Windows.Documents.TextRange textRange = new System.Windows.Documents.TextRange(
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
                if (line != String.Empty && line != " ")
                {
                    tempText.Append(" " + line);
                }
            }
            Text = tempText.ToString();

            getImgFromRtb();
        }

        public void getImgFromRtb()
        {
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

        public void setPreviewImage()
        {
            Bitmap bitmapImg = ImageContent.First();
            (bitmapImg).Save(Ms, System.Drawing.Imaging.ImageFormat.Bmp);

            BitmapImage image = new BitmapImage();
            image.BeginInit();
            Ms.Seek(0, SeekOrigin.Begin);
            image.StreamSource = Ms;
            image.EndInit();

            img.Source = image;
            img.Visibility = Visibility.Visible;
            imgBorder.Visibility = Visibility.Visible;
        }

        public double DropShadowOpac
        {
            get { return _dropShadowOpac; }
            set {
                _dropShadowOpac = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("DropShadowOpac"));
            }
        }

        public Boolean IsExported
		{
			get { return _isExported; }
			set { _isExported = value; }
		}

        public String DateTimeExported
        {
            get { return _dateTimeExported; }
            set { _dateTimeExported = value; }
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

        public List<Bitmap> ImageContent
        {
            get { return _imageContent; }
            set { _imageContent = value; }
        }

        public Range Range
        {
            get { return _range; }
            set { _range = value; }
        }

        public Bookmark Bookmark
        {
            get { return _bookmark; }
            set { _bookmark = value; }
        }

        public MemoryStream Ms
        {
            get { return _ms; }
            set { _ms = value; }
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

