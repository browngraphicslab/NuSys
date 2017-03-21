﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using NusysIntermediate;

namespace NusysIntermediate
{
    public class TextElementModel : ElementModel
    {

        public TextElementModel(string id): base(id)
        {
            ElementType = NusysConstants.ElementType.Text;
        }

        public override void UnPackFromDatabaseMessage(Message props)
        {
            base.UnPackFromDatabaseMessage(props);
        }

        public override async Task<Dictionary<string,object>> Pack()
        {
            return await base.Pack();
        }
    }
}
