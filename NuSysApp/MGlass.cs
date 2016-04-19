using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace NuSysApp
{    

    public class MGlass
    {

        private Rectangle _glass;
        private CompositeTransform _deltaTransform;
        private TransformGroup _transforms;
        private MatrixTransform _previousTransform;
        private Canvas _grid;
        private double _x;
        private double _y;

        public Rectangle Node
        {
            get { return _glass; }
        }

        public MGlass(Canvas grid)
        {
            _grid = grid;
            _glass = new Rectangle();
            _glass.ManipulationStarted += new ManipulationStartedEventHandler(this.getManipStarted);
            _glass.ManipulationDelta += new ManipulationDeltaEventHandler(this.getManipDelta);
            _glass.ManipulationCompleted += new ManipulationCompletedEventHandler(this.getManipComp);
            
            this.setUpGlass();
            grid.Children.Add(_glass);
        }

        private void getManipComp(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            Debug.WriteLine("(" + _x + ", " + _y + ")");

        }

        private void getManipDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {

            _previousTransform.Matrix = _transforms.Value;

            // Look at the Delta property of the ManipulationDeltaRoutedEventArgs to retrieve
            // the translate X and Y changes

            _deltaTransform.TranslateX = e.Delta.Translation.X;
            _x += e.Delta.Translation.X;
            _deltaTransform.TranslateY = e.Delta.Translation.Y;
            _y += e.Delta.Translation.Y;
        }

        private void getManipStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            Debug.WriteLine("(" + _x + ", " + _y + ")");


        }

        private void setUpGlass()
        {
            _glass.Height = 200;
           
            _glass.Width = 200;
            _glass.Fill = new SolidColorBrush(Colors.Blue);
            _glass.Opacity = .1;
            
          
            
            _glass.VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Center;
            _glass.HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Center;

            //instantiates tranform group and different tranforms, then adds the tranforms to the tranform group
            _transforms = new TransformGroup();
            _previousTransform = new MatrixTransform() { Matrix = Matrix.Identity };
            _deltaTransform = new CompositeTransform();

            _transforms.Children.Add(_previousTransform);
            _transforms.Children.Add(_deltaTransform);

            // Set the render transform on the text node
            _glass.RenderTransform = _transforms;

            _deltaTransform.TranslateX = 400;
            _deltaTransform.TranslateY = 200;
            _x = 400;
            _y = 200;



            // The element will listen to x and y translation manipulation events
            _glass.ManipulationMode =
                ManipulationModes.TranslateX | ManipulationModes.TranslateY;
        }

        public void remove()
        {
            _grid.Children.Remove(_glass);
        }
    
    }
}
