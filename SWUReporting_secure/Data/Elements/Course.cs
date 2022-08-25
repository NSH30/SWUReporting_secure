using System;
using System.Data.SqlClient;

namespace ReportBuilder
{

    public class Course
    {
        #region Properties
        public int ID { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        //public string Alias { get; set; }
        public int AliasID { get; set; }
        public CourseAlias Alias { get; set; }
        private DB db { get; set; }
        #endregion
        #region Constructor
        /// <summary>
        /// basic constructor
        /// <param name="dbIn">DB instance</param>
        /// </summary>
        public Course(DB dbIn) { db = dbIn; }
        #endregion
        


        public void GetByID()
        {
            SqlCommand cmd;
            string query = @"SELECT ID, Name, alias_id, Type from Courses                            
                            where ID = @ID;";
            cmd = new SqlCommand(query, db.dbConn);
            cmd.Parameters.AddWithValue("@ID", ID);
            SqlDataReader r = cmd.ExecuteReader();
            if (r.HasRows)
            {
                while (r.Read())
                {
                    //ID = (int)r["ID"];  //already have the ID, don't get it again
                    Name = (string)r["Name"];
                    if (!r.IsDBNull(r.GetOrdinal("alias_id")))
                    {
                        AliasID = (int)(long)r["alias_id"];
                    }
                    if (!r.IsDBNull(r.GetOrdinal("Type")))
                    {
                        Type = (string)r["Type"];
                    }
                }
                r.Close();
            }
            if (AliasID > 0)
            {
                Alias = new CourseAlias(db);
                Alias.GetByID(AliasID);
            }
        }

        public bool Update()
        {
            string query = @"UPDATE Courses SET alias_id = @alias, Name = @Name, Type = @type WHERE ID = @ID;";
            SqlCommand cmd = new SqlCommand(query, db.dbConn);
            //parameters            
            cmd.Parameters.AddWithValue("@alias", AliasID);
            cmd.Parameters.AddWithValue("@Name", Name);
            if (Type == null)
            {
                cmd.Parameters.AddWithValue("@type", "");
            }
            else
            {
                cmd.Parameters.AddWithValue("@type", Type);
            }

            cmd.Parameters.AddWithValue("@ID", ID);
            var r = cmd.ExecuteNonQuery();
            if ((int)r == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        public bool Insert()
        {
            //query - check if exists, if not, insert, return ID, return AliasID and ParentNameID - if empty, user intervention needed            
            string query = @"DECLARE @CourseID as int, @Added as int = 0;
                            set @CourseID = (select ID from Courses where [Name] = @CourseName);
                            IF  @CourseID IS NULL
                                BEGIN
                                   set @Added = 1;
                                   insert into Courses([Name], [Type]) 
                                   values(@CourseName, @Type)
                                   SET @CourseID = scope_identity();
                                END                            
                            select @CourseID AS OutputID;
                            select @Added as AddedCount;";
            SqlCommand cmd = new SqlCommand(query, db.dbConn);
            //parameters           
            if (Name.Length > 100)  //check for long course names - SQL Table limitation
                Name = Name.Substring(0, 100); 
            cmd.Parameters.AddWithValue("@CourseName", Name);
            cmd.Parameters.AddWithValue("@Type", Type);
            //var r = cmd.ExecuteScalar();
            int r = -1;
            int count = 0;
            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                r = (int)reader.GetValue(0);
            }

            reader.NextResult();

            while (reader.Read())
            {
                count = (int)reader.GetValue(0);
            }
            reader.Close();
            //collect response and set ID to new ID            
            ID = Convert.ToInt32(r);
            if (count == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}