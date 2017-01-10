using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Graphics.Canvas;

namespace NuSysApp
{
    class InkableUIElement : RectangleUIElement
    {
        private IInkController _inkController;
        public InkableUIElement(IInkController inkController, BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
            _inkController = inkController;
            PenPointerPressed += OnPenPointerPressed_Callback;
            PenPointerDragged += OnPenPointerDragged_Callback;
            PenPointerReleased += OnPenPointerReleased_Callback;
        }

        private void OnPenPointerPressed_Callback(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {

        }

        private void OnPenPointerDragged_Callback(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {

        }

        private void OnPenPointerReleased_Callback(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {

        }
    }
}
