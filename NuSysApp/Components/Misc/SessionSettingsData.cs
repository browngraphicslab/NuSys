using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    /// <summary>
    /// A data holding object used to maintain all the settings for the current session. 
    /// This will also fire events when the options change.
    /// Make sure you don't listen to the events excessively, however.  
    ///  Should only be made once and stored in the session view.
    /// </summary>
    public class SessionSettingsData
    {
        /// <summary>
        /// Event fired whenever the link visibility is changed. 
        /// Don't overlisten to this event, aka dont have every link listening to this.
        /// </summary>
        public event EventHandler<bool> LinkVisibilityChanged;

        /// <summary>
        /// Event fired whenever the boolean representing the resizing of text in nodes changes.
        /// </summary>
        public event EventHandler<bool> ResizeElementTitlesChanged;


        /// <summary>
        /// private version of the ResizeElementTitles.
        /// </summary>
        private bool _resizeElementTitles;

        /// <summary>
        /// The private version of LinksVisible. 
        /// </summary>
        private bool _linksVisible;

        /// <summary>
        /// Boolean representing whether the links are visible or not for regular semantic links.
        /// This might be changed later to an enum representing different links visibility options (always, never, when focused, when selected, etc).
        /// The custom setter will fire the event notifying of the setting changed.
        /// </summary>
        public bool LinksVisible
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
