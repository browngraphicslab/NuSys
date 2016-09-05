﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Media;

namespace NuSysApp2
{
    public class DuplicateNodeRequest : Request
    {
        public DuplicateNodeRequest(Message message) : base(Request.RequestType.DuplicateNodeRequest, message){}

        public override async Task ExecuteRequestFunction()
        {
            var id = _message.GetString("id");
            var model = SessionController.Instance.IdToControllers[id].Model;
            
            ElementType type = model.ElementType;

            if (type == ElementType.Collection)
            {
                var childList = _message.GetList<string>("groupChildren");
                foreach (var childId in childList)
                {
                    var childModel = SessionController.Instance.IdToControllers[childId].Model;
                    var group = (SessionController.Instance.IdToControllers[childId].LibraryElementController.GetMetadata("groups"));
                    var groups = new List<string>();
                    groups.AddRange(group);

                    //TODO: refactor
                   // childModel.Creator = id;
                    groups.Add(id);
                }
            }

            var modelToDuplicate = await model.Pack();
            modelToDuplicate.Remove("id");
            modelToDuplicate["x"] = _message.GetDouble("targetX");
            modelToDuplicate["y"] = _message.GetDouble("targetY");
            modelToDuplicate["autoCreate"] = true;

            var msg = new Message( modelToDuplicate );
            var request = new NewElementRequest(msg);
            await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(request);

            var duplicateModel = SessionController.Instance.IdToControllers[msg.GetString("id")].Model;

            if (!(duplicateModel is CollectionElementModel))
                return;


            //TODO: refactor
            /*
            foreach (var child in SessionController.Instance.IdToSendables.ContentValues.Where(s => (s as ElementModel).Creator.Contains(id)))
            {
                ((NodeContainerModel)duplicateModel).AddChild(child);
            }
            */
        }
    }
}