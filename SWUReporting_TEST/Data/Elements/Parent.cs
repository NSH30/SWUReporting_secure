using System;
using System.Data.SqlClient;


namespace ReportBuilder
{

    public class Parent
    {
        #region Properties
        public string VAR_Parent { get; set; }
        public int ID { get; set; }
        private DB db { get; set; }
        #endregion

        #region Constructor
        public Parent(DB dbIn)
        {
            db = dbIn;
        }
        #endregion

        public void Insert()
        {
            if (ID > 0)
            {
                //it exists, so update
                string query = @"update VARParents
                                set VAR_Parent = @name,
                                WHERE ID = @id;";
                SqlCommand cmd = new SqlCommand(query, db.dbConn);
                //parameters            
                cmd.Parameters.AddWithValue("@name", VAR_Parent);
                cmd.Parameters.AddWithValue("@id", ID);
                dynamic r = null;
                r = cmd.ExecuteScalar();
            }
            else
            {
                //doesn't yet exist
                string query = @"DECLARE @id INT;
                                insert into VARParents(VAR_Parent) 
                               values(@name)
                               SET @id = scope_identity();
                               SELECT @id as OutputID;";
                SqlCommand cmd = new SqlCommand(query, db.dbConn);
                //parameters            
                cmd.Parameters.AddWithValue("@name", VAR_Parent);
                dynamic r = null;
                r = cmd.ExecuteScalar();
                ID = Convert.ToInt32(r);
            }
        }

        public bool GetByName(string parentName, SqlConnection db)
        {
            SqlCommand cmd;
            string query = @"SELECT ID, VAR_Parent from VARParents                        
                            where VAR_Parent = @varName;";
            cmd = new SqlCommand(query, db);
            cmd.Parameters.AddWithValue("@varName", parentName);
            SqlDataReader r = cmd.ExecuteReader();
            if (r.HasRows)
            {
                while (r.Read())
                {
                    ID = (int)r["ID"];
                    VAR_Parent = (string)r["VAR_Parent"];
                }
                r.Close();
                return true;
            }
            r.Close();
            return false;
        }

        public bool GetByID(int parentID, SqlConnection db)
        {
            SqlCommand cmd;
            string query = @"SELECT ID, VAR_Parent FROM VARParents                        
                            where ID = @varID;";
            cmd = new SqlCommand(query, db);
            cmd.Parameters.AddWithValue("@varID", parentID);
            SqlDataReader r = cmd.ExecuteReader();
            if (r.HasRows)
            {
                while (r.Read())
                {
                    ID = (int)r["ID"];
                    VAR_Parent = (string)r["VAR_Parent]"];
                }
                r.Close();
                return true;
            }
            r.Close();
            return false;
        }
    }
}