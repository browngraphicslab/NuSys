using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NusysIntermediate;

namespace NuSysApp
{
    /// <summary>
    /// iterface for any controller that can handle ink.
    /// Requires methods and events for adding and removing ink.
    /// </summary>
    public interface IInkController
    {
        /// <summary>
        /// To be fired whenever an ink model is added.  
        /// This should pass the new inkmodel in the event
        /// </summary>
        event EventHandler<InkModel> InkAdded;

        /// <summary>
        /// Event fired whenever an ink stroke is removed.
        /// The string argument is the id of the ink stroke
        /// </summary>
        event EventHandler<string> InkRemoved;

        /// <summary>
        /// Method called to create the ink model.  
        /// This should fire the InkAdded event and save the passed ink model
        /// </summary>
        /// <param name="inkModel"></param>
        void AddInk(InkModel inkModel);

        /// <summary>
        /// Method to request the removal of an inkstroke.
        /// Pass in the stroke if of the ink stroke to have it removed.
        /// This method should remove the model, fire an InkRemoved event, and make the necessary removal from whatever model saves the ink strokes.
        /// </summary>
        /// <param name="strokeId"></param>
        void RemoveInk(string strokeId);
    }
}
