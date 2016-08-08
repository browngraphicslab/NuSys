using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NusysIntermediate;

namespace NuSysApp
{
    public class TextDetailHomeTabViewModel : DetailHomeTabViewModel
    {
        public delegate void TextChangedHandler(object source, string text);
        public event TextChangedHandler TextChanged;

        public LibraryElementController LibraryElementController { get; }
        public TextDetailHomeTabViewModel(LibraryElementController controller) : base(controller)
        {
            LibraryElementController = controller;
            controller.ContentChanged += ContentChanged;
        }

        private void ContentChanged(object source, string data)
        {
            TextChanged?.Invoke(source,data);
        }

        public override void AddRegion(object sender, RegionLibraryElementController libraryElementController)
        {
            //throw new NotImplementedException();
        }

        public override void RemoveRegion(object sender, Region displayedRegion)
        {
            //throw new NotImplementedException();
        }

        public override void SizeChanged(object sender, double width, double height)
        {
            //throw new NotImplementedException();
        }

        public override void SetExistingRegions()
        {

        }

        public override Message GetNewRegionMessage()
        {
            return null;
        }

    }
}
