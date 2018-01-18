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
using Ryan_Budget.Models.Helpers;
using static Ryan_Budget.Models.Helpers.Extensions;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity.Owin;
using System.Configuration;
using SendGrid;
using System.Net.Mail;

namespace Ryan_Budget.Controllers
{
    [RequireHttps]
    public class HouseholdController : UserNames
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        private ApplicationUserManager _userManager;

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        // GET: Households/Notice
        [Authorize]
        public ActionResult Notice()
        {
            return View();
        }

        // GET: Households/Notice
        [Authorize]
        public ActionResult Warning()
        {
            return View();
        }


        // GET: Households
        [Authorize]
        public ActionResult Index()
        {
            var user = db.Users.Find(User.Identity.GetUserId());
            Household household = db.Households.Find(user.HouseholdId);
            var today = DateTime.Now.Day;
            var todayMonth = DateTime.Now.Month;
            var todayYear = DateTime.Now.Year;

            string highestUserId = "";
            decimal highestUserAmount = 0;

            foreach (var eachUser in user.Household.Users)
            {
                var data = (from transaction in db.Transactions
                            where (transaction.Account.HouseholdId == eachUser.HouseholdId && transaction.Created.Day == today && transaction.Created.Month == todayMonth && transaction.Created.Year == todayYear && transaction.AuthorUserId == eachUser.Id && transaction.Status != "VOID")
                            select transaction.Amount).DefaultIfEmpty().Sum();

                if (data < highestUserAmount)
                {
                    highestUserId = eachUser.Id;
                    highestUserAmount = data;
                }
            }

            if (highestUserId != "")
            {
                ApplicationUser highestUser = db.Users.Find(highestUserId);

                ViewBag.highestUserName = highestUser.FirstName;
                ViewBag.highestUserPic = highestUser.ProfilePic;
                ViewBag.highestUserAmount = highestUserAmount;
                ViewBag.highestUserFlag = true;
            }
            else
            {
                ViewBag.highestUserFlag = false;
            }

            return View(user.Household);
        }

        // GET: Households/Details/5
        [Authorize(Roles = "Admin")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Household household = db.Households.Find(id);
            if (household == null)
            {
                return HttpNotFound();
            }
            return View(household);
        }

        // GET: Households/Create
        [Authorize]
        public ActionResult CreateJoin(int? id, int? inviteId)
        {
            var user = db.Users.Find(User.Identity.GetUserId());

            if (user.HouseholdId != null)
            {
                return RedirectToAction("Notice", "Household");
            }
            ViewBag.InviteId = inviteId;
            Household household = db.Households.Find(id);
            return View(household);
        }

        // POST: Households/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Name,Created,Updated")] Household household, string name)
        {
            if (ModelState.IsValid)
            {
                var user = db.Users.Find(User.Identity.GetUserId());
                            
                household.Created = System.DateTime.Now;
                household.Name = name;
                db.Households.Add(household);
                db.SaveChanges();

                ApplicationUser attachUser = db.Users.FirstOrDefault(u => u.UserName.Equals(User.Identity.Name));
                Household attachHousehold = db.Households.Find(household.Id);
                HouseholdUsersHelper helper = new HouseholdUsersHelper(db);
                helper.AddUserToHousehold(attachUser.Id, attachHousehold.Id);
                db.SaveChanges();

                if (User.IsInRole("Admin"))
                {
                    return RedirectToAction("Index", "Admin");
                }
                else
                {
                    return RedirectToAction("Index");
                }
            }

            return View(household);
        }

        // GET: Households/Join/5
        [Authorize]
        public ActionResult Join(int? id, int? inviteId)
        {
            ViewBag.InviteId = inviteId;

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Household household = db.Households.Find(id);
            if (household == null)
            {
                return HttpNotFound();
            }
            return View(household);
        }

        // POST: Households/Join/5
        [Authorize]
        [HttpPost, ActionName("Join")]
        public ActionResult JoinConfirmed(int? id, int? inviteId)
        {
            ApplicationUser user = db.Users.FirstOrDefault(u => u.UserName.Equals(User.Identity.Name));
            var userid = user.Id;
            var created = System.DateTime.Now;

            Household household = db.Households.Find(id);

            ApplicationUser attachUser = db.Users.FirstOrDefault(u => u.UserName.Equals(User.Identity.Name));
            Household attachHousehold = db.Households.Find(household.Id);
            HouseholdUsersHelper helper = new HouseholdUsersHelper(db);
            attachHousehold.Updated = System.DateTimeOffset.Now;
            helper.AddUserToHousehold(attachUser.Id, attachHousehold.Id);
            db.SaveChanges();

            foreach (var member in user.Household.Users)
            {
                Notification n = new Notification()
                {
                    HouseholdId = household.Id,
                    AuthorUserId = userid,
                    UserToNotifyId = member.Id,
                    Created = created,
                    Type = "Member Join",
                    Description = user.FirstName + " has now joined your Household",
                };
                db.Notifications.Add(n);
                db.SaveChanges();
            }

            List<int> invitationIds = (from i in db.Invitations where i.InvitedUserEmail == user.Email select i.Id).ToList();
            if (invitationIds != null)
            {
                foreach (var invitationId in invitationIds)
                {
                    Invitation invite = db.Invitations.Find(invitationId);
                    db.Invitations.Remove(invite);
                    db.SaveChanges();
                }
            }

            return RedirectToAction("Index", "Household");
        }

        // GET: Households/Edit/5
        [Authorize]
        public ActionResult Edit(int? id)
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

            return View(household);
        }

        // POST: Households/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Name,Created,Updated")] Household household)
        {
            if (ModelState.IsValid)
            {
                household.Updated = System.DateTimeOffset.Now;
                db.Households.Attach(household);
                db.Entry(household).Property("Name").IsModified = true;
                db.Entry(household).Property("Updated").IsModified = true;
                db.SaveChanges();
                if (User.IsInRole("Admin"))
                {
                    return RedirectToAction("Index","Admin");
                }
                else
                {
                    return RedirectToAction("Index");
                }
            }
            return View(household);
        }

        // GET: Households/Delete/5
        [Authorize(Roles = "Admin")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Household household = db.Households.Find(id);
            if (household == null)
            {
                return HttpNotFound();
            }
            return View(household);
        }

        // POST: Households/Delete/5
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Household household = db.Households.Find(id);
            db.Households.Remove(household);
            db.SaveChanges();
          
            return RedirectToAction("Index", "Admin");           
        }

        // GET: Households/Leave/5
        [Authorize]
        public ActionResult Leave(int? id)
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

            return View(household);
        }

        // POST: Households/Leave/5
        [Authorize]
        [HttpPost, ActionName("Leave")]
        [ValidateAntiForgeryToken]
        public ActionResult LeaveConfirmed(Household household)
        {
            ApplicationUser user = db.Users.FirstOrDefault(u => u.UserName.Equals(User.Identity.Name));
            var userid = user.Id;
            var created = System.DateTime.Now;

            ApplicationUser detachUser = db.Users.FirstOrDefault(u => u.UserName.Equals(User.Identity.Name));
            Household detachHousehold = db.Households.Find(household.Id);
            HouseholdUsersHelper helper = new HouseholdUsersHelper(db);
            helper.RemoveUserFromHousehold(detachUser.Id, detachHousehold.Id);
            db.SaveChanges();

            foreach (var member in detachHousehold.Users.Where(u => u.Id != userid))
            {
                Notification n = new Notification()
                {
                    HouseholdId = household.Id,
                    AuthorUserId = userid,
                    UserToNotifyId = member.Id,
                    Created = created,
                    Type = "Member Leave",
                    Description = user.FirstName + " has left your Household",
                };
                db.Notifications.Add(n);
                db.SaveChanges();
            }

            return RedirectToAction("Index", "Home");
        }


        // GET: Households/InviteOthers/5
        [Authorize]
        public ActionResult InviteOthers(int? id)
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

            return View(household);
        }

        // POST: Households/InviteOthers/5
        [HttpPost]
        [Authorize]
        public async Task<ActionResult> InviteOthers(int hId, string email, string fName, string lName)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser user = db.Users.FirstOrDefault(u => u.UserName.Equals(User.Identity.Name));
                ApplicationUser emailCheck = db.Users.FirstOrDefault(u => u.UserName.Equals(email));

                Household household = db.Households.Find(hId);

                Invitation n = new Invitation
                {
                    HouseholdId = hId,
                    HouseholdUserId = user.Id,
                    Message = user.DisplayName + " invited you to join a Household: " + household.Name,              
                    Created = System.DateTimeOffset.Now,
                    InvitedUserEmail = email,
                    InvitedUserFirstName = fName,
                    InvitedUserLastName = lName
                };
                db.Invitations.Add(n);
                db.SaveChanges();

                // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                // Send an email with this link
                var apiKey = ConfigurationManager.AppSettings["SendGridAPIKey"];
                var from = ConfigurationManager.AppSettings["ContactEmail"];

                SendGridMessage myMessage = new SendGridMessage();
                myMessage.AddTo(email);
                myMessage.From = new MailAddress(from);
                myMessage.Subject = "NEW Household Invitation";

                if (emailCheck != null)
                {
                    var callbackUrl = Url.Action("CreateJoin", "Household", new { id = household.Id, inviteId = n.Id }, protocol: Request.Url.Scheme);
                    myMessage.Html = "You've been invited to join a Household on Pocket by " + user.DisplayName + "!  Please click <a href=\"" + callbackUrl + "\">here</a> to join.";
                }
                else
                {
                    var callbackUrl = Url.Action("Register", "Account", new { id = household.Id, Email = email, FirstName = fName, LastName = lName, inviteId = n.Id }, protocol: Request.Url.Scheme);
                    myMessage.Html = "You've been invited to join a Household on Pocket by " + user.DisplayName + "!  Our records indicate you have yet to create a Pocket account.  Please click <a href=\"" + callbackUrl + "\">here</a> to register. Remember to use the same email address as the one you received this email for.  Once you register, click Create/Join under the 'Household' tab in the side-bar.";
                }

                // Create a Web transport, using API Key
                var transportWeb = new Web(apiKey);
                // Send the email.
                try
                {
                    await transportWeb.DeliverAsync(myMessage);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    await Task.FromResult(0);
                }
            }
            return RedirectToAction("InviteOthers", "Household");
        }

        // POST: Notes/Create/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult CreateNote(Note note)
        {
            ApplicationUser user = db.Users.FirstOrDefault(u => u.UserName.Equals(User.Identity.Name));
            var userid = user.Id;
            var created = System.DateTime.Now;

            if (ModelState.IsValid)
            {
                note.AuthorUserId = User.Identity.GetUserId();
                note.Created = System.DateTime.Now;
                db.Notes.Add(note);
                db.SaveChanges();

                foreach (var member in user.Household.Users)
                {
                    Notification n = new Notification()
                    {
                        HouseholdId = user.Household.Id,
                        AuthorUserId = userid,
                        UserToNotifyId = member.Id,
                        Created = created,
                        Type = "New Note",
                        Description = "A new note has been added to your Household",
                    };
                    db.Notifications.Add(n);
                    db.SaveChanges();
                }
            }
            return RedirectToAction("Index");
        }

        // POST: Notes/Delete/5
        [Authorize]
        public ActionResult DeleteNote(int id)
        {
            var note = db.Notes.Find(id);

            db.Notes.Remove(note);
            db.SaveChanges();
            return Redirect(HttpContext.Request.UrlReferrer.AbsoluteUri);
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
