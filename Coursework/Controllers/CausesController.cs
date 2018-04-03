using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Coursework.Models;

namespace Coursework.Controllers
{
    [RequireHttps]
    public class CausesController : Controller
    {
        private CauseDBContext db = new CauseDBContext();

        // GET: Causes
        public ActionResult Index(string q)
        {
            var causes = db.Causes.ToList();

            if (!string.IsNullOrEmpty(q))
            {
                causes = causes.Where(c => c.Title.ToLower().Contains(q.ToLower())).ToList();
                ViewBag.search = q;
            }

            return View(causes);
        }

        // GET: Causes/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Index");
            }
            Cause cause = db.Causes.Find(id);
            if (Session["UserID"] != null)
            {
                var userID = Convert.ToInt32(Session["UserID"].ToString());
                if (cause.Signers.Any(signer => signer.ID == userID))
                {
                    ViewBag.signed = true;
                }
            }
            if (cause == null)
            {
                return HttpNotFound();
            }
            return View(new CauseVM(cause));
        }

        // jQuery updating via ajax partial reloads courtesy of Stack Overflow, mattytommo, 28/02/13, https://stackoverflow.com/questions/15146212/can-you-just-update-a-partial-view-instead-of-full-page-post
        // GET: Causes/GetUpdate/5
        public ActionResult GetUpdate(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Cause cause = db.Causes.Find(id);
            if (cause == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            }
            return PartialView("Signers", new CauseVM(cause));
        } 

        // GET: Causes/Create
        public ActionResult Create()
        {
            if (Session["UserID"] != null)
            {
                return View(new CauseVM());
            }

            return RedirectToAction("Index");
        }

        // POST: Causes/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Title,Description,Pledge,Target,Image")] CauseVM cause)
        {
            if (Session["UserID"] == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }

            cause.CreatedAt = DateTime.Now;
            int memberID = Convert.ToInt32(Session["UserID"].ToString());
            Member member = db.Members.Find(memberID);
            cause.Member = member;
            cause.Signers.Add(member);
            if (cause.Image == null || cause.Image.ContentLength <= 0)
            {
                ModelState.AddModelError("Image", "Image is required.");
            }
            if (ModelState.IsValid)
            {
                // Save file to S3 and add path to model
                Stream st = cause.Image.InputStream;
                string extension = Path.GetExtension(cause.Image.FileName).ToLower();
                string fileName = string.Format("{0}{1}", Guid.NewGuid(), extension);
                string bucketName = "multitude";
                string s3dir = "UserImages";
                AmazonUploader uploader = new AmazonUploader();
                bool a = uploader.sendMyFileToS3(st, bucketName, s3dir, fileName);

                if (a != true)
                {
                    ModelState.AddModelError("Image", "Image upload to S3 failed.");
                    return View(cause);
                }

                cause.ImageURL = "https://s3-eu-west-1.amazonaws.com/multitude/UserImages/" + fileName;

                db.Causes.Add(new Cause(cause));
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(cause);
        }

        // GET: Causes/Edit/5
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
            Cause cause = db.Causes.Find(id);
            if (cause == null)
            {
                return HttpNotFound();
            }
            if (Convert.ToInt32(Session["UserID"].ToString()) != cause.Member.ID && (string)Session["Role"] != "Admin")
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }
            return View(new CauseVM(cause));
        }

        // POST: Causes/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,Title,Description,Pledge,Target,Image")] CauseVM cause)
        {
            Cause currentCause = db.Causes.Find(cause.ID);
            if (Session["UserID"] == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }
            if (Convert.ToInt32(Session["UserID"].ToString()) != currentCause.Member.ID && (string)Session["Role"] != "Admin")
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }
            if (ModelState.IsValid)
            {
                currentCause.Title = cause.Title;
                currentCause.Description = cause.Description;
                currentCause.Pledge = cause.Pledge;
                currentCause.Target = cause.Target;
                
                // if an image was attached, do upload to S3
                if (cause.Image != null && cause.Image.ContentLength > 0)
                {
                    // Save file to S3 and add path to model
                    Stream st = cause.Image.InputStream;
                    string extension = Path.GetExtension(cause.Image.FileName).ToLower();
                    string fileName = string.Format("{0}{1}", Guid.NewGuid(), extension);
                    string bucketName = "multitude";
                    string s3dir = "UserImages";
                    AmazonUploader uploader = new AmazonUploader();
                    bool a = uploader.sendMyFileToS3(st, bucketName, s3dir, fileName);

                    if (a != true)
                    {
                        ModelState.AddModelError("Image", "Image upload to S3 failed.");
                        return View(cause);
                    }

                    currentCause.ImageURL = "https://s3-eu-west-1.amazonaws.com/multitude/UserImages/" + fileName;
                }

                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(cause);
        }

        // POST: Causes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            if (Session["UserID"] == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }
            if (Session["Role"].ToString() != "Admin")
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }
            else
            {
                Cause cause = db.Causes.Find(id);
                db.Causes.Remove(cause);
                db.SaveChanges();
                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }
        }

        // POST: /Causes/Sign/5
        // Record signing of cause by member
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Sign(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            if (Session["UserID"] == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }
            Cause cause = db.Causes.Find(id);
            int memberID = Convert.ToInt32(Session["UserID"].ToString());
            Member member = db.Members.Find(memberID);

            if (member.Causes.Any(signedCause => signedCause.ID == cause.ID))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            member.Causes.Add(cause);
            db.SaveChanges();
            return new HttpStatusCodeResult(HttpStatusCode.OK);
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
