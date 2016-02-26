using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    /*
        SAMPLE REQUEST

        Copy and paste this code into a new request and then specialize as needed

    */
    public class SAMPLE_REQUEST : Request
    {
        /*
            REQUIRED 
            
            SAMPLE REQUEST constructor
            This sample uses RequestType.NewContentRequest

            Your request should not use that request type, you should add a new enum item to the RequestType Enum

            You will also then need to add your RequestType to an enum switch statement in NusysNetworkSession.cs 
            If you fail to do so, nusys will crash with an exception telling you to add it to the switch statement

            This constructor with just a message parameter is a required constructor, although you may have more

            anything in the passed-in Message m will end up in the protected Dictionary<string,object> _message
        */
        public SAMPLE_REQUEST(Message m) : base(RequestType.NewContentRequest, m){}

        /*
            OPTIONAL

            Another constructor.  Takes in whatever you might need for the request and adds them to the protected Dictionary<string,object> _message

            This constructor also uses RequestType.NewContentRequest in the base constructor call, switch it to whatever your new enum member is
        */
        public SAMPLE_REQUEST(string var1, object var2) : base(RequestType.NewContentRequest)
        {
            _message["test_key_for_var1"] = var1;
            _message["test_key_for_var2"] = var2;
        }
 
        /*
            OPTIONAL

            This is the last check before the request is sent off to verify that nobody has messed with your request

            It is also a chance to run any async functions you may need to run before sending off the request

            Can also set the server settings here
        */
        public override async Task CheckOutgoingRequest()
        {
            if (!_message.ContainsKey("test_key_for_var1"))
            {
                throw new Exception("you gotta have the key 'test_key_for_var1', otherwise this request is pointless");
            }

            /*
            REQUIRED to set these server settings somewhere

            if you're confused about the meaning of these please ask somebody before implementing this request

            the server settings below are what we'd use to tell the server and all users to delete an alias
            */
            SetServerEchoType(ServerEchoType.Everyone);//REQUIRED METHOD CALL  -- MUST BE DONE BEFORE MESSAGE IS SENT
            SetServerItemType(ServerItemType.Alias);//REUIRED METHOD CALL  -- MUST BE DONE BEFORE MESSAGE IS SENT
            SetServerRequestType(ServerRequestType.Remove);//REUIRED METHOD CALL  -- MUST BE DONE BEFORE MESSAGE IS SENT
            SetServerIgnore(false);//REUIRED METHOD CALL  -- MUST BE DONE BEFORE MESSAGE IS SENT
        }

        /*
            OPTIONAL

            if your server settings EchoType is set to ServerEchoType.None, then this method will never be run
            otherwise, this method will be run and you may choose to fill it in
        */
        public override async Task ExecuteRequestFunction()
        {
            string var1 = _message.GetString("test_key_for_var1");
            Debug.WriteLine("you used: "+var1);
            //do something with var1
        }
    }
}
