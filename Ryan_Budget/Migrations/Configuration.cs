namespace Ryan_Budget.Migrations
{
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using Ryan_Budget.Models;
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<Ryan_Budget.Models.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
        }

        protected override void Seed(Ryan_Budget.Models.ApplicationDbContext context)
        {
            var roleManager = new RoleManager<IdentityRole>(
                new RoleStore<IdentityRole>(context));

            if (!context.Roles.Any(r => r.Name == "Admin"))
            {
                roleManager.Create(new IdentityRole { Name = "Admin" });
            }

            if (!context.Roles.Any(r => r.Name == "User"))
            {
                roleManager.Create(new IdentityRole { Name = "User" });
            }

            var userManager = new UserManager<ApplicationUser>(
                new UserStore<ApplicationUser>(context));

            if (!context.Users.Any(u => u.Email == "your email address"))
            {
                userManager.Create(new ApplicationUser
                {
                    UserName = "chapman.ryansadler@gmail.com",
                    Email = "chapman.ryansadler@gmail.com",
                    FirstName = "Ryan",
                    LastName = "Chapman",
                    DisplayName = "Ryan Chapman",
                }, "Chappy24!");
            }

            var userId_Super = userManager.FindByEmail("chapman.ryansadler@gmail.com").Id;
            userManager.AddToRole(userId_Super, "Admin");

            if (!context.Users.Any(u => u.Email == "your email address"))
            {
                userManager.Create(new ApplicationUser
                {
                    UserName = "guest@bugtracker.com",
                    Email = "guest@bugtracker.com",
                    FirstName = "Guest",
                    LastName = "User",
                    DisplayName = "Guest User",
                }, "Password-1");
            }

            var userId_Guest = userManager.FindByEmail("guest@bugtracker.com").Id;
            userManager.AddToRole(userId_Guest, "User");
        }
    }
}
