using System;
using System.Diagnostics;

namespace NuSysApp
{
    public class PresentationLinkViewModel : INuSysDisposable
    {
        private readonly ElementController _inElementController;
        private readonly ElementController _outElementController;

        public EventHandler ControlPointsChanged;
        public event EventHandler Disposed;

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
            Debug.Assert(model?.ElementId1 != null && SessionController.Instance.IdToControllers.ContainsKey(model.ElementId1));
            Debug.Assert(model?.ElementId2 != null && SessionController.Instance.IdToControllers.ContainsKey(model.ElementId2));

            _inElementController = SessionController.Instance.IdToControllers[model.ElementId1];
            _outElementController = SessionController.Instance.IdToControllers[model.ElementId2];

            _inElementController.AnchorChanged += FireControlPointsChanged;
            _outElementController.AnchorChanged += FireControlPointsChanged;

            _inElementController.Disposed += FireDisposed;
            _inElementController.Disposed += FireDisposed;
        }

        private void FireDisposed(object sender, EventArgs eventArgs)
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
    }
}