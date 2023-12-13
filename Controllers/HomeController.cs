using DevDiary.Data;
using DevDiary.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;

namespace DevDiary.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<AppUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IEmailSender _emailService;
        private readonly IConfiguration _configuration;

        public HomeController(ILogger<HomeController> logger, UserManager<AppUser> userManager, ApplicationDbContext context, IEmailSender emailService, IConfiguration configuration)
        {
            _logger = logger;
            _userManager = userManager;
            _context = context;
            _emailService = emailService;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            return View();
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

        // Get
        [Authorize]
        public async Task<IActionResult> ContactMe()
        {
            string? appUserId = _userManager.GetUserId(User);

            if (appUserId == null)
            {
                return NotFound();

            }

            AppUser? appUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == appUserId);

            return View(appUser);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ContactMe([Bind("FirstName,LastName,Email")] AppUser appUser, string? message)
        {
            string? swalMessage = string.Empty;

            try
            {
                string? adminEmail = _configuration["AdminLoginEmail"] ?? Environment.GetEnvironmentVariable("AdminLoginEmail");

                if (adminEmail != null)
                {
                    await _emailService.SendEmailAsync(adminEmail, $"Contact Me Message From - {appUser.FullName}", message!);
                    swalMessage = "Email sent successfully";
                }
                else
                {
                    swalMessage = "Error: Admin email not configured.";
                }
            }
            catch (Exception)
            {
                swalMessage = "Error: Unable to send email.";
            }

            return RedirectToAction("ContactMe", new { swalMessage });
        }

    }
}