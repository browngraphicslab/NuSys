﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NusysIntermediate;

namespace NusysIntermediate
{
    public class CollectionElementModel : ElementModel
    {

        public enum CollectionViewType { List, FreeForm, Timeline }

        public CollectionViewType ActiveCollectionViewType;

        public double LocationX { get; set; }
        public double LocationY { get; set; }
        public double CenterX { get; set; }
        public double CenterY { get; set; }
        public double Zoom { get; set; }

        public CollectionElementModel(string id):base(id)
        {

        }

        public override async Task UnPackFromDatabaseMessage(Message props)
        {
            if (props.ContainsKey("collectionview"))
            {
                string t = props.GetString("collectionview");
                ActiveCollectionViewType = (CollectionViewType)Enum.Parse(typeof(CollectionViewType), t);
            }
            await base.UnPackFromDatabaseMessage(props);
        }
    }
}