using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Configuration;
using IdeaBag.Portable.Data.Models;

namespace IdeaBag.Server.DataAccess
{
    public class DatabaseManager
    {
        #region Private Properties

        private string _connectionString;
        private static DatabaseManager _instance;

        #endregion


        #region Public Properties

        public static DatabaseManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    bool useDevDatabase = bool.Parse(WebConfigurationManager.AppSettings["UseDevDatabase"].ToString());
                    string connection = "";

                    if (useDevDatabase)
                        connection = WebConfigurationManager.AppSettings["DevDatabase"];
                    else
                        connection = WebConfigurationManager.AppSettings["ProdDatabase"];

                    _instance = new DatabaseManager(connection);
                }

                return _instance;
            }
        }

        #endregion


        #region Constructor

        private DatabaseManager(string connectionstring)
        {
            _connectionString = connectionstring;
        }

        #endregion


        #region Authentication 

        public LoginResultModel AuthenticateStandardUser(string userID, string passwordHash)
        {
            LoginResultModel result = new LoginResultModel();

            SqlParameter[] parameters = new SqlParameter[]{
                new SqlParameter("@UserID",userID),
                new SqlParameter("@Password",passwordHash)};

            string cmd = "SELECT * FROM Users WHERE UserID = @UserID and Password = @Password";

            try
            {
                DataSet resultSet = DatabaseHelper.ExecuteQuery(cmd, _connectionString, parameters);

                //- Determine if user exists within database
                if (resultSet != null && resultSet.Tables[0].Rows.Count > 0)
                {
                    DataTable resultTable = resultSet.Tables[0];

                    result.Message = "User successfully authenticated";
                    result.ResultStatus = LoginResultType.Success;
                    result.ConnectionIDs = GetUserConnectionIDs(userID);
                }
                else
                {
                    result.Message = "User not found";
                    result.ResultStatus = LoginResultType.UserNotFound;
                }

            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                result.ResultStatus = LoginResultType.UnexpectedException;

                LogMessageModel logmessage = new LogMessageModel(LogStatus.Error,
                    string.Format("Login exception occurred within DatabaseManager.AuthenticateStandardUser: {0}", ex.Message), -1);

                ExceptionManager.Instance.InsertLogMessage(logmessage);
            }


            return result;
        }

        #endregion


        #region Users

        /// <summary>
        /// Determine if a database record for the User already exists
        /// </summary>
        /// <param name="userid">ID for the User (should be an Email address)</param>
        /// <returns>TRUE - Database record for the User already exists; FALSE - No database record for the User exists</returns>
        public bool CheckUserExists(string userid)
        {
            bool doesexist = true;

            string cmd = "SELECT * FROM USERS WHERE UserID = @UserID";

            DataSet resultset = DatabaseHelper.ExecuteQuery(cmd, _connectionString, new SqlParameter("@UserID", userid));

            if (resultset == null || resultset.Tables.Count == 0 || resultset.Tables[0].Rows.Count == 0)
                doesexist = false;


            return doesexist;
        }

        /// <summary>
        /// Determine if the provided User account has been activated 
        /// </summary>
        /// <param name="userid">ID for the User (should be an Email address)</param>
        /// <returns>TRUE - User has been activated; FALSE - User has not yet been activated</returns>
        public bool CheckIfUserActive(string userid)
        {
            bool isactive = false;

            UserModel user = GetUserByUserID(userid);

            if (user != null && user.IsActivated)
                isactive = true;

            return isactive;
        }


        #region GET

        /// <summary>
        /// Get user associated with the provided UserID
        /// </summary>
        public UserModel GetUserByUserID(string userID)
        {
            UserModel user = null;

            string cmd = "SELECT * FROM Users WHERE UserID = @UserID";

            try
            {
                DataSet ds = DatabaseHelper.ExecuteQuery(cmd, _connectionString, new SqlParameter("@UserID", userID));

                if (ds != null && ds.Tables[0].Rows.Count > 0)
                {
                    DataRow row = ds.Tables[0].Rows[0];
                    user = new UserModel();
                    user.CreateDate = DateTime.Parse(row["CreateDate"].ToString());
                    user.LastModified = DateTime.Parse(row["LastModified"].ToString());
                    user.FirstName = row["FirstName"].ToString();
                    user.LastName = row["LastName"].ToString();
                    user.UserID = row["UserID"].ToString();
                    user.GlobalID = row["GlobalID"].ToString();
                    user.UserLoginType = (LoginType)int.Parse(row["IsFacebookLogin"].ToString());
                    user.PasswordHash = row["Password"].ToString();
                    user.IsActivated = bool.Parse(row["IsActivated"].ToString());


                }
            }
            catch (Exception ex)
            {
                LogMessageModel logmessage = new LogMessageModel(LogStatus.Error,
                    string.Format("Login exception occurred within DatabaseManager.GetUserByUserID: {0}", ex.Message), -1);

                ExceptionManager.Instance.InsertLogMessage(logmessage);
            }

            return user;
        }

        /// <summary>
        /// Get all device id's bleonging to this user
        /// </summary>
        /// <param name="userID">ID of the user the device id are associated with</param>
        public List<string> GetUserDeviceIDs(string userID)
        {
            List<string> deviceIDs = new List<string>();
            string cmd = "SELECT DeviceID FROM UserConnections WHERE UserID_FK = @UserID_FK";

            try
            {
                DataSet ds = DatabaseHelper.ExecuteQuery(cmd, _connectionString, new SqlParameter("@UserID_FK", userID));

                if (ds != null && ds.Tables[0].Rows.Count > 0)
                {
                    string device = ds.Tables[0].Rows[0]["DeviceID"].ToString();
                    deviceIDs.Add(device);
                }
            }
            catch (Exception ex)
            {
                LogMessageModel logmessage = new LogMessageModel(LogStatus.Error,
                    string.Format("Login exception occurred within DatabaseManager.GetUserDeviceIDs: {0}", ex.Message), -1);

                ExceptionManager.Instance.InsertLogMessage(logmessage);
            }

            return deviceIDs;
        }

        /// <summary>
        /// GET the current Signalr connection id associated with a specified user's device
        /// </summary>
        /// <param name="userid">ID of the user</param>
        /// <param name="deviceid">ID for the device the user is accessing</param>
        /// <returns>The Signalr connection id for that user on that device</returns>
        public string GetUserConnectionByDevice(string userid, string deviceid)
        {
            string connectionid = "";

            string cmd = "SELECT ConnectionID FROM UserConnections WHERE UserID_FK = @UserID_FK and DeviceID = @DeviceID";

            try
            {
                DataSet ds = DatabaseHelper.ExecuteQuery(cmd, _connectionString, new SqlParameter("@UserID_FK", userid), new SqlParameter("@DeviceID", deviceid));

                if (ds != null && ds.Tables[0].Rows.Count > 0)
                    connectionid = ds.Tables[0].Rows[0]["ConnectionID"].ToString();

            }
            catch (Exception ex)
            {
                LogMessageModel logmessage = new LogMessageModel(LogStatus.Error,
                    string.Format("Login exception occurred within DatabaseManager.GetUserConnectionByDevice: {0}", ex.Message), -1);

                ExceptionManager.Instance.InsertLogMessage(logmessage);
            }

            return connectionid;
        }

        /// <summary>
        /// Get all connection id's belonging to the specified user
        /// </summary>
        public List<string> GetUserConnectionIDs(string userID)
        {
            List<string> ids = new List<string>();
            string cmd = "SELECT * FROM UserConnections WHERE UserID_FK = @UserID_FK";

            try
            {
                DataSet ds = DatabaseHelper.ExecuteQuery(cmd, _connectionString, new SqlParameter("@UserID_FK", userID));

                if (ds != null && ds.Tables[0].Rows.Count > 0)
                {
                    DataTable table = ds.Tables[0];

                    //- iterate through results 
                    foreach (DataRow row in table.Rows)
                    {
                        ids.Add(row["ConnectionID"].ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                LogMessageModel logmessage = new LogMessageModel(LogStatus.Error,
                    string.Format("Login exception occurred within DatabaseManager.GetUserConnectionIDs: {0}", ex.Message), -1);

                ExceptionManager.Instance.InsertLogMessage(logmessage);
            }

            return ids;
        }

        /// <summary>
        /// Gets user associated with the Connection
        /// </summary>
        public string GetUserByConnectionID(string connectionID)
        {
            string userid = "";
            string cmd = "SELECT * FROM UserConnections WHERE Connection = @ConnectionID";

            try
            {
                DataSet ds = DatabaseHelper.ExecuteQuery(cmd, _connectionString, new SqlParameter("@ConnectionID", connectionID));

                if (ds != null && ds.Tables[0].Rows.Count > 0)
                {
                    userid = ds.Tables[0].Rows[0]["ConnectionID"].ToString();

                }
            }
            catch (Exception ex)
            {
                LogMessageModel logmessage = new LogMessageModel(LogStatus.Error,
                    string.Format("Login exception occurred within DatabaseManager.GetUserByConnectionID: {0}", ex.Message), -1);

                ExceptionManager.Instance.InsertLogMessage(logmessage);
            }

            return userid;
        }

        /// <summary>
        /// Get all active users
        /// </summary>
        public List<UserModel> GetAllActiveUsers()
        {
            List<UserModel> activeusers = new List<UserModel>();

            string cmd = "SELECT * FROM Users WHERE IsActivated = 1";

            try
            {
                DataSet ds = DatabaseHelper.ExecuteQuery(cmd, _connectionString);

                if (ds != null && ds.Tables[0].Rows.Count > 0)
                {
                    DataTable table = ds.Tables[0];

                    //- iterate through results 
                    foreach (DataRow row in table.Rows)
                    {
                        UserModel user = new UserModel();
                        user.CreateDate = DateTime.Parse(row["CreateDate"].ToString());
                        user.FirstName = row["FirstName"].ToString();
                        user.LastName = row["LastName"].ToString();
                        user.UserID = row["UserID"].ToString();
                        user.GlobalID = row["GlobalID"].ToString();
                        user.UserLoginType = (LoginType)int.Parse(row["LoginType"].ToString());
                        user.PasswordHash = row["Password"].ToString();
                        user.IsActivated = true;

                        activeusers.Add(user);
                    }
                }
            }
            catch (Exception ex)
            {
                LogMessageModel logmessage = new LogMessageModel(LogStatus.Error,
                    string.Format("Login exception occurred within DatabaseManager.GetUserConnectionIDs: {0}", ex.Message), -1);

                ExceptionManager.Instance.InsertLogMessage(logmessage);
            }

            return activeusers;
        }


        #endregion

        #region INSERT

        /// <summary>
        /// Add a new UserID / ConnectionID mapping
        /// </summary>
        public int InsertUserConnection(string userID, string connectionID)
        {
            int result = 0;
            string cmd = "INSERT INTO UserConnections(UserID_FK, ConnectionID) VALUES(@UserID_FK, @ConnectionID)";

            SqlParameter useridparam = new SqlParameter("@UserID_FK", userID);
            SqlParameter connectidparam = new SqlParameter("@ConnectionID", connectionID);
            
            try
            {
                //- check if connection is already mapped
                string existingUser = GetUserByConnectionID(connectionID);

                if(string.IsNullOrEmpty(existingUser))
                    result = DatabaseHelper.ExecuteNonQuery(cmd, _connectionString, useridparam, connectidparam);

            }
            catch (Exception ex)
            {
                LogMessageModel logmessage = new LogMessageModel(LogStatus.Error,
                    string.Format("Login exception occurred within DatabaseManager.InsertUserConnection: {0}", ex.Message), -1);

                ExceptionManager.Instance.InsertLogMessage(logmessage);

                result = -1;
            }

            return result;
        }



        #region Signup/Activation

        public SignupResultModel SignupStandardUser(UserModel user)
        {
            SignupResultModel model = new SignupResultModel();

            string cmd = "INSERT INTO USERS(GlobalID, UserID, LoginType, Password, FirstName, LastName, CreateDate, IsActivated)";
            cmd += "VALUES(@GlobalID, @UserID, @LoginType, @Password, @FirstName, @LastName, @CreateDate, @IsActivated)";

            SqlParameter globalid = new SqlParameter("@GlobalID", user.GlobalID);
            SqlParameter userid = new SqlParameter("@UserID", user.UserID);
            SqlParameter userlogintype = new SqlParameter("@LoginType", user.UserLoginType);
            SqlParameter password = new SqlParameter("@Password", user.PasswordHash);
            SqlParameter fname = new SqlParameter("@FirstName", user.FirstName);
            SqlParameter lname = new SqlParameter("@LastName", user.LastName);
            SqlParameter createdate = new SqlParameter("@CreateDate", user.CreateDate);
            SqlParameter isactive = new SqlParameter("@IsActivated", user.IsActivated);

            try
            {
                //- check if user exists
                bool doesExist = CheckUserExists(user.UserID);

                if (doesExist)
                {
                    model.ResultStatus = SignupResultType.UserExists;
                    model.Message = "The user id already exists";
                    return model;
                }

                // - insert new user into database
                int queryresult = DatabaseHelper.ExecuteNonQuery(cmd, _connectionString,
                    globalid, userid, userlogintype, password, fname, lname, createdate, isactive);

                if (queryresult < 0)
                {
                    model.ResultStatus = SignupResultType.UnexpectedException;
                    model.Message = "The database was unable to add the user record";
                }
                else
                {
                    model.ResultStatus = SignupResultType.Success;
                    model.Message = "User account successfully added";
                }
            }
            catch (Exception ex)
            {
                LogMessageModel message = new LogMessageModel(LogStatus.Error,
                    string.Format("An error occurred during user signup within DatabaseManager.SignupStandardUser: {0}", ex.Message), -1);

                model.ResultStatus = SignupResultType.UnexpectedException;
                model.Message = string.Format("An exception occurred while attempted to add the user account: {0}", ex.Message);
            }

            return model;
        }

        public void SignupFacebookUser(UserModel user)
        {

        }


        #endregion

        #endregion

        #region UPDATE

        /// <summary>
        /// Activate User record within the database
        /// </summary>
        public int ActivateUser(string userid)
        {
            int result = 0;
            string cmd = "UPDATE USERS SET IsActivated = 1 WHERE UserID = @UserID";

            try
            {
                result = DatabaseHelper.ExecuteNonQuery(cmd, _connectionString,
                    new SqlParameter("@UserID", userid));
            }
            catch (Exception ex)
            {
                LogMessageModel message = new LogMessageModel(LogStatus.Error,
                    string.Format("An error occurred during user activation within DatabaseManager.ActivateUser: {0}", ex.Message), -1);

                ExceptionManager.Instance.InsertLogMessage(message);

                result = -1;
            }

            return result;
        }

        /// <summary>
        /// Updates the signalr connection for a specific user on a specific device
        /// </summary>
        public int UpdateUserDeviceConnection(string userid, string deviceid, string connectionid)
        {
            int result = 0;
            string cmd = "UPDATE UserConnections SET ConnectionID = @ConnectionID WHERE UserID_FK = @UserID_FK and DeviceID = @DeviceID";

            try
            {
                result = DatabaseHelper.ExecuteNonQuery(cmd, _connectionString,
                    new SqlParameter("@ConnectionID", connectionid),
                    new SqlParameter("@UserID_FK", userid),
                    new SqlParameter("@DeviceID", deviceid));
            }
            catch (Exception ex)
            {
                LogMessageModel logmessage = new LogMessageModel(LogStatus.Error,
                    string.Format("Login exception occurred within DatabaseManager.UpdateUserDeviceConnection: {0}", ex.Message), -1);

                ExceptionManager.Instance.InsertLogMessage(logmessage);

                result = -1;
            }


            return 0;
        }
        
        #endregion

        #endregion


        #region User Relationships

        #region GET

        /// <summary>
        /// Gets a user's relationships 
        /// </summary>
        /// <param name="userid">Identifier for the user to get UserRelationships for</param>
        /// <param name="isactive">0 - gets all pending UserRelationships; 1 - gets all active relationships</param>
        /// <returns>List of UserModels belonging to users that this user relates to </returns>
        public Task<List<UserModel>> GetUserRelationshipsAsync(string userid, DateTime datecreated, bool isactive)
        {
            Task<List<UserModel>> usertask = new Task<List<UserModel>>(() =>
            {
                List<UserModel> users = new List<UserModel>();

                try
                {
                    string cmd = "SELECT * FROM UserRelationships WHERE (Source_UserID = @UserID or Target_UserID = @UserID) and IsActive = @IsActive and ";
                    cmd += "DateCreated > '@DateCreated'";
                    SqlParameter userparam = new SqlParameter("@UserID", userid);
                    SqlParameter createparam = new SqlParameter("@DateCreated", datecreated);
                    SqlParameter isactiveparam = new SqlParameter("@IsActive", isactive);

                    DataSet resultSet = DatabaseHelper.ExecuteQuery(cmd, _connectionString, userparam, createparam, isactiveparam);

                    //- Create UserModel's from the other user in the relationship
                    if (resultSet != null && resultSet.Tables[0].Rows.Count > 0)
                    {
                        foreach (DataRow row in resultSet.Tables[0].Rows)
                        {
                            UserModel user = new UserModel();

                            if (row["Source_UserID"].ToString() == userid)
                                user = GetUserByUserID(row["Target_UserID"].ToString());
                            else
                                user = GetUserByUserID(row["Source_UserID"].ToString());

                            users.Add(user);
                        }

                    }

                }
                catch (Exception ex)
                {

                    LogMessageModel logmessage = new LogMessageModel(LogStatus.Error,
                        string.Format("Login exception occurred within DatabaseManager.GetActiveUserRelationshipAsync: {0}", ex.Message), -1);

                    ExceptionManager.Instance.InsertLogMessage(logmessage);
                }

                return users;
            });

            return usertask;
        }

        /// <summary>
        /// Gets a user's relationships 
        /// </summary>
        /// <param name="userid">Identifier for the user to get UserRelationships for</param>
        /// <param name="isactive">0 - gets all pending UserRelationships; 1 - gets all active relationships</param>
        /// <returns>List of UserModels belonging to users that this user relates to </returns>
        public List<UserModel> GetUserRelationship(string userid, bool isactive)
        {
                List<UserModel> users = new List<UserModel>();

                try
                {
                    string cmd = "SELECT * FROM UserRelationships WHERE (Source_UserID = @UserID or Target_UserID = @UserID) and IsActive = @IsActive";
                    SqlParameter userparam = new SqlParameter("@UserID", userid);
                    SqlParameter isactiveparam = new SqlParameter("@IsActive", isactive);

                    DataSet resultSet = DatabaseHelper.ExecuteQuery(cmd, _connectionString, userparam, isactiveparam);

                    //- Create UserModel's from the other user in the relationship
                    if (resultSet != null && resultSet.Tables[0].Rows.Count > 0)
                    {
                        foreach (DataRow row in resultSet.Tables[0].Rows)
                        {
                            UserModel user = new UserModel();

                            if (row["Source_UserID"].ToString() == userid)
                                user = GetUserByUserID(row["Target_UserID"].ToString());
                            else
                                user = GetUserByUserID(row["Source_UserID"].ToString());

                            users.Add(user);
                        }

                    }

                }
                catch (Exception ex)
                {

                    LogMessageModel logmessage = new LogMessageModel(LogStatus.Error,
                        string.Format("Login exception occurred within DatabaseManager.GetActiveUserRelationshipAsync: {0}", ex.Message), -1);

                    ExceptionManager.Instance.InsertLogMessage(logmessage);
                }

                return users;
        }

        /// <summary>
        /// Gets a relationship requests sent to this user 
        /// </summary>
        /// <param name="userid">Identifier for the user to get UserRelationships for</param>
        /// <returns>List of UserModels belonging to users that have sent requests to this</returns>
        public Task<List<UserModel>> GetUserPendingRelationshipRequestsAsync(string userid)
        {
            Task<List<UserModel>> usertask = new Task<List<UserModel>>(() =>
            {
                List<UserModel> users = new List<UserModel>();

                try
                {
                    string cmd = "SELECT * FROM UserRelationships WHERE Target_UserID = @UserID and IsActive = 0";
                    SqlParameter userparam = new SqlParameter("@UserID", userid);

                    DataSet resultSet = DatabaseHelper.ExecuteQuery(cmd, _connectionString, userparam);

                    //- Create UserModel's from the other user in the relationship
                    if (resultSet != null && resultSet.Tables[0].Rows.Count > 0)
                    {
                        foreach (DataRow row in resultSet.Tables[0].Rows)
                        {
                            UserModel user = new UserModel();
                            user = GetUserByUserID(row["Source_UserID"].ToString());

                            users.Add(user);
                        }

                    }

                }
                catch (Exception ex)
                {

                    LogMessageModel logmessage = new LogMessageModel(LogStatus.Error,
                        string.Format("Login exception occurred within DatabaseManager.GetActiveUserRelationshipAsync: {0}", ex.Message), -1);

                    ExceptionManager.Instance.InsertLogMessage(logmessage);
                }

                return users;
            });

            return usertask;
        }

        #endregion


        #region INSERT

        /// <summary>
        /// Inserts a new relationship request from one user to another
        /// </summary>
        /// <param name="sourceuserid">Requesting User ID</param>
        /// <param name="targetuserid">Requested User ID</param>
        public Task<int> InsertUserRelationshipRequestAsync(string sourceuserid, string targetuserid)
        {
            Task<int> requesttask = new Task<int>(() =>
            {
                int result = 0;

                string cmd = "INSERT INTO UserRelationships(Source_UserID, Target_UserID, IsActive, DateCreated) ";
                cmd += "VALUES(@Source_UserID, @Target_UsesrID, 0, @DateCreated);";

                SqlParameter sourceparam = new SqlParameter("@Source_UserID", sourceuserid);
                SqlParameter targetparam = new SqlParameter("@Target_UserID", targetuserid);
                SqlParameter datecreated = new SqlParameter("@DateCreated", DateTime.UtcNow);

                try
                {
                    //- check if target user exists and is active
                    bool targetexists = CheckUserExists(targetuserid);
                    bool targetactive = CheckIfUserActive(targetuserid);


                    if (targetexists && targetactive)
                        result = DatabaseHelper.ExecuteNonQuery(cmd, _connectionString, sourceparam, targetparam, datecreated);
                    else if (!targetexists)
                        throw new Exception(string.Format("Error:  Could not create relationship request.  Target user '{0}' does not exist", targetuserid));
                    else
                        throw new Exception(string.Format("Error:  Could not create relationship request.  Target user '{0}' is not active", targetuserid));

                }
                catch (Exception ex)
                {
                    result = -1;
                    LogMessageModel logmessage = new LogMessageModel(LogStatus.Error,
                        string.Format("Login exception occurred within DatabaseManager.InsertUserRelationshipRequesAsync: {0}", ex.Message), -1);

                    ExceptionManager.Instance.InsertLogMessage(logmessage);
                }
                return result;
            });

            return requesttask;
        }

        /// <summary>
        /// Inserts a new relationship request from one user to another
        /// </summary>
        /// <param name="sourceuserid">Requesting User ID</param>
        /// <param name="targetuserid">Requested User ID</param>
        public int InsertUserRelationshipRequest(string sourceuserid, string targetuserid)
        {
                int result = 0;

                string cmd = "INSERT INTO UserRelationships(Source_UserID, Target_UserID, IsActive, DateCreated) ";
                cmd += "VALUES(@Source_UserID, @Target_UserID, 0, @DateCreated);";

                SqlParameter sourceparam = new SqlParameter("@Source_UserID", sourceuserid);
                SqlParameter targetparam = new SqlParameter("@Target_UserID", targetuserid);
                SqlParameter datecreated = new SqlParameter("@DateCreated", DateTime.UtcNow);

                try
                {
                    //- check if target user exists and is active
                    bool targetexists = CheckUserExists(targetuserid);
                    bool targetactive = CheckIfUserActive(targetuserid);


                    if (targetexists && targetactive)
                        result = DatabaseHelper.ExecuteNonQuery(cmd, _connectionString, sourceparam, targetparam, datecreated);
                    else if (!targetexists)
                        throw new Exception(string.Format("Error:  Could not create relationship request.  Target user '{0}' does not exist", targetuserid));
                    else
                        throw new Exception(string.Format("Error:  Could not create relationship request.  Target user '{0}' is not active", targetuserid));

                }
                catch (Exception ex)
                {
                    result = -1;
                    LogMessageModel logmessage = new LogMessageModel(LogStatus.Error,
                        string.Format("Login exception occurred within DatabaseManager.InsertUserRelationshipRequesAsync: {0}", ex.Message), -1);

                    ExceptionManager.Instance.InsertLogMessage(logmessage);
                }

                return result;
        }

        #endregion


        #region UPDATE

        /// <summary>
        /// Updates an existing User relationship
        /// </summary>
        /// <param name="sourceuserid">Requesting User ID</param>
        /// <param name="targetuserid">Requested User ID</param>
        public Task<int> UpdateUserRelationshipRequesAsync(string sourceuserid, string targetuserid, bool isactive)
        {
            Task<int> requesttask = new Task<int>(() =>
            {
                int result = 0;

                string cmd = "UPDATE UserRelationships SET IsActive = @IsActive, DateActivated = @DateActivated ";
                cmd += "WHERE (Source_UserID = @Source_UserID and Target_UserID = @Target_UserID) or (Source_UserID = @Target_UserID and Target_UserID = @Source_UserID);";

                SqlParameter sourceparam = new SqlParameter("@Source_UserID", sourceuserid);
                SqlParameter targetparam = new SqlParameter("@Target_UserID", targetuserid);
                SqlParameter isactiveparam = new SqlParameter("@IsActive", isactive);
                SqlParameter dateactivated = new SqlParameter("@DateActivated", DateTime.UtcNow);

                try
                {
                    result = DatabaseHelper.ExecuteNonQuery(cmd, _connectionString, sourceparam, targetparam, isactiveparam, dateactivated);
                }
                catch (Exception ex)
                {
                    result = -1;
                    LogMessageModel logmessage = new LogMessageModel(LogStatus.Error,
                        string.Format("Login exception occurred within DatabaseManager.UpdateUserRelationshipRequesAsync: {0}", ex.Message), -1);

                    ExceptionManager.Instance.InsertLogMessage(logmessage);
                }
                return result;
            });

            return requesttask;
        }

        #endregion


        #endregion


        #region Messaage Methods

        #region GET

        /// <summary>
        /// Get Messages that have been received after the provided date
        /// </summary>
        public List<MessageModel> GetUserMessagesByDate(DateTime date, string userid)
        {
            List<MessageModel> messages = new List<MessageModel>();

            string cmd = "SELECT * FROM Messages WHERE SendDate > @SendDate and TargetUser_FK = @TargetUser_FK";

            try
            {
                SqlParameter dateparam = new SqlParameter("@SendDate", date);
                SqlParameter idparam = new SqlParameter("@TargetUser_FK", userid);

                DataSet ds = DatabaseHelper.ExecuteQuery(cmd, _instance._connectionString, dateparam, idparam);
                if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        MessageModel model = new MessageModel();
                        model.GlobalID = row["GlobalID"].ToString();
                        model.IsEvent = bool.Parse(row["IsEvent"].ToString());
                        model.Latitude = double.Parse(row["Latitude"].ToString());
                        model.Longitude = double.Parse(row["Longitude"].ToString());
                        model.Message = row["Message"].ToString();
                        model.SendDate = DateTime.Parse(row["SendDate"].ToString());
                        model.SourceUserID = row["SourceUser_FK"].ToString();
                        model.TargetUserID = row["TargetUser_FK"].ToString();
                        model.Title = row["Title"].ToString();

                        if(!string.IsNullOrEmpty(row["EventStartDate"].ToString()))
                            model.EventStartDate = DateTime.Parse(row["EventStartDate"].ToString());

                        messages.Add(model);
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionManager.Instance.InsertLogMessage(new LogMessageModel(LogStatus.Error, ex.Message, -1));

            }

            return messages;
        }

        #endregion

        #region INSERT

        /// <summary>
        /// Insert a Message 
        /// </summary>
        /// <returns>Success status code</returns>
        public  int InsertMessage(MessageModel message)
        {
            int result = 0;

            string cmd = "INSERT INTO Messages(GlobalID, TargetUser_FK, SourceUser_FK, SendDate, Message, Title, Latitude, Longitude, IsEvent, EventStartDate)";
            cmd += "VALUES(@GlobalID, @TargetUser_FK, @SourceUser_FK, @SendDate, @Message, @Title, @Latitude, @Longitude, @IsEvent, @EventStartDate)";

            SqlParameter globalid = new SqlParameter("@GlobalID", message.GlobalID);
            SqlParameter target = new SqlParameter("@TargetUser_FK", message.TargetUserID);
            SqlParameter source = new SqlParameter("@SourceUser_FK", message.SourceUserID);
            SqlParameter senddate = new SqlParameter("@SendDate", DateTime.UtcNow);
            SqlParameter messageparam = new SqlParameter("@Message", message.Message);
            SqlParameter title = new SqlParameter("@Title", message.Title);
            SqlParameter latitude = new SqlParameter("@Latitude", message.Latitude);
            SqlParameter longitude = new SqlParameter("@Longitude", message.Longitude);
            SqlParameter isevent = new SqlParameter("@IsEvent", message.IsEvent);
            SqlParameter startdate = new SqlParameter("@EventStartDate", DateTime.UtcNow);
            
            if(message.IsEvent)
                startdate = new SqlParameter("@EventStartDate", message.EventStartDate);

            try
            {
                result = DatabaseHelper.ExecuteNonQuery(cmd, _instance._connectionString, globalid, target, source, senddate, messageparam,
                    title, latitude, longitude, isevent, startdate);
            }
            catch (Exception ex)
            {
                ExceptionManager.Instance.InsertLogMessage(new LogMessageModel(LogStatus.Error, ex.Message, -1));
                result = -1;
            }

            return result;
        }

        #endregion

        #endregion

    }
}
