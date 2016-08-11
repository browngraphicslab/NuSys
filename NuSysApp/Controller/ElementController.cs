﻿using System;
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
                _debouncingDictionary = new DebouncingDictionary(model.Id);
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

            _debouncingDictionary.Add("scaleX", sx);
            _debouncingDictionary.Add("scaleY", sy);
        }

        public void Selected(bool selected)
        {
            SelectionChanged?.Invoke(this, selected);
        }

        public virtual void SetSize(double width, double height)
        {
            if (width < 20 || height < 20)
            {
                return;
            }

            Model.Width = width;
            Model.Height = height;
            SizeChanged?.Invoke(this, width, height);
            FireAnchorChanged();
            _debouncingDictionary.Add("width", width);
            _debouncingDictionary.Add("height", height);
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

            _debouncingDictionary.Add("x", x);
            _debouncingDictionary.Add("y", y);
        }

        public void SetAlpha(double alpha)
        {
            Model.Alpha = alpha;

            AlphaChanged?.Invoke(this, alpha);

            _debouncingDictionary.Add("alpha", alpha);
        }

        public void Delete(object sender)
        {
            Deleted?.Invoke(this);
            SessionController.Instance.ActiveFreeFormViewer.DeselectAll();

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

            //delete it locally (may need to check if it was succesful first)
            return request.RemoveNodeLocally();
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

        public Dictionary<string, object> CreateImageDictionary(double x, double y, double height, double width)
        {
            Dictionary<string, object> dic = new Dictionary<string, object>();
            dic.Add("x", x);
            dic.Add("y", x);
            dic.Add("height", x);
            dic.Add("width", x);
            return dic;
        }

        public Dictionary<string, object> CreateMediaDictionary(TimeSpan start, TimeSpan end)
        {
            Dictionary<string, object> dic = new Dictionary<string, object>();
            dic.Add("start", start);
            dic.Add("end", end);
            return dic;
        }

        public Dictionary<string, object> CreateTextDictionary(double x, double y, double height, double width)
        {
            Dictionary<string, object> dic = new Dictionary<string, object>();
            dic.Add("x", x);
            dic.Add("y", x);
            dic.Add("height", x);
            dic.Add("width", x);
            return dic;
        }

        public virtual async Task RequestMoveToCollection(string newCollectionLibraryID, double x = 50000, double y = 50000)
        {

            var newElementArgs = new NewElementRequestArgs();
            newElementArgs.LibraryElementId = Model.LibraryId;
            newElementArgs.Height = 200;//TODO not hard code this shit
            newElementArgs.Width = 200;//TODO not hard code this shit
            newElementArgs.X = x;
            newElementArgs.Y = y;
            newElementArgs.ParentCollectionId = newCollectionLibraryID;
            newElementArgs.Id = Model.Id;

            //delete the old node
            var deleteElementRequest = new DeleteElementRequest(Model.Id);
            await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(deleteElementRequest);

            //remove locally (may want to check if it was successful)
            deleteElementRequest.RemoveNodeLocally();

            //create the new element
            var request = new NewElementRequest(newElementArgs);
            await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(request);

            //add the new element locally
            request.AddReturnedElementToSession();
        }

        public ElementModel Model
        {
            get { return _model; }
        }
        public LibraryElementController LibraryElementController
        {
            get
            {
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
            if (props.ContainsKey("x") || props.ContainsKey("y"))
            {
                //if either "x" or "y" are not found in props, x/y stays the current value stored in Model.X/Y
                var x = props.GetDouble("x", this.Model.X);
                var y = props.GetDouble("y", this.Model.Y);
                Model.X = x;
                Model.Y = y;

                PositionChanged?.Invoke(this, x, y);
                FireAnchorChanged();
            }
            if (props.ContainsKey("width") || props.ContainsKey("height"))
            {
                var width = props.GetDouble("width", this.Model.Width);
                var height = props.GetDouble("height", this.Model.Height);
                SizeChanged?.Invoke(this, width, height);
                FireAnchorChanged();
            }

            if (props.ContainsKey("region"))
            {
                string region = props.Get("region");
                //RegionChanged?.Invoke(this, region);
            }
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
