using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.Xaml.Media;

namespace NuSysApp.Components.Tools
{
    public class BarChartItemViewModel : BaseINPC
    {

        private bool _isSelected;
        private SolidColorBrush _color;
        public SolidColorBrush SelectedColor;
        public SolidColorBrush NotSelectedColor;
        private double _height;
        private string _title;
        private int _count;
        private SolidColorBrush _listSelectedItemColor = new SolidColorBrush(Colors.LightGray);
        private SolidColorBrush _listNotSelectedItemColor = new SolidColorBrush(Colors.Transparent);
        private SolidColorBrush _listItemColor;
        private FontWeight _listFontWeight;

        public BarChartItemViewModel(KeyValuePair<string, int> title_count, Color color)
        {           
            NotSelectedColor = new SolidColorBrush(color);

            SelectedColor = getLighterColorBrush(color);

            Title = title_count.Key;
            Count = title_count.Value;
        }

        public SolidColorBrush ListItemColor
        {
            get { return _listItemColor; }
            set
            {
                _listItemColor = value;
                RaisePropertyChanged("ListItemColor");
            }
        }

        public FontWeight ListFontWeight
        {
            get { return _listFontWeight; }
            set
            {
                _listFontWeight = value;
                RaisePropertyChanged("ListFontWeight");
            }
        }

        public int Count
        {
            get { return _count;}
            set
            {
                _count = value;
                RaisePropertyChanged("Count");
            }
        }

        public string Title
        {
            get
            {
                return _title; 
            }
            set
            {
                _title = value;
                RaisePropertyChanged("Title");
            }
        }

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                if (value)
                {
                    Color = SelectedColor;
                    ListItemColor = _listSelectedItemColor;
                    ListFontWeight = FontWeights.Bold;
                }
                else
                {
                    Color = NotSelectedColor;
                    ListItemColor = _listNotSelectedItemColor;
                    ListFontWeight = FontWeights.Normal;
                }
                RaisePropertyChanged("IsSelected");
            }
        }

        public SolidColorBrush Color
        {
            get
            {
                return _color;
            }
            set
            {
                _color = value;
                RaisePropertyChanged("Color");
            }
        }

        public double Height
        {
            get { return _height; }
            set
            {
                _height = value;
                RaisePropertyChanged("Height");
            }
        }



        private SolidColorBrush getLighterColorBrush(Color color)
        {
            var alpha = (byte)(color.A / 2);
            var lighterColor = ColorHelper.FromArgb(alpha, color.R, color.G, color.B);
            return new SolidColorBrush(lighterColor);
        }

        

    }
}
