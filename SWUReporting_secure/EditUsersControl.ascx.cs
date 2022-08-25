using ReportBuilder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SWUReporting
{
    public partial class EditUsersControl : System.Web.UI.UserControl
    {
        static string activeState = "ACTIVE";
        static string deletedState = "DELETED";
        public int learnerID;
        public bool closing = false;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPostBack)
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
        }

        protected void btnHide_Click(object sender, EventArgs e)
        {
            //do something
            closing = true;
            //modal.Hide();  //not needed?
        }
        protected void btnSave_Click(object sender, EventArgs e)
        {
            closing = false;
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
                //ModalPopupExtender1.Hide();
                //showData2();
                closing = true;
            }

        }
    }
}