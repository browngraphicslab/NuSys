using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp2
{
    public class DataGridViewModel
    {

        public ObservableCollection<GridHeader> Header { get; set; } 

        public ObservableCollection<GridRowCell> Data { get; set; } 

        public int NumRows { get; set; }

        public int NumCols { get; set; }

        //public async Task Sort(int propertyIndex)
        //{
        //    List<GridRowCell> ordered = null;
        //    var sorted = new List<GridRowCell>(Data.OrderBy(l => l.))
        //    switch (s.ToLower().Replace(" ", string.Empty))
        //    {
        //        //case "title":
        //        //    ordered = ((ObservableCollection<LibraryElement>)ListView.ItemsSource).OrderBy(l => l.Title);
        //        //    break;
        //        //case "nodetype":
        //        //    ordered = ((ObservableCollection<LibraryElement>)ListView.ItemsSource).OrderBy(l => l.NodeType.ToString());
        //        //    break;
        //        case "title":
        //            ordered = new List<LibraryElementModel>(PageElements.OrderBy(l => ((LibraryElementModel)l).Title));
        //            break;
        //        case "nodetype":
        //            ordered = new List<LibraryElementModel>(PageElements.OrderBy(l => ((LibraryElementModel)l).Type.ToString()));
        //            break;
        //        case "timestamp":
        //            ordered = new List<LibraryElementModel>(PageElements.OrderByDescending(l => ((LibraryElementModel)l).GetTimestampTicks()));
        //            break;
        //        default:
        //            break;
        //    }
        //    if (ordered != null)
        //    {

        //        //  ObservableCollection<LibraryElementModel> newCollection = new ObservableCollection<LibraryElementModel>();
        //        PageElements.Clear();

        //        foreach (var item in ordered)
        //        {
        //            PageElements.Add(item);
        //        }

        //    }
        //}

    }
}
