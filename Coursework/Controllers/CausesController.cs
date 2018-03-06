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
    public class CausesController : Controller
    {
        private CauseDBContext db = new CauseDBContext();

        // GET: Causes
        public ActionResult Index()
        {
            return View(db.Causes.ToList());
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
            if (cause.Image != null && cause.Image.ContentLength > 0)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                string extension = Path.GetExtension(cause.Image.FileName).ToLower();
                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError("Image", "Allowed file formats are jpg, jpeg and png.");
                }
            } else
            {
                ModelState.AddModelError("Image", "An image is required.");
            }
            cause.CreatedAt = DateTime.Now;
            int memberID = Convert.ToInt32(Session["UserID"].ToString());
            Member member = db.Members.Find(memberID);
            cause.Member = member;
            cause.Signers.Add(member);
            if (ModelState.IsValid)
            {
                if (cause.Image != null && cause.Image.ContentLength > 0)
                {
                    // TODO: Add file size checking
                    string extension = Path.GetExtension(cause.Image.FileName).ToLower();
                    string fileName = string.Format("{0}.{1}", Guid.NewGuid(), extension);
                    string path = Path.Combine(Server.MapPath("~/UserImages/"), fileName);
                    cause.Image.SaveAs(path);
                    cause.ImageURL = Path.Combine("/UserImages/", fileName);
                }
                db.Causes.Add(new Cause(cause));
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            foreach (var error in ViewData.ModelState.Values.SelectMany(modelState => modelState.Errors))
            {
                System.Diagnostics.Debug.WriteLine(error.ErrorMessage);
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
            if (Session["UserID"] == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }
            if (Convert.ToInt32(Session["UserID"].ToString()) != cause.Member.ID && (string)Session["Role"] != "Admin")
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }
            // check for correct extensions
            if (cause.Image != null && cause.Image.ContentLength > 0)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                string extension = Path.GetExtension(cause.Image.FileName).ToLower();
                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError("Image", "Allowed file formats are jpg, jpeg and png.");
                }
            }
            if (ModelState.IsValid)
            {
                Cause currentCause = db.Causes.Find(cause.ID);
                currentCause.Title = cause.Title;
                currentCause.Description = cause.Description;
                currentCause.Pledge = cause.Pledge;
                currentCause.Target = cause.Target;

                // save file to disk and save path to model
                if (cause.Image != null && cause.Image.ContentLength > 0)
                {
                    // TODO: Add file size checking
                    string extension = Path.GetExtension(cause.Image.FileName).ToLower();
                    string fileName = string.Format("{0}.{1}", Guid.NewGuid(), extension);
                    string path = Path.Combine(Server.MapPath("~/UserImages/"), fileName);
                    cause.Image.SaveAs(path);
                    currentCause.ImageURL = Path.Combine("/UserImages/", fileName);
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
