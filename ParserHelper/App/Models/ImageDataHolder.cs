﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParserHelper
{
    public class ImageDataHolder : DataHolder
    {
        public Uri Uri { get; set; }
        public ImageDataHolder(Uri uri, String title) : base(DataType.Image,title)
        {
            Uri = uri;
        }
    }
}