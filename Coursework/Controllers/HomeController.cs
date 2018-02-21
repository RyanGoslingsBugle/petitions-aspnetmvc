using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using Coursework.Models;

namespace Coursework.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(Login loginUser)
        {
            System.Diagnostics.Debug.WriteLine(loginUser.Email);
            System.Diagnostics.Debug.WriteLine(loginUser.Password);
            if (!ModelState.IsValid)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (CauseDBContext db = new CauseDBContext())
            {
                var matchedUsers = db.Members.FirstOrDefault(a => a.Email.Equals(loginUser.Email));
                if (matchedUsers != null && Crypto.VerifyHashedPassword(matchedUsers.Password, loginUser.Password))
                {
                    Session["UserID"] = matchedUsers.ID.ToString();
                    Session["UserName"] = matchedUsers.Name.ToString();
                    return new HttpStatusCodeResult(HttpStatusCode.OK);
                }
                else
                {
                    return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
                }
            }
        }

    }
}