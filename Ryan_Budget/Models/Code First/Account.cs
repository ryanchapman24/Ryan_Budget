using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Ryan_Budget.Models
{
    public class Account
    {
        public Account()
        {
            this.Transactions = new HashSet<Transaction>();          
        }
       
        public int Id { get; set; }
        public int HouseholdId { get; set; }
        [Required]
        public decimal Balance { get; set; }
        [Required]
        public string Name { get; set; }
        public DateTimeOffset Created { get; set; }
        public DateTimeOffset? Updated { get; set; }
        public DateTimeOffset? LastDateReconciled { get; set; }

        public virtual Household Household { get; set; }

        public virtual ICollection<Transaction> Transactions { get; set; }
    }
}