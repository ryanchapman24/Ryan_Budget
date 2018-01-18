using Ryan_Budget.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Ryan_Budget.Models
{
    public class UserNames : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (User.Identity.IsAuthenticated)
            {
                var user = db.Users.Find(User.Identity.GetUserId());
                ViewBag.UserId = user.Id;
                if (user.Household != null)
                {
                ViewBag.Notifications = user.Household.Notifications.Where(n => n.UserToNotifyId == user.Id).OrderByDescending(n => n.Id).ToList();
                ViewBag.Invitations = user.Household.Invitations;
                }
                //if (user.Household == null)
                //{
                //    var invitation = db.Invitations.Where(i => i.InvitedUserEmail == user.Email);
                //    ViewBag.Invitation = invitation;
                //}
                ViewBag.DisplayName = user.DisplayName;
                ViewBag.FirstName = user.FirstName;
                ViewBag.LastName = user.LastName;
                ViewBag.ProfilePic = user.ProfilePic;
                base.OnActionExecuting(filterContext);
            }
        }
    }
}