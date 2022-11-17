using ReportBuilder;
using System;
using System.Data;
using System.IO;
using System.Text;


namespace SWUReporting
{
    public partial class Import : System.Web.UI.Page
    {
        protected static Report report = null;
        private static DataTable importedLearners;
        private static DataTable importedCourses;

        protected void Page_Load(object sender, EventArgs e)
        {
            
            if (!IsPostBack)
            {
                
            }
        }

        /// <summary>
        /// Import Learner Transcripts and report results
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnUpload_Click(object sender, EventArgs e)  //CSV LT uploader
        {
            StringBuilder sb = new StringBuilder();
            bool success = false;
            string filePath = "";
            Report r = new Report();


            if (Uploader.HasFile)
            {
                try
                {
                    sb.AppendFormat("Uploading file: {0}", Uploader.FileName);
                    //save the file
                    filePath = "C:\\Upload\\" + Uploader.FileName;
                    Uploader.SaveAs(filePath);
                    //show file info
                    sb.AppendFormat("<br/> File type: {0}", Uploader.PostedFile.ContentType);
                    sb.AppendFormat("<br/> File size: {0}", Uploader.PostedFile.ContentLength);
                    success = true;
                }
                catch (Exception ex )
                {

                    sb.AppendFormat("<br/> Error <br/>");
                    sb.AppendFormat("Unable to save file <br/> {0}", ex.Message);
                    success = false;
                }
            }

           

            

            lblMessage.Text = sb.ToString();
            if (success)
            {
                DBReporting dbr = new DBReporting();
                DB db = new DB();
                db.Connect();
                DBReporting.db = db;
                r = dbr.ImportCSV(filePath);
                db.Disconnect();
                DBReporting.db = null;
            }
            try
            {
                System.IO.File.Delete(filePath);
            }
            catch (Exception)
            {
            
                //ignore failure to delete the import file
            }

            Messaging.logresults(r.Message);
            lblMessage.Text = "Import completed!";
            //load import message first            
            tbMessage.Text = r.Message;
            tbMessage.Visible = true;
            report = r; //for later use  
            pnlReview.Visible = true;
          
        }

       

        /// <summary>
        /// Import FTE report
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnUploadFTE_Click(object sender, EventArgs e)  //FTE upload script
        {
            //Upload the csv file
            StringBuilder sb = new StringBuilder();
            bool success = false;
            string filePath = "";
            Report r = new Report();
            if (UploaderFTE.HasFile)
            {
                try
                {
                    sb.AppendFormat("Uploading file: {0}", UploaderFTE.FileName);
                    //save the file
                    filePath = "C:\\Upload\\" + UploaderFTE.FileName;
                    UploaderFTE.SaveAs(filePath);
                    //show file info
                    sb.AppendFormat("<br/> File type: {0}", UploaderFTE.PostedFile.ContentType);
                    sb.AppendFormat("<br/> File size: {0}", UploaderFTE.PostedFile.ContentLength);
                    success = true;
                }
                catch (Exception ex)
                {
                    Messaging.LogErrorToText(ex);
                    sb.AppendFormat("<br/> Error <br/>");
                    sb.AppendFormat("Unable to save file <br/> {0}", ex.Message);
                    success = false;
                }
            }
            lblMessageFTE.Text = sb.ToString();
            
            //update the database from the file
            if (success)
            {
                //make any modifications to the FTE csv file first
                EditCSVFTEFile(filePath);

                //load the csv file into a table and update
                try
                {
                    DBReporting dbr = new DBReporting();
                    DB db = new DB();
                    db.Connect();
                    r = dbr.setFTEValuesByCSVFile(filePath);                
                    db.Disconnect();
                }
                catch (Exception x)
                {
                    Messaging.LogErrorToText(x);
                    Messaging.SendAlert("Something went wrong.  Check the error log.\n" + x.Message, Page);
                    return;
                }                
            }
            try
            {
                System.IO.File.Delete(filePath);
            }
            catch (Exception)
            {
                //ignore failure to delete the import file
            }
            lblMessageFTE.Text = "Import completed!";
            //load import message first
            tbMessageFTE.Text = r.Message;
            tbMessageFTE.Visible = true;
            report = r; 
            //review doesn't work with this method since we don't have IDs   
            //pnlReview.Visible = true;

        }

        protected void btnImportVT_Click(object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            bool success = false;
            string filePath = "";
            Report r = new Report();
            report = r;

            if (UploadVT.HasFile)
            {
                try
                {
                    sb.AppendFormat("Uploading file: {0}", UploadVT.FileName);
                    //save the file
                    filePath = "C:\\Upload\\" + UploadVT.FileName;
                    UploadVT.SaveAs(filePath);
                    //show file info
                    sb.AppendFormat("<br/> File type: {0}", UploadVT.PostedFile.ContentType);
                    sb.AppendFormat("<br/> File size: {0}", UploadVT.PostedFile.ContentLength);
                    success = true;
                }
                catch (Exception ex)
                {

                    sb.AppendFormat("<br/> Error <br/>");
                    sb.AppendFormat("Unable to save file <br/> {0}", ex.Message);
                    success = false;
                }
            }
            lblMessageVT.Text = sb.ToString();
            
            //filePath = "D:\\Demo.txt";

            //File.WriteAllText(filePath, tbMessageVT.Text.Trim());
                
            //System.IO.File.WriteAllText(MapPath("D:\\messager.txt"), Request.Form[lblMessageVT.Text]);

            if (success)
            {
                try
                {
                    DBReporting dbr = new DBReporting();
                    DB db = new DB();
                    db.Connect();
                    //r = dbr.setVTValuesByCSVFile(filePath);
                    ImportVT(db, filePath);
                    db.Disconnect();
                }
                catch (Exception x)
                {
                    Messaging.LogErrorToText(x);
                    Messaging.SendAlert("Something went wrong.  Check the error log.", Page);
                    return;
                }
            }
            try
            {
                System.IO.File.Delete(filePath);
            }
            catch (Exception)
            {
                //ignore failure to delete the import file
            }
             
                lblMessageVT.Text = "Import completed!";
                //load import message first
                tbMessageVT.Text = report.Message;
                tbMessageVT.Visible = true;
            
            

            

        }

        protected void btnReviewLearners_Click(object sender, EventArgs e)
        {
            //show all added learners in the edit learners page/control
            //get all learner ids from the report
            var res = report.GetAddedIDs(Report.elementType.learner);
            //add that string to the @ids variable for this query
            if (res == null)
            {
                //ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('No learners added.')", true);
                Messaging.SendAlert("No learners added.", Page);
                return;
            }
            elc1.ShowResults(res, DB.dataSource.import, false);
            elc1.Visible = true;
        }

        protected void btnReviewCompanies_Click(object sender, EventArgs e)
        {            
            var res = report.GetAddedIDs(Report.elementType.company);
            if (res == null)
            {                
                Messaging.SendAlert("No companies added.", Page);
                return;
            }
            ecc1.ShowResults(res, DB.dataSource.import);
            ecc1.Visible = true;
        }

        protected void btnReviewCourses_Click(object sender, EventArgs e)
        {
            //not yet implemented
        }

        #region Helper Functions
        private void ImportVT(DB db, string filepath)
        {
            Importer i = new Importer(db);
            i.ImportVT(filepath);
            //check error reporting, etc
            importedLearners = i.ImportedLearners.Copy();
            importedCourses = i.ImportedCourses.Copy();

            report.AddLine("VT data imported.Please review the imported learners list.");
            report.AddLine(String.Format("{0} Courses added.\n{1} Learners added.",
                importedCourses.Rows.Count, importedLearners.Rows.Count));
            
            if (importedCourses.Rows.Count > 0)
                btnDownloadVTCourses.Enabled = true;
            if (importedLearners.Rows.Count > 0)
                btnDownloadVTRes.Enabled = true;
        }
        private static void EditCSVFTEFile(string filepath)
        {
            StreamReader sr = new StreamReader(filepath);
            string data = sr.ReadToEnd();
            sr.Close();
            //remove ; from emails
            data = data.Replace(";\t", "\t");

            //remove extra quotes from var names
            data = data.Replace(@"""", "");
            StreamWriter sw = new StreamWriter(filepath);
            sw.Write(data);
            sw.Flush();
            sw.Close();
        }
        #endregion

        protected void btnDownloadVTRes_Click(object sender, EventArgs e)
        {
            ReportIO.DownloadSingleSheet(importedLearners);
        }

        protected void btnDownloadVTCourses_Click(object sender, EventArgs e)
        {
            ReportIO.DownloadSingleSheet(importedCourses);
        }

        protected void tbMessage_TextChanged(object sender, EventArgs e)
        {
            
        }
    }
}