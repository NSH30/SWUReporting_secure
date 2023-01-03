using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Windows;
using System.Data.SqlClient;
using System.Web.Configuration;
using System.Data;
using System.Configuration;
using ReportBuilder;

namespace SWUReporting_TEST
{
    public partial class WelcomePage : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        //protected void Enter_click(object sender, EventArgs e)
        //{
        //    AuthenticateUser.AuthUser.Value = 
        //    if(AuthenticateUser).Value!= null )
        //    {

        //    }

        //}
        public void Enter_click(object sender, EventArgs e)
        {
            DB db = new DB();
            db.Connect();
            String UserName = System.Web.HttpContext.Current.User.Identity.Name;
            String AuthUser = string.Empty;
            String[] separator = { "DSONE\\" };            
            String[] Trigram = UserName.Split(separator,StringSplitOptions.RemoveEmptyEntries);

            ////load the stats


            //List<string> Trigram = db.getAdminUsers();

            //hide menus from non-admins                        
            foreach (var item in Trigram)
            {
                //item = item.Trim();

                string NewUser = "DSONE\\" + item;

                if (UserName == NewUser)
                {

                    Response.Redirect("~/Admins/Tools.aspx");
                }

            }

        }

    }
}