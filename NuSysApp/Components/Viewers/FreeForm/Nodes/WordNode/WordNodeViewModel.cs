using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using LdaLibrary;
using NusysIntermediate;

namespace NuSysApp
{
    public class WordNodeViewModel : PdfNodeViewModel
    {
        public bool IsLocked;
        public WordNodeViewModel(ElementController controller) : base(controller)
        {
            Color = new SolidColorBrush(Windows.UI.Color.FromArgb(175, 100, 175, 255));
            Debug.Assert(controller?.LibraryElementController is WordNodeLibraryElementController);
            var wnlec = controller?.LibraryElementController as WordNodeLibraryElementController;
            wnlec.Locked += LibraryElementController_Locked;
            wnlec.UnLocked += LibraryElementController_UnLocked;
            controller.LibraryElementController.ContentDataController.ContentDataUpdated += ChangeContent;
        }
        private void ChangeContent(object source, string contentData)
        {
            Task.Run(async delegate {
                await UITask.Run(async delegate { await Goto(CurrentPageNumber); });
            });
        }
        private void LibraryElementController_UnLocked(object sender)
        {
            IsLocked = false;
        }

        private void LibraryElementController_Locked(object sender, NetworkUser user)
        {
            IsLocked = true;
        }

        public override void Dispose()
        {
            var wnlec = Controller?.LibraryElementController as WordNodeLibraryElementController;
            if (wnlec != null)
            {
                wnlec.Locked -= LibraryElementController_Locked;
                wnlec.UnLocked -= LibraryElementController_UnLocked;
            }
            if (Controller?.LibraryElementController?.ContentDataController != null)
            {
                Controller.LibraryElementController.ContentDataController.ContentDataUpdated -= ChangeContent;
            } 
            base.Dispose();
        }

        public async override Task Init()
        {
            if (!Controller.LibraryElementController.ContentLoaded)
            {
                await Controller.LibraryElementController.LoadContentDataModelAsync();
            }
            await DisplayPdf();

        }

        private async Task DisplayPdf()
        {
            await Goto(CurrentPageNumber);
            SetSize(Width, Height);
        }
    }
}