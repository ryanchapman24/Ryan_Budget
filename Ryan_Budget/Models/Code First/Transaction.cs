using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Ryan_Budget.Models
{
    public class Transaction
    {
        public int Id { get; set; }
        [Required]
        public decimal Amount { get; set; }
        [Required]
        public int AccountId { get; set; }
        [Required]
        public int? CategoryId { get; set; }
        [Required]
        public bool Type { get; set; }       
        public string Description { get; set; }
        public string Status { get; set; }
        public string AuthorUserId { get; set; }
        public string UpdateUserId { get; set; }
        public string Documentation { get; set; }
        public bool Reconciled { get; set; }
        [Required]
        public DateTimeOffset TransactionDate { get; set; }
        public DateTimeOffset Created { get; set; }
        public DateTimeOffset? Updated { get; set; }
        
        public virtual Account Account { get; set; }
        public virtual Category Category { get; set; }
        public virtual ApplicationUser AuthorUser { get; set; }
        public virtual ApplicationUser UpdateUser { get; set; }

    }
}