﻿
using Microsoft.Office.Interop.PowerPoint;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace PowerPointAddIn
{
    /// <summary>
    /// Interaction logic for SelectionItem.xaml
    /// </summary>
    public partial class SelectionItem : UserControl
    {
        private Boolean _isExported;
        private Comment _comment;
        private ScaleTransform _renderTransform;
        private string _text;
        private Selection _selection;
        private string _rtfContent;
        private MemoryStream _ms;
        private Bitmap _imageContent;

        public SelectionItem()
        {
            InitializeComponent();
            _renderTransform = new ScaleTransform(1, 1);
            DataContext = this;
        }

        public SelectionItemView GetView()
        {
            String path = null;
            if (Globals.ThisAddIn.Application.ActivePresentation != null && !String.IsNullOrEmpty(Globals.ThisAddIn.Application.ActivePresentation.FullName))
            {
                path = Globals.ThisAddIn.Application.ActivePresentation.FullName;
            }

            string ImageName = String.Empty;
            if (ImageContent != null)
            {
                ImageName = string.Format(@"{0}", Guid.NewGuid()) + ".png";
            }

            return new SelectionItemView(Guid.NewGuid().ToString(), IsExported, RtfContent, path, ImageName);
        }

        private void StackPanel_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!(sender is SelectionItem))
                return;

            var selectionItem = (SelectionItem)sender;
            //Globals.ThisAddIn.Application.ActiveWindow.View.GotoSlide(selectionItem.SlideNumber);
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

            if (ImageContent != null)
            {
                setPreviewImage();
            }
        }

        public void fromClipboard()
        {

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

            ImageContent = bitmapImg;
        }

        private void setPreviewImage() {
            Bitmap bitmapImg = ImageContent;

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

        public Selection ThisSelection
        {
            get { return _selection; }
            set { _selection = value; }
        }

        public Bitmap ImageContent
        {
            get { return _imageContent; }
            set { _imageContent = value; }
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

