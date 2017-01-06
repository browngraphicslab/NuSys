using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    class BrushManager
    {
        private static HashSet<LibraryElementController> _oldControllers = new HashSet<LibraryElementController>();

        public static void ApplyBrush(IEnumerable<LibraryElementController> controllers)
        {
            foreach (var controller in _oldControllers)
            {
                controller.RemoveHighlight();
            }
            _oldControllers.Clear();

            foreach (var controller in controllers)
            {
                _oldControllers.Add(controller);
            }

            foreach (var controller in _oldControllers)
            {
                controller.AddHighlight();
            }

        }

        public static void RemoveBrush()
        {
            foreach (var controller in _oldControllers)
            {
                controller.RemoveHighlight();
            }

            _oldControllers.Clear();
        }

    }
}
