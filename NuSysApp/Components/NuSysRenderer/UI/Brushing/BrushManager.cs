using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    class BrushManager
    {

        public delegate void BrushUpdatedHandler(IEnumerable<LibraryElementController> controllersRemoved, IEnumerable<LibraryElementController> controllersAdded);

        /// <summary>
        /// Event fired whenever the brush is updated
        /// </summary>
        public static event BrushUpdatedHandler BrushUpdated;

        /// <summary>
        /// Hashset of the controllers which currently have highglith
        /// </summary>
        public static HashSet<LibraryElementController> ControllersWithHighlight = new HashSet<LibraryElementController>();

        public static void ApplyBrush(IEnumerable<LibraryElementController> controllers)
        {
            // get all the controllers that lost highight and remove it
            var controllersWhichLostHighlight = ControllersWithHighlight.Except(controllers);
            foreach (var controller in controllersWhichLostHighlight)
            {
                controller.RemoveHighlight();
            }

            // get all the controllers that got highlight and add it
            var controllersWhichGotHighlight = controllers.Except(ControllersWithHighlight); 
            foreach (var controller in controllersWhichGotHighlight)
            {
                controller.AddHighlight();
            }

            // update old controllers to contain the new controllers
            ControllersWithHighlight = new HashSet<LibraryElementController> (controllers);


            BrushUpdated?.Invoke(controllersWhichLostHighlight, controllersWhichGotHighlight);
        }

        public static void RemoveBrush()
        {
            foreach (var controller in ControllersWithHighlight)
            {
                controller.RemoveHighlight();
            }

            BrushUpdated?.Invoke(new List<LibraryElementController>(), ControllersWithHighlight);

            ControllersWithHighlight.Clear();
        }

    }
}
