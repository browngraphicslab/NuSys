using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace NuSysApp
{
    public class PresentationLinkViewModel : INuSysDisposable
    {

        // maintain a list of models so that we can use presentation mode
        public static HashSet<PresentationLinkModel> Models;

        private readonly ElementController _inElementController;
        private readonly ElementController _outElementController;

        public EventHandler ControlPointsChanged;
        public event EventHandler Disposed;
        public PresentationLinkModel Model { get; private set; }
        public Point2d InAnchor
        {
            get
            {
                Debug.Assert(_inElementController != null);
                return _inElementController.Anchor;
            }
        }
        public Point2d OutAnchor
        {
            get
            {
                Debug.Assert(_outElementController != null);
                return _outElementController.Anchor;
            }
        }

        public PresentationLinkViewModel(PresentationLinkModel model) 
        {
            //if you fail these debugs, keep them and simply dont create presentation link instead
            Debug.Assert(model?.InElementId != null && SessionController.Instance.IdToControllers.ContainsKey(model.InElementId));
            Debug.Assert(model?.OutElementId != null && SessionController.Instance.IdToControllers.ContainsKey(model.OutElementId));

            _inElementController = SessionController.Instance.IdToControllers[model.InElementId];
            _outElementController = SessionController.Instance.IdToControllers[model.OutElementId];

            _inElementController.AnchorChanged += FireControlPointsChanged;
            _outElementController.AnchorChanged += FireControlPointsChanged;

            _inElementController.Disposed += FireDisposed;
            _outElementController.Disposed += FireDisposed;
            Model = model;
        }

        public void FireDisposed(object sender, EventArgs eventArgs)
        {
            _inElementController.AnchorChanged -= FireControlPointsChanged;
            _outElementController.AnchorChanged -= FireControlPointsChanged;

            _inElementController.Disposed -= FireDisposed;
            _outElementController.Disposed -= FireDisposed;

            Disposed?.Invoke(this,EventArgs.Empty);
        }

        private void FireControlPointsChanged(object sender, Point2d e)
        {
            ControlPointsChanged?.Invoke(this, EventArgs.Empty);
        }

        public void DeletePresentationLink()
        {
            SessionController.Instance.NuSysNetworkSession.RemovePresentationLink(_inElementController.Model.Id, _outElementController.Model.Id);
            FireDisposed(this, EventArgs.Empty);
        }
    }
}