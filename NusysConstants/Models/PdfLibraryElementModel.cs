﻿using System;
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

        public PdfLibraryElementModel(string libraryId, NusysConstants.ElementType type = NusysConstants.ElementType.PDF) : base(libraryId, type)
        {
        }

        public override void UnPackFromDatabaseKeys(Message message)
        {
            base.UnPackFromDatabaseKeys(message);
            if (message.ContainsKey(NusysConstants.PDF_PAGE_START_KEY))
            {
                PageStart = message.GetInt(NusysConstants.PDF_PAGE_START_KEY);
            }
            if (message.ContainsKey(NusysConstants.PDF_PAGE_END_KEY))
            {
                PageEnd = message.GetInt(NusysConstants.PDF_PAGE_END_KEY);
            }
        }
    }
}
