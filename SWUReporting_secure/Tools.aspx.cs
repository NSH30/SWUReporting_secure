using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using ReportBuilder;
using System.Data;

namespace SWUReporting
{
    public partial class Tools : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                ////GEO selection to filter VAR Alias report
                //DB db = new DB();
                //db.Connect();

                //ddGEOs.DataSource = db.Geos;
                //db.Disconnect();
                //ddGEOs.DataTextField = "GEO";
                //ddGEOs.DataValueField = "ID";
                //ddGEOs.DataBind();
                //ddGEOs.Items.Insert(0, new ListItem("All GEOs", "-1"));
            }
        }

        protected void btnBatchDelete_Click(object sender, EventArgs e)
        {
            ModalPopupExtender1.Show();
        }

        protected void btnAddCourse_Click(object sender, EventArgs e)
        {
            ModalPopupExtender2.Show();
        }


        protected void btnClose_Click(object sender, EventArgs e)
        {

        }

        protected void btnUpdate_Click(object sender, EventArgs e)
        {
            string emailVal = tbEmails.Text;
            string results = null;
            List<string> emails = new List<string>(
                emailVal.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries));
            if (emails.Count != 0)
            {
                DBReporting dbr = new DBReporting();
                DB db = new DB();
                db.Connect();
                DBReporting.db = db;
                bool deletedStatus = true; //assume batch deletion, not batch active
                results = dbr.setUserStateBatch(emails: emails, deletedStatus: deletedStatus);
                db.Disconnect();
            }
            else
            {
                //some error or warning back to the user
            }
            if (results == "0")
            {
                lblBDResponse.CssClass = "label label-warning";
                lblBDResponse.Text = "No learners updated.";
            }
            else
            {
                lblBDResponse.CssClass = "label label-info";
                lblBDResponse.Text = results;
            }            
        }

        protected void btnDownload_Click(object sender, EventArgs e)
        {
            //DB db = new DB();
            //db.Connect();
            //string geoFilter = "";
            //if (ddGEOs.SelectedIndex > 0)
            //{
            //    geoFilter = ddGEOs.SelectedItem.Text;
            //}

            //DataTable res = db.GetRecentActivities(new DateTime(1995,1,1), tbSearchFilter.Text, cbShowDeleted.Checked, geoFilter); //db.GetLearnerSearchRes(tbSearchFilter.Text, geoFilter, cbShowDeleted.Checked);
            //db.Disconnect();
            //res.TableName = "RawLearnerData";
            //DBReporting.DownloadSingleSheet(res);
        }

        protected void btnCheck_Click(object sender, EventArgs e)
        {
            string emailVal = tbEmailsCheck.Text;
            string results = null;
            List<string> emails = new List<string>(
                emailVal.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries));
            if (emails.Count != 0)
            {
                DBReporting dbr = new DBReporting();
                DB db = new DB();
                db.Connect();
                DBReporting.db = db;            
                results = dbr.getMissingEmails(emails: emails);
                db.Disconnect();
            }
            else
            {
                //some error or warning back to the user
            }
            if (results == "0")
            {
                lblBDResponse.CssClass = "label label-warning";
                lblBDResponse.Text = "No learners updated.";
            }
            else
            {
                lblBDResponse.CssClass = "label label-info";
                lblBDResponse.Text = results;
            }
        }

        protected void btnUpdateCourse_Click(object sender, EventArgs e)
        {
            DBReporting dbr = new DBReporting();
            DB db = new DB();
            db.Connect();
            DBReporting.db = db;
            //Report r = dbr.BatchLoadActivities();
            Messaging.SendAlert(r.Message, this.Page);
            //or use a label
        }

        protected void btnCloseCourse_Click(object sender, EventArgs e)
        {

        }
    }
}