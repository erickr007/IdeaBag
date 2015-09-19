using IdeaBag.Portable.Data.Models;
using IdeaBag.Portable.Utilty;
using IdeaBag.Server.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace IdeaBag.Server.Core.Controllers
{
    public class DataController : Controller
    {

        #region Users

        // GET: Data
        public string GetUserInfo(string email)
        {
            string result = "";

            UserModel user = DatabaseManager.Instance.GetUserByUserID(email);
            result = JsonTools.Serialize<UserModel>(user);

            return result;
        }

        #endregion

    }
}