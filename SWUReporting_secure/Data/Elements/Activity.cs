using System;
using System.Data.SqlClient;

namespace ReportBuilder
{

    public class Activity
    {
        #region Properties
        public int ID { get; set; }
        public Course course { get; set; }
        public DateTime? enrollDate { get; set; }
        public DateTime? startDate { get; set; }
        public DateTime? completionDate { get; set; }
        public string status { get; set; }
        public Learner learner { get; set; }
        public Decimal? progress { get; set; }
        public Decimal? quizScore { get; set; }
        private DB db { get; set; }
        #endregion

        #region Constructors
        public Activity(DB dbIn)
        {
            db = dbIn;
        }
        #endregion
        public bool Insert()
        {
            //query - check if exists, if not, insert, return ID, return AliasID and ParentNameID - if empty, user intervention needed         
            string query = @"DECLARE @ActID as int, @recordStatus nvarchar(20);
                            set @ActID = (select ID from Activities where [course_id] = @CourseID AND [learner_id] = @LearnerID);
                            IF  @ActID IS NULL
                            BEGIN
                               insert into Activities(enrollDate, startDate, completionDate, status, course_id, learner_id, progress, quiz_score) 
                               values(@enrollDate, @startDate, @completionDate, @status, @CourseID, @LearnerID, @progress, @quiz_score)
                               SET @ActID = scope_identity();
                            END
                            ELSE 
                                BEGIN
	                                SET @recordStatus = (select [status] from Activities where ID = @ActID);
	                                IF @status = 'Completed' AND @recordStatus != 'Completed'
		                                UPDATE Activities SET startDate = @startDate, completionDate = @completionDate, status = @status, progress = @progress, quiz_score = @quiz_score WHERE ID = @ActID
	                                ELSE IF @recordStatus = 'Not Started' AND @status = 'In Progress' 
		                                UPDATE Activities SET startDate = @startDate, status = @status, progress = @progress, quiz_score = @quiz_score WHERE ID = @ActID;
                                    ELSE IF @recordStatus = 'In Progress'
                                        UPDATE Activities SET progress = @progress, quiz_score = @quiz_score WHERE ID = @ActID;
                                    ELSE IF @status = 'Unenrolled'
                                        UPDATE Activities SET status = @status WHERE ID = @ActID;	
                                END
                            select @ActID AS OutputID;";
            SqlCommand cmd = new SqlCommand(query, db.dbConn);
            //parameters            
            cmd.Parameters.AddWithValue("@CourseID", course.ID);
            cmd.Parameters.AddWithValue("@LearnerID", learner.ID);

            if (enrollDate == null)
                cmd.Parameters.AddWithValue("@enrollDate", DBNull.Value);
            else
                cmd.Parameters.AddWithValue("@enrollDate", enrollDate);

            if (startDate == null)
                cmd.Parameters.AddWithValue("@startDate", DBNull.Value);
            else
                cmd.Parameters.AddWithValue("@startDate", startDate);

            if (completionDate == null)
                cmd.Parameters.AddWithValue("@completionDate", DBNull.Value);
            else
                cmd.Parameters.AddWithValue("@completionDate", completionDate);

            //cmd.Parameters.AddWithValue("@completionDate", completionDate);
            cmd.Parameters.AddWithValue("@status", status);

            //added decimal columns
            if (progress != null)
                cmd.Parameters.AddWithValue("@progress", progress);
            else
                cmd.Parameters.AddWithValue("@progress", DBNull.Value);
            if (quizScore != null)
                cmd.Parameters.AddWithValue("@quiz_score", quizScore);
            else
                cmd.Parameters.AddWithValue("@quiz_score", DBNull.Value);

            dynamic r = null;
            try
            {
                r = cmd.ExecuteScalar();
            }
            catch (Exception e)
            {                
                return false;
            }
            //collect response and set ID to new ID            
            ID = Convert.ToInt32(r);
            return true;
        }
    }
}