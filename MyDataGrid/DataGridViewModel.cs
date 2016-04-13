using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyDataGrid
{
    public class DataGridViewModel
    {

        public ObservableCollection<HarshHeader> Header { get; set; } 

        public ObservableCollection<GridRowCell> Data { get; set; } 

        public int NumRows { get; set; }

        public int NumCols { get; set; }

    }
}
