using Microsoft.AspNet.Identity;
using Ryan_Budget.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Ryan_Budget.Controllers
{
    [RequireHttps]
    public class HomeController : UserNames
    {

        private ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult Landing()
        {
            return View();
        }

        [Authorize]
        public ActionResult Index()
        {
            return View();
        }

        [Authorize]
        public ActionResult DismissNotification(int? aId, int? bId, string uId, int? hId, int? nId, bool dA)
        {
            var user = User.Identity.GetUserId();
            var notification = db.Notifications.Find(nId);
            var account = db.Accounts.Find(aId);
            var budget = db.Budgets.Find(bId);
            var member = db.Users.Find(uId);
            var household = db.Households.Find(hId);

            if (dA && aId == null && bId == null && uId == null && nId == null)
            {
                var notes = db.Notifications.Where(u => u.UserToNotifyId == user);
                foreach (var note in notes)
                {
                    db.Notifications.Remove(note);
                }
                db.SaveChanges();
                return Redirect(HttpContext.Request.UrlReferrer.AbsoluteUri);
            }

            if (account != null)
            {
                db.Notifications.Remove(notification);
                db.SaveChanges();
                if (notification.Type == "Account Deleted")
                {
                    return RedirectToAction("Index", "Accounts");
                }
                return RedirectToAction("Details", "Accounts", new { id = account.Id });
            }
            if (budget != null)
            {
                db.Notifications.Remove(notification);
                db.SaveChanges();
                if (notification.Type == "Budget Deleted")
                {
                    return RedirectToAction("Index", "Budgets");
                }
                return RedirectToAction("Details", "Budgets", new { id = budget.Id });
            }
            if (member != null)
            {
                db.Notifications.Remove(notification);
                db.SaveChanges();
                return RedirectToAction("ProfilePage", "Home", new { id = member.Id });
            }
            if (household != null)
            {
                db.Notifications.Remove(notification);
                db.SaveChanges();
                return RedirectToAction("Index", "Household");
            }
            if (notification.Type == "Account Deleted")
            {
                db.Notifications.Remove(notification);
                db.SaveChanges();
                return RedirectToAction("Index", "Accounts");
            }
            if (notification.Type == "Budget Deleted")
            {
                db.Notifications.Remove(notification);
                db.SaveChanges();
                return RedirectToAction("Index", "Budgets");
            }
            else
            {
                db.Notifications.Remove(notification);
                db.SaveChanges();
                return Redirect(HttpContext.Request.UrlReferrer.AbsoluteUri);
            }
        }

        [Authorize]
        public ActionResult ProfilePage(string id)
        {
            var user = db.Users.Find(User.Identity.GetUserId());

            if (!string.IsNullOrWhiteSpace(id))
            {
                var userCheck = db.Users.Find(id);
                if (userCheck != null)
                {

                    if (user.HouseholdId != userCheck.HouseholdId)
                    {
                        return RedirectToAction("Warning", "Household");
                    }

                    var userA = userCheck.Id;
                    var activitiesA = db.Transactions.Where(a => a.AuthorUserId == userA && a.Account.HouseholdId == user.HouseholdId).OrderByDescending(b => b.Id);
                    ViewBag.Activities = activitiesA.ToList();

                    return View(userCheck);
                }

            }

            var userAc = User.Identity.GetUserId();
            var activitiesAc = db.Transactions.Where(a => a.AuthorUserId == userAc).OrderByDescending(b => b.Id);
            ViewBag.Activities = activitiesAc.ToList();

            return View(user);
        }
    }
}