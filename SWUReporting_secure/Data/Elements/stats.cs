using System;
using System.Collections.Generic;
using System.Data.SqlClient;


namespace ReportBuilder
{
    class Stats
    {
        DB db;
        public Stats(DB dbInstance)
        {
            db = dbInstance;
        }

        public int VARAliasCount
        {
            get
            {
                string query = "SELECT COUNT(ID) as c FROM VARAlias WHERE status = 1;";
                return db.SingleValueQuery(query);
            }
        }
        public int CourseCount
        {
            get
            {
                string query = "SELECT COUNT(ID) as c FROM Courses;";
                return db.SingleValueQuery(query);
            }
        }
        public List<string> Courses { get; }
        public int LearnerCount
        {
            get
            {
                string query = "SELECT COUNT(ID) as c FROM Learners WHERE userState = 'ACTIVE';";
                return db.SingleValueQuery(query);
            }
        }

        public double FTEValue
        {
            get
            {
                string query = "SELECT SUM(fte_value) as f FROM Learners WHERE userState = 'ACTIVE';";
                db.Connect();
                SqlCommand cmd = new SqlCommand(query, db.dbConn);
                var res = cmd.ExecuteScalar();
                db.Disconnect();
                return (double)(decimal)res;
            }
        }
        public List<string> Learners { get; }
        public int VARCount
        {
            get
            {
                string query = "SELECT COUNT(ID) as c FROM VARs WHERE VARs.status = 1;";
                return db.SingleValueQuery(query);
            }
        }
        public List<string> VARs { get; }
    }
}