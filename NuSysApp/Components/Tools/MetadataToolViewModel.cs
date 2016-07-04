using System;
using System.Collections.Generic;

namespace NuSysApp
{
    public class MetadataToolViewModel : ToolViewModel
    {
        public MetadataToolViewModel(ToolController toolController) : base(toolController)
        {
            AllMetadataDictionary = new Dictionary<string, HashSet<ToolItemTemplate>>();

        }

        public Tuple<string, string> Selection { get { return (_controller as MetadataToolController).MetadataToolModel.Selection; } set { (_controller as MetadataToolController).SetSelection(value); } }

        public ToolModel.ToolFilterTypeTitle Filter { get { return (_controller as MetadataToolController).MetadataToolModel.Filter; } set { (_controller as MetadataToolController).SetFilter(value); } }


        public Dictionary<string, HashSet<ToolItemTemplate>> AllMetadataDictionary { get; set; }


        protected override void ReloadPropertiesToDisplay()
        {

            AllMetadataDictionary = (_controller as MetadataToolController).GetAllMetadata();
            if ((_controller as MetadataToolController).MetadataToolModel.Selection != null && (_controller as MetadataToolController).MetadataToolModel.Selected == true)
            {
                if (!AllMetadataDictionary.ContainsKey((_controller as MetadataToolController).MetadataToolModel.Selection.Item1))
                {

                    (_controller as MetadataToolController).UnSelect();
                    //AllMetadataDictionary = (_controller as MetadataToolController).GetAllMetadata();
                }
                else if (Selection.Item2 != null && !ValueContainedWithinKey(Selection.Item1, Selection.Item2))
                {
                    Selection = new Tuple<string, string>(Selection.Item1, null);
                }
            }
            InvokePropertiesToDisplayChanged();
        }

        private bool ValueContainedWithinKey(string key, string value)
        {
            foreach (ToolItemTemplate item in AllMetadataDictionary[key])
            {
                if (value.Equals(item.Value))
                {
                    return true;
                }
            }
            return false;
        }
    }
}