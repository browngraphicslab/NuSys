using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using LdaLibrary;

namespace NuSysApp
{
    public class RegionLibraryElementController : LibraryElementController
    {
        public event EventHandler<string> TitleChanged;
        public delegate void SelectHandler(RegionLibraryElementController regionLibraryElementController);
        public event SelectHandler OnSelect;
        public delegate void DeselectHandler(RegionLibraryElementController regionLibraryElementController);
        public event DeselectHandler OnDeselect;
        public event EventHandler<LinkLibraryElementController> LinkAdded;
        public event EventHandler<string> LinkRemoved;
        public delegate void MetadataChangedEventHandler(object source);
        public event MetadataChangedEventHandler MetadataChanged;

        public Region RegionModel
        {
            get
            {
                Debug.Assert(LibraryElementModel is Region);
                return LibraryElementModel as Region;
            }
        }


        private bool _selected;
        private bool _blockServerUpdates;
        public RegionLibraryElementController(Region model): base(model)
        {

        }



        //public bool AddMetadata(MetadataEntry entry)
        //{
        //    if (entry.Values==null || string.IsNullOrEmpty(entry.Key) || string.IsNullOrWhiteSpace(entry.Key))
        //        return false;
        //    if (Model.Metadata.ContainsKey(entry.Key))
        //    {
        //        if (entry.Mutability==MetadataMutability.IMMUTABLE)//weird syntax in case we want to change mutability to an enum eventually
        //        {
        //            return false;
        //        }
        //        Model.Metadata.Remove(entry.Key);
        //    }
        //    Model.Metadata.Add(entry.Key,entry);
        //    return true;
        //}

        protected void SetBlockServerBoolean(bool blockServerUpdates)
        {
            _blockServerUpdates = blockServerUpdates;
        }
        

        public void Select()
        {
            _selected = true;
            OnSelect?.Invoke(this);
        }

        public void Deselect()
        {
            _selected = false;
            OnDeselect?.Invoke(this);
        }

        /// <summary>
        /// This mehtod should only be called from the server upon other updates.  It will pass in a region
        /// you should extract the region's properties and call the update methods in the controllers
        /// </summary>
        /// <param name="region"></param>
        public override void UnPack(Message message)
        { 
            SetBlockServerBoolean(true);//this is a must otherwise infinite loops will occur
            if (message.ContainsKey("clipping_parent_library_id"))
            {
                RegionModel.ClippingParentId = message.GetString("clipping_parent_library_id");
            }
            base.UnPack(message);
            SetBlockServerBoolean(false);//THIS is a must otherwise changes wont be saved
        }

        public Uri LargeIconUri
        {
            get
            {
                if (LibraryElementModel.LargeIconUrl != null)
                {
                    return new Uri("http://" + WaitingRoomView.ServerName + "/" + LibraryElementModel.LargeIconUrl);
                }
                switch (LibraryElementModel.Type)
                {
                    case ElementType.Image:
                    case ElementType.Video:
                        return new Uri("http://" + WaitingRoomView.ServerName + "/" + LibraryElementModel.LibraryElementId + "_thumbnail_large.jpg");
                        break;
                    case ElementType.PDF:
                        return new Uri("ms-appx:///Assets/library_thumbnails/pdf.png");
                        break;
                    case ElementType.Audio:
                        return new Uri("ms-appx:///Assets/library_thumbnails/audio.png");
                        break;
                    case ElementType.Text:
                        return new Uri("ms-appx:///Assets/library_thumbnails/text.png");
                        break;
                    case ElementType.Collection:
                        return new Uri("ms-appx:///Assets/library_thumbnails/collection_1.png");
                        break;
                    case ElementType.Word:
                        return new Uri("ms-appx:///Assets/library_thumbnails/word.png");
                        break;
                    case ElementType.Link:
                        return new Uri("ms-appx:///Assets/library_thumbnails/link.png");
                        break;
                    default:
                        return new Uri("ms-appx:///Assets/icon_chat.png");
                }
            }
        }
        public Uri MediumIconUri
        {
            get
            {
                if (LibraryElementModel.MediumIconUrl != null)
                {
                    return new Uri("http://" + WaitingRoomView.ServerName + "/" + LibraryElementModel.MediumIconUrl);
                }
                switch (LibraryElementModel.Type)
                {
                    case ElementType.Image:
                    case ElementType.Video:
                        return new Uri("http://" + WaitingRoomView.ServerName + "/" + LibraryElementModel.LibraryElementId + "_thumbnail_medium.jpg");
                        break;
                    case ElementType.PDF:
                        return new Uri("ms-appx:///Assets/library_thumbnails/pdf.png");
                        break;
                    case ElementType.Audio:
                        return new Uri("ms-appx:///Assets/library_thumbnails/audio.png");
                        break;
                    case ElementType.Text:
                        return new Uri("ms-appx:///Assets/library_thumbnails/text.png");
                        break;
                    case ElementType.Collection:
                        return new Uri("ms-appx:///Assets/library_thumbnails/collection_1.png");
                        break;
                    case ElementType.Word:
                        return new Uri("ms-appx:///Assets/library_thumbnails/word.png");
                        break;
                    case ElementType.Link:
                        return new Uri("ms-appx:///Assets/library_thumbnails/link.png");
                        break;
                    default:
                        return new Uri("ms-appx:///Assets/icon_chat.png");
                }
            }
        }

        public Uri SmallIconUri
        {
            get
            {
                return new Uri("ms-appx:///Assets/icon_delete_color.png");

                //TODO uncomment this and remove line above to fix
                //if (LibraryElementModel.SmallIconUrl != null)
                //{
                //    return new Uri("http://" + WaitingRoomView.ServerName + "/" + LibraryElementModel.SmallIconUrl);
                //}
                //switch (LibraryElementModel.Type)
                //{
                //    case ElementType.Image:
                //    case ElementType.Video:
                //        return new Uri("http://" + WaitingRoomView.ServerName + "/" + LibraryElementModel.LibraryElementId + "_thumbnail_small.jpg");
                //        break;
                //    case ElementType.PDF:
                //        return new Uri("ms-appx:///Assets/library_thumbnails/pdf.png");
                //        break;
                //    case ElementType.Audio:
                //        return new Uri("ms-appx:///Assets/library_thumbnails/audio.png");
                //        break;
                //    case ElementType.Text:
                //        return new Uri("ms-appx:///Assets/library_thumbnails/text.png");
                //        break;
                //    case ElementType.Collection:
                //        return new Uri("ms-appx:///Assets/library_thumbnails/collection_1.png");
                //        break;
                //    case ElementType.Word:
                //        return new Uri("ms-appx:///Assets/library_thumbnails/word.png");
                //        break;
                //    case ElementType.Link:
                //        return new Uri("ms-appx:///Assets/library_thumbnails/link.png");
                //        break;
                //    default:
                //        return new Uri("ms-appx:///Assets/icon_chat.png");
                //        break;
                //}
            }
        }
    }
}
