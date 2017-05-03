using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using NusysIntermediate;

namespace NuSysApp
{
    public class NuSysUtils
    {

        public static Rect? GetRegionBounds(LibraryElementModel model)
        {
            switch (model.Type)
            {
                case NusysConstants.ElementType.Image:
                    var imageModel = (ImageLibraryElementModel)model;
                    return new Rect(imageModel.NormalizedX, imageModel.NormalizedY, imageModel.NormalizedWidth, imageModel.NormalizedHeight);
                case NusysConstants.ElementType.PDF:
                    var pdfModel = (PdfLibraryElementModel)model;
                    return new Rect(pdfModel.NormalizedX, pdfModel.NormalizedY, pdfModel.NormalizedWidth, pdfModel.NormalizedHeight);
                default:
                    return null;
            }
        }
    }
}
