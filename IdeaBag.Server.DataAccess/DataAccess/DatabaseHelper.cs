using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdeaBag.Server.DataAccess
{
    public static class DatabaseHelper
    {

        #region Query

        public static DataSet ExecuteQuery(string cmd, string connectstring, params SqlParameter[] parameters)
        {
            DataSet ds = new DataSet();

            using (SqlConnection connection = new SqlConnection(connectstring))
            {

                SqlCommand command = new SqlCommand(cmd, connection);
                command.Parameters.AddRange(parameters);

                connection.Open();

                SqlDataReader reader = command.ExecuteReader();
                ds.Load(reader, LoadOption.OverwriteChanges, new string[] { "" });
                reader.Close();
            }

            return ds;
        }

        #endregion


        #region Non Query

        public static int ExecuteNonQuery(string cmd, string connectstring, params SqlParameter[] parameters)
        {
            int result = -1;

            using (SqlConnection connection = new SqlConnection(connectstring))
            {
                connection.Open();

                SqlCommand command = new SqlCommand(cmd, connection);
                command.Parameters.AddRange(parameters);


                result = command.ExecuteNonQuery();
            }

            return result;

        }

        #endregion

    }
}
