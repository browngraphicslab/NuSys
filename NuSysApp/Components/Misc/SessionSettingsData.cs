using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NusysIntermediate;

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
        NoTrails
    }

    /// <summary>
    /// The window visbility options for read only mode
    /// </summary>
    public enum ReadOnlyViewingMode
    {
        AlwaysVisible, VisibleOnFocus
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
        /// Event fired when the read only mode window visibility option is toggled.
        /// </summary>
        public event EventHandler<ReadOnlyViewingMode> ReadOnlyModeSettingChanged;

        /// <summary>
        /// Event fired when the visibility of tags no nodes is changed.
        /// Passed the enw visibility bool
        /// </summary>
        public event EventHandler<bool> TagsVisibleChanged;

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
        private bool _resizeElementTitles;

        /// <summary>
        /// The private version of LinksVisible. 
        /// </summary>
        private LinkVisibilityOption _linksVisible = LinkVisibilityOption.AllLinks;

        /// <summary>
        /// Accessibility setting for increasing the size of fonts and some buttons.
        /// </summary>
        private double _textScale = 1;

        /// <summary>
        /// Private version of ReadOnlyViewingMode
        /// </summary>
        private ReadOnlyViewingMode _readOnlyViewingModeOption = ReadOnlyViewingMode.AlwaysVisible;

        /// <summary>
        /// bool representing the visibility of tags on nodes
        /// </summary>
        private bool _tagsVisible = true;

        private string _serverName;

        public string ServerName
        {
            get { return _serverName; }
            set
            {
                if (_serverName != value)
                {
                    _serverName = value;
                    NusysConstants.ServerName = value;
                    SaveToFile();
                }
            }
        }

        /// <summary>
        /// Public bool for representing the visibility of keywords on nodes.
        /// True if they are visible, false otherwise
        /// </summary>
        public bool TagsVisible
        {
            get { return _tagsVisible; }
            set
            {
                var fireEvent = _tagsVisible != value;
                _tagsVisible = value;
                if (fireEvent)
                {
                    TagsVisibleChanged?.Invoke(this, value);
                    SaveToFile();
                }
            }
        }

        /// <summary>
        /// Accessibility setting for increasing the size of fonts and some buttons.
        /// setting this will not automatically save to file.
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
                }
            }
        }

        /// <summary>
        /// Enum representing the visibility of links and trails in the session.
        /// This might be changed later to have more link visibility options  (when focused, when filtered etc).
        /// The custom setter will fire the event notifying of the setting changed.
        /// Setting this value will automatially save to file.
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
        /// This is the enum which represents the settings for the read-only mode windows. Either the 
        /// windows are always visible or only visible when a node is in focus. 
        /// The setter for this property fires the event notifying the change in setting. 
        /// Setting value automatically causes the setting to be saved to file.
        /// </summary>
        public ReadOnlyViewingMode ReadOnlyModeWindowsVisible
        {
            get { return _readOnlyViewingModeOption; }
            set
            {
                var fireEvent = _readOnlyViewingModeOption != value;
                _readOnlyViewingModeOption = value;
                if (fireEvent)
                {
                    ReadOnlyModeSettingChanged?.Invoke(this, value);
                    SaveToFile();
                }
            }
        }

        /// <summary>
        /// Boolean representing whether the titles of elements will resize while zooming. 
        /// This also might be changed to an enum later to represent more options.
        /// Setting this value will automatially save to file.
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
        /// Setting this value will automatially save to file.
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
        /// Setting this value will automatially save to file.
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
        public void SaveToFile()
        {
            if(SessionController.Instance.SessionSettings == null)
            {
                return;
            }
            Task.Run(async delegate { StorageUtil.SaveSettings(this); });
        }
    }
}
