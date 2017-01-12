using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NusysIntermediate
{
    /// <summary>
    /// This is the base return arguments class for the FullArgsRequest class.
    /// This base class should only hold properties of all returned requests.
    /// All the classes that extend from this MUST HAVE AN EMPTY CONSTRUCTOR.
    /// </summary>
    public class ServerReturnArgsBase
    {
        /// <summary>
        /// Paramaeterless constructor for the sake of serializability
        /// </summary>
        public ServerReturnArgsBase() { }

        /// <summary>
        /// This bool represents whether the requested function succesfully executed server-side.
        /// </summary>
        public bool WasSuccessful = false;

        /// <summary>
        /// This string represents a server error message.
        /// Should only be non-null if there was an error.
        /// </summary>
        public string ErrorMessage = null;

        /// <summary>
        /// Method to make sure the returned args from the server have been correctly populated.
        /// Should only be called once on the server.
        /// This should return true if the args class is valid, false otherwise.
        /// </summary>
        /// <returns></returns>
        protected virtual bool CheckIsValid() { return true}

        /// <summary>
        /// Should be called to make sure that an args class has been properly created;
        /// </summary>
        public void CheckValidity()
        {
            var validArgs = CheckIsValid();
            Debug.Assert(WasSuccessful == false || validArgs);
        }
    }
}
