using System.Collections.ObjectModel;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace NuSysApp
{
    public class MinimapViewModel : BaseINPC
    {
        private MatrixTransform _transform;
        private UserControl _view;
        public MinimapViewModel() : base()
        {
            Transform = new MatrixTransform();
        }

        public MatrixTransform Transform
        {
            get { return _transform; }
            set
            {
                if (_transform == value)
                {
                    return;
                }
                _transform = value;

                RaisePropertyChanged("Transform");
            }
        }

        public UserControl View
        {
            get { return _view; }
            set
            {
                if (_view == value)
                {
                    return;
                }
                _view = value;

                RaisePropertyChanged("View");
            }
        }

        public ObservableCollection<PinViewModel> PinList { get; set; }
    }
}
