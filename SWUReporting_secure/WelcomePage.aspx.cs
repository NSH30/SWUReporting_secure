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

namespace SWUReporting
{
    public partial class WelcomePage : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void Enter_click(object sender, EventArgs e)
        {
            DB db = new DB();
            db.Connect();
            String UserName = System.Web.HttpContext.Current.User.Identity.Name;
           

                ////load the stats
                              

                List<string> Trigram = db.getAdminUsers();

                //hide menus from non-admins 
                if(Trigram != null) { 
                    foreach (var item in Trigram)
                    {                        
                        string NewUser = "DSONE\\" + item;

                        if ( UserName == NewUser)
                        {                            
                            Response.Redirect("Tools.aspx");
                            
                        }                        
                    }                   
                }
                else
                {
                    Response.Write("<script>alert('User Not Found')</script>");
                }
            
        }
           
        
    }
}