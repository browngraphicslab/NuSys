using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Windows.UI.Xaml;
using NusysIntermediate;

namespace NuSysApp.Tools
{
    public abstract class ToolStartable:ElementController
    {

        public ToolStartable(ElementModel model) : base(model)
        {
            
        }
        /// <summary>
        /// Returns the list of output library ids
        /// If recursively refresh is true, then we will reload the output library ids starting from the parent.
        /// If it is false, it just returns the output library ids it had before.
        /// </summary>
        public abstract HashSet<string> GetOutputLibraryIds();

        /// <summary>
        /// the list of all the library ids from all of the startable's parents, if any.
        /// </summary>
        /// <returns></returns>
        public abstract IEnumerable<string> GetUpdatedDataList();

        public event EventHandler<HashSet<string>> OutputLibraryIdsChanged;

        protected void FireOutputLibraryIdsChanged(HashSet<string> newOutputIds)
        {
            OutputLibraryIdsChanged?.Invoke(this, newOutputIds);
        }
        //public event EventHandler<string> Disposed;

        /// <summary>
        /// This is listend to by all children so that when you change from metadata to basic or basic to metadata,
        /// The children can create a new link to the new tool and re add the new tool as a parent (since the old tool was disposed of). (The reason why the link can't listen to this is the link takes in an element controller.) It
        /// takes an element controller beause it already has all the anchor functionality.
        /// </summary>
        public event EventHandler<ToolViewModel> FilterTypeAllMetadataChanged;

        /// <summary>
        /// So that subclasses can fire filter type all metadata changed
        /// </summary>
        /// <param name="vm"></param>
        public void FireFilterTypeAllMetadataChanged(ToolViewModel vm)
        {
            FilterTypeAllMetadataChanged?.Invoke(this, vm);
        }


        /// <summary>
        /// Returns the toolStartable id
        /// </summary>
        public abstract string GetID();

        /// <summary>
        /// Returns the ids of all its parents
        /// </summary>
        public abstract HashSet<string> GetParentIds();

        /// <summary>
        /// This should refresh the entire tool chain no matter which tool controller calls it
        /// </summary>
        public abstract void RefreshFromTopOfChain();

        /// <summary>
        /// Will either add this tool as a parent if dropped on top of an existing tool, or create a brand new tool filter chooser view.
        /// </summary>
        public void FilterIconDropped(float x, float y)
        {
            var dragDestinationController = (SessionController.Instance.SessionView.FreeFormViewer.RenderEngine.GetRenderItemAt(new Vector2(x, y), null, 2) as ToolWindow)?.Vm?.Controller; //maybe replace null w render engine.root

            if (dragDestinationController != null)
            {
                AddFilterToExistingTool(dragDestinationController, null); //FIX THIS SHIT
            }
            else
            {
                var canvasCoordinate = SessionController.Instance.SessionView.FreeFormViewer.RenderEngine.ScreenPointerToCollectionPoint(new Vector2(x, y), SessionController.Instance.SessionView.FreeFormViewer.CurrentCollection);
                AddNewFilterTool(canvasCoordinate.X, canvasCoordinate.Y);
            }
        }

        /// <summary>
        ///creates new filter tool at specified location
        /// </summary>
        public void AddNewFilterTool(double x, double y)
        {
            UITask.Run(() =>
            {
                BasicToolModel model = new BasicToolModel();
                BasicToolController controller = new BasicToolController(model);
                BasicToolViewModel viewmodel = new BasicToolViewModel(controller);
                viewmodel.Filter = ToolModel.ToolFilterTypeTitle.Title;
                viewmodel.Controller.AddParent(this);
                controller.SetPosition(x, y);
                controller.SetSize(500, 500);
                SessionController.Instance.ActiveFreeFormViewer.AddTool(viewmodel);
            });
        }


        /// <summary>
        ///Adds tool as parent to existing tool. 
        /// </summary>
        public void AddFilterToExistingTool(ToolController toolController, FreeFormViewerViewModel wvm)
        {
            if (toolController != null && toolController != this)
            {
                if (!CreatesLoop(toolController))
                {
                    toolController.AddParent(this);
                }
            }
        }

        /// <summary>
        /// Returns a boolean representing if creating a tool chain from this tool to the passed in tool will create a loop
        /// </summary>
        public bool CreatesLoop(ToolController toolController)
        {
            bool createsLoop = false;
            var controllers = new List<ToolStartable>(GetParentIds().Select(item => ToolController.ToolControllers.ContainsKey(item) ? ToolController.ToolControllers[item] : null));

            while (controllers != null && controllers.Count != 0)
            {
                if (controllers.Contains(toolController))
                {
                    createsLoop = true;
                    break;
                }
                var tempControllers = new List<ToolStartable>();
                foreach (var controller in controllers)
                {
                    tempControllers = new List<ToolStartable>(tempControllers.Union(new List<ToolStartable>(
                            controller.GetParentIds().Select(
                                item =>
                                    ToolController.ToolControllers.ContainsKey(item)
                                        ? ToolController.ToolControllers[item]
                                        : null))));
                }
                controllers = tempControllers;
            }
            return createsLoop;
        }

    }
}