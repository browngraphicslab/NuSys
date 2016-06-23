using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace NuSysApp
{
    public class RegionViewModel : BaseINPC
    {
        #region Public variables
        public Sizeable ContainerViewModel;
        public Region Model { get; private set; }


        public delegate void SizeChangedEventHandler(object sender, double width, double height);
        public event SizeChangedEventHandler ContainerSizeChanged;
        #endregion Public variables
        protected LibraryElementController LibraryElementController;
        protected RegionController RegionController;

        public RegionViewModel(Region model, LibraryElementController controller, RegionController regionController, Sizeable sizeable)
        {
            Model = model;
            LibraryElementController = controller;
            ContainerViewModel = sizeable;
            RegionController = regionController;
            regionController.SizeChanged += OnSizeChanged;
            regionController.RegionChanged += OnRegionChanged;
        }

        private void OnRegionChanged(object sender, double height, double width)
        {
            throw new NotImplementedException();
        }

        private void OnSizeChanged(object sender, Point topLeft, Point bottomRight)
        {
            throw new NotImplementedException();
        }

        public void ChangeSize(object sender, double width, double height)
        {
            ContainerSizeChanged?.Invoke(sender,width,height);
        }
    }
}
