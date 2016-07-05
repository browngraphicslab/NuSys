using System.Diagnostics;

namespace NuSysApp.Viewers
{
    public class RectangleModel
    {
        public delegate void selectHandler();
        public event selectHandler OnSelect;

        public delegate void deselectHandler();
        public event deselectHandler OnDeselect;

        public RectangleModel()
        {
            
        }

        public void Select()
        {
            OnSelect?.Invoke();
        }

        public void Deselect()
        {
            OnDeselect?.Invoke();
        }
    }
}