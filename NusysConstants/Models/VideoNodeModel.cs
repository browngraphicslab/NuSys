﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NusysIntermediate;

namespace NusysIntermediate
{
    public class VideoNodeModel : ElementModel
    {
        private byte[] _byteArray;
        private int _resX, _resY;
        public delegate void JumpEventHandler(TimeSpan time);
        public event JumpEventHandler OnJump;

        public VideoNodeModel(string id) : base(id)
        {
            //Recording = new InMemoryRandomAccessStream();
            ElementType = NusysConstants.ElementType.Video;
        }
        public byte[] ByteArray
        {
            get { return _byteArray; }
            set { _byteArray = value; }
        }
        public int ResolutionX
        {
            get { return _resX; }
            set { _resX = value; }
        }
        public int ResolutionY
        {
            get { return _resY; }
            set { _resY = value; }
        }
        public override async Task<Dictionary<string, object>> Pack()
        {
            var props = await base.Pack();
            if (ByteArray != null)
            {
                props.Add("video", Convert.ToBase64String(ByteArray));
            }
 
            props.Add("resolutionX", ResolutionX);
            props.Add("resolutionY", ResolutionY);
            

            return props;
        }

        public override void UnPackFromDatabaseMessage(Message props)
        {
            if (props.ContainsKey(NusysConstants.VIDEO_ELEMENT_VIDEO_DATA_BYTES))
            {
                ByteArray = Convert.FromBase64String(props.GetString(NusysConstants.VIDEO_ELEMENT_VIDEO_DATA_BYTES));
            }
            if (props.ContainsKey(NusysConstants.VIDEO_ELEMENT_RESOLUTION_X_KEY))
            {
                ResolutionX = props.GetInt(NusysConstants.VIDEO_ELEMENT_RESOLUTION_X_KEY);
            }
            if (props.ContainsKey(NusysConstants.VIDEO_ELEMENT_RESOLUTION_Y_KEY))
            {
                ResolutionY = props.GetInt(NusysConstants.VIDEO_ELEMENT_RESOLUTION_Y_KEY);
            }
            base.UnPackFromDatabaseMessage(props);
        }
    }
}
