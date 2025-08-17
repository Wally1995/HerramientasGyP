using HerramientasGyP.Api.DataAccess;
using HerramientasGyP.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HerramientasGyP.Api.Helpers;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        // 1. Ensure default branch exists
        const string defaultBranchName = "Herramientas GyP - Central";
        const string defaultBranchIdNumber = "0011502991000M"; // Replace with valid legal ID

        var defaultBranch = await context.Branches
            .FirstOrDefaultAsync(b => b.Name == defaultBranchName);

        if (defaultBranch == null)
        {
            defaultBranch = new Branch
            {
                Id = Guid.NewGuid(),
                Name = defaultBranchName,
                IdentificationNumber = defaultBranchIdNumber
            };
            await context.Branches.AddAsync(defaultBranch);
            await context.SaveChangesAsync();
        }

        // 2. Ensure roles exist
        string role = "Super Admin";
        
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
        
        // 3. Create SuperAdmin user if missing
        var email = "walterb@herramientasgyp.com";
        var superAdmin = await userManager.FindByEmailAsync(email);

        if (superAdmin == null)
        {
            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                BranchId = defaultBranch.Id
            };

            var result = await userManager.CreateAsync(user, "SuperSecurePwd#2025");

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, "Super Admin");
            }
        }

        Person firstPerson = new Person()
        {
            FirstName = "Walter",
            LastName = "Blandon",
            Gender = "M",
            DateOfBirth = new DateTime(1995, 7, 31).ToUniversalTime(),
            DocumentId = "0013107950023P",
            ApplicationUserId = userManager.FindByEmailAsync(email).Result.Id
        };
        
        var firstPersonExists = await context.Persons.Where(x => x.DocumentId == firstPerson.DocumentId).FirstOrDefaultAsync();

        if (firstPersonExists == null)
        {
            context.Persons.Add(firstPerson);
            await context.SaveChangesAsync();
        }
        
        var status = scope.ServiceProvider.GetRequiredService<IStartupStatusService>();
        status.IsSeedComplete = true;
    }
}