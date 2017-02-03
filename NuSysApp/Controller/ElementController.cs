using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.ApplicationSettings;
using Windows.UI.Xaml.Controls;
using NusysIntermediate;
using WinRTXamlToolkit.Controls.DataVisualization;
using WinRTXamlToolkit.IO.Extensions;

namespace NuSysApp
{
    public class ElementController : ILinkable
    {
        private ElementModel _model;
        protected DebouncingDictionary _debouncingDictionary;
        protected bool IsDisposed;

        /// <summary>
        /// count to represent how many unpacks are currently running.
        /// This is being used to replace the boolean. If this number is greater than 0, then an unpack is currently happening
        /// </summary>
        protected int _blockServerInteractionCount;

        private bool _blockServerInteraction
        {
            get
            {
                return _blockServerInteractionCount != 0;
            }
        }

        public delegate void AlphaChangedEventHandler(object source, double alpha);

        public delegate void DeleteEventHandler(object source);

        public delegate void LocationUpdateEventHandler(object source, double x, double y, double dx = 0, double dy = 0);

        public delegate void MetadataChangeEventHandler(object source, string key);

        public delegate void ScaleChangedEventHandler(object source, double sx, double sy);

        public delegate void SizeUpdateEventHandler(object source, double width, double height);

        public delegate void SelectionChangedHandler(object source, bool selected);

        public delegate void LinksUpdatedEventHandler(object source);

        public event EventHandler Disposed;
        public event DeleteEventHandler Deleted;
        public event MetadataChangeEventHandler MetadataChange;
        public event LocationUpdateEventHandler PositionChanged;
        public event SizeUpdateEventHandler SizeChanged;
        public event ScaleChangedEventHandler ScaleChanged;
        public event AlphaChangedEventHandler AlphaChanged;
        public event SelectionChangedHandler SelectionChanged;
        public event EventHandler<Point2d> AnchorChanged;
        public event LinksUpdatedEventHandler LinksUpdated;
        // Events for when a user starts/stops editing this node
        public event EventHandler<string> UserAdded;
        public event EventHandler<string> UserDropped;
        public event EventHandler<bool> TitleVisiblityChanged; 

        public virtual Point2d Anchor
        {
            get
            {
                return new Point2d(Model.X + Model.Width / 2, Model.Y + Model.Height / 2);
            }
        }
        public ElementController() { }


        public ElementController(ElementModel model)
        {
            _model = model;

            Debug.Assert(model != null, "wtf");

            _debouncingDictionary = new ElementDebouncingDictionary(model.Id);

            if (LibraryElementController != null)
            {
                LibraryElementController.Deleted += Delete;
                var title = LibraryElementModel.Title;
                Model.Title = title;
            }
            Debug.Assert(this.Id != null);
            SessionController.Instance.LinksController.AddLinkable(this);
        }


        public virtual void Dispose()
        {
            IsDisposed = true;
            if (LibraryElementController != null)
            {
                LibraryElementController?.FireAliasRemoved(Model);
                LibraryElementController.Deleted -= Delete;
            }
            Disposed?.Invoke(this, EventArgs.Empty);
        }

        public void SetScale(double sx, double sy)
        {
            Model.ScaleX = sx;
            Model.ScaleY = sy;

            ScaleChanged?.Invoke(this, sx, sy);
        }

        public void Selected(bool selected)
        {
            SelectionChanged?.Invoke(this, selected);
        }

        /// <summary>
        /// sets the width and height of the element.  
        /// Will fire an event notiftying all listeners of the size change.
        /// Will update the element model.
        /// The save to server boolean will block server updates from being sent if 'false' is passed in.
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="saveToServer"></param>
        public virtual void SetSize(double width, double height, bool saveToServer = true)
        {
            if (width < Constants.MinNodeSize || height < Constants.MinNodeSize || width * Model.Height / Model.Width < Constants.MinNodeSize)
            {
                return;
            }

            Model.Width = width;
            Model.Height = height;
            SizeChanged?.Invoke(this, width, height);
            FireAnchorChanged();
            if (saveToServer && !_blockServerInteraction)
            {
                _debouncingDictionary.Add(NusysConstants.ALIAS_SIZE_WIDTH_KEY, width);
                _debouncingDictionary.Add(NusysConstants.ALIAS_SIZE_HEIGHT_KEY, height);
            }
        }

        /// <summary>
        /// public method to show the titles on nodes
        /// </summary>
        /// <param name="titleVisible"></param>
        public void SetTitleVisiblity(bool titleVisible)
        {
            Model.ShowTitle = titleVisible;

            TitleVisiblityChanged?.Invoke(this, titleVisible);

            if (!_blockServerInteraction)
            {
                _debouncingDictionary.Add(NusysConstants.ALIAS_TITLE_VISIBILITY_KEY, titleVisible);
            }

        }

        private void FireAnchorChanged()
        {
            AnchorChanged?.Invoke(this, Anchor);
        }

        public void SetPosition(double x, double y)
        {
            if (IsDisposed)
                return;

            var px = Model.X;
            var py = Model.Y;
            Model.X = x;
            Model.Y = y;

            PositionChanged?.Invoke(this, x, y, x - px, y - py);
            FireAnchorChanged();

            if (!_blockServerInteraction)
            {
                _debouncingDictionary.Add(NusysConstants.ALIAS_LOCATION_X_KEY, x);
                _debouncingDictionary.Add(NusysConstants.ALIAS_LOCATION_Y_KEY, y);
            }
        }

        public void SetAlpha(double alpha)
        {
            Model.Alpha = alpha;

            AlphaChanged?.Invoke(this, alpha);

            if (!_blockServerInteraction)
            {
                _debouncingDictionary.Add("alpha", alpha);
            }
        }

        public void Delete(object sender)
        {
            Deleted?.Invoke(this);
            SessionController.Instance.ActiveFreeFormViewer?.DeselectAll();

            Dispose();
        }

        /// <summary>
        /// this method will send a server request to delete an element.
        /// If successful, it will remove it locally.  
        /// Returns whether the local removal was successful.
        /// </summary>
        /// <returns></returns>
        public async virtual Task<bool> RequestDelete()
        {
            //create and execute the request
            var request = new DeleteElementRequest(Model.Id);
            await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(request);
            if (request.WasSuccessful() == true)
            {
                //delete it locally (may need to check if it was succesful first)
                return request.RemoveNodeLocally();
            }
            return false;
        }

        /// <summary>
        /// Requests a duplicate of the controller's element that will be located at the given x and y coordinates
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public async virtual Task RequestDuplicate(double x, double y)
        {
            // Set up the request args
            var args = new NewElementRequestArgs();
            args.X = x;
            args.Y = y;
            args.Width = Model.Width;
            args.Height = Model.Height;
            args.ParentCollectionId = Model.ParentCollectionId;
            args.LibraryElementId = Model.LibraryId;

            // Set up the request, execute it, and add the new element to the session
            var request = new NewElementRequest(args);
            await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(request);
            request.AddReturnedElementToSession();

        }


        /// <summary>
        /// This method will move this alias to a different collection.  
        /// Give it LibaryElementId of the new collection you want to move it to.
        /// You can also pass in the x and y coordinates for it in the new collection
        /// </summary>
        /// <param name="newCollectionLibraryID"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public virtual async Task RequestMoveToCollection(string newCollectionLibraryID, double x=50000, double y=50000, double width = Double.NaN, double height =Double.NaN)
        {
            if(newCollectionLibraryID == Model.LibraryId)
            {
                return;
            }

            var args = new MoveElementToCollectionRequestArgs();
            args.ElementId = Id;
            args.NewParentCollectionId = newCollectionLibraryID;
            args.XCoordinate = x;
            args.YCoordinate = y;

            var request = new MoveElementToCollectionRequest(args);

            await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(request);

            if (request.WasSuccessful() == true)
            {
                await request.UpdateContentLocally();
            }
            else
            {
                Debug.Assert(false, "request failed");
                //alert the user it failed
            }
        }

        public ElementModel Model
        {
            get { return _model; }
        }
        public LibraryElementController LibraryElementController
        {
            get
            {
                //Debug.Assert(Model.LibraryId != null);
                return SessionController.Instance.ContentController.GetLibraryElementController(Model.LibraryId);
            }
        }
        public LibraryElementModel LibraryElementModel
        {
            get
            {
                return LibraryElementController?.LibraryElementModel;
            }
        }

        public string Id
        {
            get
            {
                Debug.Assert(Model != null);
                return Model.Id;
            }
        }

        public string LibraryElementId
        {
            get
            {
                Debug.Assert(Model != null);
                return Model.LibraryId;
            }
        }

        public virtual async Task UnPack(Message props)
        {
            _blockServerInteractionCount++;
            if (props.ContainsKey(NusysConstants.ALIAS_LOCATION_X_KEY) || props.ContainsKey(NusysConstants.ALIAS_LOCATION_Y_KEY))
            {
                //if either "x" or "y" are not found in props, x/y stays the current value stored in Model.X/Y
                var x = props.GetDouble(NusysConstants.ALIAS_LOCATION_X_KEY, this.Model.X);
                var y = props.GetDouble(NusysConstants.ALIAS_LOCATION_Y_KEY, this.Model.Y);
                SetPosition(x,y);
            }
            if (props.ContainsKey(NusysConstants.ALIAS_SIZE_WIDTH_KEY) || props.ContainsKey(NusysConstants.ALIAS_SIZE_HEIGHT_KEY))
            {
                var width = props.GetDouble(NusysConstants.ALIAS_SIZE_WIDTH_KEY, this.Model.Width);
                var height = props.GetDouble(NusysConstants.ALIAS_SIZE_HEIGHT_KEY, this.Model.Height);
                SetSize(width,height);
            }
            if (props.ContainsKey(NusysConstants.ALIAS_TITLE_VISIBILITY_KEY))
            {
                SetTitleVisiblity(props.GetBool(NusysConstants.ALIAS_TITLE_VISIBILITY_KEY));
            }
            _blockServerInteractionCount--;
        }

        public void UpdateCircleLinks()
        {
            LinksUpdated?.Invoke(this);
        }

        /// <summary>
        /// ILinkable-required method so we can see if we need to draw a link on a specific collection.
        /// This is the same as typing Model.ParentCollectionId
        /// </summary>
        /// <returns></returns>
        public string GetParentCollectionId()
        {
            return Model.ParentCollectionId;
        }

        // User starts editing this node
        public void AddUser(string userId)
        {
            UserAdded?.Invoke(this, userId);
        }

        // User stops editing this node
        public void DropUser(string userId)
        {
            UserDropped?.Invoke(this, userId);
        }

        /// <summary>
        /// export a library element to an HTML page
        /// 
        /// creates an html page from the element's contents (for now, can also take rendered image of node)
        /// 
        /// takes in previous and next node as options for trail export
        /// </summary>
        public async Task ExportToHTML(string previous = null, string next = null)
        {
            /// create the node's HTML file in the HTML folder
            /// if there already is an HTML folder, add the sample file to that folder, otherwise make a new folder
            StorageFolder htmlFolder = null;
            if (await NuSysStorages.NuSysTempFolder.ContainsFolderAsync("HTML"))
            {
                htmlFolder = await NuSysStorages.NuSysTempFolder.GetFolderAsync("HTML");
            }
            else
            {
                htmlFolder = await NuSysStorages.NuSysTempFolder.CreateFolderAsync("HTML");
            }

            ///make file for the element
            var nodeFile = await htmlFolder.CreateFileAsync(Id + ".html", CreationCollisionOption.ReplaceExisting);

            ///create the css file if it does not already exist
            if (!await htmlFolder.ContainsFileAsync("node_template.css"))
            {
                var cssFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Themes/node_template.css"));
                string css = await FileIO.ReadTextAsync(cssFile);
                var newCssFile = await htmlFolder.CreateFileAsync("node_template.css", CreationCollisionOption.ReplaceExisting);
                await FileIO.WriteTextAsync(newCssFile, css);
            }

            ///copy template for element. 
            var type = LibraryElementModel.Type.ToString();
            type = type.ToLower();
            var template = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Themes/" + type + "_node_template.html"));

            ///replace info for file - essentially string replaceing 
            string text = await FileIO.ReadTextAsync(template);

            ///title
            text = text.Replace("[[title]]", LibraryElementController.Title);

            ///if pdf, replace the data in a specific way to get div to scroll w/ images
            if (LibraryElementModel.Type == NusysConstants.ElementType.PDF)
            {
                string htmlImages = "";
                foreach (var page in ((PdfContentDataModel)(LibraryElementController.ContentDataController).ContentDataModel).PageUrls)
                {
                    htmlImages += "<img src=\"" + page + "\">" + "\n";
                }
                text = text.Replace("[[data]]", htmlImages);
            }
            else
            {
                ///else replace with content data model data
                text = text.Replace("[[data]]", LibraryElementController.ContentDataController.ContentDataModel.Data);
            }

            ///creator
            text = text.Replace("[[creator]]", SessionController.Instance.NuSysNetworkSession.GetDisplayNameFromUserId(LibraryElementModel.Creator));

            ///timestamp
            text = text.Replace("[[timestamp]]", LibraryElementModel.LastEditedTimestamp);

            ///if there are keywords, replace them
            if (LibraryElementModel.Keywords != null)
            {
                var tags = LibraryElementModel.Keywords.ToList();
                string tagtext = string.Join(", ", tags.Select(tag => string.Join(", ", tag.Text)));
                text = text.Replace("[[tags]]", tagtext);
            }
            else
            {
                text = text.Replace("[[tags]]", "None");
            }

            ///replace metadata
            ///first, turn metadata list into a string that puts new line characters at end of each key value pair
            string metadataString = "";
            foreach (var metadata in LibraryElementModel.Metadata ?? new ConcurrentDictionary<string, MetadataEntry>())
            {
                metadataString += metadata.Value.GetMetadataAsString() + "<br>";
            }
            text = text.Replace("[[metadata]]", metadataString);

            ///set links for previous and next buttons
            if (previous != null)
            {
                text = text.Replace("[[previous]]", previous + ".html");
            }
            else
            {
                text = text.Replace("[[previous]]", "");
            }

            if (next != null)
            {
                text = text.Replace("[[next]]", next + ".html");
            }
            else
            {
                text = text.Replace("[[next]]", "");
            }

            ///update file with replaced text
            await FileIO.WriteTextAsync(nodeFile, text);
        }
    }
}
