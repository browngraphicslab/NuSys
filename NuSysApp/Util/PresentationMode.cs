using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp.Util
{
    /// <summary>
    /// Implements PresentationMode for nodes.
    /// </summary>
    class PresentationMode
    {
        private ObservableCollection<ElementModel> previousNodes;
        private ElementModel currentNode;
        private ElementModel nextNode;

        public PresentationMode(ElementModel start)
        {
            previousNodes = new ObservableCollection<ElementModel>();
            currentNode = start;
            FullScreen(currentNode);
        }

        /// <summary>
        /// Checks if there is a valid next node and stores it
        /// </summary>
        /// <returns></returns>
        public bool Next()
        {
            var vmList = SessionController.Instance.ActiveFreeFormViewer.AtomViewList.Where(
                item => ((ElementViewModel)item.DataContext).Model.Id == currentNode.Id);

            var vm = (ElementViewModel)vmList.Single().DataContext;

            nextNode = getNextNode(vm);

            return (nextNode != null);
        }

        /// <summary>
        /// Full screen zooms into the next node found
        /// </summary>
        public void MoveToNext()
        {
            previousNodes.Add(currentNode);
            currentNode = nextNode;
            FullScreen(currentNode);
        }

        /// <summary>
        /// Checks if there are any previous nodes
        /// </summary>
        /// <returns></returns>
        public bool Previous()
        {
            return (previousNodes.Count > 0);
        }

        /// <summary>
        /// Full screen zooms into the previous node
        /// </summary>
        public void MoveToPrevious()
        {
            currentNode = previousNodes.Last();
            previousNodes.Remove(previousNodes.Last());
            FullScreen(currentNode);
        }

        public void ExitMode()
        {
            
        }

        /// <summary>
        /// Finds next possible node for presentation. Currently searches for any node linked to the current node 
        /// that is not the previous.
        /// </summary>
        /// <param name="vm"></param>
        /// <returns></returns>
        private ElementModel getNextNode(ElementViewModel vm)
        {
            var prev = Previous() ? previousNodes.Last() : null;
            foreach (LinkElementController link in vm.LinkList)
            {
                if (link.OutElement.Model.Equals(vm.Model) && !link.InElement.Model.Equals(prev))
                {
                    return link.InElement.Model;
                }

                if (link.InElement.Model.Equals(vm.Model) && !link.OutElement.Model.Equals(prev))
                {
                    return link.OutElement.Model;
                }

            }
            return null;
        }

        /// <summary>
        /// Will make a full screen appeareance for the passed in element view model. Use this for presentation view.
        /// </summary>
        /// <param name="e"></param>

        public static void FullScreen(ElementModel e)
        {
            // Define some variables that will be used in future translation/scaling
            var sv = SessionController.Instance.SessionView;
            var x = e.X + e.Width / 2;
            var y = e.Y + e.Height / 2;
            var widthAdjustment = sv.ActualWidth / 2;
            var heightAdjustment = sv.ActualHeight / 2;

            // Reset the scaling and translate the free form viewer so that the passed in element is at the center
            var compositeTransform = SessionController.Instance.ActiveFreeFormViewer.CompositeTransform;
            compositeTransform.ScaleX = 1;
            compositeTransform.ScaleY = 1;
            compositeTransform.TranslateX = widthAdjustment - x;
            compositeTransform.TranslateY = heightAdjustment - y;

            // Obtain correct scale value based on width/height ratio of passed in element
            double scale;
            if (e.Width > e.Height)
                scale = sv.ActualWidth / e.Width;
            else
                scale = sv.ActualHeight / e.Height;

            // Scale the active free form viewer so that the passed in element appears to be full screen.
            scale = scale * .7; // adjustment so things don't get cut off
            compositeTransform.CenterX = x;
            compositeTransform.CenterY = y;
            compositeTransform.ScaleX = scale;
            compositeTransform.ScaleY = scale;
        }
    }
}
