﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

namespace NuSysApp
{
    public class LinkLibraryElementController : LibraryElementController
    {
        public event EventHandler<LinkDirectionEnum> LinkDirectionChanged;
        public LinkLibraryElementModel LinkLibraryElementModel { get; private set; }
        public int NumDirectionButtonClicks { get; set; }
        public LinkLibraryElementController(LinkLibraryElementModel model) : base(model)
        {
            Debug.Assert(model != null);
            LinkLibraryElementModel = model;
            NumDirectionButtonClicks = 0;
        }

        public void RaiseLinkDirectionChanged(object sender, LinkDirectionEnum e)
        {
            LinkDirectionChanged(sender, e);
        }

        public override void UnPack(Message message)
        {
            if (message.ContainsKey("id1"))
            {
                LinkLibraryElementModel.InAtomId = message["id1"] as string;
                Debug.Assert(LinkLibraryElementModel.InAtomId != null);
            }
            if (message.ContainsKey("id2"))
            {
                LinkLibraryElementModel.OutAtomId = message["id2"] as string;
                Debug.Assert(LinkLibraryElementModel.OutAtomId != null);
            }
            if (message.ContainsKey("color"))
            {

                string hexColor = message.GetString("color");
                byte a = byte.Parse(hexColor.Substring(1, 2), NumberStyles.HexNumber);
                byte r = byte.Parse(hexColor.Substring(3, 2), NumberStyles.HexNumber);
                byte g = byte.Parse(hexColor.Substring(5, 2), NumberStyles.HexNumber);
                byte b = byte.Parse(hexColor.Substring(7, 2), NumberStyles.HexNumber);
                LinkLibraryElementModel.Color = Color.FromArgb(a, r, g, b);
                //Color = Color.FromArgb(message.GetString("color"));
            }
            if (message.ContainsKey("isBiDirectional"))
            {
                if (message.GetString("isBiDirectional").Equals("True"))
                {
                    LinkLibraryElementModel.IsBiDirectional = true;
                }
                else
                {
                    LinkLibraryElementModel.IsBiDirectional = false;
                }
            }
            base.UnPack(message);
        }

    }
}
