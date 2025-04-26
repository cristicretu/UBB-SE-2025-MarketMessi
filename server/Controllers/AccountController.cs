using Microsoft.AspNetCore.Mvc;
// using server.Services; // Removed service dependency
using server.Repositories; // Added repository dependency
using server.Models; // Changed to use server models
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace server.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // Route: /api/account
    public class AccountController : ControllerBase
    {
        private readonly IAccountRepository _accountRepository; // Added repository field
        private readonly ILogger<AccountController> _logger;

        public AccountController(IAccountRepository accountRepository, ILogger<AccountController> logger)
        {
            _accountRepository = accountRepository;
            _logger = logger;
        }

        // GET: api/account/{userId}
        [HttpGet("{userId}")]
        [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<User>> GetUser(int userId)
        {
            _logger.LogInformation("GetUser endpoint called for userId: {UserId}", userId);
            if (userId <= 0)
            {
                _logger.LogWarning("GetUser called with invalid userId: {UserId}", userId);
                return BadRequest("User ID must be positive.");
            }

            try
            {
                var user = await _accountRepository.GetUserByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogInformation("User not found for userId: {UserId}", userId);
                    return NotFound();
                }
                return Ok(user);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error in GetUser endpoint for userId: {UserId}", userId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred fetching user data.");
            }
        }

        // GET: api/account/{userId}/orders
        [HttpGet("{userId}/orders")]
        [ProducesResponseType(typeof(List<UserOrder>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<UserOrder>>> GetUserOrders(int userId)
        {
            _logger.LogInformation("GetUserOrders endpoint called for userId: {UserId}", userId);
            if (userId <= 0)
            {
                 _logger.LogWarning("GetUserOrders called with invalid userId: {UserId}", userId);
                return BadRequest("User ID must be positive.");
            }

            try
            {
                var orders = await _accountRepository.GetUserOrdersAsync(userId);
                return Ok(orders);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error in GetUserOrders endpoint for userId: {UserId}", userId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred fetching user orders.");
            }
        }
    }
} 