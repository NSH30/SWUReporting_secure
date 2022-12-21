using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using ReportBuilder;

namespace SWUReporting_TEST
{
    public partial class EditCompanies : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                //GEO selection to filter VAR Alias report
                DB db = new DB();
                db.Connect();

                ddGEOs.DataSource = db.Geos;
                db.Disconnect();
                ddGEOs.DataTextField = "GEO";
                ddGEOs.DataValueField = "ID";
                ddGEOs.DataBind();
                ddGEOs.Items.Insert(0, new ListItem("All GEOs", "-1"));
            }
        }
        protected void btnSearch_Click(object sender, EventArgs e)
        {
            string search = tbSearch.Text;
            EditCompanyControl.UsedSearch = true;
            ecc2.ShowResults(search, ReportBuilder.DB.dataSource.search);
            ecc2.Visible = true;
            EditAliasControl.Visible = false;
        }

        protected void btnNotMapped_Click(object sender, EventArgs e)
        {
            EditCompanyControl.UsedSearch = false;
            ecc2.ShowNotMapped();
            ecc2.Visible = true;
            EditAliasControl.Visible = false;
        }

        protected void btnAlias_Click(object sender, EventArgs e)
        {
            int geoID = Convert.ToInt32(ddGEOs.SelectedValue);                
            EditAliasControl.ShowResults(geoID);
            EditAliasControl.Visible = true;
            ecc2.Visible = false;
        }
    }
}