using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using NusysIntermediate;

namespace NuSysApp
{
    public class LinkLibraryElementController : LibraryElementController
    {
        public LinkLibraryElementModel LinkLibraryElementModel { get; private set; }
        public LinkLibraryElementController(LinkLibraryElementModel model) : base(model)
        {
            Debug.Assert(model != null);
            LinkLibraryElementModel = model;
        }

        /// <summary>
        /// Event fired whenever the directionality of the link changes.
        /// This event will pass the new direction of the link.  
        /// </summary>
        public event EventHandler<NusysConstants.LinkDirection> DirectionChanged;

        /// <summary>
        /// controller method to set the model's direction for the link.
        /// This will fire the DirectionChanged event and will update the server if it appropriate to do so.
        /// </summary>
        /// <param name="direction"></param>
        public void SetLinkDirection(NusysConstants.LinkDirection direction)
        {
            LinkLibraryElementModel.Direction = direction;
            DirectionChanged?.Invoke(this,direction);
            if (!_blockServerInteraction)
            {
                _debouncingDictionary.Add(NusysConstants.LINK_LIBRARY_ELEMENT_DIRECTIONALITY_KEY,direction);
            }
        }


        public override void UnPack(Message message)
        {
            _blockServerInteractionCount++;
            if (message.ContainsKey(NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_LINK_ID_IN_KEY))

            {
                LinkLibraryElementModel.InAtomId = message[NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_LINK_ID_IN_KEY] as string;
                Debug.Assert(LinkLibraryElementModel.InAtomId != null);
            }
            if (message.ContainsKey(NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_LINK_ID_OUT_KEY))
            {
                LinkLibraryElementModel.OutAtomId = message[NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_LINK_ID_OUT_KEY] as string;
                Debug.Assert(LinkLibraryElementModel.OutAtomId != null);
            }
            if (message.ContainsKey(NusysConstants.LINK_LIBRARY_ELEMENT_DIRECTIONALITY_KEY))
            {
                SetLinkDirection(message.GetEnum<NusysConstants.LinkDirection>(NusysConstants.LINK_LIBRARY_ELEMENT_DIRECTIONALITY_KEY));
            }
            _blockServerInteractionCount--;
            base.UnPack(message);
        }

    }
}
