﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NusysIntermediate;

namespace NuSysApp
{
    public abstract class SystemRequest: Request
    {
        public enum SystemRequestType
        {
            RemoveClient,
            SetHost,
            SendWorkspace
        }

        private SystemRequestType _systemRequestType;
        public SystemRequest(SystemRequestType systemRequestType) : base(NusysConstants.RequestType.SystemRequest)
        {
            _systemRequestType = systemRequestType;
        }

        public SystemRequest(SystemRequestType type, Message m) : base(NusysConstants.RequestType.SystemRequest, m)
        {
            _systemRequestType = type;
        }

        public SystemRequest(Message m) : base(m){}

        public override void CheckOutgoingRequest()
        {
            _message["system_request_type"] = _systemRequestType.ToString();
        }
        public virtual async Task ExecuteSystemRequestFunction(NuSysNetworkSession nusysSession, ServerClient serverClient) { }

    }
}
