using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace NuSysApp
{
    public class LibraryBucketViewModel
    {

        public Dictionary<string, LibraryElement> _elements = new Dictionary<string, LibraryElement>();

        public delegate void NewContentsEventHandler(ICollection<LibraryElement> elements);
        public event NewContentsEventHandler OnNewContents;

        public delegate void NewElementAvailableEventHandler(LibraryElement element);
        public event NewElementAvailableEventHandler OnNewElementAvailable;

        private double _width, _height;

        public LibraryBucketViewModel()
        {
        }

     
     
        public async Task InitializeLibrary()
        {
            Task.Run(async delegate
            {
                var dictionaries = await SessionController.Instance.NuSysNetworkSession.GetAllLibraryElements();
                foreach (var kvp in dictionaries)
                {
                    var id = (string)kvp.Value["id"];
                    var element = new LibraryElement(kvp.Value);
                    if (!_elements.ContainsKey(id))
                    {
                        _elements.Add(id, element);
                    }
                }
                UITask.Run(delegate {
                    OnNewContents?.Invoke(_elements.Values);
                });
            });
        }
        public void ListViewBase_OnDragItemsStarting(object sender, DragItemsStartingEventArgs e)
        {
            List<LibraryElement> elements = new List<LibraryElement>();
            foreach (var element in e.Items)
            {
                var id = ((LibraryElement)element).ContentID;
                elements.Add((LibraryElement)element);
                if (SessionController.Instance.ContentController.Get(id) == null)
                {
                    Task.Run(async delegate
                    {
                        SessionController.Instance.NuSysNetworkSession.FetchContent(id);
                    });
                }
            }
            e.Data.OperationCompleted += DataOnOperationCompleted;
            e.Data.Properties.Add("LibraryElements", elements);
            var title = ((LibraryElement)e.Items[0]).Title ?? "";
            var type = ((LibraryElement)e.Items[0]).ElementType.ToString();
            e.Data.SetText(type + "  :  " + title);
            e.Cancel = false;
        }
        private void DataOnOperationCompleted(DataPackage sender, OperationCompletedEventArgs args)
        {

            UITask.Run(async delegate
            {
                var ids = (List<LibraryElement>)sender.Properties["LibraryElements"];

                var width = SessionController.Instance.SessionView.ActualWidth;
                var height = SessionController.Instance.SessionView.ActualHeight;
                var centerpoint = SessionController.Instance.ActiveFreeFormViewer.CompositeTransform.Inverse.TransformPoint(new Point(width / 2, height / 2));
                foreach (var element in ids)
                {

                    await Task.Run(async delegate
                    {
                        Message message = new Message();
                        message["contentId"] = element.ContentID;
                        message["x"] = centerpoint.X - 200;
                        message["y"] = centerpoint.Y - 200;
                        message["width"] = 400;
                        message["height"] = 400;
                        message["nodeType"] = element.ElementType.ToString();
                        message["creator"] = SessionController.Instance.ActiveFreeFormViewer.Id;
                        message["creatorContentID"] = SessionController.Instance.ActiveFreeFormViewer.ContentId;

                        await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(new NewElementRequest(message));
                    });
                       
                    if (element.ElementType == ElementType.Collection)
                    {
                        List<Message> messages = new List<Message>();
                        await Task.Run(async delegate
                        {
                            messages = await SessionController.Instance.NuSysNetworkSession.GetWorkspaceAsElementMessages(element.ContentID);
                        });

                        foreach (var m in messages)
                        {
                            await Task.Run(async delegate
                            {
                                await SessionController.Instance.NuSysNetworkSession.ExecuteRequestLocally(new NewElementRequest(m));
                            });
                            if (m.ContainsKey("contentId"))
                            {
                                var newNodeContentId = m.GetString("contentId");
                                if (SessionController.Instance.ContentController.Get(newNodeContentId) == null)
                                {
                                    Task.Run(async delegate
                                    {
                                        SessionController.Instance.NuSysNetworkSession.FetchContent(newNodeContentId);
                                    });
                                }
                            }

                        }
                    }
                }
            });
        }

        public void AddNewElement(LibraryElement element)
        {
            _elements.Add(element.ContentID, element);
            OnNewElementAvailable?.Invoke(element);
        }

        public void GridViewDragStarting(object sender, DragStartingEventArgs e)
        {
            //e.Data.Properties.
        }
    }
}
