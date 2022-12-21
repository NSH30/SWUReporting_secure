using ReportBuilder;
using SWUReporting_TEST.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;


namespace SWUReporting_TEST
{

    public partial class EditLearnersControl : System.Web.UI.UserControl
    {
        protected static int learnerID;
        static string activeState = "ACTIVE";
        static string deletedState = "DELETED";
        protected static string searchString;
        protected static DB.dataSource sourceStored;
        protected static DataTable LearnersTable;
        protected static DataTable DuplicateTable;
        protected static DataTable MissingTable;
        protected static bool showDeleted = false;
        protected static bool refreshSearch = false;
        protected static List<int> MergeUserIDs;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                //pnlPopup.Visible = false;
            }
        }

        public void ShowResults(string searchText, DB.dataSource source, bool showDeleted = false)
        {
            sourceStored = source;
            lblMessage.Text = "";            
            DB db = new DB();
            DataTable dt = null;
            db.Connect();
            try
            {
                if (source == DB.dataSource.import)
                {
                    dt = db.GetAddedLearners(searchText);
                    searchString = searchText;
                }
                else
                {
                    string escapedFilter = string.Format("%{0}%", searchText);
                    dt = db.GetLearnerSearchRes(filter: escapedFilter, showDeleted: showDeleted);
                    searchString = escapedFilter;
                }
                db.Disconnect();
                gvLearners.DataSource = dt;
                gvLearners.DataBind();
                LearnersTable = dt; //save for later use
                if (dt.Rows.Count == 50 && source == DB.dataSource.search) { lblMessage.Text = "Additional results may not be displayed. Consider narrowing the search."; }
            }
            catch (Exception e)
            {

                Messaging.SendAlert(e.Message, Page);
            }
            
        }

        public void showDuplicates()
        {
            DB db = new DB();
            DataTable dt = null;
            db.Connect();
            dt = db.getDuplicateLearners();
            db.Disconnect();
            try
            {
                gvDuplicates.DataSource = dt;
                gvDuplicates.DataBind();
                DuplicateTable = dt; //save for later use
            }
            catch (Exception e)
            {

                Messaging.SendAlert(e.Message, Page);
            }
            ModalPopupExtender3.Show();
        }

        public void showMissingData()
        {
            DB db = new DB();
            DataTable dt = null;
            db.Connect();
            dt = db.GetUsersMissingData();
            db.Disconnect();
            try
            {
                gvMissing.DataSource = dt;
                gvMissing.DataBind();
                MissingTable = dt;
            }
            catch (Exception e)
            {
                Messaging.SendAlert(e.Message, Page);
            }
            refreshSearch = false;
            ModalPopupExtender4.Show();
        }

        protected void btnHide_Click(object sender, EventArgs e)
        {

        }

        //edit learner from main gridview
        protected void lnkEdit_Click(object sender, EventArgs e)
        {
            learnerID = Convert.ToInt32((sender as LinkButton).CommandArgument);
            //cfEditUsers.learnerID = learnerID;
            fillForm();
            refreshSearch = true;
            //continue adding user data            
            ModalPopupExtender1.Show();
        }

        protected void lnkEditBlanks_Click(object sender, EventArgs e)
        {
            learnerID = Convert.ToInt32((sender as LinkButton).CommandArgument);
            //cfEditUsers.learnerID = learnerID;
            fillForm();
            refreshSearch = false;
            //continue adding user data            
            ModalPopupExtender1.Show();
        }

        protected void fillForm()
        {
            //query the learner data from the ID
            DB db = new DB();
            db.Connect();
            Learner l = new Learner(db);
            l.GetLearnerByID(learnerID);
            db.Disconnect();

            cbDelete.Checked = false;
            //add learner details here
            tbName.Text = l.Name;
            tbEmail.Text = l.email;
            if (l.company.Name == null)
            {
                tbCompany.Text = "";
            }
            else
            {
                tbCompany.Text = l.company.Name;
            }            
            tbRole.Text = l.Role;
            tbCountry.Text = l.Country;
            tbProfile.Text = l.Profile;
            tbUserState.Text = l.userState;
            if (l.GEO == null)
            {
                tbGEO.Text = "";
            }
            else
            {
                tbGEO.Text = l.GEO;
            }
            tbFTEVal.Text = l.fte_value.ToString();

            //GEO selection for VAR report
            //load GEO filter list
            DataTable dt = new DataTable();            
            db.Connect();
            dt = db.Geos;
            db.Disconnect();
            ddGEOs.DataSource = dt;            
            ddGEOs.DataTextField = "GEO";
            ddGEOs.DataValueField = "ID";  //connect this to ID for VAR Alias query
            ddGEOs.DataBind();
            ddGEOs.Items.Insert(0, new ListItem("Select GEO", "-1"));
            if (l.GEO != null)
            {
                ddGEOs.Items.FindByText(l.GEO).Selected = true;
                l.geo_id = Convert.ToInt32(ddGEOs.SelectedItem.Value);
            }
                        
            //VAR Alias dropdown
            db.Connect();
            dt.Clear();
            dt = db.GetVARList(l.geo_id, activeOnly: true);  //get all VAR Alias values (active only)
            db.Disconnect();
            ddVARAlias.DataSource = dt;
            ddVARAlias.DataValueField = "ID";
            ddVARAlias.DataTextField = "VAR_Alias";
            ddVARAlias.DataBind();
            ddVARAlias.Items.Insert(0, new ListItem("Select the parent VAR name", "-1"));
            if (l.company.AliasId == 0 || l.company.AliasId == null)
            {
                //select var mapping
                ddVARAlias.SelectedIndex = 0;
                //emphasize it with red or yellow?
                ddVARAlias.BackColor = System.Drawing.Color.LightYellow;
                int i = 1;
            }
            else
            {
                //ddVARAlias.Items.FindByText(l.company.Name).Selected = true;
                try
                {
                    ddVARAlias.SelectedValue = l.company.AliasId.ToString();
                    tbVARAlias.Text = ddVARAlias.SelectedItem.Text;
                }
                catch (Exception)
                {
                    //For DS emlpoyees, their VAR Alias name won't show up in the GEO filtered list
                    //so it is ignored by this try block        
                    tbVARAlias.Text = "";  //when lookup fails, show as blank            
                }
                
            }

            if (l.userState == deletedState)
            {
                cbDelete.Text = "ACTIVATE Learner";
            }
            else
            {
                cbDelete.Text = "DELETE Learner";
            }
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            DB db = new DB();
            db.Connect();
            //save the updates
            Learner l = new Learner(db);            
            l.GetLearnerByID(learnerID); //get all learner and company data before updating
            l.Name = tbName.Text;
            l.ID = learnerID;
            l.email = tbEmail.Text;
            l.Country = tbCountry.Text;
            l.GEO = tbGEO.Text;
            l.fte_value = Convert.ToDouble(tbFTEVal.Text);
            //update with new geo
            if (ddGEOs.SelectedItem.Text != "Select GEO")
            {
                l.geo_id = Convert.ToInt32(ddGEOs.SelectedItem.Value);
                l.GEO = ddGEOs.SelectedItem.Text;
            }
            
            Company c = l.company;
            bool updateCompany = false;
            if (!c.GetByName(tbCompany.Text))
            {
                updateCompany = false;
                Messaging.SendAlert(string.Format("Failed to find the VAR: {0}", tbCompany.Text ), Page);
            }
            else
            {
                l.company = c;
                updateCompany = true;
            }
            if (cbDelete.Checked && tbUserState.Text == activeState)
            {
                l.userState = deletedState;
            }
            else if (cbDelete.Checked && tbUserState.Text == deletedState)
            {
                l.userState = activeState;
            }
            else
            {
                l.userState = tbUserState.Text;
            }
            l.Role = tbRole.Text;
            l.Profile = tbProfile.Text;
            
            var msg = l.Update();
            if (msg.StartsWith("ERROR:"))
            {
                Messaging.SendAlert(msg, Page);
            }
            //update the VAR Alias- dissabled May 11, 2022 - causing too many mapping errors.
            //if (Convert.ToInt32(ddVARAlias.SelectedItem.Value) > 0 && updateCompany)
            //{                
            //    //Company c = l.company;
            //    c.AliasId = Convert.ToInt32(ddVARAlias.SelectedItem.Value);
            //    var msg2 = c.Update();
            //    if (msg2 != "")
            //    {
            //        Messaging.SendAlert(msg2, Page);
            //    }
            //}
            db.Disconnect();
            if (msg == "" && refreshSearch == true)
            {
                ModalPopupExtender1.Hide();
                cbDelete.Checked = false;                
                ShowResults(searchString, sourceStored, false);
            }

        }

        protected void lnkActivities_Click(object sender, EventArgs e)
        {
            learnerID = Convert.ToInt32((sender as LinkButton).CommandArgument);
            DB db = new ReportBuilder.DB();
            db.Connect();
            Learner l = new Learner(db);            
            l.GetLearnerByID(learnerID);
            DataTable dt = db.GetLearnerActivities(learnerID);
            db.Disconnect();
            gvActivities.DataSource = dt;
            gvActivities.DataBind();
            tbNameAct.Text = l.Name;
            tbCompanyAct.Text = l.company.Name;
            tbEmailAct.Text = l.email;
            //load datagrid with activities
            ModalPopupExtender2.Show();
        }

        protected void ddVARAlias_SelectedIndexChanged(object sender, EventArgs e)
        {
            ddVARAlias.BackColor = default(System.Drawing.Color);           
        }

        protected void chkMerge_CheckedChanged(object sender, EventArgs e)
        {
            //TODO: If two checkboxes are checked, enable the Merge button
            //((sender as CheckBox).CommandArgument);
            if (countCheckBoxes() == 2)  //some condition
            {
                btnMergeLearners.Enabled = true;
            }
            else
            {
                btnMergeLearners.Enabled = false;
            }
            
        }

        protected int countCheckBoxes()
        {
            int count = 0;
            foreach (GridViewRow row in gvLearners.Rows)
            {
                bool isChecked = ((CheckBox)row.FindControl("chkMerge")).Checked;
                if (isChecked)
                {
                    count++;
                }
            }
            return count;
        }

        protected void btnMergeLearners_Click(object sender, EventArgs e)
        {
            //show the confirmation panel
            List<int> ids = new List<int>();
            MergeUserIDs = new List<int>(); //clear in case of previous results
            var rows = gvLearners.Rows;
            int count = gvLearners.Rows.Count;
            for (int i = 0; i < count; i++)
            {
                bool isChecked = ((CheckBox)rows[i].FindControl("chkMerge")).Checked;
                if (isChecked)
                {
                    //who is this row?
                    DataRow dr = LearnersTable.Rows[i];
                    ids.Add((int)dr[0]);
                }
            }
            if (ids.Count == 2)
            {
                //get the learners and show them in the merge confirmation panel
                MergeUserIDs = ids;
                //load the learners int the panel
                DB db = new DB();
                db.Connect();
                DataTable dt = db.GetLearnersByID(ids);
                db.Disconnect();
                gvMergeLearners.DataSource = dt;
                gvMergeLearners.DataBind();
                mpeConfirm.Show();
            }
            
           
        }

        protected void lnkSaveExcel_Click(object sender, EventArgs e)
        {
            LearnersTable.TableName = "Learners";
            ReportIO.DownloadSingleSheet(LearnersTable);
        }

        protected void gvLearners_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                SharedTools.FormatGridViewRows(6, "DELETED", e, System.Drawing.Color.Red);  //turn them red
            }
        }

        protected void btnCloseDup_Click(object sender, EventArgs e)
        {
            //do nothing
        }

        protected void lnkDownloadDup_Click(object sender, EventArgs e)
        {
            DuplicateTable.TableName = "Duplicates";
            ReportIO.DownloadSingleSheet(DuplicateTable);
        }

        protected void btnDownloadMissing_Click(object sender, EventArgs e)
        {
            MissingTable.TableName = "MissingData";
            ReportIO.DownloadSingleSheet(MissingTable);
        }

        protected void btnCloseMissing_Click(object sender, EventArgs e)
        {
            //do nothing
        }

        protected void btnConfirmMerge_Click(object sender, EventArgs e)
        {
            //continue with the merge
            MergeLearners(MergeUserIDs);
            //clear selected learners from the gridview

        }

        protected void btnCancelMerge_Click(object sender, EventArgs e)
        {
            //close and don't do anything
            
        }

        protected void MergeLearners(List<int> ids)
        {
            //which learners are selected?            
            DB db = new DB();
            db.Connect();
            db.MergeLearners(ids);
            db.Disconnect();
            //if successful
            Messaging.SendAlert("Learners merged.", Page);
        }
    }
}