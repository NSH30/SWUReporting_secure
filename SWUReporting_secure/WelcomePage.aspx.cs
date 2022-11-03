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
                              

                List<string> NewUsers = db.getAdminUsers();

                //hide menus from non-admins 
                if(NewUsers != null) { 
                    foreach (var item in NewUsers)
                    {
                        item.Trim();
                        string trigram = "DSONE\\" + item;

                        if ( UserName == trigram)
                        {                            
                            Response.Redirect("Tools.aspx");
                            
                        }
                        else
                        {
                            Response.Write("<script>alert('Not authorized')</script>");
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