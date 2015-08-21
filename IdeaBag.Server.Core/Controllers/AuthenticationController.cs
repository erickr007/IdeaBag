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
    public class AuthenticationController : Controller
    {

        #region Login

        [HttpGet]
        public LoginResultModel LoginStandardUser(string userid, string passwordhash)
        {
            LoginResultModel result = new LoginResultModel();

            result = DatabaseManager.Instance.AuthenticateStandardUser(userid, passwordhash);

            return result;
        }


        #endregion


        #region Signup


        public string SignupStandardUser(string user)
        {
            string result = "";

            UserModel signupUser = JsonTools.Deserialize<UserModel>(user);

#warning PASSWORD MUST BE ENCRYPTED.  PREFERABLY THIS SHOULD BE DONE ON THE CLIENT

            SignupResultModel model = (SignupResultModel)DatabaseManager.Instance.SignupStandardUser(signupUser);

            //- Create json object based on the result type
            result = JsonTools.Serialize<SignupResultModel>(model);

            return result;
        }

        #endregion


        #region Troubleshoot

        [HttpGet]
        public string EchoTest()
        {
            return "connect";
        }

        #endregion
    }
}