using boiDokan.dal.Data;
using boiDokan.entities.Models;
using boiDokan.utility;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace boiDokan.dal;

public class DbInitializer : IDbInitializer
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ApplicationDbContext _dbContext;

    public DbInitializer(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager,
        ApplicationDbContext dbContext)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _dbContext = dbContext;
    }


    public void Initialize()
    {
        //migrations if not applied

        try
        {
            if (_dbContext.Database.GetPendingMigrations().Any())
            {
                _dbContext.Database.Migrate();
            }
        }
        catch (Exception ex)
        {
        }

        //create role if not exists
        if (!_roleManager.RoleExistsAsync(SD.RoleAdmin).GetAwaiter().GetResult())
        {
            _roleManager.CreateAsync(new IdentityRole(SD.RoleAdmin)).GetAwaiter().GetResult();
            _roleManager.CreateAsync(new IdentityRole(SD.RoleEmployee)).GetAwaiter().GetResult();
            _roleManager.CreateAsync(new IdentityRole(SD.RoleUserIndividual)).GetAwaiter().GetResult();
            _roleManager.CreateAsync(new IdentityRole(SD.RoleUserCompany)).GetAwaiter().GetResult();

            //create user

            _userManager.CreateAsync(user: new ApplicationUser()
            {
                UserName = "admin@boidokan.live",
                Email = "admin@boidokan.live",
                Name = "Admin",
                PhoneNumber = "+5943252234",
                StreetAddress = "address",
                State = "Il",
                PostalCode = "3434",
                City = "Chicago",
                EmailConfirmed = true
            }, password: "admin").GetAwaiter().GetResult();
            
            var user = _dbContext.ApplicationUsers!.FirstOrDefault(u => u.Email == "admin@boidokan.live");

            _userManager.AddToRoleAsync(user, SD.RoleAdmin).GetAwaiter().GetResult();
        }

        
    }
}