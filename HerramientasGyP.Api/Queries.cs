using HerramientasGyP.Api.DataAccess;
using HerramientasGyP.Api.Models;
using HerramientasGyP.Api.Models.Dtos.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HerramientasGyP.Api;

public class Queries
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _applicationDbContext;

    public Queries(ApplicationDbContext applicationDbContext, UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
        _applicationDbContext = applicationDbContext;
    }
    
    public async Task<object> GetLoginDto(LoginModel loginModel)
    {
        var anon = await _applicationDbContext.Users
            .Where(x => x.Email == loginModel.ApplicationUserEmail)
            .Select(x => new
            {
                x.Email,
                Person = new
                {
                    x.Person.DocumentId
                }
            })
            .AsNoTracking()
            .FirstOrDefaultAsync();
        
        
        return anon;
    }
    
    public async Task<object> GetLoginsDto(LoginModel loginModel)
    {
        var anon = await _applicationDbContext.Users
            .Where(x => x.Email == loginModel.ApplicationUserEmail)
            .Select(x => new
            {
                x.Email,
                Person = new
                {
                    x.Person.DocumentId
                }
            })
            .AsNoTracking()
            .ToListAsync();
        
        
        return anon;
    }
    
    public async Task<object> GetApplicationUser(string Email)
    {
        var anon = await _applicationDbContext.Users
            .Where(x => x.Email == Email)
            .Select(x => new
            {
                x.Email,
                Person = new
                {
                    x.Person.FirstName,
                    x.Person.LastName,
                    x.Person.Gender,
                    x.Person.DateOfBirth
                }
            })
            .AsNoTracking()
            .FirstOrDefaultAsync();
        
        
        return anon;
    }
}