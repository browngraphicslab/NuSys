using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    /// <summary>
    /// The options for link visiblity.
    /// </summary>
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
        /// Event fired whenever the link visibility is changed. 
        /// Don't overlisten to this event, aka dont have every link listening to this.
        /// </summary>
        public event EventHandler<LinkVisibilityOption> LinkVisibilityChanged;

        /// <summary>
        /// Event fired whenever the boolean representing the resizing of text in nodes changes.
        /// </summary>
        public event EventHandler<bool> ResizeElementTitlesChanged;

        /// <summary>
        /// event fired whenever the visibility of the bread crumb trail changes
        /// </summary>
        public event EventHandler<bool> BreadCrumbPositionChanged;

        /// <summary>
        /// event fired whenever the visibility of the minimap changes
        /// </summary>
        public event EventHandler<bool> MinimapVisiblityChanged;

        /// <summary>
        /// event fired whenever the visibility of the minimap changes
        /// </summary>
        public event EventHandler<double> TextScaleChanged;

        /// <summary>
        /// private version of the bradcrumb visibility bool
        /// </summary>
        private bool _breadCrumbsDocked = true;

        /// <summary>
        /// private version of the minimap visiblity bool
        /// </summary>
        private bool _minimapVisible = true;

        /// <summary>
        /// private version of the ResizeElementTitles.
        /// </summary>
        private bool _resizeElementTitles = false;

        /// <summary>
        /// The private version of LinksVisible. 
        /// </summary>
        private LinkVisibilityOption _linksVisible = LinkVisibilityOption.AllLinks;

        /// <summary>
        /// Accessibility setting for increasing the size of fonts and some buttons.
        /// </summary>
        private double _textScale = 1;

        /// <summary>
        /// Accessibility setting for increasing the size of fonts and some buttons.
        /// </summary>
        public double TextScale
        {
            get
            {
                return _textScale;
            }
            set
            {
                var fireEvent = _textScale != value;
                _textScale = value;
                if (fireEvent)
                {
                    TextScaleChanged?.Invoke(this, value);
                    SaveToFile();
                }
            }
        }

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
                var fireEvent = _linksVisible != value;
                _linksVisible = value;
                if (fireEvent)
                {
                    LinkVisibilityChanged?.Invoke(this, value);
                    SaveToFile();
                }
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
                SaveToFile();
            }
        }

        /// <summary>
        /// Boolean representing whether the bread crumb trail is docked.   
        /// Setting this will fire this class's event for when the boolean changes.
        /// </summary>
        public bool BreadCrumbsDocked
        {
            get { return _breadCrumbsDocked; }
            set
            {
                _breadCrumbsDocked = value;
                BreadCrumbPositionChanged?.Invoke(this, _breadCrumbsDocked);
                SaveToFile();
            }
        }

        /// <summary>
        /// Boolean representing whether the minimap is visible.   
        /// Setting this will fire this class's event for when the boolean changes.
        /// </summary>
        public bool MinimapVisible
        {
            get { return _minimapVisible; }
            set {
                _minimapVisible = value;
                MinimapVisiblityChanged?.Invoke(this, value);
                SaveToFile();
            }
        }


        /// <summary>
        /// saves file to folder so we can get the same settings on the same machine every time.  
        /// </summary>
        private void SaveToFile()
        {
            if(SessionController.Instance.SessionSettings == null)
            {
                return;
            }
            Task.Run(async delegate { StorageUtil.SaveSettings(this); });
        }
    }
}
