using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;
using System.Web.SessionState;
using SWUReporting;

namespace SWUReporting_secure
{
    public class Global : HttpApplication
    {
        void Application_Start(object sender, EventArgs e)
        {
            // Code that runs on application startup
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        void Application_Error(object sender, EventArgs e)
        {
            Exception exc = Server.GetLastError();
            //log it
            Messaging.LogErrorToText(exc);

            if (exc is HttpUnhandledException)
            {
                // Pass the error on to the error page.
                Server.Transfer("Error.aspx?handler=Application_Error%20-%20Global.asax", true);
            }
        }
    }
}