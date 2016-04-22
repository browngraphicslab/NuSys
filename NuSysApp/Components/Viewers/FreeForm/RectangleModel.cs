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
            Debug.WriteLine("SSSSSSSSSSSSSSSSSSSSS");
            OnSelect?.Invoke();
        }

        public void Deselect()
        {
            Debug.WriteLine("DDDDDDDDDDDDDDDDDDD");
            OnDeselect?.Invoke();
        }
    }
}