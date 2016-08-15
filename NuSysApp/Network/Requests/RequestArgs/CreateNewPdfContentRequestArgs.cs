using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NusysIntermediate;

namespace NuSysApp
{
    /// <summary>
    /// the args class for adding in the pdf text upon creation
    /// </summary>
    public class CreateNewPdfContentRequestArgs : CreateNewContentRequestArgs
    {

        /// <summary>
        /// the text of the pdf to send along with the creation
        /// </summary>
        public string PdfText { get; set; }

        /// <summary>
        /// Total number of pages in the pdf
        /// </summary>
        public int PageCount { get; set; } 

        /// <summary>
        /// overrides the base classes but still adds in its keys
        /// </summary>
        /// <returns></returns>
        public override Message PackToRequestKeys()
        {
            var message =  base.PackToRequestKeys();

            if (PdfText != null)
            {
                message[NusysConstants.CREATE_NEW_PDF_CONTENT_REQUEST_PDF_TEXT_KEY] = PdfText;
            }
            return message;
        }
    }
}
