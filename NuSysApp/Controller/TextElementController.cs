using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace NuSysApp
{


    public class TextElementController: ElementController
    {
        public delegate void NodeChangedEventHandler(object source, string text);
        public event NodeChangedEventHandler NodeChanged;
        public delegate void DetailViewChangedEventHandler(object source, string text);
        public event DetailViewChangedEventHandler DetailViewChanged;

        public TextElementController(ElementModel model) : base(model)
        {

        }

        public void SetNodeText(String s)
        {
            ((TextElementModel) Model).Text = s;
            DetailViewChanged?.Invoke(this, s);
        }

        public void SetDetailText(String s)
        {
            ((TextElementModel)Model).Text = s;
            NodeChanged?.Invoke(this, s);
        }
    }
}
