using Microsoft.AspNetCore.Mvc;
// using server.Services; // Removed service dependency
using server.Models; // Using server models
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System;
using server.MarketMinds.Repositories.AccountRepository;

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

        // POST: api/account/{userId}/orders
        [HttpPost("{userId}/orders")]
        [ProducesResponseType(typeof(List<Order>), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<Order>>> CreateOrderFromBasket(int userId, [FromBody] CreateOrderRequest request)
        {
            _logger.LogInformation("CreateOrderFromBasket endpoint called for userId: {UserId}", userId);

            if (userId <= 0)
            {
                _logger.LogWarning("CreateOrderFromBasket called with invalid userId: {UserId}", userId);
                return BadRequest("User ID must be positive.");
            }

            if (request == null || request.BasketId <= 0)
            {
                _logger.LogWarning("CreateOrderFromBasket called with invalid basketId for userId: {UserId}", userId);
                return BadRequest("Basket ID must be provided and positive.");
            }

            try
            {
                // First get the user to check their balance
                var user = await _accountRepository.GetUserByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("User not found for userId: {UserId}", userId);
                    return NotFound($"User with ID {userId} not found.");
                }

                // Get the basket total cost
                var basketTotal = await _accountRepository.GetBasketTotalAsync(userId, request.BasketId);

                // Check if user has enough balance
                if (user.Balance < basketTotal)
                {
                    _logger.LogWarning("User {UserId} has insufficient funds. Balance: {Balance}, Required: {Total}",
                        userId, user.Balance, basketTotal);
                    return BadRequest($"Insufficient funds. Your balance is ${user.Balance:F2}, but the total cost is ${basketTotal:F2}.");
                }

                // Create orders from basket
                var createdOrders = await _accountRepository.CreateOrderFromBasketAsync(userId, request.BasketId);

                // Update user's balance
                user.Balance -= basketTotal;
                await _accountRepository.UpdateUserAsync(user);

                _logger.LogInformation("Successfully created {OrderCount} orders for userId: {UserId} from basketId: {BasketId}. New balance: {Balance}",
                    createdOrders.Count, userId, request.BasketId, user.Balance);

                return StatusCode(StatusCodes.Status201Created, createdOrders);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument in CreateOrderFromBasket for userId: {UserId}, basketId: {BasketId}",
                    userId, request.BasketId);
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Operation not valid in CreateOrderFromBasket for userId: {UserId}, basketId: {BasketId}",
                    userId, request.BasketId);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CreateOrderFromBasket for userId: {UserId}, basketId: {BasketId}",
                    userId, request.BasketId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "An unexpected error occurred creating orders from basket.");
            }
        }
    }

    // DTO for the create order request
    public class CreateOrderRequest
    {
        public int BasketId { get; set; }
    }
}