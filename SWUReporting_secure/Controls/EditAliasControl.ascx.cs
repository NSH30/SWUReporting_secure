using System;
using System.Web.UI.WebControls;
using ReportBuilder;
using System.Data;
using SWUReporting.Data;

namespace SWUReporting
{
    public partial class EditAliasControl : System.Web.UI.UserControl
    {
        protected static int companyID;
        public DB db;
        protected void Page_Load(object sender, EventArgs e)
        {                                    
            if (db == null)
                db = new DB();            
        }

        public void ShowResults(int geoID)
        {
            //query all VAR Alias's from the DB
            //Consider adding GEO filter?
            db.Connect();     
            DataTable dt = db.GetVARList(geoID, activeOnly: false);
            db.Disconnect();
            gvAlias.DataSource = dt;
            gvAlias.DataBind();
            gvAlias.Visible = true;
        }

        private void fillAliasForm()
        {            

            db.Connect();
            //fill the geos dropdown
            DataTable dt = new DataTable();
            dt = db.Geos;
            ddGEOs.DataSource = dt;
            ddGEOs.DataTextField = "GEO";
            ddGEOs.DataValueField = "ID";
            ddGEOs.DataBind();
            
            Alias a = new Alias(db);
            a.GetByID(companyID);            
            db.Disconnect();

            tbAlias.Text = a.Name;
            tbGEO.Text = a.geoVal;
            if (a.status == 1)
                cbStatus.Checked = true;
            else
                cbStatus.Checked = false;

            if (a.geoVal != "")
                ddGEOs.Items.FindByText(a.geoVal).Selected = true;

            if (a.DashboardDate != null)
                cbDashboard.Checked = true;
            else
                cbDashboard.Checked = false;
            if (a.Contract3DX == 1)
                cb3DX.Checked = true;
            else
                cb3DX.Checked = false;
            if (a.ContractDW  == 1)
                cbDW.Checked = true;
            else  
                cbDW.Checked = false;

        }


        protected void lnkEdit_Click(object sender, EventArgs e)
        {
            companyID = Convert.ToInt32((sender as LinkButton).CommandArgument);
            fillAliasForm();

            //continue adding user data            
            lblEditAlias.Text = "Edit VAR Alias";
            btnSave.Enabled = true;
            ModalPopupExtender3.Show();

        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            db.Connect();
            //save the changes back to the database
            Alias a = new Alias(db);
            //DB db = new DB();
            
            bool createdNew = false;
            if (companyID != 0)
            {
                a.GetByID(companyID);
                createdNew = false;
            }
            else
            {
                //creating a new one, see if it exists
                if (a.GetByName(tbAlias.Text))
                {
                    Messaging.SendAlert(tbAlias.Text + " already exists.", Page);
                    db.Disconnect();
                    return;
                }
            }
            a.Name = tbAlias.Text;
            a.geoID = Convert.ToInt32(ddGEOs.SelectedValue);
            a.geoVal = ddGEOs.SelectedItem.Text;
            if (cbStatus.Checked)
                a.status = 1;
            else
                a.status = 0;

            if (cbDashboard.Checked && a.DashboardDate == null)
            {
                a.DashboardDate = DateTime.Now;
            }
            else if (!cbDashboard.Checked && a.DashboardDate != null)
            {
                a.DashboardDate = null;
            }
            if (cbDashboardCallMade.Checked && a.DashboardCommunicatedDate == null)
            {
                a.DashboardCommunicatedDate = DateTime.Now;
            }
            else if (!cbDashboardCallMade.Checked && a.DashboardCommunicatedDate != null)
            {
                a.DashboardCommunicatedDate = null;
            }
            //CONTRACT STATUS
            if (cb3DX.Checked)
                a.Contract3DX = 1;
            else
                a.Contract3DX = 0;
            if (cbDW.Checked)
                a.ContractDW = 1;
            else
                a.ContractDW = 0;
            if (cbEDU.Checked)
                a.EDUOnly = 1;
            else
                a.EDUOnly = 0;


            //update the database
            a.Insert();
            db.Disconnect();

        }


        protected void btnAddAlias_Click(object sender, EventArgs e)
        {
            //show the modaldialog with empty values for a new Alias
            tbAlias.Text = "";
            tbGEO.Text = "";
            companyID = 0;
            cbStatus.Checked = true;
            cbEDU.Checked = false;
            cb3DX.Checked = false;
            cbDW.Checked = false;
            db.Connect();
            ddGEOs.DataSource = db.Geos;
            db.Disconnect();            
            ddGEOs.DataTextField = "GEO";
            ddGEOs.DataValueField = "ID";
            ddGEOs.DataBind();
            ddGEOs.Items.Insert(0, new ListItem("Select a GEO", "-1"));
            lblEditAlias.Text = "Create New Alias";
            btnSave.Enabled = false;
            ModalPopupExtender3.Show();

        }

        protected void ddGEOs_SelectedIndexChanged(object sender, EventArgs e)
        {
            //enable the save button after selecting a geo
            if (tbAlias.Text.Length > 0)
            {
                btnSave.Enabled = true;
            }            

        }

        protected void btnClose_Click(object sender, EventArgs e)
        {
 
        }

        protected void tbAlias_TextChanged(object sender, EventArgs e)
        {
            if (ddGEOs.SelectedIndex != 0)
                btnSave.Enabled = true;
            
        }

        protected void btnAddParent_Click(object sender, EventArgs e)
        {
            //show the modaldialog with empty values for a new Parent
            tbParent.Text = "";            
            companyID = 0;
            btnSaveParent.Enabled = false;
            mpeParent.Show();
        }

        protected void tbParent_TextChanged(object sender, EventArgs e)
        {
            btnSaveParent.Enabled = false;
            if (tbParent.Text !="")
               btnSaveParent.Enabled = true;                          
        }

        protected void btnSaveParent_Click(object sender, EventArgs e)
        {
            //save the changes back to the database
            db.Connect();
            Parent p = new Parent(db);            
            if (companyID != 0)
            {
                p.GetByID(companyID, db.dbConn);
            }
            else
            {
                //creating a new one, see if it exists
                if (p.GetByName(tbParent.Text, db.dbConn))
                {
                    Messaging.SendAlert(tbParent.Text + " already exists.", Page);
                    db.Disconnect();
                    return;
                }
            }
            p.VAR_Parent = tbParent.Text;
            p.Insert();
            db.Disconnect();
        }

        protected void btnCloseParent_Click(object sender, EventArgs e)
        {

        }

        protected void gvAlias_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                SharedTools.FormatGridViewRows(1, "0", e, System.Drawing.Color.Red);  //turn them red
            }
        }
    }
}