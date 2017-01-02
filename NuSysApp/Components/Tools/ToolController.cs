using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyToolkit.Utilities;
using NetTopologySuite.Utilities;
using NusysIntermediate;
using NuSysApp.Tools;
using SharpDX.DirectWrite;
using Wintellect.PowerCollections;

namespace NuSysApp
{
    public abstract class ToolController : ElementController, ToolStartable
    {
        public static Dictionary<string, ToolStartable> ToolControllers = new Dictionary<string, ToolStartable>();
        public delegate void FilterChangedEventHandler(object sender, ToolModel.ToolFilterTypeTitle filter);
        public delegate void IdsToDisplayChangedEventHandler();
        public delegate void NumberOfParentsChangedEventHandler (int numberOfParents);
        public event FilterChangedEventHandler FilterChanged;

        /// <summary>
        /// Fires when its library ids change. Its children tools listen to this event on when to refresh the 
        /// properties displayed
        /// </summary>
        public event EventHandler<HashSet<string>> OutputLibraryIdsChanged;
        public event EventHandler<string> Disposed;
        public event EventHandler<ToolViewModel> FilterTypeAllMetadataChanged;

        /// <summary>
        /// Fires when the properties to display change (e.g. when a parent tool changes selection). View listens to this for when to show the AND/OR box.
        /// </summary>
        public event IdsToDisplayChangedEventHandler IdsToDisplayChanged;
        public event NumberOfParentsChangedEventHandler NumberOfParentsChanged;

        public override Point2d Anchor
        {
            get
            {
                if (GetParentIds().Count() > 1)
                {
                    return new Point2d(Model.X + Model.Width / 2, Model.Y);
                }
                else
                {
                    return new Point2d(Model.X + Model.Width / 2, Model.Y + 20);
                }
            }
        }

        public string ContentId
        {
            get { return null; }
        }

        public ToolModel ToolModel
        {
            get { return Model as ToolModel; }
        }
        public ToolController(ToolModel model):base(model)
        {
            Debug.Assert(model != null);
            ToolControllers.Add(model.Id, this);
            ToolModel.SetOutputLibraryIds(Filter(GetUpdatedDataList()));
            _blockServerInteractionCount++;//never send server updates about tool information
        }
        

        public void SetFilter(ToolModel.ToolFilterTypeTitle filter)
        {
            ToolModel.SetFilter(filter);
            FilterChanged?.Invoke(this, filter);
            FireIdsToDisplayChanged();
            FireOutputLibraryIdsChanged();
        }

        /// <summary>
        /// So that subclasses can fire filter type all metadata changed
        /// </summary>
        /// <param name="vm"></param>
        public void FireFilterTypeAllMetadataChanged(ToolViewModel vm)
        {
            FilterTypeAllMetadataChanged?.Invoke(this, vm);
        }
        
        /// <summary>
        /// Adds a parent to the tool. Listens to the parent's library ids changed event. Refreshes the library ids. Invokes, outputLibraryIdsChanged, parentsLibraryIdsChanged, and numberofParentsChanged.
        /// </summary>
        public void AddParent(ToolStartable parentController)
        {
            if (parentController != null)
            {
                if (ToolModel.ParentIds.Add(parentController.GetID()))
                {
                    var linkModel = new ToolLinkModel();
                    linkModel.InAtomId = this.Id;
                    linkModel.OutAtomId = parentController.GetID();
                    Debug.Assert((parentController as ElementController) != null);
                    var linkController = new ToolLinkController(linkModel, this, parentController as ElementController);
                    var linkViewModel = new ToolLinkViewModelWin2d(linkController);
                    SessionController.Instance.ActiveFreeFormViewer.AddToolLink(linkViewModel);

                    parentController.OutputLibraryIdsChanged += IdsToDiplayChanged;
                    
                    parentController.FilterTypeAllMetadataChanged += ParentController_FilterTypeAllMetadataChanged;
                    ToolModel.SetOutputLibraryIds(Filter(GetUpdatedDataList()));
                    OutputLibraryIdsChanged?.Invoke(this, ToolModel.OutputLibraryIds);
                    IdsToDisplayChanged?.Invoke();
                    parentController.Disposed += OnParentDisposed;
                    NumberOfParentsChanged?.Invoke(ToolModel.ParentIds.Count);
                }
            }
        }

        /// <summary>
        /// Whenever a parent controller changes from all metadata to basic or vice versa, set the new parent
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ParentController_FilterTypeAllMetadataChanged(object sender, ToolViewModel vm)
        {
            AddParent(vm.Controller);
            //var linkModel = new ToolLinkModel();
            //linkModel.InAtomId = vm.Id;
            //linkModel.OutAtomId = Id;
            //var linkController = new ToolLinkController(linkModel, vm.Controller, this);
            //var linkViewModel = new ToolLinkViewModelWin2d(linkController);
            //SessionController.Instance.ActiveFreeFormViewer.AddToolLink(linkViewModel);
        }

        /// <summary>
        /// Sets the Parent Operator (AND/OR). Refreshes library ids and invokes outputLibraryids changed event. Also invokes the idsToDisplay event. 
        /// </summary>
        public void SetParentOperator(ToolModel.ParentOperatorType parentOperator)
        {
            ToolModel.SetParentOperator(parentOperator);
            ToolModel.SetOutputLibraryIds(Filter(GetUpdatedDataList()));
            OutputLibraryIdsChanged?.Invoke(this, ToolModel.OutputLibraryIds);
            IdsToDisplayChanged?.Invoke();
        }

        /// <summary>
        /// Stops listening to deleted parent events, removes from parent IDS, resets output library ids invokes output ids changed, invokes ids to display changed. Called when the the parent gets deleted.
        /// </summary>
        public void OnParentDisposed(object sender, string parentid)
        {
            RemoveParent(ToolControllers[parentid]);
            ToolModel.SetOutputLibraryIds(Filter(GetUpdatedDataList()));
            OutputLibraryIdsChanged?.Invoke(this, ToolModel.OutputLibraryIds);
            IdsToDisplayChanged?.Invoke();
            //ToolControllers[parentid].Disposed -= OnParentDisposed;
        }

        /// <summary>
        /// Removes parent controller from the model that is about to be deleted, and stops listening to all its parents event.
        /// </summary>
        public virtual void RemoveParent(ToolStartable parentController)
        {
            if (ToolModel.RemoveParentId(parentController?.GetID()))
            {
                if (parentController != null)
                {
                    parentController.FilterTypeAllMetadataChanged -= ParentController_FilterTypeAllMetadataChanged;
                    parentController.OutputLibraryIdsChanged -= IdsToDiplayChanged;
                    parentController.Disposed -= OnParentDisposed;
                }
            }
            NumberOfParentsChanged?.Invoke(ToolModel.ParentIds.Count);

        }

        public string GetID()
        {
            return Model?.Id;
        }

        public abstract void UnSelect();

        /// <summary>
        /// Deletes the current node and removes itself from parents's knowledge
        /// </summary>
        public virtual void Dispose()
        {
            foreach(var parentController in new List<ToolStartable>(ToolModel.ParentIds.Select(id => ToolControllers.ContainsKey(id) ? ToolControllers[id] : null)))
            {
                RemoveParent(parentController);
            }
            Disposed?.Invoke(this, GetID());
            ToolControllers.Remove(GetID());
        }

        /// <summary>
        /// Filters a hashset of ids to only include items based on the GetFunc() function defined in sub class controllers.
        /// </summary>
        public IEnumerable<string> Filter(IEnumerable<string> ids)
        {
            if (!ToolModel.Selected)
            {
                return ids;
            }
            return ids.Where(id => GetFunc()(id));
        }

        /// <summary>
        /// So that subclasses can fire the event
        /// </summary>
        public void FireOutputLibraryIdsChanged()
        {
            OutputLibraryIdsChanged?.Invoke(this, ToolModel.OutputLibraryIds);
        }

        /// <summary>
        /// So that subclasses can fire the event
        /// </summary>
        public void FireIdsToDisplayChanged()
        {
            IdsToDisplayChanged?.Invoke();
        }

        /// <summary>
        /// Should return a function that filters whether or not a certain string should be included int output library ids
        /// </summary>
        public abstract Func<string, bool> GetFunc();

        /// <summary>
        ///Gets the creation date of a library element model as a string
        /// </summary>
        protected string GetDate(LibraryElementModel libraryElementModel)
        {
            if (string.IsNullOrEmpty(libraryElementModel?.Timestamp))
            {
                return DateTime.UtcNow.ToStartOfDay().ToString();
            }
            var time = DateTime.Parse(libraryElementModel.Timestamp);
            var date = time.ToStartOfDay().Add(new TimeSpan(0,time.Hour,0,0));
            return date.ToLocalTime().ToString();
        }

        /// <summary>
        ///Gets the last edited date of a library element model as a string
        /// </summary>
        protected string GetLastEditedDate(LibraryElementModel libraryElementModel)
        {
            if (string.IsNullOrEmpty(libraryElementModel?.LastEditedTimestamp))
            {
                return DateTime.UtcNow.ToStartOfDay().ToString();
            }
            return DateTime.Parse(libraryElementModel.LastEditedTimestamp).ToStartOfDay().ToString();
        }

        /// <summary>
        ///Retuns a dictionary of the basic metadata for a libraryelementmodel
        /// </summary>
        public Dictionary<string, Dictionary<string,double>> GetMetadata(string libraryId)
        {
            var controller = SessionController.Instance.ContentController.GetLibraryElementController(libraryId);
            if (controller != null)
            {
                var metadata = (controller?.FullMetadata?.ToDictionary(k=>k.Key,v=> v.Value.Values.Where(item => item != null).ToDictionary( k => k, j => 1.0)) ?? new Dictionary<string, Dictionary<string,double>>());

                if (SessionController.Instance.ContentController.HasAnalysisModel(controller.LibraryElementModel.ContentDataModelId) && ((Model as MetadataToolModel)?.IncludeSuggestedTags ?? false)) { 
                    var analysisController = SessionController.Instance.ContentController.GetAnalysisModel(controller.LibraryElementModel.ContentDataModelId);
                    metadata.Add("Suggested_Keywords", analysisController?.GetSuggestedTagsAsync(false)?.Result.ToDictionary(k => k.Key, v => 2.5 + (double)Math.Log(v.Value)));
                }
                var element = controller.LibraryElementModel;
                Debug.Assert(element != null);
                metadata["Title"] = new Dictionary<string, double>(){ { element.Title,1 } };
                metadata["Type"] = new Dictionary<string, double>() { { element.Type.ToString(), 1 }};
                metadata["Date"] = new Dictionary<string, double>() { { GetDate(element), 1 } };
                metadata["LastEditedDate"] = new Dictionary<string, double>() { { GetLastEditedDate(element), 1} };
                metadata["Creator"] = new Dictionary<string, double>()
                {//map from the UserID hash to the User Dissplay Name
                    { SessionController.Instance.NuSysNetworkSession.UserIdToDisplayNameDictionary.ContainsKey(element.Creator ?? "") ?
                        SessionController.Instance.NuSysNetworkSession.UserIdToDisplayNameDictionary[element.Creator] : element.Creator, 1 }
                };
                return metadata;
            }
            return new Dictionary<string, Dictionary<string, double>>();
        }

        /// <summary>
        ///When the parents output ids change (i.e when its ids to display changed), this function is called to refresh its own output library ids. Also fires ids to display changed to let the view know to refresh the list.
        /// </summary>
        private void IdsToDiplayChanged(object sender, HashSet<string> libraryIds)
        {
            ToolModel.SetOutputLibraryIds(Filter(GetUpdatedDataList()));
            OutputLibraryIdsChanged?.Invoke(this, ToolModel.OutputLibraryIds);
            IdsToDisplayChanged?.Invoke();
        }

        /// <summary>
        ///Gets all the output library ids of each of the parents and creates a hashset of those ids based on the parent operator (AND/OR).  
        /// recursive refresh boolean represents whether this controller should take its parents' cached IDs or if all its parents should update entirely.
        /// </summary>
        public IEnumerable<string> GetUpdatedDataList()
        {
            var controllers = ToolModel.ParentIds.Select(item => ToolControllers.ContainsKey(item) ? ToolControllers[item] : null);
            if (controllers == null || !controllers.Any())
            {
                return SessionController.Instance.ContentController.IdList;
            }

            var first = controllers.First();

            IEnumerable<string> list = first.GetOutputLibraryIds();//get the first parent's list of elements
            foreach (var enumerable in controllers.Skip(1).Select(controller => controller.GetOutputLibraryIds()))
            {
                switch (ToolModel.ParentOperator)
                {
                    case ToolModel.ParentOperatorType.And:
                        list = list.Intersect(enumerable);
                        break;
                    case ToolModel.ParentOperatorType.Or:
                        list = list.Concat(enumerable ?? new HashSet<string>());
                        break;
                }
            }

            return list;
        }

        /// <summary>
        /// This should refresh the entire tool chain. It recursively finds the orphans, reloads its output library ids and fires the event 
        /// signaling that the library ids have changed.
        /// </summary>
        public void RefreshFromTopOfChain()
        {
            if (!ToolModel.ParentIds.Any())
            {
                ToolModel.SetOutputLibraryIds(Filter(GetUpdatedDataList()));
                FireOutputLibraryIdsChanged();
                IdsToDisplayChanged?.Invoke();
            }
            foreach (var parentController in ToolModel.ParentIds.Select(parentId => ToolController.ToolControllers[parentId]))
            {
                parentController.RefreshFromTopOfChain();
            }
        }

        /// <summary>
        /// Returns the list of output library ids
        /// If recursively refresh is true, then we will reload the output library ids starting from the parent.
        /// If it is false, it just returns the output library ids it had before.
        /// </summary>
        /// <param name="recursivelyRefresh"></param>
        /// <returns></returns>
        public HashSet<string> GetOutputLibraryIds()
        {
            return ToolModel.OutputLibraryIds;
        }

        public HashSet<string> GetParentIds()
        {
            return ToolModel.ParentIds;
        }
        
    }
    
}
