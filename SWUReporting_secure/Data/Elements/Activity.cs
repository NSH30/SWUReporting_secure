using System;
using System.Data.SqlClient;

namespace SWUReporting
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
        public decimal? progress { get; set; }
        public decimal? quizScore { get; set; }
        private DB db { get; set; }
        public string SelectionCriteria { get; set; }
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

        /// <summary>
        /// new version to correctly update only current course completions, 
        /// ignoring expired certs unless they have been recently completed
        /// </summary>
        /// <returns>true if successful</returns>
        public bool Insert2()
        {
            //query - check if exists, if not, insert, return ID, return AliasID and ParentNameID - if empty, user intervention needed         
            string query = @"DECLARE @ActID as int, @recordStatus nvarchar(20), @recordEnrollDate DATETIME;
                            SELECT @ActID = ID, @recordStatus = [status], @recordEnrollDate = [enrollDate] FROM (select ID, [status], enrollDate from Activities where [course_id] = @CourseID AND [learner_id] = @LearnerID) as T1;
                            IF  @ActID IS NULL
                            BEGIN
                               insert into Activities(enrollDate, startDate, completionDate, status, course_id, learner_id, progress, quiz_score) 
                               values(@enrollDate, @startDate, @completionDate, @status, @CourseID, @LearnerID, @progress, @quiz_score)
                               SET @ActID = scope_identity();
                            END
                            ELSE 
                                BEGIN
	                                SET @recordStatus = (select [status] from Activities where ID = @ActID);
                                    --expired, incoming record is not a new completion
                                    IF @recordStatus = 'Expired' AND @enrollDate < DATEADD(year, 2, @recordEnrollDate)
                                        BEGIN
                                        --IGNORE an UPDATE in this condition
                                        PRINT 'Still Expired'
                                        END
	                                IF @status = 'Completed' AND @recordStatus != 'Completed'
		                                UPDATE Activities SET enrollDate = @enrollDate, startDate = @startDate, completionDate = @completionDate, status = @status, progress = @progress, quiz_score = @quiz_score WHERE ID = @ActID
	                                ELSE IF @recordStatus = 'Not Started' AND @status = 'In Progress' 
		                                UPDATE Activities SET enrollDate = @enrollDate, startDate = @startDate, status = @status, progress = @progress, quiz_score = @quiz_score WHERE ID = @ActID;
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

        /// <summary>
        /// updated version to handle expiring certification courses
        /// using a pre-process query method to mark expired certifications instead
        /// </summary>
        /// <returns>true if successful</returns>
        public bool Insert2DoNotUse()
        {

            //query - check if exists, if not, insert, return ID, return AliasID and ParentNameID - if empty, user intervention needed         
            string query = @"DECLARE @ActID as int, @recordStatus nvarchar(20), @recordEnrollDate DATETIME;
                            SELECT @ActID = ID, @recordStatus = [status], @recordEnrollDate = [enrollDate] FROM (select ID, [status], enrollDate from Activities where [course_id] = @CourseID AND [learner_id] = @LearnerID) as T1;

                            IF  @ActID IS NULL
                            BEGIN
                               insert into Activities(enrollDate, startDate, completionDate, status, course_id, learner_id, progress, quiz_score) 
                               values(@enrollDate, @startDate, @completionDate, @status, @CourseID, @LearnerID, @progress, @quiz_score)
                               SET @ActID = scope_identity();
                            END
                            ELSE 
                            BEGIN
	                            SET @recordStatus = (select [status] from Activities where ID = @ActID);
		                        print('Record Status:' +  @recordStatus); 
		                        --Expired condition
		                        IF @recordStatus = 'Completed' AND @autoEnrolled = 1 AND @status = 'Not Started' AND @enrollDate > DATEADD(year, 2, @recordEnrollDate)
			                        BEGIN
			                        UPDATE Activities SET status = 'Expired', enrollDate = @enrollDate, completionDate = null WHERE ID = @ActID;
			                        --print 'set to expired.';
			                        END
		                        --Re-certification, learner started right away
		                        ELSE IF @recordStatus = 'Completed' AND @autoEnrolled = 1 AND @status = 'In Progress' AND @enrollDate > DATEADD(year, 2, @recordEnrollDate)
			                        BEGIN
			                        UPDATE Activities SET status = @status, enrollDate = @enrollDate, completionDate = null, progress = @progress, quiz_score = @quiz_score WHERE ID = @ActID;
			                        --print 'set to In Progress from re-certification.';
			                        END
		                        --new completion condition
	                            ELSE IF @status = 'Completed' AND @recordStatus !='Completed'
		                            BEGIN 
			                        UPDATE Activities SET startDate = @startDate, completionDate = @completionDate, status = @status, progress = @progress, quiz_score = @quiz_score WHERE ID = @ActID;
			                        --print 'set to completed - first case';
			                        END
		                        --moved to In Progress condition
	                            ELSE IF (@recordStatus = 'Not Started' OR @recordStatus = 'Expired') AND @status = 'In Progress' 
		                            BEGIN
			                        UPDATE Activities SET startDate = @startDate, [status] = @status, progress = @progress, quiz_score = @quiz_score WHERE ID = @ActID;
			                        --print 'change to in progress';
			                        END
		                        --updated In Progress condition with time spent on course
                                ELSE IF @recordStatus = 'In Progress'
                                    BEGIN
			                        UPDATE Activities SET progress = @progress, quiz_score = @quiz_score WHERE ID = @ActID;
			                        --print 'updated In Progress info'
			                        END
		                        --Unenrolled condition
                                ELSE IF @status = 'Unenrolled'
			                        BEGIN
                                    UPDATE Activities SET status = @status WHERE ID = @ActID;	
			                        --print 'set Unenrolled'
			                        END
		                        ELSE
			                        print 'ignored case'
                            END
                            select @ActID AS OutputID;";
            SqlCommand cmd = new SqlCommand(query, db.dbConn);
            //parameters            
            cmd.Parameters.AddWithValue("@CourseID", course.ID);
            cmd.Parameters.AddWithValue("@LearnerID", learner.ID);
            if (SelectionCriteria == "auto enrolled") { cmd.Parameters.AddWithValue("@autoEnrolled", 1); }
            else { cmd.Parameters.AddWithValue("@autoEnrolled", 0); }
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