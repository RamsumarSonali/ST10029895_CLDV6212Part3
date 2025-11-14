using ABCRetailers.Models;
using ABCRetailers.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ABCRetailers.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(IAuthService authService, ILogger<AccountController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        // GET: /Account/Login
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginDto model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // 1. Use the AuthService to validate the user
                var authResult = await _authService.AuthenticateAsync(model.Email, model.Password);

                if (authResult.IsSuccess && authResult.User != null)
                {
                    _logger.LogInformation("User {Email} logged in successfully.", model.Email);

                    // 2. Create the security claims
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, authResult.User.UserId.ToString()),
                        new Claim(ClaimTypes.Name, authResult.User.Username),
                        new Claim(ClaimTypes.Email, authResult.User.Email),
                        new Claim(ClaimTypes.Role, authResult.User.Role)
                        // Add any other claims from the User object you need
                    };

                    // Add the JWT token as a claim (optional, but can be useful)
                    if (!string.IsNullOrEmpty(authResult.Token))
                    {
                        claims.Add(new Claim("access_token", authResult.Token));
                    }

                    var claimsIdentity = new ClaimsIdentity(
                        claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    var authProperties = new AuthenticationProperties
                    {
                        // Allow a persistent cookie (RememberMe)
                        IsPersistent = model.RememberMe,

                        // Set cookie expiration
                        ExpiresUtc = DateTimeOffset.UtcNow.AddDays(model.RememberMe ? 7 : 1)
                    };

                    // 3. Sign the user in (creates the auth cookie)
                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        authProperties);

                    // 4. Redirect to the original page or home
                    return RedirectToLocal(returnUrl);
                }
                else
                {
                    // If auth failed, show a generic error
                    var error = authResult.Errors?.FirstOrDefault() ?? "Invalid email or password.";
                    _logger.LogWarning("Failed login attempt for {Email}: {Error}", model.Email, error);
                    ModelState.AddModelError(string.Empty, error);
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during login for {Email}", model.Email);
                ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again.");
                return View(model);
            }
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            _logger.LogInformation("User {Name} logging out.", User.Identity?.Name);

            // Deletes the authentication cookie
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Index", "Home");
        }

        // Helper to prevent open redirect attacks
        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }
    }
}