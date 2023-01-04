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
using System.Web.Security;

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
            string UserName = System.Web.HttpContext.Current.User.Identity.Name;
            string[] separator = { "DSONE\\" };
            string[] sysTrigram = UserName.Split(separator, StringSplitOptions.RemoveEmptyEntries);

            if (AuthenticateUser("sysTrigram"))
            {
                FormsAuthentication.RedirectFromLoginPage("sysTrigram", false);
            }
            else
            {
                lblMessage.Text = "UnAuthorised User";
            }
            //DB db = new DB();
            //db.Connect();
            //String UserName = System.Web.HttpContext.Current.User.Identity.Name;
            //String AuthUser = string.Empty;
            //String[] separator = { "DSONE\\" };            
            //String[] Trigram = UserName.Split(separator,StringSplitOptions.RemoveEmptyEntries);

            //////load the stats


            ////List<string> Trigram = db.getAdminUsers();

            ////hide menus from non-admins                        
            //foreach (var item in Trigram)
            //{
            //    //item = item.Trim();

            //    string NewUser = "DSONE\\" + item;

            //    if (UserName == NewUser)
            //    {

            //        Response.Redirect("~/Admins/Tools.aspx");
            //    }

            //}

        }
        private bool AuthenticateUser(string sysTrigram)
        {
            DB db = new DB();
            db.Connect();
            String UserName = System.Web.HttpContext.Current.User.Identity.Name;
            String[] separator = { "DSONE\\" };
            String[] Trigram = UserName.Split(separator, StringSplitOptions.RemoveEmptyEntries);

            SqlCommand cmd = new SqlCommand("spAuthenticateUser2", db.dbConn)
            {
                CommandType = CommandType.StoredProcedure
            };

            SqlParameter paramTrigram = new SqlParameter("@Trigram", Trigram);            

            cmd.Parameters.Add(paramTrigram);            

            int ReturnCode = (int)cmd.ExecuteScalar();

            if (ReturnCode == 1)
            {
                //Code to Authenticate for all the pages
                return ReturnCode == 1;
            }
            else if (ReturnCode == 2)

            {
                //Code to Authenticate for only Users Folder Pages
                return ReturnCode == 2;
            }
            else

            {
                return ReturnCode == -1;
            }
        }

    }
}