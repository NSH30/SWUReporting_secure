using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace ReportBuilder
{
    class User
    {

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Trigram { get; set; }
        public int Admin { get; set; }
        public DB db { get; set; }

        public void Adduser()
        {
            SqlCommand cmd;
            //query - check if exists, if not, insert, return ID, return AliasID and ParentNameID - if empty, user intervention needed            
            string query = @"insert into Users(Fname,Lname,Trigram,Admin) 
                               values(@Fname, @Lname, @Trigram, @Admin );";                                                     

            cmd = new SqlCommand(query, db.dbConn);
            //parameters            
            cmd.Parameters.AddWithValue("@Fname", FirstName);
            cmd.Parameters.AddWithValue("@Lname", LastName);
            cmd.Parameters.AddWithValue("@Trigram", Trigram);
            cmd.Parameters.AddWithValue("@Admin", Admin);

            dynamic r = null;
            try
            {
                r = cmd.ExecuteScalar();
            }
            catch (Exception e)
            {
                return;

            }
            db.Disconnect();
        }

        internal void editUser()
        {
            SqlCommand cmd;
            //query - check if exists, if not, insert, return ID, return AliasID and ParentNameID - if empty, user intervention needed            
            string query = @"update users set fname = @Fname, lname = @Lname, admin = @Admin
                               WHERE trigram = @trigram";

            cmd = new SqlCommand(query, db.dbConn);
            //parameters            
            cmd.Parameters.AddWithValue("@Fname", FirstName);
            cmd.Parameters.AddWithValue("@Lname", LastName);
            cmd.Parameters.AddWithValue("@Trigram", Trigram);
            cmd.Parameters.AddWithValue("@Admin", Admin);

            dynamic r = null;
            try
            {
                r = cmd.ExecuteScalar();
            }
            catch (Exception e)
            {
                return;

            }
            db.Disconnect();

        }

        internal void GetUserByTrigram(string trigram)
        {
            string query = "select * from users where trigram = @trigram;";
            SqlCommand cmd = new SqlCommand(query, db.dbConn);

            cmd.Parameters.AddWithValue("@Trigram", trigram);

            SqlDataReader r = cmd.ExecuteReader();
            if (r.HasRows)
            {
                while (r.Read())
                {

                    FirstName = (string)r["Fname"];
                    LastName = (string)r["Lname"];
                    Trigram = (string)r["Trigram"];
                    Admin = (int)r["Admin"];                  
                }
            }
            else
            {
                return;
            }
            r.Close();
            db.Disconnect();

        }
    }
}