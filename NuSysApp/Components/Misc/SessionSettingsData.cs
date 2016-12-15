using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public enum LinkVisibilityOption
    {
        AllLinks,
        NoLinks,
        VisibleWhenSelected,
        NoTrails,
    }
    /// <summary>
    /// A data holding object used to maintain all the settings for the current session. 
    /// This will also fire events when the options change.
    /// Make sure you don't listen to the events excessively, however.  
    ///  Should only be made once and stored in the session view.
    /// </summary>
    public class SessionSettingsData
    {
        /// <summary>
        /// The options for link visiblity.
        /// </summary>

        /// <summary>
        /// Event fired whenever the link visibility is changed. 
        /// Don't overlisten to this event, aka dont have every link listening to this.
        /// </summary>
        public event EventHandler<LinkVisibilityOption> LinkVisibilityChanged;

        /// <summary>
        /// Event fired whenever the boolean representing the resizing of text in nodes changes.
        /// </summary>
        public event EventHandler<bool> ResizeElementTitlesChanged;


        /// <summary>
        /// private version of the ResizeElementTitles.
        /// </summary>
        private bool _resizeElementTitles = false;

        /// <summary>
        /// The private version of LinksVisible. 
        /// </summary>
        private LinkVisibilityOption _linksVisible = LinkVisibilityOption.AllLinks;

        /// <summary>
        /// Enum representing the visibility of links and trails in the session.
        /// This might be changed later to have more link visibility options  (when focused, when filtered etc).
        /// The custom setter will fire the event notifying of the setting changed.
        /// </summary>
        public LinkVisibilityOption LinksVisible
        {
            get { return _linksVisible; }
            set
            {
                _linksVisible = value;
                LinkVisibilityChanged?.Invoke(this,value);
            }
        }

        /// <summary>
        /// Boolean representing whether the titles of elements will resize while zooming. 
        /// This also might be changed to an enum later to represent more options.
        /// </summary>
        public bool ResizeElementTitles
        {
            get { return _resizeElementTitles; }
            set
            {
                _resizeElementTitles = value;
                ResizeElementTitlesChanged?.Invoke(this, value);
            }
        }
    }
}
