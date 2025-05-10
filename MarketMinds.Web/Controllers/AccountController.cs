using System;
using System.Security.Claims;
using System.Threading.Tasks;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.Services.UserService;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MarketMinds.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserService _userService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(IUserService userService, ILogger<AccountController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        // GET: Account/Login
        public IActionResult Login(string returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password, string returnUrl = null)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError(string.Empty, "Username and password are required.");
                ViewBag.ReturnUrl = returnUrl;
                return View();
            }

            try
            {
                var user = await _userService.GetUserByCredentialsAsync(username, password);
                
                if (user != null && user.Id != 0)
                {
                    await SignInUserAsync(user);
                    _logger.LogInformation($"User {username} logged in successfully");
                    
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    
                    return RedirectToAction("Index", "Home");
                }
                
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                ViewBag.ReturnUrl = returnUrl;
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error during login for user {username}");
                ModelState.AddModelError(string.Empty, "An error occurred during login. Please try again.");
                ViewBag.ReturnUrl = returnUrl;
                return View();
            }
        }

        // GET: Account/Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(User user)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Check if username is taken
                    var isUsernameTaken = await _userService.IsUsernameTakenAsync(user.Username);
                    if (isUsernameTaken)
                    {
                        ModelState.AddModelError("Username", "This username is already taken.");
                        return View(user);
                    }

                    // Check if email is taken
                    var existingUserByEmail = await _userService.GetUserByEmailAsync(user.Email);
                    if (existingUserByEmail != null && existingUserByEmail.Id != 0)
                    {
                        ModelState.AddModelError("Email", "This email is already registered.");
                        return View(user);
                    }

                    var registeredUser = await _userService.RegisterUserAsync(user);
                    
                    if (registeredUser != null && registeredUser.Id != 0)
                    {
                        await SignInUserAsync(registeredUser);
                        _logger.LogInformation($"User {user.Username} registered successfully");
                        return RedirectToAction("Index", "Home");
                    }
                    
                    ModelState.AddModelError(string.Empty, "Registration failed. Please try again.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error during registration for user {user.Username}");
                    ModelState.AddModelError(string.Empty, "An error occurred during registration. Please try again.");
                }
            }
            
            return View(user);
        }

        // POST: Account/Logout
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            _logger.LogInformation("User logged out");
            return RedirectToAction("Index", "Home");
        }

        // GET: Account/AccessDenied
        public IActionResult AccessDenied()
        {
            return View();
        }

        // Helper method to sign in a user
        private async Task SignInUserAsync(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
        }
    }
} 