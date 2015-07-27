using Windows.UI.Xaml.Media.Imaging;
﻿using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace NuSysApp
{
    public class Factory
    {
        WorkspaceViewModel _workSpaceViewModel;
        public Factory(WorkspaceViewModel vm)
        {
            _workSpaceViewModel = vm;
        }

        public TextNodeViewModel CreateNewText(string data)
        {
            TextNodeViewModel textVM = new TextNodeViewModel(_workSpaceViewModel);
            textVM.Data = data;
            return textVM;
        }

        //public static TextNodeViewModel CreateNewText(string data)
        //{
        //    return new TextNodeViewModel(new WorkspaceViewModel()) {Data = data};
        //}

        public RichTextNodeViewModel CreateNewRichText(string html)
        {
            RichTextNodeViewModel richTextVM = new RichTextNodeViewModel(_workSpaceViewModel);
            richTextVM.Data = html;
            return richTextVM;
        }

        //public static RichTextNodeViewModel CreateNewRichText(string html)
        //{
        //    return new RichTextNodeViewModel(new WorkspaceViewModel()) {Data = html};
        //}

        public ImageNodeViewModel CreateNewImage(BitmapImage bmi)
        {
            ImageNodeViewModel imageVM = new ImageNodeViewModel(_workSpaceViewModel, bmi);
            return imageVM;
        }

        public PdfNodeViewModel CreateNewPdfNodeViewModel()
        {
            //var pdfNodeVM = new PdfNodeViewModel(_workSpaceViewModel);
            //return pdfNodeVM;
            return new PdfNodeViewModel(_workSpaceViewModel);
        }

        public InkNodeViewModel CreateNewInk()
        {
            return new InkNodeViewModel(_workSpaceViewModel);
        }
   
        public NodeViewModel CreateFromDataString(string data)
        {
            //TO DO
            return null;
        }
    }

    
}
 