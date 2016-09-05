﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace NuSysApp2
{
    public class MetadataToolViewModel : ToolViewModel
    {
        public MetadataToolViewModel(ToolController toolController) : base(toolController)
        {
            AllMetadataDictionary = new Dictionary<string, HashSet<string>>();

        }

        public Tuple<string, HashSet<string>> Selection { get { return (_controller as MetadataToolController).MetadataToolModel.Selection; } set { (_controller as MetadataToolController).SetSelection(value); } }

        public ToolModel.ToolFilterTypeTitle Filter { get { return (_controller as MetadataToolController).MetadataToolModel.Filter; } set { (_controller as MetadataToolController).SetFilter(value); } }


        public Dictionary<string, HashSet<string>> AllMetadataDictionary { get; set; }


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
                else if (Selection.Item2 != null && !Enumerable.Intersect(AllMetadataDictionary[Selection.Item1], Selection.Item2).Any() || Selection.Item2 == null)
                {
                    Selection = new Tuple<string, HashSet<string>>(Selection.Item1, new HashSet<string>());
                }
                else if (Selection.Item2 != null && Enumerable.Intersect(AllMetadataDictionary[Selection.Item1], Selection.Item2).Any())
                {
                    foreach (var item in new List<string>(Selection.Item2))
                    {
                        if (!AllMetadataDictionary[Selection.Item1].Contains(item))
                        {
                            Selection.Item2.Remove(item);
                        }
                    }
                    Selection = Selection;
                }
            }
        }

    }
}