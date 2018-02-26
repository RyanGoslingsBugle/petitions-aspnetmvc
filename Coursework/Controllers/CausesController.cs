﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Globalization;
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
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Cause cause = db.Causes.Find(id);
            if (cause == null)
            {
                return HttpNotFound();
            }
            return View(cause);
        }

        // GET: Causes/Create
        public ActionResult Create()
        {
            if (Session["UserID"] != null)
            {
                return View();
            }

            return RedirectToAction("Index");
        }

        // POST: Causes/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Title,Description,Pledge,Target,ImageURL,Member")] Cause cause)
        {
            if (Session["UserID"] == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }
            int memberID = Convert.ToInt32(Session["UserID"].ToString());
            Member member = db.Members.Find(memberID);
            cause.Member = member;
            cause.CreatedAt = DateTime.Now;
            if (ModelState.IsValid)
            {
                db.Causes.Add(cause);
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
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Cause cause = db.Causes.Find(id);
            if (cause == null)
            {
                return HttpNotFound();
            }
            return View(cause);
        }

        // POST: Causes/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Title,Description,Pledge,Target,ImageURL")] Cause cause)
        {
            if (ModelState.IsValid)
            {
                db.Entry(cause).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(cause);
        }

        //// GET: Causes/Delete/5
        //public ActionResult Delete(int? id)
        //{
        //    // Casting session vars to string from MSDN, 21/09/16, https://code.msdn.microsoft.com/How-to-create-and-access-447ada98
        //    if (Session["Role"] as string != "Admin")
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
        //    }
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    Cause cause = db.Causes.Find(id);
        //    if (cause == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(cause);
        //}

        //// POST: Causes/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public ActionResult DeleteConfirmed(int id)
        //{
        //    if (Session["Role"] as string != "Admin")
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
        //    } 
        //    else
        //    {
        //        Cause cause = db.Causes.Find(id);
        //        db.Causes.Remove(cause);
        //        db.SaveChanges();
        //        return RedirectToAction("Index");
        //    }
        //}

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
