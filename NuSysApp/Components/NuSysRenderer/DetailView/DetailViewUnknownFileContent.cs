using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.System;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Microsoft.Graphics.Canvas;
using NusysIntermediate;

namespace NuSysApp
{
    public class DetailViewUnknownFileContent : RectangleUIElement
    {
        public ButtonUIElement _launchButton;
        private LibraryElementController _controller;
        public DetailViewUnknownFileContent(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator, LibraryElementController controller) : base(parent, resourceCreator)
        {
            Debug.Assert(controller != null);
            _launchButton = new RectangleButtonUIElement(this,resourceCreator)
            {
                ButtonText = "Launch",
                Height = 100,
                Width = 250,
            };
            _launchButton.Tapped += LaunchButtonOnTapped;
            AddChild(_launchButton);
            _controller = controller;
        }

        private async void LaunchButtonOnTapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            var args = new GetFileBytesRequestArgs()
            {
                ContentId = _controller.ContentDataController.ContentDataModel.ContentId
            };
            var request =  new GetFileBytesRequest(args, new CallbackArgs<CallbackRequest<GetFileBytesRequestArgs, GetFileBytesRequestReturnArgs>>()
            {
                SuccessFunction = SuccessFunction,
                FailureFunction = FailureFunction
            });
            request.Execute();
        }

        private bool FailureFunction(CallbackRequest<GetFileBytesRequestArgs, GetFileBytesRequestReturnArgs> callbackRequest)
        {
            Debug.Fail("Shouldn't be here");
            return true;
        }

        private bool SuccessFunction(CallbackRequest<GetFileBytesRequestArgs, GetFileBytesRequestReturnArgs> callbackRequest)
        {
            var request = callbackRequest as GetFileBytesRequest;
            var bytes = request.GetRetunedBytes();

            var fileExtension = _controller.GetMetadata("file_extension");
            if (fileExtension?.Any() != true)
            {
                return false;
            }
            var path = _controller.LibraryElementModel.ContentDataModelId +fileExtension.First();
            var folder = ApplicationData.Current.LocalFolder;
            var fullPath = folder.Path + "\\" + path;

            UITask.Run(async delegate
            {
                await Task.Run(async delegate
                {
                    ApplicationData.Current.LocalFolder.CreateFileAsync(path, CreationCollisionOption.ReplaceExisting);
                    try
                    {
                        File.WriteAllBytes(fullPath, bytes);
                    }
                    catch (Exception e)
                    {
                        //do nothing
                    }
                });
                var launcherOptions = new LauncherOptions() { UI = { PreferredPlacement = Placement.Right, InvocationPoint = new Point(SessionController.Instance.SessionView.ActualWidth / 2, 0.0) } };
                launcherOptions.TreatAsUntrusted = false;
                launcherOptions.PreferredApplicationDisplayName = "NUSYS";
                launcherOptions.PreferredApplicationPackageFamilyName = "NuSys";
                launcherOptions.DesiredRemainingView = ViewSizePreference.UseHalf;

                await Task.Run(async delegate
                {
                    var storageFile = await StorageFile.GetFileFromPathAsync(fullPath);
                    File.SetAttributes(fullPath, System.IO.FileAttributes.Normal);
                    await Launcher.LaunchFileAsync(storageFile, launcherOptions);
                });
            });

            return true;
        }

        public override void Dispose()
        {
            _launchButton.Tapped -= LaunchButtonOnTapped;
            base.Dispose();
        }

        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            _launchButton.Width = Width/2;
            _launchButton.Height = Height / 2;
            _launchButton.Transform.LocalPosition = new Vector2( Width/4, Height/4);

            base.Update(parentLocalToScreenTransform);
        }
    }
}
