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
    public partial class EditCourseAliasControl : System.Web.UI.UserControl
    {
        protected static DataTable dt;
        protected static int courseID;
        protected static CourseAlias ca;
        protected static int points;
        protected DB db;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (db == null)
            {
                db = new DB();
            }
            if (!IsPostBack)
            {
                //FillGrid();
            }
        }

        internal void FillGrid()
        {
            db.Connect();
            if (dt != null)
            {
                dt.Clear();
            }
            dt = db.getCoursePoints();
            db.Disconnect();
            gvAlignment.DataSource = dt;
            gvAlignment.DataBind();
            if (!gvAlignment.Visible) { gvAlignment.Visible = true; }
        }

        private void fillForm()
        {
            //load Alias filter list
            DataTable dt = new DataTable();
            db.Connect();
            dt = db.GetCourseList();
            ddCourses.DataSource = dt;
            ddCourses.DataTextField = "Name";
            ddCourses.DataValueField = "ID";
            ddCourses.DataBind();
            ddCourses.Items.Insert(0, new ListItem("Select the associated course", "-1"));

            DataTable dtDomains = new DataTable();
            dtDomains = db.GetDomains();
            db.Disconnect();
            ddDomains.DataSource = dtDomains;
            ddDomains.DataTextField = "DomainName";
            ddDomains.DataValueField = "ID";
            ddDomains.DataBind();
            ddDomains.Items.Insert(0, new ListItem("Select the Domain", "-1"));
        }

        protected void lnkShowCourses_Click(object sender, EventArgs e)
        {
            courseID = Convert.ToInt32((sender as LinkButton).CommandArgument.ToString());
            //show all courses connected to the course alias
            DataTable dt = new DataTable();
            db.Connect();
            Alias a = new Alias(courseID, db);
            dt = db.GetCoursesFromAlias(a);
            db.Disconnect();
            gvCourses.DataSource = dt;
            gvCourses.DataBind();
            if (dt.Rows.Count < 1)
            {
                lblMessage.Visible = true;
            }
            else
            {
                lblMessage.Visible = false;
            }
            gvCourses.Visible = true;
            mpe11.Show();
        }

        protected void btnAddCourse_Click(object sender, EventArgs e)
        {
            fillForm();
            mpe1.Show();
        }

        protected void lnkEditAlignment_Click(object sender, EventArgs e)
        {
            courseID = Convert.ToInt32((sender as LinkButton).CommandArgument.ToString());

            db.Connect();
            if (ca == null)
                ca = new CourseAlias(db); 
            ca.GetByID(courseID);
            //db.Disconnect();  //try keeping the connection open while editing
            points = ca.alignment_points;
            tbCourseName.Text = ca.CourseNameAlias;
            tbAlignment.Text = points.ToString();
            tbTarget.Text = ca.Target.ToString();
            DataTable dt = getKPIList();
            ddKPI.DataSource = dt;
            ddKPI.DataTextField = "Name";
            ddKPI.DataValueField = "Value";
            ddKPI.DataBind();
            //if (ca.KPI != null)
            //{
                ddKPI.SelectedValue = ca.KPI.ToString();               
            //}
            mpe8.Show();
        }

        private DataTable getKPIList()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Name");
            dt.Columns.Add("Value");
            dt.Rows.Add(new object[] { "", 0 });
            dt.Rows.Add(new object[] { "Sales", 1});
            dt.Rows.Add(new object[] { "Tech", 2 });
            return dt;
        }

        protected void lnkSaveCourse_Click(object sender, EventArgs e)
        {
            db.Connect();
            Course c = new Course(db);
            CourseAlias ca = new CourseAlias(db);
            
            c.ID = Convert.ToInt32(ddCourses.SelectedItem.Value);
            c.GetByID();
            ca.alignment_points = Convert.ToInt32(tbNewAlignmentPoints.Text);
            ca.CourseNameAlias = tbNewCourseName.Text;
            if (tbNewDefaultTarget.Text != "")
            {
                ca.has_target = 1;  //if there is a target value
                ca.Target = (float)Convert.ToDouble(tbNewDefaultTarget.Text);
            }
            ca.domain_id = Convert.ToInt32(ddDomains.SelectedItem.Value);
            ca.Insert();
            //also update the course with the newly created course alias
            c.AliasID = ca.ID;
            c.Alias = ca;
            c.Update();
            db.Disconnect();
        }

        protected void btnCloseCourse_Click(object sender, EventArgs e)
        {
            db.Disconnect();
            ca = null;
        }

        protected void pnlEdit_PreRender(object sender, EventArgs e)
        {

        }

        protected void lnkSave_Click(object sender, EventArgs e)
        {
            
            db.Connect();
            ca.alignment_points = Convert.ToInt32(tbAlignment.Text);
            ca.CourseNameAlias = tbCourseName.Text;
            ca.Target = (float)Convert.ToDouble(tbTarget.Text);
            ca.KPI = Convert.ToInt32(ddKPI.SelectedValue);
            if (ca.Target > 0)
            {
                ca.has_target = 1;
            }
            else
            {
                ca.has_target = 0;
            }
            ca.Insert();
            db.Disconnect();
            ca = null;
            FillGrid();
        }

        protected void btnClose_Click(object sender, EventArgs e)
        {
            ca = null;
            points = 0;
        }
    }
}