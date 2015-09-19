using IdeaBag.Portable.Data.Models;
using IdeaBag.Portable.Utilty;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace IdeaBag.Server.Core.Controllers
{
    public class TestController : Controller
    {
        // GET: Test
        public ActionResult StandardSignup()
        {
            AuthenticationController ctrl = new AuthenticationController();
            
            UserModel user = new UserModel() { GlobalID = Guid.NewGuid().ToString() };
            user.UserID = "ui_testing";
            user.PasswordHash = "test";

            string userstring = JsonTools.Serialize<UserModel>(user);

            string result = ctrl.SignupStandardUser(userstring);


            return Content(result);
        }
    }
}