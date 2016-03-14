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

        public Dictionary<string, NodeContentModel> _elements = new Dictionary<string, NodeContentModel>();

        public delegate void NewContentsEventHandler(ICollection<NodeContentModel> elements);
        public event NewContentsEventHandler OnNewContents;

        public delegate void NewElementAvailableEventHandler(NodeContentModel element);
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
                    //var element = new NodeContentModel(kvp.Value);

                    var dict = kvp.Value;   

                    var contentId = (string)dict["id"];
                    string title = null;
                    ElementType type = ElementType.Document;

                    if (dict.ContainsKey("title"))
                    {
                        title = (string)dict["title"]; // title
                    }
                    if (dict.ContainsKey("type"))
                    {
                        type = (ElementType)Enum.Parse(typeof(ElementType), (string)dict["type"], true);
                    }

                    var element = new NodeContentModel((string)dict["data"], id,type,title);

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
            List<NodeContentModel> elements = new List<NodeContentModel>();
            foreach (var element in e.Items)
            {
                var id = ((NodeContentModel)element).ContentID;
                elements.Add((NodeContentModel)element);
                if (SessionController.Instance.ContentController.Get(id) == null)
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

            UITask.Run(delegate
            {
                var ids = (List<NodeContentModel>)sender.Properties["NodeContentModel"];

                var width = SessionController.Instance.SessionView.ActualWidth;
                var height = SessionController.Instance.SessionView.ActualHeight;
                var centerpoint =
                    SessionController.Instance.ActiveFreeFormViewer.CompositeTransform.Inverse.TransformPoint(
                        new Point(width / 2, height / 2));
                Task.Run(delegate
                {
                    foreach (var element in ids)
                    {
                        Message m = new Message();
                        m["contentId"] = element.ContentID;
                        m["x"] = centerpoint.X - 200;
                        m["y"] = centerpoint.Y - 200;
                        m["width"] = 400;
                        m["height"] = 400;
                        m["nodeType"] = element.Type.ToString();
                        m["creator"] = SessionController.Instance.ActiveFreeFormViewer.Id;
                        m["creatorContentID"] = SessionController.Instance.ActiveFreeFormViewer.ContentId;

                        SessionController.Instance.NuSysNetworkSession.ExecuteRequest(new NewElementRequest(m));
                    }
                });
            });
        }

        public void AddNewElement(NodeContentModel element)
        {
            _elements.Add(element.Title, element);
            OnNewElementAvailable?.Invoke(element);
        }

        public void GridViewDragStarting(object sender, DragStartingEventArgs e)
        {
            //e.Data.Properties.
        }
    }
}
