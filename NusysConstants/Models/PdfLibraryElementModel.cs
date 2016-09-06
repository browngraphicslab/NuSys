using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NusysIntermediate;

namespace NusysIntermediate
{
    public class PdfLibraryElementModel : ImageLibraryElementModel 
    {

        public int PageStart { get; set; }
        public int PageEnd { get; set; }

        public PdfLibraryElementModel(string libraryId) : base(libraryId, NusysConstants.ElementType.PDF)
        {
        }

        public override void UnPackFromDatabaseKeys(Message message)
        {
            base.UnPackFromDatabaseKeys(message);

            //TODO  put basck in

            //if (message.ContainsKey(NusysConstants.PDF_REGION_PAGE_NUMBER_KEY))
            //{
            //    PageLocation = message.GetInt(NusysConstants.PDF_REGION_PAGE_NUMBER_KEY);
            //}
        }
    }
}
