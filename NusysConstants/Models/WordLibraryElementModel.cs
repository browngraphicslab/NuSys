﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NusysIntermediate
{ 
    public class WordLibraryElementModel : PdfLibraryElementModel
    {
        public WordLibraryElementModel(string libraryId) : base(libraryId, NusysConstants.ElementType.Word) { }
    }
}
