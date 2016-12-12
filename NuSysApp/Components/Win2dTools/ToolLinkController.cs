using System;
using System.Diagnostics;
using NuSysApp.Tools;

namespace NuSysApp
{
    public class ToolLinkController : INuSysDisposable
    {

        public event EventHandler<Point2d> AnchorChanged;

        public delegate void TitleChangedEventHandler(object source, string title);
        public event LinkController.TitleChangedEventHandler TitleChanged;

        public event EventHandler Disposed;

        //public LibraryElementController LibraryElementController { get; private set; }

        public ElementController InElement { get; private set; }
        public ElementController OutElement { get; private set; }
        public ToolLinkModel Model { get; private set; }

        public string Id
        {
            get
            {
                Debug.Assert(Model != null);
                return Model.Id;
            }
        }
        //public string Title
        //{
        //    get { return LibraryElementController.Title; }
        //}

        public Point2d Anchor
        {
            get
            {
                return new Point2d((InElement.Anchor.X + OutElement.Anchor.X) / 2, (OutElement.Anchor.Y + InElement.Anchor.Y) / 2);
            }
        }

        //public string LibraryElementId
        //{
        //    get
        //    {
        //        Debug.Assert(Model != null);
        //        return Model.LibraryId;
        //    }
        //}

        public ToolLinkController(ToolLinkModel model, ElementController inVm, ElementController outVm)
        {
            Debug.Assert(model != null);
            Debug.Assert(model.InAtomId != null);
            Debug.Assert(model.OutAtomId != null);


            Model = model;
            //Model.SetLibraryId(controller.LibraryElementModel.LibraryElementId);
            //Debug.Assert(model.LibraryId != null);
            //LibraryElementController = controller;

            InElement = inVm;
            OutElement = outVm;

            InElement.AnchorChanged += ChangeAnchor;
            OutElement.AnchorChanged += ChangeAnchor;
            //controller.TitleChanged += ChangeTitle;

            //controller.Disposed += Dispose;

            InElement.Disposed += Dispose;
            OutElement.Disposed += Dispose;

            //SessionController.Instance.LinksController.AddLinkable(this);
        }
        private void ChangeTitle(object sender, string title)
        {
            TitleChanged?.Invoke(this, title);
        }

        private void ChangeAnchor(object source, Point2d anchor)
        {
            AnchorChanged?.Invoke(this, Anchor);
        }

        public void Dispose(object sender, EventArgs eventArgs)
        {
            if (InElement != null)
            {
                InElement.AnchorChanged -= ChangeAnchor;
                InElement.Disposed -= Dispose;
            }
            if (OutElement != null)
            {
                OutElement.AnchorChanged -= ChangeAnchor;
                OutElement.Disposed -= Dispose;
            }
            Disposed?.Invoke(this, EventArgs.Empty);
        }

        public void UpdateCircleLinks()
        {
            //TODO add in circle links?
        }

        public string GetParentCollectionId()
        {
            return SessionController.Instance.ActiveFreeFormViewer.LibraryElementId;
        }

    }
}