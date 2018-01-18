using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Ryan_Budget.Models
{
    public class Invitation
    {
        public int Id { get; set; }
        public int HouseholdId { get; set; }
        public string HouseholdUserId { get; set; }
        public string InvitedUserFirstName { get; set; }
        public string InvitedUserLastName { get; set; }
        public string InvitedUserEmail { get; set; }
        public string Message { get; set; }
        public DateTimeOffset Created { get; set; }

        public virtual Household Household { get; set; }       
        public virtual ApplicationUser HouseholdUser { get; set; }
    }
}