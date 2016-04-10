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
        private ElementModel _previousNode = null;
        private ElementModel _nextNode = null;
        private ElementModel _currentNode;

        public PresentationMode(ElementModel start)
        {
            _currentNode = start;
            Load();
            FullScreen(_currentNode);
        }

        /// <summary>
        /// Checks if there is a valid next node and stores it
        /// </summary>
        /// <returns></returns>
        private void Load()
        {
            var vmList = SessionController.Instance.ActiveFreeFormViewer.AtomViewList.Where(
                item => ((ElementViewModel)item.DataContext).Model.Id == _currentNode.Id);

            var vm = (ElementViewModel)vmList.Single().DataContext;

            _nextNode = GetNextOrPrevNode(vm, false);
            _previousNode = GetNextOrPrevNode(vm, true);
        }

        public bool Next()
        {
            return (_nextNode != null);
        }

        /// <summary>
        /// Full screen zooms into the next node found
        /// </summary>
        public void MoveToNext()
        {
            _currentNode = _nextNode;
            Load();
            FullScreen(_currentNode);
        }

        /// <summary>
        /// Checks if there are any previous nodes
        /// </summary>
        /// <returns></returns>
        public bool Previous()
        {
            return (_previousNode != null);
        }

        /// <summary>
        /// Full screen zooms into the previous node
        /// </summary>
        public void MoveToPrevious()
        {
            _currentNode = _previousNode;
            Load();
            FullScreen(_currentNode);
        }

        public void ExitMode()
        {
            
        }

        /// <summary>
        /// Finds previous node for presentation if reverse is true, next node otherwise.
        /// </summary>
        /// <param name="vm"></param>
        /// <returns></returns>
        private ElementModel GetNextOrPrevNode(ElementViewModel vm, bool reverse)
        {
            foreach (LinkElementController link in vm.LinkList)
            {
                if (link.OutElement.Model.Equals(vm.Model) && reverse)
                {
                    return link.InElement.Model;
                }

                if (link.InElement.Model.Equals(vm.Model) && !reverse)
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
