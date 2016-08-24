using System;
using System.Collections.Generic;
using System.Diagnostics;
using Windows.UI;
using Windows.UI.Xaml.Media;
using NusysIntermediate;

namespace NuSysApp
{
    public class PresentationLinkViewModel : BaseINPC, INuSysDisposable, ISelectable
    {

        // maintain a list of models so that we can use presentation mode
        public static HashSet<PresentationLinkModel> Models = new HashSet<PresentationLinkModel>();

        private readonly ElementController _inElementController;
        private readonly ElementController _outElementController;
        private bool _selected;
        private SolidColorBrush _color;

        private SolidColorBrush _selectedColor = new SolidColorBrush(Colors.YellowGreen);
        private SolidColorBrush _notSelectedColor = new SolidColorBrush(ColorHelper.FromArgb(0xFF,0xDB, 0x97, 0xB3));

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

            // makes sure that this is instantiated with a not selected color
            IsSelected = false;
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

        public async void DeletePresentationLink()
        {
            var request = new DeletePresentationLinkRequest(Model.LinkId);
            await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(request);
            request.DeletePresentationLinkFromLibrary();
        }

        /// <summary>
        /// From the selectable interface, defines the culling points, if null the link is never culled
        /// we should find a series of culling points that follow the bezier link curve
        /// </summary>
        public PointCollection ReferencePoints
        {
            get { return null; }
        }

        /// <summary>
        /// From the ISelectable Interface, determines whether the presentation link has been selected
        /// </summary>
        public bool IsSelected
        {
            get { return _selected; }
            set
            {
                _selected = value;
                // Change the color of the link based on selection
                Color = _selected == true ? _selectedColor : _notSelectedColor;
                RaisePropertyChanged("IsSelected");
            }
        }

        /// <summary>
        /// From the ISelectable interface, probably deprecated
        /// </summary>
        public bool ContainsSelectedLink { get; }

        public SolidColorBrush Color
        {
            get { return _color; }
            set
            {
                _color = value;
                RaisePropertyChanged("Color");
            }
        }
    }
}