using Microsoft.AspNetCore.Mvc;
using server.Models;
using System.Collections.Generic;
using System;
using System.Net;
using MarketMinds.Repositories.BasketRepository;
using server.DataAccessLayer;
using System.Linq;
using server.Models.DTOs;
using server.Models.DTOs.Mappers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MarketMinds.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BasketController : ControllerBase
    {
        private readonly IBasketRepository _basketRepository;
        private const int NOQUANTITY = 0;
        private const int NOUSER = 0;
        private const int NOBASKET = 0;
        private const int NOITEM = 0;
        private const double NODISCOUNT = 0;
        private const int MaxQuantityPerItem = 10;
        private const double SMALLEST_VALID_PRICE = 0;

        // Add JsonSerializerOptions that disables reference handling
        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            ReferenceHandler = ReferenceHandler.Preserve, // Use Preserve but we'll manually handle serialization
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public BasketController(IBasketRepository basketRepository)
        {
            _basketRepository = basketRepository;
        }

        [HttpGet("user/{userId}")]
        [ProducesResponseType(typeof(BasketDTO), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult GetBasketByUserId(int userId)
        {
            if (userId <= NOUSER)
            {
                return BadRequest("Invalid user ID");
            }

            try
            {
                var basket = _basketRepository.GetBasketByUserId(userId);

                // Ensure all basket items have ProductId set
                if (basket.Items != null)
                {
                    foreach (var item in basket.Items)
                    {
                        if (item.Product != null && item.ProductId == 0)
                        {
                            // If ProductId is not set, set it from the Product object
                            item.ProductId = item.Product.Id;
                        }
                    }
                }

                var basketDto = BasketMapper.ToDTO(basket);

                // Use the custom serializer settings and return the serialized JSON directly
                return new JsonResult(basketDto, _jsonOptions);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting basket for user ID {userId}: {ex}");
                return StatusCode((int)HttpStatusCode.InternalServerError, "An internal error occurred.");
            }
        }

        [HttpGet("{basketId}/items")]
        [ProducesResponseType(typeof(List<BasketItemDTO>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult GetBasketItems(int basketId)
        {
            if (basketId <= NOBASKET)
            {
                return BadRequest("Invalid basket ID");
            }

            try
            {
                var items = _basketRepository.GetBasketItems(basketId);

                // Ensure each item has ProductId set
                foreach (var item in items)
                {
                    if (item.Product != null && item.ProductId == 0)
                    {
                        // If ProductId is not set, set it from the Product object
                        item.ProductId = item.Product.Id;
                    }
                }

                var itemDtos = items.Select(item => BasketMapper.ToDTO(item)).ToList();

                // Use the custom serializer settings
                return new JsonResult(itemDtos, _jsonOptions);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting items for basket ID {basketId}: {ex}");
                return StatusCode((int)HttpStatusCode.InternalServerError, "An internal error occurred.");
            }
        }

        [HttpPost("user/{userId}/product/{productId}")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult AddProductToBasket(int userId, int productId, [FromBody] int quantity)
        {
            if (userId <= NOUSER)
            {
                return BadRequest("Invalid user ID");
            }
            if (productId <= NOITEM)
            {
                return BadRequest("Invalid product ID");
            }
            if (quantity < NOQUANTITY)
            {
                return BadRequest("Quantity cannot be negative");
            }

            try
            {
                // Apply the maximum quantity limit
                int limitedQuantity = Math.Min(quantity, MaxQuantityPerItem);

                // Get the user's basket
                Basket basket = _basketRepository.GetBasketByUserId(userId);

                // Add the item with the limited quantity
                _basketRepository.AddItemToBasket(basket.Id, productId, limitedQuantity);

                return Ok();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding product {productId} to basket for user {userId}: {ex}");
                return StatusCode((int)HttpStatusCode.InternalServerError, "An internal error occurred.");
            }
        }

        [HttpPut("user/{userId}/product/{productId}")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult UpdateProductQuantity(int userId, int productId, [FromBody] int quantity)
        {
            if (userId <= NOUSER)
            {
                return BadRequest("Invalid user ID");
            }
            if (productId <= NOITEM)
            {
                return BadRequest("Invalid product ID");
            }
            if (quantity < NOQUANTITY)
            {
                return BadRequest("Quantity cannot be negative");
            }

            try
            {
                int limitedQuantity = Math.Min(quantity, MaxQuantityPerItem);

                // Get the user's basket
                Basket basket = _basketRepository.GetBasketByUserId(userId);

                if (limitedQuantity == NOQUANTITY)
                {
                    // If quantity is zero, remove the item
                    _basketRepository.RemoveItemByProductId(basket.Id, productId);
                }
                else
                {
                    // Update the quantity
                    _basketRepository.UpdateItemQuantityByProductId(basket.Id, productId, limitedQuantity);
                }

                return Ok();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating quantity for product {productId} in basket for user {userId}: {ex}");
                return StatusCode((int)HttpStatusCode.InternalServerError, "An internal error occurred.");
            }
        }

        [HttpPut("user/{userId}/product/{productId}/increase")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult IncreaseProductQuantity(int userId, int productId)
        {
            if (userId <= NOUSER)
            {
                return BadRequest("Invalid user ID");
            }
            if (productId <= NOITEM)
            {
                return BadRequest("Invalid product ID");
            }

            try
            {
                // Get the user's basket
                Basket basket = _basketRepository.GetBasketByUserId(userId);

                // Get the current quantity of the item
                List<BasketItem> items = _basketRepository.GetBasketItems(basket.Id);
                BasketItem targetItem = items.FirstOrDefault(item => item.Product.Id == productId);

                if (targetItem == null)
                {
                    return NotFound("Item not found in basket");
                }

                // Calculate new quantity, ensuring it doesn't exceed the maximum
                int newQuantity = Math.Min(targetItem.Quantity + 1, MaxQuantityPerItem);

                // Update the quantity
                _basketRepository.UpdateItemQuantityByProductId(basket.Id, productId, newQuantity);

                return Ok();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error increasing quantity for product {productId} in basket for user {userId}: {ex}");
                return StatusCode((int)HttpStatusCode.InternalServerError, "An internal error occurred.");
            }
        }

        [HttpPut("user/{userId}/product/{productId}/decrease")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult DecreaseProductQuantity(int userId, int productId)
        {
            if (userId <= NOUSER)
            {
                return BadRequest("Invalid user ID");
            }
            if (productId <= NOITEM)
            {
                return BadRequest("Invalid product ID");
            }

            try
            {
                // Get the user's basket
                Basket basket = _basketRepository.GetBasketByUserId(userId);

                // Get the current quantity of the item
                List<BasketItem> items = _basketRepository.GetBasketItems(basket.Id);
                BasketItem targetItem = items.FirstOrDefault(item => item.Product.Id == productId);

                if (targetItem == null)
                {
                    return NotFound("Item not found in basket");
                }

                if (targetItem.Quantity > 1)
                {
                    // Decrease quantity by 1
                    _basketRepository.UpdateItemQuantityByProductId(basket.Id, productId, targetItem.Quantity - 1);
                }
                else
                {
                    // Remove item if quantity would be 0
                    _basketRepository.RemoveItemByProductId(basket.Id, productId);
                }

                return Ok();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error decreasing quantity for product {productId} in basket for user {userId}: {ex}");
                return StatusCode((int)HttpStatusCode.InternalServerError, "An internal error occurred.");
            }
        }

        [HttpDelete("user/{userId}/product/{productId}")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult RemoveProductFromBasket(int userId, int productId)
        {
            if (userId <= NOUSER)
            {
                return BadRequest("Invalid user ID");
            }
            if (productId <= NOITEM)
            {
                return BadRequest("Invalid product ID");
            }

            try
            {
                // Get the user's basket
                Basket basket = _basketRepository.GetBasketByUserId(userId);

                // Remove the product
                _basketRepository.RemoveItemByProductId(basket.Id, productId);

                return Ok();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error removing product {productId} from basket for user {userId}: {ex}");
                return StatusCode((int)HttpStatusCode.InternalServerError, "An internal error occurred.");
            }
        }

        [HttpDelete("user/{userId}/clear")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult ClearBasket(int userId)
        {
            if (userId <= NOUSER)
            {
                return BadRequest("Invalid user ID");
            }

            try
            {
                // Get the user's basket
                Basket basket = _basketRepository.GetBasketByUserId(userId);

                // Clear the basket
                _basketRepository.ClearBasket(basket.Id);

                return Ok();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error clearing basket for user {userId}: {ex}");
                return StatusCode((int)HttpStatusCode.InternalServerError, "An internal error occurred.");
            }
        }

        [HttpPost("{basketId}/promocode")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult ApplyPromoCode(int basketId, [FromBody] string code)
        {
            if (basketId <= NOBASKET)
            {
                return BadRequest("Invalid basket ID");
            }
            if (string.IsNullOrWhiteSpace(code))
            {
                return BadRequest("Promo code cannot be empty");
            }

            try
            {
                // Convert to uppercase for case-insensitive comparison
                string normalizedCode = code.ToUpper().Trim();

                // Dictionary of valid promo codes
                Dictionary<string, double> validCodes = new Dictionary<string, double>
                {
                    { "DISCOUNT10", 0.10 },  // 10% discount
                    { "WELCOME20", 0.20 },   // 20% discount
                    { "FLASH30", 0.30 },     // 30% discount
                };

                // Check if the code exists in the valid codes
                if (validCodes.TryGetValue(normalizedCode, out double discountRate))
                {
                    return Ok(new { DiscountRate = discountRate });
                }

                return BadRequest("Invalid promo code");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error applying promo code for basket {basketId}: {ex}");
                return StatusCode((int)HttpStatusCode.InternalServerError, "An internal error occurred.");
            }
        }

        [HttpGet("{basketId}/totals")]
        [ProducesResponseType(typeof(BasketTotalsDTO), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult CalculateBasketTotals(int basketId, [FromQuery] string promoCode = null)
        {
            if (basketId <= NOBASKET)
            {
                return BadRequest("Invalid basket ID");
            }

            try
            {
                List<BasketItem> items = _basketRepository.GetBasketItems(basketId);
                double subtotal = 0;

                foreach (var item in items)
                {
                    subtotal += item.GetPrice();
                }

                double discount = NODISCOUNT;

                if (!string.IsNullOrEmpty(promoCode))
                {
                    // Convert to uppercase for case-insensitive comparison
                    string normalizedCode = promoCode.ToUpper().Trim();

                    // Dictionary of valid promo codes
                    Dictionary<string, double> validCodes = new Dictionary<string, double>
                    {
                        { "DISCOUNT10", 0.10 },  // 10% discount
                        { "WELCOME20", 0.20 },   // 20% discount
                        { "FLASH30", 0.30 },     // 30% discount
                    };

                    // Check if the code exists in the valid codes
                    if (validCodes.TryGetValue(normalizedCode, out double discountRate))
                    {
                        discount = subtotal * discountRate;
                    }
                }

                double totalAmount = subtotal - discount;

                var basketTotals = new BasketTotals
                {
                    Subtotal = subtotal,
                    Discount = discount,
                    TotalAmount = totalAmount
                };

                var totalsDto = BasketMapper.ToDTO(basketTotals);

                // Use the custom serializer settings
                return new JsonResult(totalsDto, _jsonOptions);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error calculating totals for basket {basketId}: {ex}");
                return StatusCode((int)HttpStatusCode.InternalServerError, "An internal error occurred.");
            }
        }

        [HttpGet("{basketId}/validate")]
        [ProducesResponseType(typeof(bool), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult ValidateBasketBeforeCheckout(int basketId)
        {
            if (basketId <= NOBASKET)
            {
                return BadRequest("Invalid basket ID");
            }

            try
            {
                // Get the basket items
                List<BasketItem> items = _basketRepository.GetBasketItems(basketId);

                // Check if the basket is empty
                if (items.Count == 0)
                {
                    return Ok(false);
                }

                // Check if all items have valid quantities
                foreach (BasketItem item in items)
                {
                    if (item.Quantity <= NOQUANTITY)
                    {
                        return Ok(false);
                    }

                    if (item.Price <= SMALLEST_VALID_PRICE)
                    {
                        return Ok(false);
                    }
                }

                return Ok(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error validating basket {basketId} for checkout: {ex}");
                return StatusCode((int)HttpStatusCode.InternalServerError, "An internal error occurred.");
            }
        }

        // Helper class to return the basket total values
        public class BasketTotals
        {
            public double Subtotal { get; set; }
            public double Discount { get; set; }
            public double TotalAmount { get; set; }
        }
    }
}