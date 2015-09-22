using IdeaBag.Portable.Data.Models;
using IdeaBag.Portable.Utilty;
using IdeaBag.Server.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace IdeaBag.Server.Core.Controllers
{
    public class DataController : Controller
    {

        #region Users

        // GET: Data

        /// <summary>
        /// Get User Information
        /// </summary>
        public string GetUserInfo(string email)
        {
            string result = "";

            UserModel user = DatabaseManager.Instance.GetUserByUserID(email);
            result = JsonTools.Serialize<UserModel>(user);

            return result;
        }

        /// <summary>
        /// Gets all Users associated with a User that were added after the provided date
        /// </summary>
        public string GetUserContacts(string email, string utcdate)
        {
            string result = "";
            List<UserModel> users = new List<UserModel>();

            Task<List<UserModel>> getuserstask = Task.Run(async () =>
            {
                DateTime lastreceived = DateTime.Parse(utcdate);
                users = await DatabaseManager.Instance.GetUserRelationshipsAsync(email, lastreceived, true);

                return users;
            });
            getuserstask.Wait();

            result = JsonTools.Serialize<List<UserModel>>(getuserstask.Result);

            return result;
        }

        #endregion

    }
}