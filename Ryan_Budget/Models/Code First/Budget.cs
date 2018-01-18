using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Ryan_Budget.Models
{
    public class Budget
    {
        public int Id { get; set; }
        public int HouseholdId { get; set; }
        [Required]
        public decimal Amount { get; set; }
        [Required]
        public int FrequencyId { get; set; }
        public int? CategoryId { get; set; }
        [Required]
        public bool Type { get; set; }
        [Required]
        public string Description { get; set; }
        public string AuthorUserId { get; set; }
        public string UpdateUserId { get; set; }
        public DateTimeOffset Created { get; set; }
        public DateTimeOffset? Updated { get; set; }

        public virtual Category Category { get; set; }
        public virtual Household Household { get; set; }
        public virtual ApplicationUser AuthorUser { get; set; }
        public virtual ApplicationUser UpdateUser { get; set; }
        public virtual Frequency Frequency { get; set; }
    }
}