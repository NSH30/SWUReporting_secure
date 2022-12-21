using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using ReportBuilder;
using SWUReporting_TEST.Data;
using System.Data;

namespace SWUReporting_TEST
{
    public partial class _Default : System.Web.UI.Page
    {
        public static DataTable geoDT;
        protected void Page_Load(object sender, EventArgs e)
        {           
            if (!Page.IsPostBack)
            {
#if DEBUG
                btnDebugTest.Visible = true;
#endif
                //load GEO filter list
                DataTable dt = new DataTable();
                DB db = new DB();
                db.Connect();
                dt = db.Geos;
                geoDT = dt;
                db.Disconnect();

                //GEO selection for GEO report
                ddGEOs.DataSource = dt;
                ddGEOs.DataTextField = "GEO";
                ddGEOs.DataValueField = "GEO";            
                ddGEOs.DataBind();
                ddGEOs.Items.Insert(0, new ListItem("All GEOs", "%"));

                //GEO selection for Raw Data report
                ddGEOs2.DataSource = dt;
                ddGEOs2.DataTextField = "GEO";
                ddGEOs2.DataValueField = "ID";
                ddGEOs2.DataBind();
                ddGEOs2.Items.Insert(0, new ListItem("All GEOs", "%"));

                //GEO selection for VAR report
                ddGEOsVAR.DataSource = dt;
                ddGEOsVAR.DataTextField = "GEO";
                ddGEOsVAR.DataValueField = "ID";  //connect this to ID for VAR Alias query
                ddGEOsVAR.DataBind();
                ddGEOsVAR.Items.Insert(0, new ListItem("Select a GEO", ""));


                //GEO selection for numeric completions report
                ddGEOs3.DataSource = dt;
                ddGEOs3.DataTextField = "GEO";
                ddGEOs3.DataValueField = "ID";  //connect this to ID for VAR Alias query
                ddGEOs3.DataBind();
                ddGEOs3.Items.Insert(0, new ListItem("All GEOs", "0"));

                tbSearchEndDate.Text = DateTime.Now.ToString("yyyy-MM-dd");

                //set visibility of Sandrine learner progress report
                List<string> validUsers = new List<string>(new string[] { "DSONE\\T35", "DSONE\\IQ1" });
                if (validUsers.Contains(System.Web.HttpContext.Current.User.Identity.Name, StringComparer.OrdinalIgnoreCase))
                    Sandrine.Visible = true;
                else
                    Sandrine.Visible = false;
            }
            
        }

        /// <summary>
        /// Download WW GEO Dashboard reports (for internal dashboards)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnGEOReport_Click(object sender, EventArgs e)
        {            
            DBReporting dbr = new DBReporting();
            dbr.GEOFilter = "%";
            dbr.VARFilter = "%";
            dbr.RoleFilter = "%";
            DB db = new DB("tmpVARDATA");
            db.Connect();

            DBReporting.db = db;
            bool status = false;
            List<string> serverFiles = new List<string>();
            try
            {
                //create standard report files on the server
                serverFiles.AddRange(dbr.CreateGEOReportFiles(out status));
            }
            catch (Exception ex)
            {
                Messaging.SendAlert(ex.Message, this);
            }
            //include transposed report in the zip package
            db.Connect();
            string serverFile = dbr.CreateGEOTransposedReport(out status);
            serverFiles.Add(serverFile);
            serverFile = dbr.CreateSimLegacyReport(out status);
            serverFiles.Add(serverFile);
            serverFile = dbr.CreateDraftSightReport(out status);
            serverFiles.Add(serverFile);
            db.Disconnect();

            if (status)
            { 
                FileDownloader.ZipAndDownload(serverFiles.ToArray());
            }
            else
            {
                Messaging.SendAlert("Error creating reports.", Page);
            }
            
        }

        protected void btnGEOTrans_Click(object sender, EventArgs e)
        {
            DBReporting dbr = new DBReporting();
            DB db = new DB("tmpVARDATA");
            db.Connect();
            dbr.GEOFilter = ddGEOs.Text;
            dbr.VARFilter = "%";
            dbr.RoleFilter = "%"; 
            DBReporting.db = db;
            bool status;
            string serverFile = dbr.CreateGEOTransposedReport(out status);
            db.Disconnect();
            if (status)
            {
                //FileDownloader fd = new FileDownloader();
                //fd.DownloadFile(serverFile);
                string[] files = new string[] { serverFile };
                FileDownloader.ZipAndDownload(files);
            }
            else
            {
                //error message back to user that the report failed to create
                Messaging.SendAlert(serverFile, this);
            }
        }

        /// <summary>
        /// Download button event for selected VARs from GEO-filtered list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnVARReport_Click(object sender, EventArgs e)
        {
            //create and download all VAR reports from selected list
            List<string> vars = new List<string>();
            foreach (ListItem item in cbListVARs.Items)
            {
                if (item.Selected)
                {
                    vars.Add(item.Value);
                }
            }
            //generate the reports
            DBReporting dbr = new DBReporting();
            dbr.GEOFilter = "%";
            dbr.VARFilter = "%";
            dbr.RoleFilter = "%";
            DB db = new DB("tmpVARDATA");
            db.Connect();
            DBReporting.db = db;
            Report r = new Report();
            db.CreateTempTables();
            List<string> files = dbr.ReportMulipleVARs2(vars, out r);
            db.Disconnect();            
            FileDownloader.ZipAndDownload(files.ToArray());
            Messaging.SendAlert(r.Message, this);
        }

        protected void ddGEOsVAR_SelectedIndexChanged(object sender, EventArgs e)
        {
            //get list of VARs from that GEO, populate the checkbox list, then make it visible            
            DropDownList l = (DropDownList)sender;
            DataTable dt = new DataTable();
            DB db = new DB();
            db.Connect();
            if (l.SelectedIndex == 0)
            {
                cbListVARs.DataSource = null;
            }
            else
            {
                dt = db.GetVARList(Convert.ToInt32(l.SelectedValue));
                db.Disconnect();
                cbListVARs.DataSource = dt;
                cbListVARs.DataTextField = "VAR_Alias";
                cbListVARs.DataValueField = "VAR_Alias";
                cbListVARs.DataBind();
            }
            db.Disconnect();      
            
            if (dt.Rows.Count > 0) {
                cbListVARs.Visible = true;
                cbSelectAll.Visible = true; }
            cbSelectAll.Checked = false;
        }

        protected void cbSelectAll_CheckedChanged(object sender, EventArgs e)
        {
            if (cbSelectAll.Checked)
            {
                cbSelectAll.Text = "Unselect All";
            }
            else
            {
                cbSelectAll.Text = "Select All";
            }

            //process all checkboxes
            foreach (ListItem item in cbListVARs.Items)
            {
                item.Selected = cbSelectAll.Checked;
            }

        }

        /// <summary>
        /// Download raw learner data
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnDownload_Click(object sender, EventArgs e)
        {
            DB db = new DB();
            db.Connect();
            string geoFilter = "%";
            if (ddGEOs2.SelectedIndex > 0)
            {
                geoFilter = ddGEOs2.SelectedItem.Text;
            }
            DateTime endDate = DateTime.Now;

            DataTable res = db.GetRecentActivities(new DateTime(1995, 1, 1), tbSearchFilter.Text, endDate, 
                cbShowDeleted.Checked, geoFilter, completed: cbCompletedOnly.Checked); //db.GetLearnerSearchRes(tbSearchFilter.Text, geoFilter, cbShowDeleted.Checked);
            db.Disconnect();
            res.TableName = "RawLearnerData";
            ReportIO.DownloadSingleSheet(res);
        }

        protected void btnDebugTest_Click(object sender, EventArgs e)
        {
            Testing.EditCSVFTEFile(@"C:\Users\IQ1\Desktop\test.txt");
            //run any test code here:
            //transposed virtual tester table
            //Testing.CreateTransposedVTTable();


            //new sandrine report

            //DBReporting dbr = new DBReporting();
            //DBReporting.db = new DB();
            //DBReporting.db.Connect();

            ////test importing virtual tester users
            //Testing.ImportVT(DBReporting.db);


            //download partner alignment report
            //Testing.PAP(dbr, this);


            //System.Threading.Thread.Sleep(3000);
        }

        /// <summary>
        /// Search for Learner Activities
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnSearch_Click(object sender, EventArgs e)
        {
            var startDate = tbSearchDate.Text;
            var searchString = tbSearchString.Text;
            string searchEndDate = tbSearchEndDate.Text;
           
            //added new checkbox for sales roles only (Stefanie request) Dec. 21, 2021
            vac.ShowResults(startDate,searchEndDate, searchString, ReportBuilder.DB.dataSource.search, cbCompleted.Checked, cbSalesOnly.Checked);
            vac.Visible = true;
        }

        internal Dictionary<string, object> GetDictionary(DataTable dt)
        {
            return dt.AsEnumerable()
                .ToDictionary<DataRow, string, object>(row => row.Field<string>("geo"),
                                                    row => row.Field<object>("ID"));                                                   
        }

        protected void btnDownloadAllVAR_Click(object sender, EventArgs e)
        {
            //create and download all VAR reports in geo folders        
            List<string> files = new List<string>();

            //List<int> geos = new List<int>();
            ////get GEOs from db
            //geos = geoDT.AsEnumerable().Select(r => r.Field<int>("ID")).ToList();

            Dictionary<string, object> geos = GetDictionary(geoDT);
            
            DB db = new DB("tmpVARDATA");  //initialize a temp table for transposed data
            db.Connect();
            //create the base query table first
            db.CreateTempTables();
            foreach (var row in geos)
            {
                List<string> vars = new List<string>();
                //get VAR list from db

                DataTable dt = new DataTable();
                int geoID = Convert.ToInt32( row.Value);
                dt = db.GetVARList(geoID);
                vars = dt.AsEnumerable().Select(a => a.Field<string>("VAR_Alias")).ToList();
                //generate the reports
                DBReporting dbr = new DBReporting();
                dbr.GEOFilter = "%";
                dbr.VARFilter = "%";
                dbr.RoleFilter = "%";
                DBReporting.db = db;
                Report r = new Report();
                string folderName = "GEOs\\" + row.Key.ToUpper() + " CRE Certification Data";
                
                files.AddRange(dbr.ReportMulipleVARs2(vars, out r, folderName: folderName));                
            }
            db.Disconnect();
            //FileDownloader fd = new FileDownloader();
            FileDownloader.ZipAndDownload(files.ToArray(), System.IO.Path.GetDirectoryName( System.IO.Path.GetDirectoryName(files[0])));
            //Messaging.SendAlert(r.Message, this);
        }

        protected void btnLearnerProgress_Click(object sender, EventArgs e)
        {
            DBReporting dbr = new DBReporting();
            DBReporting.db = new DB();
            DBReporting.db.Connect();

            if (Convert.ToInt32(ddGEOs3.SelectedValue) > 0)
                dbr.GEOFilter = ddGEOs3.SelectedItem.Text;
            else
                dbr.GEOFilter = "%";

            bool status;
            string serverFile = dbr.CreatePAPUserReport(ReportType.SandrineIndividualScoreCSV, out status);
            DBReporting.db.Disconnect();

            if (status)
            {
                string[] files = new string[] { serverFile };
                FileDownloader.ZipAndDownload(files);
            }
            else
            {
                //error message back to user that the report failed to create
                Messaging.SendAlert(serverFile, this);
            }

        }
    }
}