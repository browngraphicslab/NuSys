using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NuSysApp
{
    public class ChartSlice
    {
        public string Name { get; set; }
        public int Amount { get; set; }
    }
    public class BasicToolViewModel : ToolViewModel
    {
        /// <summary>
        ///This is the list of items to display
        /// </summary>
        public List<string> PropertiesToDisplay { get; set; }

        public HashSet<string> Selection { get {return (_controller as BasicToolController).BasicToolModel.Selection;} set {(_controller as BasicToolController).SetSelection(value); } }

        public ToolModel.ToolFilterTypeTitle Filter { get { return (_controller as BasicToolController).BasicToolModel.Filter; } set { (_controller as BasicToolController).SetFilter(value); } }

        public BasicToolViewModel(BasicToolController toolController) : base(toolController)
        {
            PropertiesToDisplay = new List<string>();
        }

       /// <summary>
       /// Switches from basic tool view to all metadata tool view. Transfers all parents from basic tool view to metadata toolview. Fires events to let 
       /// children know they have a new parent and let the links know to replace the basic tool view with the new metadata tool view. After, it disposes of the 
       /// basic tool.
       /// </summary>
        public void SwitchToAllMetadataTool()
        {
            MetadataToolModel model = new MetadataToolModel();
            MetadataToolController controller = new MetadataToolController(model);
            MetadataToolViewModel viewmodel = new MetadataToolViewModel(controller);
            viewmodel.Filter = ToolModel.ToolFilterTypeTitle.AllMetadata;
            MetadataToolView view = new MetadataToolView(viewmodel, this.X, this.Y);
            foreach (var id in Controller.GetParentIds())
            {
                controller.AddParent(ToolController.ToolControllers[id]);
            }
            var wvm = SessionController.Instance.ActiveFreeFormViewer;
            wvm.AtomViewList.Add(view);

            Controller.FireFilterTypeAllMetadataChanged(viewmodel);
            this.FireReplacedToolLinkAnchorPoint(viewmodel);
            //this.Dispose();
        }

        /// <summary>
        ///reloads PropertiesToDisplay List. Also sets the selection based on if ther new PropertiesToDisplay contains previous selection. Invokes properties to display changed.
        /// </summary>
        public override void ReloadPropertiesToDisplay()
        {
            bool editedSelection = false;
            PropertiesToDisplay = new List<string>((_controller as BasicToolController).GetAllProperties().OrderBy(key => !string.IsNullOrEmpty(key) && char.IsNumber(key[0])).ThenBy(key => key));
            InvokePropertiesToDisplayChanged();
            if ((_controller as BasicToolController).BasicToolModel.Selection != null &&
                (_controller as BasicToolController).BasicToolModel.Selected == true &&
                !PropertiesToDisplay.Intersect((_controller as BasicToolController).BasicToolModel.Selection).Any())
            {
                if (Selection.Any())
                {
                    (_controller as BasicToolController).UnSelect();
                    editedSelection = true;
                }
            }
            else if ((_controller as BasicToolController).BasicToolModel.Selected == true)
            {
                foreach (var item in new List<string>(Selection))
                {
                    if (!PropertiesToDisplay.Contains(item))
                    {
                        Selection.Remove(item);
                        editedSelection = true;
                    }
                }
                if (editedSelection)
                {
                    Selection = Selection;
                }
            }
            if(editedSelection == false)
            {
                InvokePropertiesToDisplayChanged();
                (Controller as BasicToolController).FireSelectionChanged();
            }
        }
    }
}