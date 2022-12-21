using ReportBuilder;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SWUReporting
{
    public partial class EditLearners : System.Web.UI.Page
    {
        static string activeState = "ACTIVE";
        static string deletedState = "DELETED";
        string connectionString = @"Data Source=LW5-IQ1-DSA\SOLIDWORKS;Initial Catalog=SWUReporting;Persist Security Info=True;User ID=sa;Password=sql";
        protected static int learnerID;
        protected void Page_Load(object sender, EventArgs e)
        {
            //show nothing by default
        }

        protected void showData2()
        {
            lblMessage.Text = "";
            string escapedFilter = string.Format("%{0}%", tbSearch.Text);
            DB db = new DB();
            db.Connect();
            DataTable dt = db.GetLearnerSearchRes(escapedFilter);
            db.Disconnect();
            gvLearners.DataSource = dt;
            gvLearners.DataBind();
            if (dt.Rows.Count == 50) { lblMessage.Text = "Additional results are not displayed. Consider narrowing the search."; }
        }        

        protected void btnSearch_Click(object sender, EventArgs e)
        {            
            showData2();
        }

        protected void btnHide_Click(object sender, EventArgs e)
        {
            //do nothing?
        }

        protected void lnkEdit_Click(object sender, EventArgs e)
        {
            learnerID = Convert.ToInt32((sender as LinkButton).CommandArgument);
            //cfEditUsers.learnerID = learnerID;
            fillForm();                        

            //continue adding user data            
            ModalPopupExtender1.Show();
        }

        protected void fillForm()
        {
            //query the learner data from the ID
            DB db = new DB();
            db.Connect();
            Learner l = new Learner();
            l.GetLearnerByID(learnerID, db.dbConn);
            db.Disconnect();

            cbDelete.Checked = false;
            //add learner details here
            tbName.Text = l.Name;
            tbEmail.Text = l.email;
            tbCompany.Text = l.company.Name;
            tbRole.Text = l.Role;
            tbCountry.Text = l.Country;
            tbProfile.Text = l.Profile;
            tbUserState.Text = l.userState;
            tbGEO.Text = l.GEO;
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
            //save the updates
            Learner l = new Learner();
            l.Name = tbName.Text;
            l.ID = learnerID;
            l.email = tbEmail.Text;
            l.Country = tbCountry.Text;
            l.GEO = tbGEO.Text;
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
            DB db = new DB();
            db.Connect();
            var msg = l.Update(db.dbConn);
            db.Disconnect();
            if (msg != "")
            {
                lbMessage.Text = msg;
                lbMessage.Visible = true;
            }
            else
            {
                cbDelete.Checked = false;
                ModalPopupExtender1.Hide();
                showData2();
            }

        }

        protected void lnkActivities_Click(object sender, EventArgs e)
        {
            learnerID = Convert.ToInt32((sender as LinkButton).CommandArgument);
            Learner l = new Learner();
            DB db = new ReportBuilder.DB();
            db.Connect();
            l.GetLearnerByID(learnerID, db.dbConn);                        
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
    }
}