using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Ryan_Budget.Models
{   
    public class Household
    {
        public Household()
        {
            this.Budgets = new HashSet<Budget>();
            this.Accounts = new HashSet<Account>();
            this.Users = new HashSet<ApplicationUser>();
            this.Notes = new HashSet<Note>();
            this.Invitations = new HashSet<Invitation>();
            this.Notifications = new HashSet<Notification>();
        }
        
        public int Id { get; set; }
        //public int BudgetId { get; set; }
        [Required]
        public string Name { get; set; }
        public DateTimeOffset Created { get; set; }
        public DateTimeOffset? Updated { get; set; }

        public virtual ICollection<Budget> Budgets { get; set; }
        public virtual ICollection<Account> Accounts { get; set; }
        public virtual ICollection<ApplicationUser> Users { get; set; }
        public virtual ICollection<Invitation> Invitations { get; set; }
        public virtual ICollection<Note> Notes { get; set; }
        public virtual ICollection<Notification> Notifications { get; set; }
    }
}
