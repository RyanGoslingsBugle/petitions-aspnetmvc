using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using Coursework.Models;
using X.PagedList;

namespace Coursework.Controllers
{
    [RequireHttps]
    public class HomeController : Controller
    {
        // import database connection
        private CauseDBContext db = new CauseDBContext();

        public ActionResult Index()
        {
            // pass causes object to view
            return View(db.Causes.ToList());
        }

        // Password hashing from Stack Overflow, Kevin Nelson, 16/08/17, https://stackoverflow.com/questions/45723140/how-to-salt-and-compare-password-in-asp-net-mvc
        // Wrote my own login logic rather than using UserManager as an exercise, and for an excuse to return Json
        // obviously would use built-ins for proper security in production
        // I've been working with Node for too long

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Login(Login loginUser)
        {
            Response.ContentType = "application/json; charset=utf-8";

            if (!ModelState.IsValid)
            {
                Response.StatusCode = 400;
                return Json(new { message = "Bad request, please check your input and try again." }, JsonRequestBehavior.AllowGet);
            }

            using (CauseDBContext db = new CauseDBContext())
            {
                var matchedUsers = db.Members.FirstOrDefault(a => a.Email.Equals(loginUser.Email));
                if (matchedUsers != null && Crypto.VerifyHashedPassword(matchedUsers.Password, loginUser.Password))
                {
                    Session["UserID"] = matchedUsers.ID.ToString();
                    Session["UserName"] = matchedUsers.Name.ToString();
                    Response.StatusCode = 200;
                    return Json(new { message = "Login complete, welcome back." }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    Response.StatusCode = 403;
                    return Json(new { message = "The username/password was incorrect. Please try again." }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        // GET: Destroy active session if any
        [HttpGet]
        public ActionResult Logout()
        {
            Session.Clear();
            Session.Abandon();
            Response.Cookies["ASP.NET_SessionId"].Value = string.Empty;
            Response.Cookies["ASP.NET_SessionId"].Expires = DateTime.Now.AddMonths(-10);
            return RedirectToAction("Index");
        }

        // GET: Return a paged list of all cause objects
        // Paging courtesy ITworld, Matthew Mombrea, 05/08/15, https://www.itworld.com/article/2956575/development/how-to-sort-search-and-paginate-tables-in-asp-net-mvc-5.html
        // Admin currently only applies to Causes, should be broken out so as to include Users as well

        public ActionResult Admin(int page = 1, int pageSize = 25)
        {
            if (Session["UserID"] != null)
            {
                Member requester = db.Members.Find(Convert.ToInt32(Session["UserID"].ToString()));
                if (requester.Role == Coursework.Models.Role.Admin)
                {
                    page = page > 0 ? page : 1;
                    pageSize = pageSize > 0 ? pageSize : 25;

                    var causes = db.Causes.OrderBy(a => a.ID);
                    return View(causes.ToPagedList(page, pageSize));
                }
            }
            return RedirectToAction("Index");
        }
    }
}