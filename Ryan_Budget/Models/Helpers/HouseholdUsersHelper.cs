using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Ryan_Budget.Models.Helpers
{
    public class HouseholdUsersHelper
    {
        ApplicationDbContext db = new ApplicationDbContext();

        public HouseholdUsersHelper(ApplicationDbContext context)
        {
            this.db = context;
        }

        public void AddUserToHousehold(string userId, int householdId)
        {
            ApplicationUser user = db.Users.Find(userId);
            Household household = db.Households.Find(householdId);
            household.Users.Add(user);
            db.SaveChanges();
        }

        public void RemoveUserFromHousehold(string userId, int householdId)
        {
            ApplicationUser user = db.Users.Find(userId);
            Household household = db.Households.Find(householdId);
            household.Users.Remove(user);
            db.SaveChanges();
        }
    }
}