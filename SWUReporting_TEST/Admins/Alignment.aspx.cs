using ReportBuilder;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SWUReporting_TEST
{
    public partial class Alignment : System.Web.UI.Page
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
                FillGrid();
            }
        }

        private void FillGrid()
        {
            //DB db = new DB();
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

        protected void btnClose_Click(object sender, EventArgs e)
        {
            ca = null;
            points = 0;
        }

        //protected void pnlEdit_Load(object sender, EventArgs e)
        //{
        //    tbAlignment.Text = "";            
        //}

        protected void pnlEdit_PreRender(object sender, EventArgs e)
        {
            
        }

        protected void btnAddCourse_Click(object sender, EventArgs e)
        {
            fillForm();
            mpe1.Show();
        }

        protected void lnkSaveCourse_Click(object sender, EventArgs e)
        {
            db.Connect();
            Course c = new Course(db);
            CourseAlias ca = new CourseAlias(db);
            
            c.ID = Convert.ToInt32(ddCourses.SelectedItem.Value);
            c.GetByID();
            ca.alignment_points = Convert.ToInt32( tbNewAlignmentPoints.Text);
            ca.CourseNameAlias = tbNewCourseName.Text;
            //ca.domain_id = something;
            if (tbNewDefaultTarget.Text != "") { 
                ca.has_target = 1;  //if there is a target value
                ca.Target = (float)Convert.ToDouble(tbNewDefaultTarget.Text);
            }
            ca.domain_id = Convert.ToInt32(ddDomains.SelectedItem.Value);
            ca.Insert();
            db.Disconnect();
        }

        protected void btnCloseCourse_Click(object sender, EventArgs e)
        {
            ca = null;
        }

        protected void lnkEditAlignment_Click1(object sender, EventArgs e)
        {
            courseID = Convert.ToInt32((sender as LinkButton).CommandArgument.ToString());

            DB db = new DB();
            db.Connect();
            if (ca == null) { ca = new CourseAlias(db); }
            ca.GetByID(courseID);
            db.Disconnect();
            points = ca.alignment_points;
            tbCourseName.Text = ca.CourseNameAlias;
            tbAlignment.Text = points.ToString();
            tbTarget.Text = ca.Target.ToString();
            mpe8.Show();
        }

        /// <summary>
        /// save an existing course alias
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void lnkSave_Click(object sender, EventArgs e)
        {
            DB db = new DB();
            db.Connect();
            ca.alignment_points = Convert.ToInt32(tbAlignment.Text);
            ca.CourseNameAlias = tbCourseName.Text;
            ca.Target = (float)Convert.ToDouble(tbTarget.Text);
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

    }
}