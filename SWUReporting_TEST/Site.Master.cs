using System;
using System.Web.UI;
using System.Web;
using ReportBuilder;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Data.SqlClient;
using System.Web.Configuration;
using System.Data;
using System.Configuration;


namespace SWUReporting_TEST
{
    public partial class SiteMaster : MasterPage
    {
        
   

        protected void Page_Load(object sender, EventArgs e)
        {


            if (!IsPostBack)
            {

                //load the stats
                DB db = new DB();
                db.Connect();
                Stats s = new Stats(db);
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("{0} Learners", s.LearnerCount));
                sb.Append(String.Format(" | {0} Courses", s.CourseCount));
                sb.Append(String.Format(" | {0} VARs", s.VARAliasCount));
                sb.Append(String.Format(" | {0} FTEs", s.FTEValue));
                
                lblStats.Text = sb.ToString();
                lblStats.Visible = true;
                db.Connect();
                 List<string> users = db.getAdminUsers();
                                                                                                       
                  //hide menus from non-admins                                    
                foreach (var items in users)
                {                                
                          if (("DSONE\\" + items).Contains(System.Web.HttpContext.Current.User.Identity.Name))
                          {                                    
                              //HideMenus();
                                    //show the edit learners page?                                    
                          }
                }

                db.Disconnect();
            }
        }

        private void HideMenus()
        {
            reports.Visible = false;
            admin.Visible = false;
            targets.Visible = false;
            courses.Visible = false;
            companies.Visible = false;
            alignment.Visible = false;
        }
    }
}