using System;
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
        public LinkLibraryElementController(LinkLibraryElementModel model) : base(model)
        {
            Debug.Assert(model != null);
            LinkLibraryElementModel = model;
            //NumDirectionButtonClicks = 0;
        }

        public void RaiseLinkDirectionChanged(object sender, LinkDirectionEnum e)
        {
            LinkDirectionChanged(sender, e);
            _debouncingDictionary.Add("LinkDirectionEnum", e);
            LinkLibraryElementModel.LinkedDirectionEnum = e;
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
            //if (message["creator_user_id"].Equals("grant") || message["creator_user_id"].Equals("tnarg"))
            //{
                
            //}
            if (message.ContainsKey("LinkDirectionEnum") && (long.Parse(message["LinkDirectionEnum"] as string) != null))
            {
                long enumIndex = message.GetLong("LinkDirectionEnum");
                if (enumIndex == 0)
                {
                    LinkLibraryElementModel.LinkedDirectionEnum = LinkDirectionEnum.Mono1;
                }
                else if (enumIndex == 1)
                {
                    LinkLibraryElementModel.LinkedDirectionEnum = LinkDirectionEnum.Mono2;
                }
                else
                {
                    LinkLibraryElementModel.LinkedDirectionEnum = LinkDirectionEnum.Bi;
                }
            }
            else
            {
                LinkLibraryElementModel.LinkedDirectionEnum = LinkDirectionEnum.Bi;

            }
            base.UnPack(message);
        }

    }
}
