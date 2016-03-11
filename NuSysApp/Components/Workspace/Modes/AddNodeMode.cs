using NuSysApp.Util;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Foundation;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace NuSysApp
{
    public class AddNodeMode : AbstractWorkspaceViewMode
    {
        readonly NodeType _nodeType;
        private bool _isDragging;
        private PseudoNode _tempNode;
        private Point _startPos;
        private bool _isFixed;

        public AddNodeMode(WorkspaceView view, NodeType nodeType, bool isFixed) : base(view) {
            _nodeType = nodeType;
            _tempNode = new PseudoNode();
            _isFixed = isFixed;
        }
        
        public override async Task Activate()
        {
            _view.IsRightTapEnabled = true;
            _view.ManipulationMode = ManipulationModes.All;

            _view.ManipulationStarted += OnManipulationStarted;
            _view.ManipulationDelta += OnManipulationDelta;
            _view.ManipulationCompleted += OnManipulationCompleted;
            _view.ManipulationInertiaStarting += OnManipulationInertiaStarting;
        }

        private async void OnManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            if (!(e.OriginalSource is WorkspaceView))
            {
             //   e.Handled = true;
                return;
            }
            _tempNode.Width = 1;
            _tempNode.Height = 1;
            _startPos = new Point(e.Position.X, e.Position.Y);
            Canvas.SetLeft(_tempNode, _startPos.X);
            Canvas.SetTop(_tempNode, _startPos.Y);
            SessionController.Instance.SessionView.MainCanvas.Children.Add(_tempNode);         
            _isDragging = true;
         //   e.Handled = true;
        }

        private void OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (_isDragging) {
                var translation = e.Cumulative.Translation;
                if (translation.X > 0)
                    _tempNode.Width = translation.X;
                if (translation.Y > 0)
                    _tempNode.Height = translation.Y;
            }
        //    e.Handled = true;
        }

        private void OnManipulationInertiaStarting(object sender, ManipulationInertiaStartingRoutedEventArgs e)
        {
            e.TranslationBehavior.DesiredDeceleration = 20.0 * 96.0 / Constants.MaxCanvasSize;

       //     e.Handled =  true;
        } 
        
        private async void OnManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            if (_isDragging) { 
                SessionController.Instance.SessionView.MainCanvas.Children.Remove(_tempNode);

                var wvm = (WorkspaceViewModel) _view.DataContext;
                var r = wvm.CompositeTransform.Inverse.TransformBounds(new Rect(0, 0, _tempNode.Width, _tempNode.Height));
                BackgroundExecutionManager.RequestAccessAsync();

                // Unregister all background tasks associated with this app. 
                foreach (var tsk in BackgroundTaskRegistration.AllTasks)
                {
                    Debug.WriteLine(String.Format("name={0}, ID={1}", tsk.Value.Name, tsk.Value.TaskId));
                    tsk.Value.Unregister(true);
                }

               //// ApplicationTrigger trigger = new ApplicationTrigger();
               // var task = await RegisterBackgroundTask("RuntimeComponent1.ExampleBackgroundTask", "ExampleBackgroundTask", trigger);

               // task.Completed += Task_Completed;
                //task.Result.Completed += new BackgroundTaskCompletedEventHandler(OnCompleted);

                //UpdateUI();


                // Register the background task.
                // To keep the memory footprint of the background task as low as possible, is has been implemented in a C++ Windows Runtime Component for Windows Phone.  
                // The memory footprint will be higher if written in C# and will cause out of memory exception on low-cost-tier devices which will terminate the background task.
                /* BackgroundTaskBuilder builder = new BackgroundTaskBuilder();
                 builder.Name = "ExampleBackgroundTask";
                 builder.TaskEntryPoint = "RuntimeComponent1.ExampleBackgroundTask";
                 */

                // The trigger used in this sample is the TimeZoneChange trigger. This is for illustration purposes.
                // In a real scenario, choose the trigger that meets your needs. 
                // Note: There are two ways to start the background task for testing purposes:
                // 1. Change the time zone setting so that the system time changes - this will cause a TimeZoneChange to fire
                // 2. Find the background task "AppTileUpdater" in the LifecycleEvents drop-down on the main toolbar and tap it.

                // time trigger only works in 15 min intervals....WHY CAN'T WE DO THIS IMMEDIATELY

                //ApplicationTrigger trigger = new ApplicationTrigger();
                //builder.SetTrigger(trigger);
                //var task = RegisterBackgroundTask(builder);
                //builder.Register();

                //await task;

              //  var rr = await trigger.RequestAsync();
               

                await AddNode(_view, _startPos, new Size(r.Width, r.Height), _nodeType);
            }
            _isDragging = false;
         //   e.Handled = true;
        }

        private void Task_Completed(BackgroundTaskRegistration sender, BackgroundTaskCompletedEventArgs args)
        {
            // Image i = RuntimeComponent1.ExampleBackgroundTask.Blah();
            Debug.WriteLine("fooooooooooooddddddddd");
            args.CheckResult();
        }

        private void OnCompleted(BackgroundTaskRegistration sender, BackgroundTaskCompletedEventArgs args)
        {
            Debug.WriteLine("test, THE TASK HAS FINISHED!!!!");
          
           
        }



        /*
private void OnCompleted(BackgroundTaskRegistration task, BackgroundTaskCompletedEventArgs args)
{
   UpdateUI();
}
*/
        /*
        public static async Task<BackgroundTaskRegistration> RegisterBackgroundTask(BackgroundTaskBuilder builder)
        {     
            BackgroundTaskRegistration task = builder.Register();
            task.Completed += new BackgroundTaskCompletedEventHandler(OnCompleted);
            return task;
        }*/

        public async Task<BackgroundTaskRegistration> RegisterBackgroundTask(String taskEntryPoint, String name, IBackgroundTrigger trigger)
        {
            
            var builder = new BackgroundTaskBuilder();
            builder.Name = name;
            builder.TaskEntryPoint = taskEntryPoint;
            builder.SetTrigger(trigger);

            BackgroundTaskRegistration task = builder.Register();
            
         
            return task;
        }


        public override async Task Deactivate()
        {
            _view.IsRightTapEnabled = false;
            _view.ManipulationMode = ManipulationModes.None;
            _view.ManipulationStarted -= OnManipulationStarted;
            _view.ManipulationDelta -= OnManipulationDelta;
            _view.ManipulationCompleted -= OnManipulationCompleted;
            _view.ManipulationInertiaStarting -= OnManipulationInertiaStarting;
            _isDragging = false;
        }

        public static void CheckFileType(string fileType)
        {
            if (fileType != "application/pdf" && fileType != "image/tiff" && fileType != "image/jpeg" &&
                fileType != "image/png") //TO-DO: allow other types we support that haven't been added here yet
            {
                throw new Exception("The file format you selected is currently supported.");
            }
        }

        // TODO: this should be refactored!
        private async Task AddNode(WorkspaceView view, Point pos, Size size, NodeType nodeType, object data = null)    {
            var vm = (WorkspaceViewModel)view.DataContext;
            var p = vm.CompositeTransform.Inverse.TransformPoint(pos);

            var dict = new Message();
            Dictionary<string, object> metadata;
            if (nodeType == NodeType.Document || nodeType == NodeType.Word || nodeType == NodeType.Powerpoint || nodeType == NodeType.Image || nodeType == NodeType.PDF ||  nodeType == NodeType.Video)
            {
                var storageFile = await FileManager.PromptUserForFile(Constants.AllFileTypes);
                if (storageFile == null) return;

                var fileType = storageFile.FileType.ToLower();
                dict["title"] = storageFile.DisplayName;


                var token = Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Add(storageFile);

                try
                {
             //       CheckFileType(fileType); TODO readd
                }
                catch (Exception e)
                {
                    Debug.WriteLine("The file format you selected is currently unsupported");
                    return;
                }

                if (Constants.ImageFileTypes.Contains(fileType))
                {
                    nodeType = NodeType.Image;
                    
                    data = Convert.ToBase64String(await MediaUtil.StorageFileToByteArray(storageFile));
                }

                if (Constants.WordFileTypes.Contains(fileType))
                {
                    metadata = new Dictionary<string, object>();
                    metadata["FilePath"] = storageFile.Path;
                    metadata["Token"] = token.Trim();

                    dict["metadata"] = metadata;

                    nodeType = NodeType.Word;

                    //data = File.ReadAllBytes(storageFile.Path);
                }

                if (Constants.PowerpointFileTypes.Contains(fileType))
                {
                    metadata = new Dictionary<string, object>();
                    metadata["FilePath"] = storageFile.Path;
                    metadata["Token"] = token.Trim();

                    dict["metadata"] = metadata;

                    nodeType = NodeType.Powerpoint;

                    //data = File.ReadAllBytes(storageFile.Path);
                }

                if (Constants.PdfFileTypes.Contains(fileType))
                {
                    nodeType = NodeType.PDF;
                    IRandomAccessStream s = await storageFile.OpenReadAsync();

                    byte[] fileBytes = null;
                    using (IRandomAccessStreamWithContentType stream = await storageFile.OpenReadAsync()){
                        fileBytes = new byte[stream.Size];
                        using (DataReader reader = new DataReader(stream)){
                            await reader.LoadAsync((uint)stream.Size);
                            reader.ReadBytes(fileBytes);
                        }
                    }

                    data = Convert.ToBase64String(fileBytes);
                }
                if (Constants.VideoFileTypes.Contains(fileType))
                {
                    nodeType = NodeType.Video;
                    IRandomAccessStream s = await storageFile.OpenReadAsync();

                    byte[] fileBytes = null;
                    using (IRandomAccessStreamWithContentType stream = await storageFile.OpenReadAsync()){
                        fileBytes = new byte[stream.Size];
                        using (DataReader reader = new DataReader(stream)){
                            await reader.LoadAsync((uint)stream.Size);
                            reader.ReadBytes(fileBytes);
                        }
                    }

                    data = Convert.ToBase64String(fileBytes);
                }
                if (Constants.AudioFileTypes.Contains(fileType))
                {
                    nodeType = NodeType.Audio;
                    IRandomAccessStream s = await storageFile.OpenReadAsync();

                    byte[] fileBytes = null;
                    using (IRandomAccessStreamWithContentType stream = await storageFile.OpenReadAsync())
                    {
                        fileBytes = new byte[stream.Size];
                        using (DataReader reader = new DataReader(stream))
                        {
                            await reader.LoadAsync((uint)stream.Size);
                            reader.ReadBytes(fileBytes);
                        }
                    }

                    data = Convert.ToBase64String(fileBytes);
                }
            }
            var contentId = SessionController.Instance.GenerateId();

            metadata = new Dictionary<string, object>();
            metadata["node_creation_date"] = DateTime.Now;
            metadata["node_type"] = nodeType + "Node";

            dict = new Message();
            dict["width"] = size.Width.ToString();
            dict["height"] = size.Height.ToString();
            dict["nodeType"] = nodeType.ToString();
            dict["x"] = p.X;
            dict["y"] = p.Y;
            dict["contentId"] = contentId;
            dict["metadata"] = metadata;
            dict["autoCreate"] = true;
            dict["creators"] = new List<string>() {SessionController.Instance.ActiveWorkspace.Id};

            var request = new NewNodeRequest(dict);
            await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(request);
            await SessionController.Instance.NuSysNetworkSession.ExecuteSystemRequest(new NewContentSystemRequest(contentId, data == null ? "" : data.ToString()), NetworkClient.PacketType.TCP, null, true);

            vm.ClearSelection();
            vm.ClearMultiSelection();

            if (!_isFixed) { 
                SessionController.Instance.SessionView.FloatingMenu.Reset();
            }           
        }
    }
}
