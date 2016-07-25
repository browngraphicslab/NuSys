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
        ///reloads PropertiesToDisplay List. Also sets the selection based on if ther new PropertiesToDisplay contains previous selection. Invokes properties to display changed.
        /// </summary>
        public override void ReloadPropertiesToDisplay()
        {
            PropertiesToDisplay = new List<string>((_controller as BasicToolController).GetAllProperties().OrderBy(key => !string.IsNullOrEmpty(key) && char.IsNumber(key[0])).ThenBy(key => key));
            InvokePropertiesToDisplayChanged();
            if ((_controller as BasicToolController).BasicToolModel.Selection != null &&
                (_controller as BasicToolController).BasicToolModel.Selected == true &&
                !PropertiesToDisplay.Intersect((_controller as BasicToolController).BasicToolModel.Selection).Any())
            {
                (_controller as BasicToolController).UnSelect();
            }
            else if ((_controller as BasicToolController).BasicToolModel.Selected == true)
            {
                foreach (var item in new List<string>(Selection))
                {
                    if (!PropertiesToDisplay.Contains(item))
                    {
                        Selection.Remove(item);
                    }
                }
                Selection = Selection;
            }
        }
    }
}