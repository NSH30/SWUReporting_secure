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
    public partial class EditCourseControl : System.Web.UI.UserControl
    {
        protected static int courseID;
        protected static string searchString;
        public DB db;
        protected static DataTable cTable;
        public DataTable CourseTable {
            get { return cTable; }
            set { cTable = value; } }
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {

            }
            if (db == null)
            {
                db = new DB();
            }

        }

        public void ShowResults(string searchText, DB.dataSource source)
        {
            DataTable dt = null;
            db.Connect();

            string escapedFilter = string.Format("%{0}%", searchText);
            dt = db.GetCourseListWithAlias(escapedFilter);
            searchString = escapedFilter;
 
            db.Disconnect();
            gvCourses.DataSource = dt;
            gvCourses.DataBind();
            CourseTable = dt;  
        }

        private void fillEditForm()
        {
            //DB db = new DB();
            db.Connect();            
            //fill the course alias dropdown
            DataTable dt = new DataTable();
            dt = db.CourseNames;
            ddAlias.DataSource = dt;
            ddAlias.DataTextField = "CourseNameAlias";
            ddAlias.DataValueField = "ID";
            ddAlias.DataBind();
            ddAlias.Items.Insert(0, new ListItem("Select a Course Alias", "-1"));

            DataTable dt2 = new DataTable();
            dt2 = db.GetCourseListWithAlias(ID: courseID);
            db.Disconnect();
            //should only have one row
            if (dt2.Rows.Count == 1)
            {
                tbCourse.Text = dt2.Rows[0]["Name"].ToString();
                if (!dt2.Rows[0].IsNull("CourseNameAlias"))
                {
                    tbAlias.Text = dt2.Rows[0]["CourseNameAlias"].ToString();
                }
                else
                {
                    tbAlias.Text = "";
                }
                if (tbAlias.Text != "")
                {
                    ddAlias.Items.FindByText(tbAlias.Text).Selected = true;
                }                
            }                        
        }


        protected void lnkEditCourse_Click(object sender, EventArgs e)
        {
            courseID = Convert.ToInt32((sender as LinkButton).CommandArgument);
            fillEditForm();            
            btnSave.Enabled = true;
            ModalPopupExtender3.Show();
        }

        protected void ddAlias_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            db.Connect();
            //save the updated alias back to the course
            Course c = new Course(db);
            c.ID = courseID;
            
            c.GetByID( );
            c.AliasID = Convert.ToInt32(ddAlias.SelectedValue);
            c.Update();
            db.Disconnect();
        }

        protected void btnClose_Click(object sender, EventArgs e)
        {

        }

        protected void lnkSaveExcel_Click(object sender, EventArgs e)
        {
            CourseTable.TableName = "Courses";
            ReportIO.DownloadSingleSheet(CourseTable);
        }
    }
}