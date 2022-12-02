using System;
using System.Data.SqlClient;

namespace SWUReporting
{

    public class CourseAlias
    {
        #region Properties
        public int ID { get; set; }
        public string CourseNameAlias { get; set; }

        public int domain_id { get; set; }

        public int has_target { get; set; }
        public int alignment_points { get; set; }
        public float Target { get; set; }
        private DB db { get; set; }
        public int KPI { get; set; }
        #endregion

        #region Constructor
        public CourseAlias(DB dbIn)
        {
            db = dbIn;
        }
        #endregion

        public void GetByID(int courseID)
        {
            string query = @"select * from CourseAlias where ID = @id;";
            SqlCommand cmd = new SqlCommand(query, db.dbConn);
            //parameters            
            cmd.Parameters.AddWithValue("@id", courseID);
            SqlDataReader r = cmd.ExecuteReader();
            if (r.HasRows)
            {
                //should only have one row                
                while (r.Read())
                {
                    ID = (int)(long)r["ID"];
                    Target = (float)(double)r["Target"];
                    CourseNameAlias = (string)r["CourseNameAlias"];
                    if (!r.IsDBNull(r.GetOrdinal("domain_id")))
                    {
                        domain_id = (int)r["domain_id"];
                    }
                    else
                    {
                        domain_id = -1;
                    }

                    has_target = (int)r["has_target"];
                    if (!r.IsDBNull(r.GetOrdinal("alignment_points")))
                    {
                        alignment_points = (int)r["alignment_points"];
                    }
                    if (!r.IsDBNull(r.GetOrdinal("kpi")))
                        KPI = (int)r["kpi"];
                    else
                        KPI = 0;
                }
            }
            r.Close();
        }
        public bool Insert()  //inserts a new CourseAlias or updates an existing alias
        {
            string query = "";
            if (ID != 0)
            {

                //update the existing CourseAlias
                query = @"UPDATE CourseAlias SET has_target = @hasTarget, Target = @target, domain_id = @domainId, 
                        CourseNameAlias = @CourseName, alignment_points = @Points, kpi = @kpi
                        where ID = @CourseID;";
            }
            else
            {
                query = @"DECLARE @CourseID as int, @Added as int = 0;
                        set @CourseID = (select ID from CourseAlias where [CourseNameAlias] = @CourseName);
                        IF  @CourseID IS NULL
                            BEGIN
                                set @Added = 1;
                                insert into CourseAlias([CourseNameAlias], [has_target], [Target], [domain_id], [alignment_points], [kpi]) 
                                values(@CourseName, @hasTarget, @target, @domainId, @Points, @kpi)
                                SET @CourseID = scope_identity();
                            END
                        ELSE --courseAlias exists, Update has_target, Target, domain_id and alignment_points
                            BEGIN
		                        UPDATE CourseAlias SET has_target = @hasTarget, Target = @target, domain_id = @domainId, 
                                alignment_points = @Points, kpi = @kpi WHERE ID = @CourseID;
                            END
                        select @CourseID AS OutputID;
                        select @Added as AddedCount;";
            }
            if (db.dbConn.State != System.Data.ConnectionState.Open)
                db.dbConn.Open();
            SqlCommand cmd = new SqlCommand(query, db.dbConn);
            cmd.Parameters.AddWithValue("@CourseName", CourseNameAlias);
            cmd.Parameters.AddWithValue("@hasTarget", has_target);
            cmd.Parameters.AddWithValue("@target", Target);
            cmd.Parameters.AddWithValue("@domainId", domain_id);
            cmd.Parameters.AddWithValue("@Points", alignment_points);
            cmd.Parameters.AddWithValue("@kpi", KPI);
            if (ID != 0)
            {
                cmd.Parameters.AddWithValue("@CourseID", ID);
            }
            int r = -1;
            int count = 0;
            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                r = (int)reader.GetValue(0);
                ID = Convert.ToInt32(r);
            }

            reader.NextResult();

            while (reader.Read())
            {
                count = (int)reader.GetValue(0);
            }
            reader.Close();

            //collect response and set ID to new ID            
            if (count == 0 && reader.RecordsAffected == 0)
            {
                return false;
            }
            return true;
        }
    }
}