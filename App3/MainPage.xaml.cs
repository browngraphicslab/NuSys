using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace App3
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        public List<DataItem> Data { get; set; }
        public int Rows { get; set; }
        public int Columns { get; set; }

        public MainPage()
        {
            Rows = 2;
            Columns = 2;
            Data = new List<DataItem> { new DataItem { X = 0, Y = 0 }, new DataItem { X = 0, Y = 1 }, new DataItem { X = 1, Y = 0 }, new DataItem { X = 1, Y = 1 } };
             this.InitializeComponent();
            xGrid.DataContext = this;

            var inkpoints = new List<InkPoint>();
            inkpoints.Add(new InkPoint(new Point(0, 0), 0.1f));
            inkpoints.Add(new InkPoint(new Point(1, 1), 0.2f));
            inkpoints.Add(new InkPoint(new Point(2, 2), 0.3f));
            var builder = new InkStrokeBuilder();
            var l = new List<InkStroke>();
            l.Add(builder.CreateStrokeFromInkPoints(inkpoints, System.Numerics.Matrix3x2.CreateTranslation( new System.Numerics.Vector2(0,0))));
            var converted = JsonConvert.SerializeObject(l);
            var deserialized = JsonConvert.DeserializeObject<List<string>>(converted);



        }
    }
}
