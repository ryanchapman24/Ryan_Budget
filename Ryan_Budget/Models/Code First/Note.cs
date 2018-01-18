using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Ryan_Budget.Models
{
    public class Note
    {
        public int Id { get; set; }
        public DateTimeOffset Created { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public string Body { get; set; }
        public string AuthorUserId { get; set; }
        public int HouseholdId { get; set; }


        public virtual ApplicationUser AuthorUser { get; set; }

        public virtual Household Household { get; set; }
    }
}