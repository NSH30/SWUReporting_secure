using SWUReporting;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using SWUReporting.Data;

namespace SWUReporting
{
    public partial class ViewActivitiesControl : System.Web.UI.UserControl
    {
        protected static DataTable dtActivities;
        
        protected void Page_Load(object sender, EventArgs e)
        {
            
        }

        /// <summary>
        /// Show Learner Activities search results
        /// </summary>
        
        public void ShowResults(string searchDate, string searchEndDate, string searchString, 
            DB.dataSource source, bool completed, bool salesOnly = false)
        {
            DB db = new DB();
            DataTable dt = null;
            DateTime startDate = new DateTime(1995,01,01); //DateTime.MinValue;
            DateTime endDate = DateTime.Now;
            db.Connect();
            if (source == DB.dataSource.search)
            {
                if (searchDate != "")
                {
                    startDate = Convert.ToDateTime(searchDate); 
                }
                if (searchEndDate != "") 
                {
                    endDate = Convert.ToDateTime(searchEndDate);
                }
                               
                dt = db.GetRecentActivities(startDate, searchString, endDate, completed: completed, salesOnly: salesOnly);
            }
            else
            {
                //not implemented
            }
            db.Disconnect();
            dt.TableName = "Learner_Activities";
            gvActivities.DataSource = dt;
            gvActivities.DataBind();
            dtActivities = dt;
            //if (dt.Rows.Count == 50 && source == DB.dataSource.search) { lblMessage.Text = "Additional results may not be displayed. Consider narrowing the search."; }
        }

        protected void btnSaveExcel_Click(object sender, EventArgs e)
        {            
            ReportIO.DownloadSingleSheet(dtActivities);
        }
    }
}