using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SWUReporting
{
    public partial class EditCourses : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            //fill the datagrid then show it
            editCourseControl.ShowResults(tbSearch.Text, SWUReporting.DB.dataSource.search);
            editCourseControl.Visible = true;
            editCourseAliasControl.Visible = false;
        }

        protected void btnAlias_Click(object sender, EventArgs e)
        {
            editCourseAliasControl.FillGrid();
            editCourseAliasControl.Visible = true;
            editCourseControl.Visible = false;
        }
    }
}