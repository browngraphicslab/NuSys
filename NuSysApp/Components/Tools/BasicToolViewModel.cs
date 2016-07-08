using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using NuSysApp.Components.Viewers.FreeForm;
using NuSysApp.Util;

namespace NuSysApp
{
    public class ChartSlice
    {
        public string Name { get; set; }
        public int Amount { get; set; }
    }
    public class BasicToolViewModel : ToolViewModel
    {

        public BasicToolViewModel(BasicToolController toolController) : base(toolController)
        {
            
            PropertiesToDisplay = new List<string>();
            //PropertiesToDisplayUnique = new ObservableCollection<string>();
            //PropertiesToDisplayPieChart = new ObservableCollection<ChartSlice>();
            
        }
        
        public string Selection { get { return (_controller as BasicToolController).BasicToolModel.Selection; } set { (_controller as BasicToolController).SetSelection(value);} }

        public ToolModel.ToolFilterTypeTitle Filter { get { return (_controller as BasicToolController).BasicToolModel.Filter;}  set { (_controller as BasicToolController).SetFilter(value);} }

        
        public override void ReloadPropertiesToDisplay()
        {
            PropertiesToDisplay = (_controller as BasicToolController).GetAllProperties();
            InvokePropertiesToDisplayChanged();
            if ((_controller as BasicToolController).BasicToolModel.Selection != null &&
                (_controller as BasicToolController).BasicToolModel.Selected == true &&
                !PropertiesToDisplay.Contains((_controller as BasicToolController).BasicToolModel.Selection))
            {
                (_controller as BasicToolController).UnSelect();
                //temp = new ObservableCollection<string>((_controller as BasicToolController).GetAllProperties());
            }
            else if ((_controller as BasicToolController).BasicToolModel.Selected == true)
            {
                Selection = Selection;
            }
            //PieChartDictionary = new Dictionary<string, int>();
            //PropertiesToDisplay.Clear();
            //PropertiesToDisplayUnique.Clear();
            //PropertiesToDisplayPieChart = new ObservableCollection<ChartSlice>();
            //foreach (var item in temp)
            //{
            //    if (item != null)
            //    {
            //        PropertiesToDisplay.Add(item);
            //if (!PieChartDictionary.ContainsKey(item))
            //{
            //    PieChartDictionary.Add(item, 1);
            //    PropertiesToDisplayUnique.Add(item);
            //}
            //else
            //{
            //    PieChartDictionary[item] = PieChartDictionary[item] + 1;
            //}
            //    }
            //}
            //foreach (var item in dic)
            //{
            //    ChartSlice slice = new ChartSlice();
            //    slice.Name = item.Key;
            //    slice.Amount = item.Value;
            //    PropertiesToDisplayPieChart.Add(slice);
            //}

        }


        //public Dictionary<string, int> PieChartDictionary { get; set; }

        public List<string> PropertiesToDisplay { get; set; }

        //public ObservableCollection<string> PropertiesToDisplayUnique { get; set; }

        //public ObservableCollection<ChartSlice> PropertiesToDisplayPieChart { get; set; } 

    }
}