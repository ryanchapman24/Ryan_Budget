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
using System.IO;

namespace Ryan_Budget.Controllers
{
    [RequireHttps]
    public class TransactionsController : UserNames
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Transactions
        [Authorize]
        public ActionResult Index()
        {
            var user = db.Users.Find(User.Identity.GetUserId());           
            var transactions = user.Household.Accounts.SelectMany(a => a.Transactions);
            return View(transactions.OrderByDescending(t=>t.Id).ToList());
        }

        // GET: Transactions/Create
        [Authorize]
        public ActionResult Create()
        {
            ViewBag.AccountId = new SelectList(db.Accounts, "Id", "Name");           
            ViewBag.CategoryId = new SelectList(db.Categories, "Id", "Name");
            var expenses = db.Categories.Where(c => c.Type == false);
            var incomes = db.Categories.Where(c => c.Type == true);
            ViewBag.Expenses = expenses;
            ViewBag.Incomes = incomes;
            return View();
        }

        // POST: Transactions/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Amount,AccountId,CategoryId,Type,Description,AuthorUserId,UpdateUserId,Documentation,Reconciled,TransactionDate,Created,Updated,Status")] Transaction transaction, HttpPostedFileBase image)
        {
            Account account = db.Accounts.Find(transaction.AccountId);
            var oldBal = account.Balance;

            if (ModelState.IsValid)
            {           
                var user = db.Users.Find(User.Identity.GetUserId());
                var userid = user.Id;
                var created = System.DateTime.Now;

                if (ImageUploadValidator.IsWebFriendlyImage(image))
                {
                    var fileName = Path.GetFileName(image.FileName);
                    image.SaveAs(Path.Combine(Server.MapPath("~/Receipts/"), fileName));
                    transaction.Documentation = "~/Receipts/" + fileName;
                }

                transaction.AuthorUserId = user.Id;
                transaction.Created = System.DateTimeOffset.Now;
                if (transaction.Type == false)
                {
                    transaction.Amount = transaction.Amount * -1;
                }
                account.Balance = account.Balance + transaction.Amount;
                account.Updated = System.DateTimeOffset.Now;
                db.Transactions.Add(transaction);
                db.SaveChanges();

                if (oldBal > 0 && account.Balance < 0)
                {
                    foreach (var member in user.Household.Users)
                    {
                        Notification n = new Notification()
                        {
                            HouseholdId = user.Household.Id,
                            AccountId = account.Id,
                            AuthorUserId = userid,
                            UserToNotifyId = member.Id,
                            Created = created,
                            Type = "Overdraft Notice",
                            Description = account.Name + " is now overdrafted",
                        };
                        db.Notifications.Add(n);
                        db.SaveChanges();
                    }
                }

                return RedirectToAction("Index", "Home");
            }

            ViewBag.AccountId = new SelectList(db.Accounts, "Id", "Name", transaction.AccountId);
            ViewBag.CategoryId = new SelectList(db.Categories, "Id", "Name", transaction.CategoryId);
            var expenses = db.Categories.Where(c => c.Type == false);
            var incomes = db.Categories.Where(c => c.Type == true);
            ViewBag.Expenses = expenses;
            ViewBag.Incomes = incomes;
            return Redirect(HttpContext.Request.UrlReferrer.AbsoluteUri);
        }

        // GET: Transactions/Edit/5
        [Authorize]
        public ActionResult Edit(int? id, int? act)
        {
            var user = db.Users.Find(User.Identity.GetUserId());

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Transaction transaction = db.Transactions.Find(id);
            if (transaction == null)
            {
                return HttpNotFound();
            }

            if (user.HouseholdId != transaction.Account.HouseholdId)
            {
                return RedirectToAction("Warning", "Household");
            }

            if (act != null)
            {
                ViewBag.Account = transaction.AccountId;
            }

            ViewBag.Accounts = user.Household.Accounts.ToList();
            ViewBag.AccountId = new SelectList(ViewBag.Accounts, "Id", "Name", transaction.AccountId);
            ViewBag.CategoryId = new SelectList(db.Categories, "Id", "Name", transaction.CategoryId);
            var expenses = db.Categories.Where(c => c.Type == false);
            var incomes = db.Categories.Where(c => c.Type == true);
            ViewBag.Expenses = expenses;
            ViewBag.Incomes = incomes;
            return View(transaction);
        }

        // POST: Transactions/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Amount,AccountId,CategoryId,Type,Description,AuthorUserId,UpdateUserId,Documentation,Reconciled,TransactionDate,Created,Updated,Status")] Transaction transaction, HttpPostedFileBase image, int? act)
        {

            Transaction oldTrans = db.Transactions.AsNoTracking().FirstOrDefault(t => t.Id == transaction.Id);

            var user = db.Users.Find(User.Identity.GetUserId());
            var userid = user.Id;
            var created = System.DateTime.Now;
            Account account = db.Accounts.Find(transaction.AccountId);
            Account oldAccount = db.Accounts.Find(oldTrans.AccountId);
            var oldBal = account.Balance;        

            if (ModelState.IsValid)
            {
                transaction.Updated = System.DateTimeOffset.Now;
                transaction.UpdateUserId = user.Id;
                transaction.AuthorUserId = user.Id;
                if (transaction.Type == false)
                {
                    transaction.Amount = transaction.Amount * -1;
                }

                if (ImageUploadValidator.IsWebFriendlyImage(image))
                {
                    var fileName = Path.GetFileName(image.FileName);
                    image.SaveAs(Path.Combine(Server.MapPath("~/Receipts/"), fileName));
                    transaction.Documentation = "~/Receipts/" + fileName;
                }

                db.Transactions.Attach(transaction);
                db.Entry(transaction).Property("Amount").IsModified = true;
                db.Entry(transaction).Property("AccountId").IsModified = true;
                db.Entry(transaction).Property("CategoryId").IsModified = true;
                db.Entry(transaction).Property("Type").IsModified = true;
                db.Entry(transaction).Property("Description").IsModified = true;
                db.Entry(transaction).Property("Documentation").IsModified = true;
                db.Entry(transaction).Property("Updated").IsModified = true;
                db.Entry(transaction).Property("TransactionDate").IsModified = true;
                db.SaveChanges();

                if (oldTrans?.Amount != transaction.Amount)
                {
                    account.Balance = account.Balance - oldTrans.Amount + transaction.Amount;
                    account.Updated = System.DateTimeOffset.Now;
                    db.SaveChanges();

                    if (oldBal > 0 && account.Balance < 0)
                    {
                        foreach (var member in user.Household.Users)
                        {
                            Notification n = new Notification()
                            {
                                HouseholdId = user.Household.Id,
                                AccountId = account.Id,
                                AuthorUserId = userid,
                                UserToNotifyId = member.Id,
                                Created = created,
                                Type = "Overdraft Notice",
                                Description = account.Name + " is now overdrafted",
                            };
                            db.Notifications.Add(n);
                            db.SaveChanges();
                        }
                    }
                }

                if (oldTrans?.AccountId != transaction.AccountId)
                {
                    oldAccount.Balance = oldAccount.Balance - oldTrans.Amount;
                    transaction.Account.Balance = transaction.Account.Balance + transaction.Amount;
                    db.SaveChanges();                   
                }

                if (act != null)
                {
                    return RedirectToAction("Details", "Accounts", new { id = act });
                }

                return RedirectToAction("Index", "Home");
                
            }
            ViewBag.AccountId = new SelectList(db.Accounts, "Id", "Name", transaction.AccountId);
            ViewBag.CategoryId = new SelectList(db.Categories, "Id", "Name", transaction.CategoryId);
            var expenses = db.Categories.Where(c => c.Type == false);
            var incomes = db.Categories.Where(c => c.Type == true);
            ViewBag.Expenses = expenses;
            ViewBag.Incomes = incomes;
            return View(transaction);
        }

        // GET: Transactions/Void/5
        [Authorize]
        public ActionResult Void(int? id, int? act)
        {
            var user = db.Users.Find(User.Identity.GetUserId());

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Transaction transaction = db.Transactions.Find(id);
            if (transaction == null)
            {
                return HttpNotFound();
            }

            if (user.HouseholdId != transaction.Account.HouseholdId)
            {
                return RedirectToAction("Warning", "Household");
            }

            if (act != null)
            {
                ViewBag.Account = transaction.AccountId;
            }

            return View(transaction);
        }

        // POST: Transactions/Void/5
        [HttpPost, ActionName("Void")]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult VoidConfirmed(int id, int? act)
        {
            var user = db.Users.Find(User.Identity.GetUserId());
            var userid = user.Id;
            var created = System.DateTime.Now;
            Transaction transaction = db.Transactions.Find(id);
            transaction.Status = "VOID";
            Account account = db.Accounts.Find(transaction.AccountId);
            var oldBal = account.Balance;
            account.Balance = account.Balance - transaction.Amount;
            account.Updated = System.DateTimeOffset.Now;
            db.SaveChanges();

            if (oldBal > 0 && account.Balance < 0)
            {
                foreach (var member in user.Household.Users)
                {
                    Notification n = new Notification()
                    {
                        HouseholdId = user.Household.Id,
                        AccountId = account.Id,
                        AuthorUserId = userid,
                        UserToNotifyId = member.Id,
                        Created = created,
                        Type = "Overdraft Notice",
                        Description = account.Name + " is now overdrafted",
                    };
                    db.Notifications.Add(n);
                    db.SaveChanges();
                }
            }

            if (act != null)
            {
                return RedirectToAction("Details", "Accounts", new { id = act });
            }

            return RedirectToAction("Index", "Home");
        }

        // GET: Transactions/Unvoid/5
        [Authorize]
        public ActionResult Unvoid(int? id, int? act)
        {
            var user = db.Users.Find(User.Identity.GetUserId());

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Transaction transaction = db.Transactions.Find(id);
            if (transaction == null)
            {
                return HttpNotFound();
            }

            if (user.HouseholdId != transaction.Account.HouseholdId)
            {
                return RedirectToAction("Warning", "Household");
            }

            if (act != null)
            {
                ViewBag.Account = transaction.AccountId;
            }

            return View(transaction);
        }

        // POST: Transactions/Unvoid/5
        [HttpPost, ActionName("Unvoid")]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult UnvoidConfirmed(int id, int? act)
        {
            var user = db.Users.Find(User.Identity.GetUserId());
            var userid = user.Id;
            var created = System.DateTime.Now;
            Transaction transaction = db.Transactions.Find(id);
            transaction.Status = null;
            Account account = db.Accounts.Find(transaction.AccountId);
            var oldBal = account.Balance;
            account.Balance = account.Balance + transaction.Amount;
            account.Updated = System.DateTimeOffset.Now;
            db.SaveChanges();

            if (oldBal > 0 && account.Balance < 0)
            {
                foreach (var member in user.Household.Users)
                {
                    Notification n = new Notification()
                    {
                        HouseholdId = user.Household.Id,
                        AccountId = account.Id,
                        AuthorUserId = userid,
                        UserToNotifyId = member.Id,
                        Created = created,
                        Type = "Overdraft Notice",
                        Description = account.Name + " is now overdrafted",
                    };
                    db.Notifications.Add(n);
                    db.SaveChanges();
                }
            }

            if (act != null)
            {
                return RedirectToAction("Details", "Accounts", new { id = act });
            }

            return RedirectToAction("Index", "Home");
        }

        // GET: Transactions/Delete/5
        [Authorize]
        public ActionResult Delete(int? id, int? act)
        {
            var user = db.Users.Find(User.Identity.GetUserId());

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Transaction transaction = db.Transactions.Find(id);
            if (transaction == null)
            {
                return HttpNotFound();
            }

            if (user.HouseholdId != transaction.Account.HouseholdId)
            {
                return RedirectToAction("Warning", "Household");
            }

            if (act != null)
            {
                ViewBag.Account = transaction.AccountId;
            }

            return View(transaction);
        }

        // POST: Transactions/Delete/5
        [HttpPost, ActionName("Delete")]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id, int? act)
        {
            var user = db.Users.Find(User.Identity.GetUserId());
            var userid = user.Id;
            var created = System.DateTime.Now;
            Transaction transaction = db.Transactions.Find(id);
            Account account = db.Accounts.Find(transaction.AccountId);
            var oldBal = account.Balance;
            account.Balance = account.Balance - transaction.Amount;
            account.Updated = System.DateTimeOffset.Now;
            db.Transactions.Remove(transaction);
            db.SaveChanges();

            if (oldBal > 0 && account.Balance < 0)
            {
                foreach (var member in user.Household.Users)
                {
                    Notification n = new Notification()
                    {
                        HouseholdId = user.Household.Id,
                        AccountId = account.Id,
                        AuthorUserId = userid,
                        UserToNotifyId = member.Id,
                        Created = created,
                        Type = "Overdraft Notice",
                        Description = account.Name + " is now overdrafted",
                    };
                    db.Notifications.Add(n);
                    db.SaveChanges();
                }
            }

            if (act != null)
            {
                return RedirectToAction("Details", "Accounts", new { id = act });
            }

            return RedirectToAction("Index", "Home");
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
