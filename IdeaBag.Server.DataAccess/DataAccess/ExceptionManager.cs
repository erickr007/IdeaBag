using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Configuration;
using IdeaBag.Portable.Data.Models;

namespace IdeaBag.Server.DataAccess
{
    public class ExceptionManager
    {
        #region Private Properties

        private string _connectionString;
        private static ExceptionManager _instance;

        #endregion


        #region Public Properties

        public static ExceptionManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    string connection = WebConfigurationManager.AppSettings["DevDatabase"];
                    _instance = new ExceptionManager(connection);
                }

                return _instance;
            }
        }

        #endregion


        #region Constructor

        private ExceptionManager(string connectionstring)
        {
            _connectionString = connectionstring;
        }

        #endregion


        #region Insert Messages

        public void InsertLogMessage(LogMessageModel message)
        {

            string cmd = "INSERT INTO Logs (Status, Message, Code, DateCreated) VALUES(@Status, @Message, @Code, @DateCreated)";
            
            DatabaseHelper.ExecuteNonQuery(cmd, _connectionString,
                new SqlParameter("@Status", message.Status),
                new SqlParameter("@Message", message.Message),
                new SqlParameter("@Code", message.Code),
                new SqlParameter("@DateCreated", message.DateCreated));
        }

        #endregion

    }
}
