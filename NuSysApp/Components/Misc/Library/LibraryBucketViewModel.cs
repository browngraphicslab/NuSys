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
        public delegate void NewContentsEventHandler(ICollection<LibraryElementModel> elements);
        public event NewContentsEventHandler OnNewContents;

        public delegate void NewElementAvailableEventHandler(LibraryElementModel element);
        public event NewElementAvailableEventHandler OnNewElementAvailable;

        public delegate void ElementDeletedEventHandler(LibraryElementModel element);
        public event ElementDeletedEventHandler OnElementDeleted;

        private double _width, _height;

        public LibraryBucketViewModel()
        {
            SessionController.Instance.ContentController.OnNewContent += FireNewContentAvailable;
            SessionController.Instance.ContentController.OnElementDelete += FireElementDeleted;
        }

     
     
        public async Task InitializeLibrary()
        {
            UITask.Run(delegate {
                //OnNewContents?.Invoke(SessionController.Instance.ContentController.Values);
                var values = new List<LibraryElementModel>(SessionController.Instance.ContentController.Values);
                foreach (var v in values)
                {
                    OnNewElementAvailable?.Invoke(v);             
                }
            });
        }

        private void FireNewContentAvailable(LibraryElementModel content)
        {
            OnNewElementAvailable?.Invoke(content);
        }

        private void FireElementDeleted(LibraryElementModel element)
        {
            OnElementDeleted?.Invoke(element);
        }

        public void ListViewBase_OnDragItemsStarting(object sender, DragItemsStartingEventArgs e)
        {
            List<LibraryElementModel> elements = new List<LibraryElementModel>();
            foreach (var element in e.Items)
            {
                var id = ((LibraryElementModel)element).Id;
                elements.Add((LibraryElementModel)element);
                if (!SessionController.Instance.ContentController.ContainsAndLoaded(id))
                {
                    Task.Run(async delegate
                    {
                        SessionController.Instance.NuSysNetworkSession.FetchLibraryElementData(id);
                    });
                }
            }
            e.Data.OperationCompleted += DataOnOperationCompleted;
            e.Data.Properties.Add("LibraryElementModel", elements);
            var title = ((LibraryElementModel)e.Items[0]).Title ?? "";
            var type = ((LibraryElementModel)e.Items[0]).Type.ToString();
            e.Data.SetText(type + "  :  " + title);
            e.Cancel = false;
        }
        private void DataOnOperationCompleted(DataPackage sender, OperationCompletedEventArgs args)
        {

            UITask.Run(async delegate
            {
                var ids = (List<LibraryElementModel>)sender.Properties["LibraryElementModel"];

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
                        m["creator"] = SessionController.Instance.ActiveFreeFormViewer.ContentId;

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
        public void GridViewDragStarting(object sender, DragStartingEventArgs e)
        {
            //e.Data.Properties.
        }
    }
}
