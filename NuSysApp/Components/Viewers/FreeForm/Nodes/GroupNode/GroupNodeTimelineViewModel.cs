using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using NuSysApp.Components.Nodes.GroupNode;

namespace NuSysApp
{
    public class GroupNodeTimelineViewModel : ElementCollectionViewModel
    {
        private CompositeTransform _compositeTransform;
        private TranslateTransform _translateTransform;
        private List<Tuple<FrameworkElement, Object>> _dataList;
        public GroupNodeTimelineViewModel(ElementCollectionController controller) : base(controller)
        {
            _compositeTransform = new CompositeTransform
            {
                //TODO Refactor
                TranslateX = 0,
                TranslateY = 0,
                CenterX = 0,
                CenterY = controller.Model.Height/2,
                ScaleX = 1,
                ScaleY = 1
            };

            _translateTransform = new TranslateTransform()
            {
                X = 0,
                Y= 0
            };
        }

        public TranslateTransform TranslateTransform
        {
            get { return _translateTransform; }
            set
            {
                if (_translateTransform == value)
                {
                    return;
                }
                _translateTransform = value;
                RaisePropertyChanged("TranslateTransform");
            }
        }

        public CompositeTransform CompositeTransform
        {
            get { return _compositeTransform; }
            set
            {
                if (_compositeTransform == value)
                {
                    return;
                }
                _compositeTransform = value;
                RaisePropertyChanged("CompositeTransform");
            }
        }

        public List<Tuple<FrameworkElement, Object>> DataList
        {
            get { return _dataList; }
            set
            {
                if (_dataList == value)
                {
                    return;
                }
                _dataList = value;
                RaisePropertyChanged("DataList");
            }
        }
    }
}