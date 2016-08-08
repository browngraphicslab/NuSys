using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Owin;

namespace NusysServer
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR();
            //to instantiate the contentcontroller singleton upon application startup,
            //rather than on the first reference
            var nothing = ContentController.Instance;
        }
    }
}