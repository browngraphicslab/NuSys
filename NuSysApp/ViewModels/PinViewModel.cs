using System;
using System.ComponentModel;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace NuSysApp
{
    public class PinViewModel : BaseINPC
    {
        private MatrixTransform _transform;
        private UserControl _view;
        private BaseINPC _model;
        private string _text = string.Empty;
        private WorkspaceViewModel _workspace;
        
        public PinViewModel(PinModel model, WorkspaceViewModel vm) : base()
        {
            Model = model;
            Transform = new MatrixTransform();   
            View = new PinView(this);
            this.Model.PropertyChanged += (s, e) => { Update(e); };
            Text = "<Enter Pin Name>";
            Workspace = vm;
            model.OnDeletion += DeletionHappened;
        }

        private void DeletionHappened(object sender)
        {
            Workspace.DeletePin(this);
        }

        private void Update(PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Model_Text":
                    this.Text = ((PinModel)Model).Text;
                    break;
                case "Model_Transform":
                    this.Transform = ((PinModel)Model).Transform;
                    break;
            }
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
                ((PinModel)Model).Transform = value;
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
        public string Text
        {
            get { return _text; }
            set
            {
                if (_text == value)
                {
                    return;
                }
                _text = value;
                ((PinModel)Model).Text = value;
                RaisePropertyChanged("Text");
            }
        }

        public BaseINPC Model
        {
            get { return _model; }
            set
            {
                if (_model == value)
                {
                    return;
                }
                _model = value;
                RaisePropertyChanged("Model");
            }
        }        
        public WorkspaceViewModel Workspace
        {
            get { return _workspace; }
            set
            {
                if (_workspace == value)
                {
                    return;
                }
                _workspace = value;
            }
        }

    }
}
