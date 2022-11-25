#nullable disable

using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;

using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

using webapptask4.Models;
using webapptask4.Data;
using webapptask4.Areas.Identity.Data;

namespace webapptask4.Controllers;

public class HomeController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger,
    ApplicationDbContext context,
    SignInManager<ApplicationUser> signInManager,
    UserManager<ApplicationUser> userManager)
    {
        _logger = logger;
        _context = context;
        _signInManager = signInManager;
        _userManager = userManager;
    }


    public async Task<IActionResult> Index()
    {
        var loginStatus = _signInManager.IsSignedIn(User);
        if (loginStatus)
        {
            var listOfContent = await _context.Users.ToListAsync() ?? new List<ApplicationUser>();
            return View(listOfContent);
        }
        else
        {
            return View();
        }
    }

    public IActionResult Privacy()
    {
        return View();
    }



    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    [HttpPost]
    public IActionResult Index(String[] ID, String action)
    {

        foreach (string id in ID)
        {
            Console.WriteLine(id);
        }

        switch (action)
        {
            case "delete": deleteUser(ID); break;
            case "block": blockUser(ID); break;
            case "unblock": unblockUser(ID); break;
        }
        return RedirectToPage("/");
    }

    public async void deleteUser(String[] ID)
    {
        var CurrentUserId = _userManager.GetUserId(User);

        foreach (var id in ID)
        {
            var person = _context.Users.Single(p => p.Id == id);
            _context.Entry(person).State = EntityState.Deleted;

            var saved = false;
            while (!saved)
            {
                try
                {
                    // Attempt to save changes to the database
                    _context.SaveChanges();
                    saved = true;
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    foreach (var entry in ex.Entries)
                    {
                        if (entry.Entity is ApplicationUser)
                        {
                            var proposedValues = entry.CurrentValues;
                            var databaseValues = entry.GetDatabaseValues();

                            foreach (var property in proposedValues.Properties)
                            {
                                var proposedValue = proposedValues[property];
                                var databaseValue = databaseValues[property];

                                // TODO: decide which value should be written to database
                                // proposedValues[property] = <value to be saved>;
                            }

                            // Refresh original values to bypass next concurrency check
                            entry.OriginalValues.SetValues(databaseValues);
                        }
                        else
                        {
                            throw new NotSupportedException(
                                "Don't know how to handle concurrency conflicts for "
                                + entry.Metadata.Name);
                        }
                    }
                }
            }
        }

        if (ID.Contains(CurrentUserId))
        {
            await _signInManager.SignOutAsync();
        }
    }

    public async void blockUser(String[] ID)
    {   
        var CurrentUserId = _userManager.GetUserId(User);

        foreach (var id in ID)
        {
            var user = _context.Users.Single(p => p.Id == id);
            if(user != null){
                user.LockoutEnd = DateTimeOffset.MaxValue;
                _context.SaveChanges();
            }  
        }

        if (ID.Contains(CurrentUserId))
        {
            await _signInManager.SignOutAsync();
        }
    }

    public void unblockUser(String[] ID)
    {   
        foreach (var id in ID)
        {
            var user = _context.Users.Single(p => p.Id == id);
            if(user != null){
                user.LockoutEnd = null;
                _context.SaveChanges();
            }  
        }
    }
}
