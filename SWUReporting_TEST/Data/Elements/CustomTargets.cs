using System;
using System.Data.SqlClient;

namespace ReportBuilder
{

    public class CustomTargets
    {
        #region Properties
        int id;
        public int ID { get { return id; } set { id = value; } }
        public float Target { get; set; }
        public int courseAliasID { get; set; }
        public int varAliasID { get; set; }
        private DB db { get;set;}
        #endregion

        #region Constructors
        public CustomTargets(DB dbIn)
        {
            db = dbIn;
        }
        #endregion
        public void Insert()
        {
            if (ID > 0)
            {
                //exists already, just update
                string query = @"update CustomTargets
                                set Target = @value,
                                course_alias_id = @courseID, var_alias_id = @varID
                                WHERE ID = @id;";
                SqlCommand cmd = new SqlCommand(query, db.dbConn);
                //parameters            
                cmd.Parameters.AddWithValue("@value", Target);
                cmd.Parameters.AddWithValue("@courseID", courseAliasID);
                cmd.Parameters.AddWithValue("@varID", varAliasID);
                cmd.Parameters.AddWithValue("@id", ID);
                dynamic r = null;
                r = cmd.ExecuteScalar();
                id = ID;//Convert.ToInt32(r);
            }
            else
            {
                //doesn't exist, add it
                string query = @"DECLARE @id INT;
                                insert into CustomTargets(Target, course_alias_id, var_alias_id) 
                               values(@value, @courseID, @varID)
                               SET @id = scope_identity();
                               SELECT @id as OutputID;";
                SqlCommand cmd = new SqlCommand(query, db.dbConn);
                //parameters            
                cmd.Parameters.AddWithValue("@value", Target);
                cmd.Parameters.AddWithValue("@courseID", courseAliasID);
                cmd.Parameters.AddWithValue("@varID", varAliasID);
                dynamic r = null;
                r = cmd.ExecuteScalar();
                id = Convert.ToInt32(r);
            }
            if (varAliasID > 0)
            {
                //also update the VARAlias table
                string query = @"UPDATE VARAlias SET custom_targets = 1 WHERE ID = @id;";
                SqlCommand cmd = new SqlCommand(query, db.dbConn);
                cmd.Parameters.AddWithValue("@id", varAliasID);
                var cnt = cmd.ExecuteNonQuery();
                if (cnt == 0)
                {
                    //there was a problem - it didn't update the VARAlias table???                    
                }
            }
        }

        public void GetByID(int varID, int courseID)
        {
            string query = "SELECT * FROM CustomTargets WHERE course_alias_id = @courseID AND var_alias_id = @varID;";
            SqlCommand cmd = new SqlCommand(query, db.dbConn);
            //parameters            
            cmd.Parameters.AddWithValue("@courseID", courseID);
            cmd.Parameters.AddWithValue("@varID", varID);
            SqlDataReader r = cmd.ExecuteReader();
            if (r.HasRows)
            {
                //should only have one row                
                while (r.Read())
                {
                    id = (int)r["ID"];
                    Target = (float)(double)r["Target"];
                    courseAliasID = courseID;
                    varAliasID = varID;
                }
            }
            r.Close();
        }
    }
}