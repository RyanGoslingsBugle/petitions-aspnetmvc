using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using Coursework.Models;

namespace Coursework.Controllers
{
    public class MembersController : Controller
    {
        private CauseDBContext db = new CauseDBContext();

        // GET: Members
        public ActionResult Index()
        {
            return View(db.Members.ToList());
        }

        // GET: Members/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Index");
            }
            Member member = db.Members.Find(id);
            if (member == null)
            {
                return HttpNotFound();
            }
            return View(new MemberVM(member));
        }

        // Password hashing from Stack Overflow, Kevin Nelson, 16/08/17, https://stackoverflow.com/questions/45723140/how-to-salt-and-compare-password-in-asp-net-mvc

        // POST: Members/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Name,Email,Password,City,Country")] Member member)
        {
            if (ModelState.IsValid)
            {
                member.Role = Coursework.Models.Role.Member;
                member.Password = Crypto.HashPassword(member.Password);

                db.Members.Add(member);
                db.SaveChanges();

                Session["UserID"] = member.ID.ToString();
                Session["UserName"] = member.Name.ToString();
                Session["Role"] = member.Role.ToString();
                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }
            else
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
        }

        // GET: Members/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            if (Session["UserID"] == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }
            if (Convert.ToInt32(Session["UserID"].ToString()) != id && (string)Session["Role"] != "Admin")
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }
            Member member = db.Members.Find(id);
            if (member == null)
            {
                return HttpNotFound();
            }
            return View(new MemberVM(member));
        }

        // POST: Members/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,Name,Email,Password,City,Country,Image")] MemberVM member)
        {
            if (Session["UserID"] == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }
            if (Convert.ToInt32(Session["UserID"].ToString()) != member.ID && (string)Session["Role"] != "Admin")
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }
            if (member.Image != null && member.Image.ContentLength > 0)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                string extension = Path.GetExtension(member.Image.FileName).ToLower();
                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError("Image", "Allowed file formats are jpg, jpeg and png.");
                }
            }
            if (ModelState.IsValid)
            {
                Member currentMember = db.Members.Find(member.ID);
                currentMember.Name = member.Name;
                Session["UserName"] = member.Name.ToString();
                currentMember.Email = member.Email;
                currentMember.City = member.City;
                currentMember.Country = member.Country;
                if (member.Password != null)
                {
                    currentMember.Password = Crypto.HashPassword(member.Password);
                }
                // Image upload courtesy of Stack Overflow, Stephen Muecke, 10/03/2016, https://stackoverflow.com/questions/35904830/asp-net-mvc-upload-image
                if (member.Image != null && member.Image.ContentLength > 0)
                {
                    // TODO: Add file size checking
                    string extension = Path.GetExtension(member.Image.FileName).ToLower();
                    string fileName = string.Format("{0}.{1}", Guid.NewGuid(), extension);
                    string path = Path.Combine(Server.MapPath("~/UserImages/"), fileName);
                    member.Image.SaveAs(path);
                    currentMember.ImagePath = Path.Combine("/UserImages/", fileName);
                }
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(member);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
