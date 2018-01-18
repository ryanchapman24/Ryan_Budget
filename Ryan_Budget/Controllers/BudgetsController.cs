using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Ryan_Budget.Models;
using Microsoft.AspNet.Identity;

namespace Ryan_Budget.Controllers
{
    [RequireHttps]
    public class BudgetsController : UserNames
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Budgets
        [Authorize]
        public ActionResult Index()
        {
            var user = db.Users.Find(User.Identity.GetUserId());
            var budgets = db.Budgets.Where(b => b.HouseholdId == user.HouseholdId);
            return View(budgets.ToList());
        }

        // GET: Budgets/Details/5
        [Authorize]
        public ActionResult Details(int? id)
        {
            var user = db.Users.Find(User.Identity.GetUserId());

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Budget budget = db.Budgets.Find(id);
            if (budget == null)
            {
                return HttpNotFound();
            }

            if (user.HouseholdId != budget.HouseholdId)
            {
                return RedirectToAction("Warning", "Household");
            }

            return View(budget);
        }

        // GET: Budgets/Create
        [Authorize]
        public ActionResult Create(int? id)
        {
            var user = db.Users.Find(User.Identity.GetUserId());

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Household household = db.Households.Find(id);
            if (household == null)
            {
                return HttpNotFound();
            }

            if (user.HouseholdId != id)
            {
                return RedirectToAction("Warning", "Household");
            }

            ViewBag.HouseholdName = household.Name;
            ViewBag.HouseholdId = household.Id;
            ViewBag.CategoryId = new SelectList(db.Categories, "Id", "Name");
            ViewBag.FrequencyId = new SelectList(db.Frequencies, "Id", "Length");

            return View();
        }

        // POST: Budgets/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,HouseholdId,Amount,FrequencyId,CategoryId,Type,Description,AuthorUserId,UpdateUserId,Created,Updated")] Budget budget)
        {
            ApplicationUser user = db.Users.FirstOrDefault(u => u.UserName.Equals(User.Identity.Name));
            var userid = user.Id;
            var created = System.DateTime.Now;

            if (ModelState.IsValid)
            {
                budget.Created = System.DateTime.Now;
                budget.AuthorUserId = user.Id;
                db.Budgets.Add(budget);
                db.SaveChanges();

                //foreach (var member in user.Household.Users)
                //{
                //    Notification n = new Notification()
                //    {
                //        HouseholdId = user.Household.Id,
                //        BudgetId = budget.Id,
                //        AuthorUserId = userid,
                //        UserToNotifyId = member.Id,
                //        Created = created,
                //        Type = "New Budget",
                //        Description = "New Budget For: " + budget.Category.Name,
                //    };
                //    db.Notifications.Add(n);
                //    db.SaveChanges();
                //}

                return RedirectToAction("Index");
            }

            Household household = db.Households.Find(budget.HouseholdId);
            ViewBag.HouseholdName = household.Name;
            ViewBag.CategoryId = new SelectList(db.Categories, "Id", "Name", budget.CategoryId);
            ViewBag.FrequencyId = new SelectList(db.Frequencies, "Id", "Length", budget.FrequencyId);           

            return View(budget);
        }

        // GET: Budgets/Edit/5
        [Authorize]
        public ActionResult Edit(int? id)
        {
            var user = db.Users.Find(User.Identity.GetUserId());

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Budget budget = db.Budgets.Find(id);
            if (budget == null)
            {
                return HttpNotFound();
            }

            if (user.HouseholdId != budget.HouseholdId)
            {
                return RedirectToAction("Warning", "Household");
            }

            ViewBag.FrequencyId = new SelectList(db.Frequencies, "Id", "Length", budget.FrequencyId);
            return View(budget);
        }

        // POST: Budgets/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,HouseholdId,Amount,FrequencyId,CategoryId,Type,Description,AuthorUserId,UpdateUserId,Created,Updated")] Budget budget)
        {
            if (ModelState.IsValid)
            {
                budget.Updated = System.DateTimeOffset.Now;
                db.Budgets.Attach(budget);
                db.Entry(budget).Property("Amount").IsModified = true;
                db.Entry(budget).Property("FrequencyId").IsModified = true;
                db.Entry(budget).Property("Description").IsModified = true;
                db.Entry(budget).Property("Updated").IsModified = true;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.FrequencyId = new SelectList(db.Frequencies, "Id", "Length", budget.FrequencyId);
            return View(budget);
        }

        // GET: Budgets/Delete/5
        [Authorize]
        public ActionResult Delete(int? id)
        {
            var user = db.Users.Find(User.Identity.GetUserId());

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Budget budget = db.Budgets.Find(id);
            if (budget == null)
            {
                return HttpNotFound();
            }

            if (user.HouseholdId != budget.HouseholdId)
            {
                return RedirectToAction("Warning", "Household");
            }

            return View(budget);
        }

        // POST: Budgets/Delete/5
        [HttpPost, ActionName("Delete")]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            ApplicationUser user = db.Users.FirstOrDefault(u => u.UserName.Equals(User.Identity.Name));
            var userid = user.Id;
            var created = System.DateTime.Now;
            Budget budget = db.Budgets.Find(id);
            db.Budgets.Remove(budget);
            db.SaveChanges();

            //foreach (var member in user.Household.Users)
            //{
            //    Notification n = new Notification()
            //    {
            //        HouseholdId = user.Household.Id,
            //        AuthorUserId = userid,
            //        UserToNotifyId = member.Id,
            //        Created = created,
            //        Type = "Budget Deleted",
            //        Description = "Budget Deleted For: " + budget.Category.Name,
            //    };
            //    db.Notifications.Add(n);
            //    db.SaveChanges();
            //}

            return RedirectToAction("Index");
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
