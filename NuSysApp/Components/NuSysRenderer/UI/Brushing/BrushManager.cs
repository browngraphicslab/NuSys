using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        /// <summary>
        /// True when a brush has been applied, false once remove brush has been called, true even when controllers with highlight is empty
        /// </summary>
        public static bool HasBrush;

        public static void ApplyBrush(IEnumerable<LibraryElementController> controllers, bool isNoFilterApplied)
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

            HasBrush = !isNoFilterApplied;

            // make sure that if no filter is applied no controllers have highlight
            if (!HasBrush) 
                Debug.Assert(!ControllersWithHighlight.Any());


            BrushUpdated?.Invoke(controllersWhichLostHighlight, controllersWhichGotHighlight);
        }

        public static void RemoveBrush()
        {
            foreach (var controller in ControllersWithHighlight)
            {
                controller.RemoveHighlight();
            }

            var controllersToBeRemoved = ControllersWithHighlight.ToArray();

            ControllersWithHighlight.Clear();

            HasBrush = false;


            BrushUpdated?.Invoke(controllersToBeRemoved, new List<LibraryElementController>());
        }

        /// <summary>
        /// temporarily hides the current brush
        /// </summary>
        /// <param name="Visibility"></param>
        public static void SetBrushVisibility(bool Visibility)
        {
            if (Visibility)
            {
                foreach (var controller in ControllersWithHighlight)
                {
                    controller.AddHighlight();
                }
            } else
            {
                foreach (var controller in ControllersWithHighlight)
                {
                    controller.RemoveHighlight();
                }
            }
        }

    }
}
