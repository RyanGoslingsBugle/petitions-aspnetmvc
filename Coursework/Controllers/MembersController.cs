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
    [RequireHttps]
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
            ICollection<Coursework.Models.Cause> createdCauses = member.Causes.Where(cause => cause.Member.ID == member.ID).ToList();
            return View(new MemberVM(member, createdCauses));
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
                return RedirectToAction("Index");
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

                // if an image was attached, do upload to S3
                if (member.Image != null && member.Image.ContentLength > 0)
                {

                    // Image upload courtesy of Stack Overflow, Stephen Muecke, 10/03/2016, https://stackoverflow.com/questions/35904830/asp-net-mvc-upload-image
                    Stream st = member.Image.InputStream;
                    string extension = Path.GetExtension(member.Image.FileName).ToLower();
                    string fileName = string.Format("{0}{1}", Guid.NewGuid(), extension);
                    string bucketName = "multitude";
                    string s3dir = "UserImages";
                    AmazonUploader uploader = new AmazonUploader();
                    bool a = uploader.sendMyFileToS3(st, bucketName, s3dir, fileName);

                    if (a != true)
                    {
                        ModelState.AddModelError("Image", "Image upload to S3 failed.");
                        return View(member);
                    }

                    currentMember.ImagePath = "https://s3-eu-west-1.amazonaws.com/multitude/UserImages/" + fileName;
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
