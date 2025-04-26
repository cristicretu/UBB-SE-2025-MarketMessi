using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using DomainLayer.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using server.Data;

namespace server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UsersController> _logger;

        public UsersController(ApplicationDbContext context, ILogger<UsersController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] User user)
        {
            try
            {
                if (user == null)
                {
                    return BadRequest("User data is null");
                }

                // Check if username is taken
                if (await _context.Users.AnyAsync(u => u.Username == user.Username))
                {
                    return Conflict("Username is already taken");
                }

                // Check if email is taken
                if (await _context.Users.AnyAsync(u => u.Email == user.Email))
                {
                    return Conflict("Email is already registered");
                }

                // Hash the password before saving
                user.PasswordHash = HashPassword(user.Password);
                user.Password = null; // Don't store plaintext password
                
                // Set default values for new users
                user.Balance = 0;
                user.Rating = 0;
                
                // Default to buyer role (1)
                user.UserType = 1;
                
                // Generate a token for the user
                user.Token = Guid.NewGuid().ToString();

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                return Created($"api/users/{user.Id}", user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering user");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
                {
                    return BadRequest("Login credentials are required");
                }

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);

                if (user == null)
                {
                    return NotFound("User not found");
                }

                // Verify the password
                if (!VerifyPassword(request.Password, user.PasswordHash))
                {
                    return Unauthorized("Invalid credentials");
                }

                // Generate a new token for the session
                user.Token = Guid.NewGuid().ToString();
                await _context.SaveChangesAsync();

                // Don't return the password hash in the response
                var userResponse = new
                {
                    user.Id,
                    user.Username,
                    user.Email,
                    user.UserType,
                    user.Balance,
                    user.Rating,
                    user.Token
                };

                return Ok(userResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("check-username/{username}")]
        public async Task<IActionResult> CheckUsername(string username)
        {
            try
            {
                if (string.IsNullOrEmpty(username))
                {
                    return BadRequest("Username is required");
                }

                bool exists = await _context.Users.AnyAsync(u => u.Username == username);
                return Ok(new { exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking username");
                return StatusCode(500, "Internal server error");
            }
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }

        private bool VerifyPassword(string password, string hashedPassword)
        {
            string hashedInput = HashPassword(password);
            return hashedInput == hashedPassword;
        }
    }

    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
} 