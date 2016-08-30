using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.ApplicationSettings;
using Windows.UI.Xaml.Controls;
using NusysIntermediate;

namespace NuSysApp
{
    public class ElementController : ILinkable
    {
        private ElementModel _model;
        protected DebouncingDictionary _debouncingDictionary;

        /// <summary>
        /// bool to indicate when we are blocking server calls from being produced by the controller.
        /// Should only be used when updating from the server. 
        /// </summary>
        private bool _blockServerInteraction;

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

        /// <summary>
        /// the event that will be fired when the access type of this element changes. 
        /// The passed access type is the new access setting for theis eelement.
        /// </summary>
        public event EventHandler<NusysConstants.AccessType> AccessChanged;

        public Point2d Anchor
        {
            get
            {
                return new Point2d(Model.X + Model.Width / 2, Model.Y + Model.Height / 2);
            }
        }


        public ElementController(ElementModel model)
        {
            _model = model;

            //   Debug.WriteLine(Model.Title);

            //   LibraryElementModel.SetTitle(Model.Title);

            if (_model != null)
            {
                _debouncingDictionary = new ElementDebouncingDictionary(model.Id);
            }
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
            if (LibraryElementController != null)
            {
                LibraryElementController.Deleted -= Delete;
            }
            Disposed?.Invoke(this, EventArgs.Empty);
        }

        public void SetScale(double sx, double sy)
        {
            Model.ScaleX = sx;
            Model.ScaleY = sy;

            ScaleChanged?.Invoke(this, sx, sy);

            if (!_blockServerInteraction)
            {
                _debouncingDictionary.Add("scaleX", sx);
                _debouncingDictionary.Add("scaleY", sy);
            }
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
            if (width < Constants.MinNodeSize || height < Constants.MinNodeSize)
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

        private void FireAnchorChanged()
        {
            AnchorChanged?.Invoke(this, Anchor);
        }

        public void SetPosition(double x, double y)
        {
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

        /// <summary>
        /// call this method to change the access type of this controller's elementmodel. 
        /// This method will fire an event so all listeners are notified of the new access type for this element
        /// </summary>
        /// <param name="newAccessType"></param>
        public void SetAccessType(NusysConstants.AccessType newAccessType)
        {
            Model.AccessType = newAccessType;

            //fire the event so all listener will know of the new access type
            AccessChanged?.Invoke(this, newAccessType);

            //update the servre and notify other clients
            _debouncingDictionary.Add(NusysConstants.ALIAS_ACCESS_KEY, newAccessType.ToString());
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
        public virtual async Task RequestMoveToCollection(string newCollectionLibraryID, double x=50000, double y=50000)
        {
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
                Debug.Fail("request failed");
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
                Debug.Assert(Model.LibraryId != null);
                return SessionController.Instance.ContentController.GetLibraryElementController(Model.LibraryId);
            }
        }
        public LibraryElementModel LibraryElementModel
        {
            get
            {
                Debug.Assert(LibraryElementController != null);
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

        public string ContentId
        {
            get
            {
                Debug.Assert(Model != null);
                return Model.LibraryId;
            }
        }

        public virtual async Task UnPack(Message props)
        {
            _blockServerInteraction = true;
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
            _blockServerInteraction = false;
        }

        public void UpdateCircleLinks()
        {
            LinksUpdated?.Invoke(this);
        }

        public string GetParentCollectionId()
        {
            return Model.ParentCollectionId;
        }
    }
}
