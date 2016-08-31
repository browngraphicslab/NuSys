using System;
using System.Collections.Generic;
using System.Linq;

namespace NuSysApp
{
    public class MetadataToolViewModel : ToolViewModel
    {
        public Tuple<string, HashSet<string>> Selection { get { return (_controller as MetadataToolController).MetadataToolModel.Selection; } set { (_controller as MetadataToolController).SetSelection(value); } }

        public ToolModel.ToolFilterTypeTitle Filter { get { return (_controller as MetadataToolController).MetadataToolModel.Filter; } set { (_controller as MetadataToolController).SetFilter(value); } }

        /// <summary>
        ///This is the dictionary of items to display
        /// </summary>
        public Dictionary<string, Dictionary<string, int>> AllMetadataDictionary { get; set; }

        public MetadataToolViewModel(ToolController toolController) : base(toolController)
        {
            AllMetadataDictionary = new Dictionary<string, Dictionary<string, int>>();

        }

        /// <summary>
        ///reloads PropertiesToDisplay List. Also sets the selection based on if ther new AllMetadataDictionary contains previous selection. Invokes properties to display changed.
        /// </summary>
        public override void ReloadPropertiesToDisplay()
        {
            AllMetadataDictionary = (_controller as MetadataToolController).GetAllMetadata();
            InvokePropertiesToDisplayChanged();
            if ((_controller as MetadataToolController).MetadataToolModel.Selection != null && (_controller as MetadataToolController).MetadataToolModel.Selected == true)
            {
                if (!AllMetadataDictionary.ContainsKey((_controller as MetadataToolController).MetadataToolModel.Selection.Item1))
                {
                    (_controller as MetadataToolController).UnSelect();
                }
                else if (Selection.Item2 != null && !Enumerable.Intersect(AllMetadataDictionary[Selection.Item1].Keys, Selection.Item2).Any() || Selection.Item2 == null)
                {
                    Selection = new Tuple<string, HashSet<string>>(Selection.Item1, new HashSet<string>());
                }
                else if (Selection.Item2 != null && Enumerable.Intersect(AllMetadataDictionary[Selection.Item1].Keys, Selection.Item2).Any())
                {
                    foreach (var item in new List<string>(Selection.Item2))
                    {
                        if (!AllMetadataDictionary[Selection.Item1].Keys.Contains(item))
                        {
                            Selection.Item2.Remove(item);
                        }
                    }
                    Selection = Selection;
                }
            }
        }

        public void SwitchToBasicTool(ToolModel.ToolFilterTypeTitle filter)
        {
            BasicToolModel model = new BasicToolModel();
            BasicToolController controller = new BasicToolController(model);
            BasicToolViewModel viewmodel = new BasicToolViewModel(controller);
            viewmodel.Filter = filter;
            BaseToolView view = new BaseToolView(viewmodel, this.X, this.Y);
            foreach (var id in Controller.GetParentIds())
            {
                controller.AddParent(ToolController.ToolControllers[id]);
            }
            var wvm = SessionController.Instance.ActiveFreeFormViewer;
            wvm.AtomViewList.Add(view);

            Controller.FireFilterTypeAllMetadataChanged(viewmodel);
            this.FireReplacedToolLinkAnchorPoint(viewmodel);
        }

    }
}