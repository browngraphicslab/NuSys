﻿using Windows.Data.Pdf;

namespace NuSysApp.ViewModels
{
    class PdfNodeViewModel : NodeViewModel
    {

        public PdfNodeViewModel(WorkspaceViewModel workspaceViewModel, string filePath) : base(workspaceViewModel)
        {
            
        }

        public PdfNodeViewModel(WorkspaceViewModel workspaceViewModel, PdfDocument pdfDocument)
            : base(workspaceViewModel)
        {
            
        }
    }
}
