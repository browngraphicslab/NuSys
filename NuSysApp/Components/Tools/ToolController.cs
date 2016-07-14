using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyToolkit.Utilities;
using NetTopologySuite.Utilities;
using SharpDX.DirectWrite;

namespace NuSysApp
{
    public abstract class ToolController
    {
        public static Dictionary<string, ToolController> ToolControllers = new Dictionary<string, ToolController>();

        public delegate void OutputLibraryIdsChangedEventHandler(object sender, HashSet<string> libraryIds);
        public delegate void LocationChangedEventHandler(object sender, double x, double y);
        public delegate void SizeChangedEventHandler(object sender, double width, double height);
        public delegate void DisposedEventHandler(string parentid);
        public delegate void IdsToDisplayChangedEventHandler();
        public delegate void NumberOfParentsChangedEventHandler (int numberOfParents);

        /// <summary>
        /// Fires when its library ids change. Its children tools listen to this event on when to refresh the 
        /// properties displayed
        /// </summary>
        public event OutputLibraryIdsChangedEventHandler OutputLibraryIdsChanged;
        public event LocationChangedEventHandler LocationChanged;
        public event SizeChangedEventHandler SizeChanged;
        public event DisposedEventHandler Disposed;
        /// <summary>
        /// Fires when the properties to display change (e.g. when a parent tool changes selection). View listens to this for when to show the AND/OR box.
        /// </summary>
        public event IdsToDisplayChangedEventHandler IdsToDisplayChanged;
        public event NumberOfParentsChangedEventHandler NumberOfParentsChanged;




        public ToolModel Model { get;}
        public ToolController(ToolModel model)
        {
            Debug.Assert(model != null);
            Model = model;
            ToolControllers.Add(model.Id, this);
            Model.SetOutputLibraryIds(Filter(GetUpdatedDataList()));

            //CODE TO DELETE Non RMS STUFF
            /*
             foreach (var id in new HashSet<string>(SessionController.Instance.ContentController.IdList))
             {
                 var s = SessionController.Instance.ContentController.GetContent(id);
                 if(s.Creator.ToLower() != "rms" && s.Creator.ToLower() != "rosemary"){
                     Task.Run(async delegate
                     {
                         await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(new DeleteLibraryElementRequest(id));
                     });
                }
             }
             */
            

            //CODE BELOW IS HACKY WAY TO DOWNLOAD ALL THE PDF'S 
            /*
            Task.Run(async delegate
            {
                int i = 0;
                foreach (var id in LibraryElementModel.PDFStrings)
                {
                    Debug.WriteLine((double)i++ / (double)LibraryElementModel.PDFStrings.Count);
                    if (SessionController.Instance.ContentController.GetContent(id) != null &&
                        SessionController.Instance.ContentController.GetContent(id).Type == ElementType.PDF)
                    {
                        await Task.Run(async delegate
                        {
                            await SessionController.Instance.NuSysNetworkSession.FetchLibraryElementData(id);
                            try
                            {
                                var lem = SessionController.Instance.ContentController.GetContent(id);
                                var document = await MediaUtil.DataToPDF(lem.Data);
                                lem.Data = null;
                                string data = "";
                                int numPages = document.PageCount;
                                int currPage = 0;
                                while (currPage < numPages)
                                {
                                    data = data + document.GetAllTexts(currPage);
                                    currPage++;
                                }

                                using (
                                    var stream =
                                        new FileStream(
                                            @"C:\Users\graphics_lab\Documents\junsu_pdfs\" + lem.Title + ".txt",
                                            FileMode.OpenOrCreate,
                                            FileAccess.Write))
                                {
                                    using (var writer = new StreamWriter(stream))
                                    {
                                        writer.Write(data);
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                return;
                            }
                        });

                    }
                }
            });*/
        }

        public void SetSize(double width, double height)
        {
            SizeChanged?.Invoke(this, width, height);
        }

        /// <summary>
        /// Adds a parent to the tool. Listens to the parent's library ids changed event. Refreshes the library ids. Invokes, outputLibraryIdsChanged, parentsLibraryIdsChanged, and numberofParentsChanged.
        /// </summary>
        public void AddParent(ToolController parentController)
        {
            if (parentController != null)
            {
                if (Model.ParentIds.Add(parentController.Model?.Id))
                {
                    parentController.OutputLibraryIdsChanged += IdsToDiplayChanged;
                    Model.SetOutputLibraryIds(Filter(GetUpdatedDataList()));
                    OutputLibraryIdsChanged?.Invoke(this, Model.OutputLibraryIds);
                    IdsToDisplayChanged?.Invoke();
                    parentController.Disposed += OnParentDisposed;
                    NumberOfParentsChanged?.Invoke(Model.ParentIds.Count);
                    
                }
            }
        }

        /// <summary>
        /// Sets the Parent Operator (AND/OR). Refreshes library ids and invokes outputLibraryids changed event. Also invokes the idsToDisplay event. 
        /// </summary>
        public void SetParentOperator(ToolModel.ParentOperatorType parentOperator)
        {
            Model.SetParentOperator(parentOperator);
            Model.SetOutputLibraryIds(Filter(GetUpdatedDataList()));
            OutputLibraryIdsChanged?.Invoke(this, Model.OutputLibraryIds);
            IdsToDisplayChanged?.Invoke();
        }

        /// <summary>
        /// Stops listening to deleted parent events, removes from parent IDS, resets output library ids invokes output ids changed, invokes ids to display changed. Called when the the parent gets deleted.
        /// </summary>
        public void OnParentDisposed(string parentid)
        {
            ToolControllers[parentid].OutputLibraryIdsChanged -= IdsToDiplayChanged;
            Model.ParentIds.Remove(parentid);
            Model.SetOutputLibraryIds(Filter(GetUpdatedDataList()));
            OutputLibraryIdsChanged?.Invoke(this, Model.OutputLibraryIds);
            IdsToDisplayChanged?.Invoke();
            ToolControllers[parentid].Disposed -= OnParentDisposed;

        }

        /// <summary>
        /// Removes parent controller from the model that is about to be deleted, and stops listening to all its parents event.
        /// </summary>
        public virtual void RemoveParent(ToolController parentController)
        {
            if (Model.RemoveParentId(parentController?.Model?.Id))
            {
                if (parentController != null)
                {
                    parentController.OutputLibraryIdsChanged -= IdsToDiplayChanged;
                }
            }
            NumberOfParentsChanged?.Invoke(Model.ParentIds.Count);

        }

        public abstract void UnSelect();

        /// <summary>
        /// Deletes the current node and removes itself from children's knowledge
        /// </summary>
        public virtual void Dispose()
        {
            foreach(var parentController in new List<ToolController>(Model.ParentIds.Select(id => ToolControllers.ContainsKey(id) ? ToolControllers[id] : null)))
            {
                RemoveParent(parentController);
            }
            Disposed?.Invoke(Model.Id);
            ToolControllers.Remove(Model.Id);
        }

        /// <summary>
        /// Filters a hashset of ids to only include items based on the GetFunc() function defined in sub class controllers.
        /// </summary>
        public HashSet<string> Filter(HashSet<string> ids)
        {
            if (!Model.Selected)
            {
                return ids;
            }
            var ret = new HashSet<string>();
            foreach (string id in ids)
            {
                if (GetFunc()(id))
                {
                    ret.Add(id);
                }
            }
            return ret;
        }


        public void FireOutputLibraryIdsChanged()
        {
            OutputLibraryIdsChanged?.Invoke(this, Model.OutputLibraryIds);
        }

        public abstract Func<string, bool> GetFunc();

        /// <summary>
        ///Gets the creation date of a library element model as a string
        /// </summary>
        protected string GetDate(LibraryElementModel libraryElementModel)
        {
            if (libraryElementModel.Timestamp == null)
            {
                return DateTime.UtcNow.ToStartOfDay().ToString();
            }
            return DateTime.Parse(libraryElementModel.Timestamp).ToStartOfDay().ToString();
        }

        /// <summary>
        ///Gets the last edited date of a library element model as a string
        /// </summary>
        protected string GetLastEditedDate(LibraryElementModel libraryElementModel)
        {
            if (libraryElementModel.LastEditedTimestamp == null)
            {
                return DateTime.UtcNow.ToStartOfDay().ToString();
            }
            return DateTime.Parse(libraryElementModel.LastEditedTimestamp).ToStartOfDay().ToString();
        }

        /// <summary>
        ///Retuns a dictionary of the basic metadata for a libraryelementmodel
        /// </summary>
        public Dictionary<string, List<string>> GetMetadata(string libraryId)
        {
            var controller = SessionController.Instance.ContentController.GetLibraryElementController(libraryId);
            if (controller != null)
            {
                var metadata = (controller?.LibraryElementModel?.FullMetadata?.ToDictionary(k=>k.Key,v=>v.Value?.Values ?? new List<string>()) ?? new Dictionary<string, List<string>>());

                var element = controller.LibraryElementModel;
                Debug.Assert(element != null);
                metadata["Title"] = new List<string>(){ element.Title};
                metadata["Type"] = new List<string>() { element.Type.ToString()};
                metadata["Date"] = new List<string>() { GetDate(element)};
                metadata["LastEditedDate"] = new List<string>() { GetLastEditedDate(element) };
                metadata["Creator"] = new List<string>() { element.Creator};
                return metadata;
            }
            return new Dictionary<string, List<string>>();
        }

        /// <summary>
        ///When the parents output ids change (i.e when its ids to display changed), this function is called to refresh its own output library ids. Also fires ids to display changed to let the view know to refresh the list.
        /// </summary>
        private void IdsToDiplayChanged(object sender, HashSet<string> libraryIds)
        {
            Model.SetOutputLibraryIds(Filter(GetUpdatedDataList()));
            OutputLibraryIdsChanged?.Invoke(this, Model.OutputLibraryIds);
            IdsToDisplayChanged?.Invoke();
        }

        /// <summary>
        ///Gets all the output library ids of each of the parents and creates a hashset of those ids based on the parent operator (AND/OR)
        /// </summary>
        public HashSet<string> GetUpdatedDataList()
        {
            var controllers = Model.ParentIds.Select(item => ToolControllers.ContainsKey(item) ? ToolControllers[item] : null);
            if (controllers.Count() == 0)
            {
                return SessionController.Instance.ContentController.IdList;
            }
            IEnumerable<string> list = controllers?.First().Model.OutputLibraryIds;
            foreach (var enumerable in controllers.Select(controller => controller?.Model.OutputLibraryIds))
            {
                switch (Model.ParentOperator)
                {
                    case ToolModel.ParentOperatorType.And:
                        list = list.Intersect(enumerable);
                        break;
                    case ToolModel.ParentOperatorType.Or:
                        list = list.Concat(enumerable ?? new HashSet<string>());
                        break;
                }
            }
           
            return new HashSet<string>(list);
        }
        public void SetLocation(double x, double y)
        {
            LocationChanged?.Invoke(this, x, y);
        }
    }
    
}
