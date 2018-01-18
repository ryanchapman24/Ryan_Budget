using Ryan_Budget.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Ryan_Budget.Controllers
{
    [RequireHttps]
    public class AdminController : UserNames
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Admin
        [Authorize(Roles = "Admin")]
        public ActionResult Index()
        {
            return View(db.Households.ToList());
        }
    }
}