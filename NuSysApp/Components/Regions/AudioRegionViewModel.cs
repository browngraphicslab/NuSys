using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Shapes;
using NuSysApp.Nodes.AudioNode;

namespace NuSysApp
{
    public class AudioRegionViewModel : RegionViewModel 
    {

        public event PropertyChangedEventHandler PropertyChanged;
        public delegate void DoubleChanged(object sender, double e);
        public event DoubleChanged WidthChanged;
        public event DoubleChanged Bound1Changed;
        public event DoubleChanged Bound2Changed;

        public double LeftHandleX
        {
            get
            {
                var model = Model as AudioRegionModel;
                return AudioWrapper.ActualWidth * model.Start;
            } 
        }

        public double RightHandleX
        {
            get
            {
                var model = Model as AudioRegionModel;
                return AudioWrapper.ActualWidth * model.End;
            } 
        }

        public double RegionWidth
        {
            get
            {
                var model = Model as AudioRegionModel;
                return (model.End - model.Start) * AudioWrapper.ActualWidth;
            }
        }

        private string _name;

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                //Model.Name = _name;
                RaisePropertyChanged("Name");
            }
        }
        //needed for xaml (setting text box to read only)
        public bool IsReadOnly { get { return !Editable; } }

        public AudioWrapper AudioWrapper { get; set; }

        public AudioRegionViewModel(AudioRegionModel model, AudioRegionLibraryElementController regionLibraryElementController, AudioWrapper wrapper) : base(model, regionLibraryElementController, null)
        {
            regionLibraryElementController.TimeChanged += RegionController_TimeChanged;
            regionLibraryElementController.TitleChanged += RegionController_TitleChanged;
            Name = Model.Title;

            AudioWrapper = wrapper;
            AudioWrapper.SizeChanged += AudioWrapper_SizeChanged;
        }

        private void AudioWrapper_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            RaisePropertyChanged("RegionWidth");
            RaisePropertyChanged("LeftHandleX");
            RaisePropertyChanged("RightHandleX");

        }

        private void RegionController_TimeChanged(object sender, double start, double end)
        {
            RaisePropertyChanged("LeftHandleX");
            RaisePropertyChanged("RegionWidth");
            RaisePropertyChanged("RightHandleX");

        }

        private void RegionController_TitleChanged(object source, string title)
        {
            Name = title;
        }

        public void SetNewPoints(double deltaStart, double deltaEnd)
        {
            var model = Model as AudioRegionModel;
            var audioRegionController = RegionLibraryElementController as AudioRegionLibraryElementController;
            if (model == null)
            {
                return;
            }
            model.Start += deltaStart / AudioWrapper.ActualWidth;
            model.End += deltaEnd / AudioWrapper.ActualWidth;

            audioRegionController.SetEndTime(model.End);
            audioRegionController.SetStartTime(model.Start);

            RaisePropertyChanged("LeftHandleX");
            RaisePropertyChanged("RightHandleX");
            RaisePropertyChanged("RegionWidth");
        }
    }

}
