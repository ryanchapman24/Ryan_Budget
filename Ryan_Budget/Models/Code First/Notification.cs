using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Ryan_Budget.Models
{
    public class Notification
    {
        public int Id { get; set; }
        [Required]
        public int HouseholdId { get; set; }
        public int? AccountId { get; set; }
        public int? BudgetId { get; set; }
        public DateTimeOffset Created { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public string AuthorUserId { get; set; }
        public string UserToNotifyId { get; set; }

        public virtual Household Household { get; set; }
        public virtual ApplicationUser AuthorUser { get; set; }
        public virtual ApplicationUser UserToNotify { get; set; }
    }
}