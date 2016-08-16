using System.Diagnostics;
using System.Threading.Tasks;
using NusysIntermediate;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Newtonsoft.Json;

namespace NuSysApp
{
    public class DeletePresentationLinkRequest : Request
    {
        /// <summary>
        /// Used for when the server returns a new message
        /// </summary>
        /// <param name="message"></param>
        public DeletePresentationLinkRequest(Message message) : base(NusysConstants.RequestType.DeletePresentationLinkRequest, message)
        {
        }

        /// <summary>
        /// Prefered constructor When creating new request to send to the server. To use, create new DeletePresentationLinkRequestArgs class and 
        /// pass in the corresponding data to it. To use this request, await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(request) then call DeletePresentationLinkFromLibrary
        /// </summary>
        /// <param name="linkId"></param>
        public DeletePresentationLinkRequest(string linkId) : base(NusysConstants.RequestType.DeletePresentationLinkRequest)
        {
            _message[NusysConstants.DELETE_PRESENTATION_LINK_REQUEST_LINK_ID_KEY] = linkId;
        }

        /// <summary>
        /// this method will parse and delete the returned presentation link after the request has successfully returned. 
        /// Will throw an exception if the request has not returned yet or has failed. 
        /// Returned whether the new presentation link was deleted
        /// </summary>
        /// <returns></returns>
        public async Task<bool> DeletePresentationLinkFromLibrary()
        {
            if (WasSuccessful() != true)
            {
                //If this fails here, check with .WasSuccessful() before calling this method.
                throw new Exception("The request hasn't returned yet or was unsuccessful");
            }

            //make sure the key to delete is present
            Debug.Assert(_returnMessage.ContainsKey(NusysConstants.DELETE_PRESENTATION_LINK_REQUEST_LINK_ID_KEY));

            var presentationLinkID = _returnMessage.GetString(NusysConstants.DELETE_PRESENTATION_LINK_REQUEST_LINK_ID_KEY);

            return await DeletePresentationLinkFromLibrary(presentationLinkID);

        }

        /// <summary>
        /// Private Helper method that deletes a presentation link from the library, when passed in a presentationLinkId
        /// </summary>
        /// <param name="presentationLinkId"></param>
        /// <returns></returns>
        private async Task<bool> DeletePresentationLinkFromLibrary(string presentationLinkId)
        {
            var success = false;
            await UITask.Run(async delegate
            {
                // A list of all the presentation lists on the active free form viewer
                var presentationLinkViewModels =
                    SessionController.Instance.ActiveFreeFormViewer.AtomViewList.Where(
                        atom => atom.DataContext is PresentationLinkViewModel).Select(item => item.DataContext as PresentationLinkViewModel).ToList();

                // the presentation link view model we are going to delete
                var toBeDeleted = presentationLinkViewModels.FirstOrDefault(vm => vm.Model.LinkId == presentationLinkId);

                // if the presentation link view model we are going to delete exists
                if (toBeDeleted != null)
                {
                    // then remove it from the wrapping and fire disposed
                    var model = toBeDeleted.Model;
                    PresentationLinkViewModel.Models.Remove(model);
                    toBeDeleted.FireDisposed(this, EventArgs.Empty);
                    success = true;
                }
                else
                {
                    // the presentation link view model we are going to delete does not exist so return false
                    success = false;
                }
            });

            return success;
        }

        /// <summary>
        /// This is called when the server sends a message to the client (except the client who initially created the request). It should delete the presentation link client side.
        /// </summary>
        /// <returns></returns>
        public override async Task ExecuteRequestFunction()
        {
            //make sure the key to delete is present
            Debug.Assert(_message.ContainsKey(NusysConstants.DELETE_PRESENTATION_LINK_REQUEST_LINK_ID_KEY));

            var presentationLinkID = _message.GetString(NusysConstants.DELETE_PRESENTATION_LINK_REQUEST_LINK_ID_KEY);

            await DeletePresentationLinkFromLibrary(presentationLinkID);

        }

        //just checks to see if the message contains an id to request
        public override async Task CheckOutgoingRequest()
        {
            Debug.Assert(_message.ContainsKey(NusysConstants.DELETE_PRESENTATION_LINK_REQUEST_LINK_ID_KEY));
        }
    }
}