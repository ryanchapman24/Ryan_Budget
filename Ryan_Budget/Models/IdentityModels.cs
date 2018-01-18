using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Collections.Generic;

namespace Ryan_Budget.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string DisplayName { get; set; }
        public string ProfilePic { get; set; }
        public int? HouseholdId { get; set; }

        //public ApplicationUser()
        //{
        //    this.Accounts = new HashSet<Account>();
        //    this.Budgets = new HashSet<Budget>();
        //    this.Transactions = new HashSet<Transaction>();
        //    this.Categories = new HashSet<Category>();
        //}

        public virtual Household Household { get; set; }
        //public virtual ICollection<Account> Accounts { get; set; }
        //public virtual ICollection<Budget> Budgets { get; set; }
        //public virtual ICollection<Transaction> Transactions { get; set; }
        //public virtual ICollection<Category> Categories { get; set; }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            userIdentity.AddClaim(new Claim("HouseholdId", HouseholdId.ToString()));
            return userIdentity;

        }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        //All Code First Models need a DbSet Collection reference here!
        public DbSet<Household> Households { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Budget> Budgets { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Invitation> Invitations { get; set; }
        public DbSet<Note> Notes { get; set; }
        public DbSet<Frequency> Frequencies { get; set; }
        public DbSet<Notification> Notifications { get; set; }

    }
}