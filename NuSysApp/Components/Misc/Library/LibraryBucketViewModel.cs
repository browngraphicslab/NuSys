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
        public delegate void NewContentsEventHandler(ICollection<NodeContentModel> elements);
        public event NewContentsEventHandler OnNewContents;

        public delegate void NewElementAvailableEventHandler(NodeContentModel element);
        public event NewElementAvailableEventHandler OnNewElementAvailable;

        private double _width, _height;

        public LibraryBucketViewModel()
        {
            SessionController.Instance.ContentController.OnNewContent += FireNewContentAvailable;
        }

     
     
        public async Task InitializeLibrary()
        {
            UITask.Run(delegate {
                OnNewContents?.Invoke(SessionController.Instance.ContentController.Values);
            });
        }

        private void FireNewContentAvailable(NodeContentModel content)
        {
            OnNewElementAvailable?.Invoke(content);
        }
        public void ListViewBase_OnDragItemsStarting(object sender, DragItemsStartingEventArgs e)
        {
            List<NodeContentModel> elements = new List<NodeContentModel>();
            foreach (var element in e.Items)
            {
                var id = ((NodeContentModel)element).Id;
                elements.Add((NodeContentModel)element);
                if (!SessionController.Instance.ContentController.ContainsAndLoaded(id))
                {
                    Task.Run(async delegate
                    {
                        SessionController.Instance.NuSysNetworkSession.FetchContent(id);
                    });
                }
            }
            e.Data.OperationCompleted += DataOnOperationCompleted;
            e.Data.Properties.Add("NodeContentModel", elements);
            var title = ((NodeContentModel)e.Items[0]).Title ?? "";
            var type = ((NodeContentModel)e.Items[0]).Type.ToString();
            e.Data.SetText(type + "  :  " + title);
            e.Cancel = false;
        }
        private void DataOnOperationCompleted(DataPackage sender, OperationCompletedEventArgs args)
        {

            UITask.Run(async delegate
            {
                var ids = (List<NodeContentModel>)sender.Properties["NodeContentModel"];

                var width = SessionController.Instance.SessionView.ActualWidth;
                var height = SessionController.Instance.SessionView.ActualHeight;
                var centerpoint = SessionController.Instance.ActiveFreeFormViewer.CompositeTransform.Inverse.TransformPoint(new Point(width / 2, height / 2));
                foreach (var element in ids)
                {
                    if (element.Type != ElementType.Collection)
                    {
                        await Task.Run(async delegate { 
                        Message m = new Message();
                        m["contentId"] = element.Id;
                        m["x"] = centerpoint.X - 200;
                        m["y"] = centerpoint.Y - 200;
                        m["width"] = 400;
                        m["height"] = 400;
                        m["nodeType"] = element.Type.ToString();
                        m["creator"] = SessionController.Instance.ActiveFreeFormViewer.Id;
                        m["creatorContentID"] = SessionController.Instance.ActiveFreeFormViewer.ContentId;

                            await
                                SessionController.Instance.NuSysNetworkSession.ExecuteRequest(
                                    new NewElementRequest(m));
                        });
                    }
                    else
                    {
                        await Task.Run(async delegate
                        {
                            await StaticServerCalls.PutCollectionInstanceOnMainCollection(centerpoint.X, centerpoint.Y,
                                element.Id);
                        });
                    }
                }
            });
        }

        public void AddNewElement(NodeContentModel element)
        {
            OnNewElementAvailable?.Invoke(element);
        }

        public void GridViewDragStarting(object sender, DragStartingEventArgs e)
        {
            //e.Data.Properties.
        }
    }
}
