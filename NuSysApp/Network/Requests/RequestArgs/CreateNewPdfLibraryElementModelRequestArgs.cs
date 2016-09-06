using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NusysIntermediate;

namespace NuSysApp
{
    public class CreateNewPdfLibraryElementModelRequestArgs : CreateNewImageLibraryElementRequestArgs
    {
        public CreateNewPdfLibraryElementModelRequestArgs() : base()
        {
            LibraryElementType = NusysConstants.ElementType.PDF;
        }

        /// <summary>
        /// REQUIRED the int representing the page number that this pdf library element model will start at.
        /// </summary>
        public int? PdfPageStart { get; set; }

        /// <summary>
        /// REQUIRED the int representing the page number that this pdf library element model will end at.
        /// </summary>
        public int? PdfPageEnd { get; set; }

        public override Message PackToRequestKeys()
        {
            Debug.Assert(PdfPageEnd != null);
            Debug.Assert(PdfPageStart != null);
            var message = base.PackToRequestKeys();

            message[NusysConstants.NEW_PDF_LIBRARY_ELEMENT_REQUEST_PAGE_START_KEY] = PdfPageStart.Value;
            message[NusysConstants.NEW_PDF_LIBRARY_ELEMENT_REQUEST_PAGE_END_KEY] = PdfPageEnd.Value;
            return message;
        }
    }
}
