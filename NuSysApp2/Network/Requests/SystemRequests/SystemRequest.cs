﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp2
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
        public SystemRequest(SystemRequestType systemRequestType) : base(RequestType.SystemRequest)
        {
            _systemRequestType = systemRequestType;
        }

        public SystemRequest(SystemRequestType type, Message m) : base(RequestType.SystemRequest, m)
        {
            _systemRequestType = type;
        }

        public SystemRequest(Message m) : base(m){}

        public override async Task<bool> CheckOutgoingRequest()
        {
            _message["system_request_type"] = _systemRequestType.ToString();
            return true;
        }
        public virtual async Task ExecuteSystemRequestFunction(NuSysNetworkSession nusysSession, ServerClient serverClient) { }

    }
}
