using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class StaticServerCalls
    {
        public static async Task<ElementCollectionController> PutCollectionInstanceOnMainCollection(double x, double y, string contentID, double width = 400, double height = 400, string id = null)
        {
            return await Task.Run(async delegate
            {
                var newId = id ?? SessionController.Instance.GenerateId();

                Message message = new Message();
                message["contentId"] = contentID;
                message["x"] = x;
                message["y"] = y;
                message["width"] = width;
                message["height"] = height;
                message["nodeType"] = ElementType.Collection;
                message["creator"] = SessionController.Instance.ActiveFreeFormViewer.ContentId;
                message["id"] = newId;

                await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(new NewElementRequest(message));

                List<Message> messages = new List<Message>();

                await Task.Run(async delegate
                {
                    messages = await SessionController.Instance.NuSysNetworkSession.GetCollectionAsElementMessages(contentID);
                });

                foreach (var m in messages)
                {
                    if (m.ContainsKey("contentId"))
                    {
                        await SessionController.Instance.NuSysNetworkSession.ExecuteRequestLocally(new NewElementRequest(m));
                        var newNodeContentId = m.GetString("contentId");
                        if (SessionController.Instance.ContentController.Get(newNodeContentId) == null)
                        {
                            Task.Run(async delegate
                            {
                                SessionController.Instance.NuSysNetworkSession.FetchLibraryElementData(newNodeContentId);
                            });
                        }
                    }

                }
                if (SessionController.Instance.IdToControllers.ContainsKey(newId))
                {
                    return (ElementCollectionController)SessionController.Instance.IdToControllers[newId];
                }
                return null;
            });
        }
    }
}
