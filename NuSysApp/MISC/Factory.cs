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

        public RichTextNodeViewModel CreateNewRichText(string html)
        {
            RichTextNodeViewModel richTextVM = new RichTextNodeViewModel(_workSpaceViewModel);
            String c;
            using (Stream s =
                  typeof(NuSysApp.App).GetTypeInfo()
                      .Assembly.GetManifestResourceStream("NuSysApp.Assets.paragraph.nusys"))
            {
                StreamReader reader = new StreamReader(s);
                c = reader.ReadToEnd();
                Debug.WriteLine(c);
            }
            richTextVM.Data = c;
            
            return richTextVM;
        }

        public ImageNodeViewModel CreateNewImage(BitmapImage bmi)
        {
            ImageNodeViewModel imageVM = new ImageNodeViewModel(_workSpaceViewModel, bmi);
            return imageVM;
        }

        public PdfNodeViewModel CreateNewPdfNodeViewModel()
        {
            PdfNodeViewModel pdfNodeVM = new PdfNodeViewModel(_workSpaceViewModel);
            return pdfNodeVM;
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
 