using ReportBuilder;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SWUReporting
{
    public partial class EditCompanyControl : System.Web.UI.UserControl
    {
        protected DB db;
        protected static int companyID;
        protected static string searchString;
        protected static DB.dataSource sourceStored;
        public static bool UsedSearch { get; set; }
        protected void Page_Load(object sender, EventArgs e)
        {
            if (db == null)
            {
                db = new DB();
            }
        }

        

        protected void btnSave_Click(object sender, EventArgs e)
        {
            db.Connect();
            Company c = new Company(db);            
            c.GetByID(companyID);
            c.ParentNameId = Convert.ToInt32(ddVARParent.SelectedItem.Value);
            c.AliasId = Convert.ToInt32(ddVARAlias.SelectedItem.Value);
            if (cbStatus.Checked)
            {
                c.status = 1;
            }
            else
            {
                c.status = 0;
            }
            
            var msg = c.Update();
            db.Disconnect();
            if (msg == "")
            {
                if (UsedSearch)
                {
                    ShowResults(searchString, DB.dataSource.search);
                }
                else
                {
                    ShowNotMapped();
                }                
            }
            else
            {
                Messaging.SendAlert(msg, Page);
            }        
        }

        protected void btnHide_Click(object sender, EventArgs e)
        {

        }

        protected void lnkEdit_Click(object sender, EventArgs e)
        {
            if ((sender as LinkButton).CommandArgument == "")
            {
                //cannot edit - nothing selected?
                return;
            }
            companyID = Convert.ToInt32((sender as LinkButton).CommandArgument);           
            fillForm();

            //continue adding user data            
            ModalPopupExtender3.Show();
        }

        protected void fillForm()
        {
            //query the learner data from the ID
            //DB db = new DB();
            db.Connect();
            Company c = new Company(db);
            c.GetByID(companyID);
            db.Disconnect();
            
            //add learner details here
            tbCompany.Text = c.Name;
            tbAlias.Text = c.AliasVal;
            tbParent.Text = c.ParentVal;
            if (c.status == 1)
            {
                cbStatus.Checked = true;
            }
            else
            {
                cbStatus.Checked = false;
            }
            //load Alias filter list
            DataTable dt = new DataTable();
            db.Connect();
            dt = db.GetParentList();
            db.Disconnect();
            ddVARParent.DataSource = dt;
            ddVARParent.DataTextField = "VAR_Parent";
            ddVARParent.DataValueField = "ID";  //connect this to ID for VAR Alias query
            ddVARParent.DataBind();
            ddVARParent.Items.Insert(0, new ListItem("Select the VAR Parent (FTE)", "-1"));
            if (c.ParentNameId == 0 || c.ParentNameId == null)
            {
                ddVARParent.SelectedIndex = 0;
                ddVARParent.BackColor = System.Drawing.Color.LightYellow;
            }
            else
            {
                ddVARParent.SelectedValue = c.ParentNameId.ToString();
            }

            //VAR Alias dropdown
            db.Connect();
            dt.Clear();
            dt = db.GetVARList(-1, activeOnly: true);  //get all VAR Alias values, only active VARs
            db.Disconnect();
            ddVARAlias.DataSource = dt;
            ddVARAlias.DataValueField = "ID";
            ddVARAlias.DataTextField = "VAR_Alias";
            ddVARAlias.DataBind();
            ddVARAlias.Items.Insert(0, new ListItem("Select VAR Alias name", "-1"));
            if (c.AliasId == 0 || c.AliasId == null)
            {
                //select var mapping
                ddVARAlias.SelectedIndex = 0;
                //emphasize it with red or yellow?
                ddVARAlias.BackColor = System.Drawing.Color.LightYellow;                
            }
            else
            {
                //ddVARAlias.Items.FindByText(l.company.Name).Selected = true;
                ddVARAlias.SelectedValue = c.AliasId.ToString();
            }

        }

        public void ShowNotMapped()
        {
            //DB db = new DB();
            DataTable dt = null;
            db.Connect();
            dt = db.GetCompaniesNotMapped();
            db.Disconnect();
            gvCompanies.DataSource = dt;
            gvCompanies.DataBind();
        }

        public void ShowResults(string searchText, DB.dataSource source)
        {
            //sourceStored = source;
            lblMessage.Text = "";
            //DB db = new DB();
            DataTable dt = null;
            db.Connect();
            if (source == DB.dataSource.import)
            {
                dt = db.GetAddedCompanies(searchText);
                searchString = searchText;
            }
            else
            {
                string escapedFilter = string.Format("%{0}%", searchText);
                dt = db.GetCompanySearchRes(escapedFilter);
                searchString = escapedFilter;
            }
            db.Disconnect();
            gvCompanies.DataSource = dt;
            gvCompanies.DataBind();
            //gvCompanies.Visible = true;            

        }
        protected void ddVARAlias_SelectedIndexChanged(object sender, EventArgs e)
        {
            ddVARAlias.BackColor = default(System.Drawing.Color);
        }

        protected void ddVARParent_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}