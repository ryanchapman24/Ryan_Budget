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
    public class AccountsController : UserNames
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Accounts
        [Authorize]
        public ActionResult Index()
        {
            var user = db.Users.Find(User.Identity.GetUserId());
            var accounts = db.Accounts.Where(a => a.HouseholdId == user.HouseholdId);
            return View(accounts.ToList());
        }

        // GET: Accounts/Details/5
        [Authorize]
        public ActionResult Details(int? id)
        {
            var user = db.Users.Find(User.Identity.GetUserId());

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Account account = db.Accounts.Find(id);


            if (account == null)
            {
                return HttpNotFound();
            }

            if (user.HouseholdId != account.HouseholdId)
            {
                return RedirectToAction("Warning", "Household");
            }

            ViewBag.AccountName = account.Name;
            ViewBag.Transactions = db.Transactions.Where(t => t.AccountId == account.Id).OrderByDescending(t => t.Id).ToList();
            return View(account);
        }

        // GET: Accounts/Create
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
            return View();
        }

        // POST: Accounts/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,HouseholdId,Balance,Name,Created,Updated,LastDateReconciled")] Account account)
        {
            ApplicationUser user = db.Users.FirstOrDefault(u => u.UserName.Equals(User.Identity.Name));
            var userid = user.Id;
            var created = System.DateTime.Now;

            if (ModelState.IsValid)
            {
                account.Created = System.DateTime.Now;
                db.Accounts.Add(account);
                db.SaveChanges();

                foreach (var member in user.Household.Users)
                {
                    Notification n = new Notification()
                    {
                        HouseholdId = user.Household.Id,
                        AccountId = account.Id,
                        AuthorUserId = userid,
                        UserToNotifyId = member.Id,
                        Created = created,
                        Type = "New Account",
                        Description = "New Account: " + account.Name,
                    };
                    db.Notifications.Add(n);
                    db.SaveChanges();
                }
                return RedirectToAction("Index");
            }

            Household household = db.Households.Find(account.HouseholdId);
            ViewBag.HouseholdName = household.Name;

            return View(account);
        }

        // GET: Accounts/Edit/5
        public ActionResult Edit(int? id)
        {
            var user = db.Users.Find(User.Identity.GetUserId());

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Account account = db.Accounts.Find(id);

            if (account == null)
            {
                return HttpNotFound();
            }

            if (user.HouseholdId != account.HouseholdId)
            {
                return RedirectToAction("Warning", "Household");
            }

            return View(account);
        }

        // POST: Accounts/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,HouseholdId,Balance,Name,Created,Updated,LastDateReconciled")] Account account)
        {
            if (ModelState.IsValid)
            {
                account.Updated = System.DateTimeOffset.Now;
                db.Accounts.Attach(account);
                db.Entry(account).Property("Name").IsModified = true;
                db.Entry(account).Property("Updated").IsModified = true;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(account);
        }

        // GET: Accounts/Reconcile/5
        [Authorize]
        public ActionResult Reconcile(int? id)
        {
            var user = db.Users.Find(User.Identity.GetUserId());

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Account account = db.Accounts.Find(id);

            if (account == null)
            {
                return HttpNotFound();
            }

            if (user.HouseholdId != account.HouseholdId)
            {
                return RedirectToAction("Warning", "Household");
            }

            return View(account);
        }

        // POST: Accounts/Reconcile/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult Reconcile([Bind(Include = "Id,HouseholdId,Balance,Name,Created,Updated,LastDateReconciled")] Account account)
        {
            var user = db.Users.Find(User.Identity.GetUserId());
            Account oldAcct = db.Accounts.AsNoTracking().FirstOrDefault(a => a.Id == account.Id);

            if (ModelState.IsValid)
            {
                account.Updated = System.DateTimeOffset.Now;
                account.LastDateReconciled = System.DateTimeOffset.Now;
                db.Accounts.Attach(account);
                db.Entry(account).Property("LastDateReconciled").IsModified = true;
                db.Entry(account).Property("Updated").IsModified = true;
                db.Entry(account).Property("Balance").IsModified = true;
                db.SaveChanges();

                if (oldAcct?.Balance != account.Balance)
                {
                    if (oldAcct?.Balance > account.Balance)
                    {
                        var diff = oldAcct.Balance - account.Balance;

                        Transaction t = new Transaction
                        {
                            Created = System.DateTimeOffset.Now,
                            TransactionDate = System.DateTimeOffset.Now,
                            AccountId = account.Id,
                            Status = "RECONCILIATION",
                            Type = false,
                            Reconciled = true,
                            AuthorUserId = user.Id,
                            CategoryId = 23,
                            Amount = diff * -1,
                        };
                        db.Transactions.Add(t);
                        db.SaveChanges();
                    }

                    if (oldAcct?.Balance < account.Balance)
                    {
                        var diff = account.Balance - oldAcct.Balance;

                        Transaction t = new Transaction
                        {
                            Created = System.DateTimeOffset.Now,
                            TransactionDate = System.DateTimeOffset.Now,
                            AccountId = account.Id,
                            Status = "RECONCILIATION",
                            Type = true,
                            Reconciled = true,
                            AuthorUserId = user.Id,
                            CategoryId = 6,
                            Amount = diff,
                        };
                        db.Transactions.Add(t);
                        db.SaveChanges();
                    }

                }

                var transactions = db.Transactions.Where(t => t.AccountId == account.Id).ToList();
                foreach (var trans in transactions)
                {
                    trans.Reconciled = true;
                    db.SaveChanges();
                }                   

                return RedirectToAction("Index");
            }

            return View(account);
        }

        // GET: Accounts/Delete/5
        [Authorize]
        public ActionResult Delete(int? id)
        {
            var user = db.Users.Find(User.Identity.GetUserId());

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Account account = db.Accounts.Find(id);


            if (account == null)
            {
                return HttpNotFound();
            }

            if (user.HouseholdId != account.HouseholdId)
            {
                return RedirectToAction("Warning", "Household");
            }

            return View(account);
        }

        // POST: Accounts/Delete/5
        [HttpPost, ActionName("Delete")]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            ApplicationUser user = db.Users.FirstOrDefault(u => u.UserName.Equals(User.Identity.Name));
            var userid = user.Id;
            var created = System.DateTime.Now;
            Account account = db.Accounts.Find(id);
            db.Accounts.Remove(account);
            db.SaveChanges();

            foreach (var member in user.Household.Users)
            {
                Notification n = new Notification()
                {
                    HouseholdId = user.Household.Id,
                    AuthorUserId = userid,
                    UserToNotifyId = member.Id,
                    Created = created,
                    Type = "Account Deleted",
                    Description = "Account Deleted: " + account.Name,
                };
                db.Notifications.Add(n);
                db.SaveChanges();
            }

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
