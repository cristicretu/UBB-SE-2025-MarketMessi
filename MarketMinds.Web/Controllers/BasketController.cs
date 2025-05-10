using Microsoft.AspNetCore.Mvc;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.Services.BasketService;
using System;
using System.Threading.Tasks;
using System.Diagnostics;

namespace MarketMinds.Web.Controllers
{
    public class BasketController : Controller
    {
        private readonly IBasketService _basketService;

        public BasketController(IBasketService basketService)
        {
            _basketService = basketService;
        }

        // GET: Basket
        public IActionResult Index()
        {
            // For demo purposes, assume the user ID is 1
            int userId = 1;
            
            try
            {
                // Get the user's basket
                var user = new User { Id = userId };
                var basket = _basketService.GetBasketByUser(user);
                
                // Calculate basket totals with no promo code
                var basketTotals = _basketService.CalculateBasketTotals(basket.Id, null);
                
                // Pass the basket and totals to the view
                ViewBag.BasketTotals = basketTotals;
                
                return View(basket);
            }
            catch (Exception ex)
            {
                // Log the error
                Debug.WriteLine($"Error getting basket: {ex.Message}");
                
                // Return an empty basket if there was an error
                return View(new Basket { Id = 0, Items = new System.Collections.Generic.List<BasketItem>() });
            }
        }

        // POST: Basket/AddItem
        [HttpPost]
        public IActionResult AddItem(int productId, int quantity = 1)
        {
            // For demo purposes, assume the user ID is 1
            int userId = 1;
            
            try
            {
                // Validate the quantity input
                if (quantity <= 0)
                {
                    quantity = 1;
                }

                // Add the product to the basket
                _basketService.AddProductToBasket(userId, productId, quantity);
                
                // Redirect to the basket page
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                // Log the error
                Debug.WriteLine($"Error adding item to basket: {ex.Message}");
                
                // Return to the basket page with an error message
                TempData["ErrorMessage"] = "Failed to add item to basket. Please try again.";
                return RedirectToAction("Index");
            }
        }

        // POST: Basket/RemoveItem
        [HttpPost]
        public IActionResult RemoveItem(int productId)
        {
            // For demo purposes, assume the user ID is 1
            int userId = 1;
            
            try
            {
                // Remove the product from the basket
                _basketService.RemoveProductFromBasket(userId, productId);
                
                // Redirect to the basket page
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                // Log the error
                Debug.WriteLine($"Error removing item from basket: {ex.Message}");
                
                // Return to the basket page with an error message
                TempData["ErrorMessage"] = "Failed to remove item from basket. Please try again.";
                return RedirectToAction("Index");
            }
        }

        // POST: Basket/UpdateQuantity
        [HttpPost]
        public IActionResult UpdateQuantity(int productId, int quantity)
        {
            // For demo purposes, assume the user ID is 1
            int userId = 1;
            
            try
            {
                // Validate the quantity input
                if (quantity <= 0)
                {
                    // If quantity is 0 or negative, remove the item
                    _basketService.RemoveProductFromBasket(userId, productId);
                }
                else
                {
                    // Update the quantity
                    _basketService.UpdateProductQuantity(userId, productId, quantity);
                }
                
                // Redirect to the basket page
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                // Log the error
                Debug.WriteLine($"Error updating quantity: {ex.Message}");
                
                // Return to the basket page with an error message
                TempData["ErrorMessage"] = "Failed to update quantity. Please try again.";
                return RedirectToAction("Index");
            }
        }

        // POST: Basket/IncreaseQuantity
        [HttpPost]
        public IActionResult IncreaseQuantity(int productId)
        {
            // For demo purposes, assume the user ID is 1
            int userId = 1;
            
            try
            {
                // Increase the quantity by 1
                _basketService.IncreaseProductQuantity(userId, productId);
                
                // Redirect to the basket page
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                // Log the error
                Debug.WriteLine($"Error increasing quantity: {ex.Message}");
                
                // Return to the basket page with an error message
                TempData["ErrorMessage"] = "Failed to increase quantity. Please try again.";
                return RedirectToAction("Index");
            }
        }

        // POST: Basket/DecreaseQuantity
        [HttpPost]
        public IActionResult DecreaseQuantity(int productId)
        {
            // For demo purposes, assume the user ID is 1
            int userId = 1;
            
            try
            {
                // Decrease the quantity by 1
                _basketService.DecreaseProductQuantity(userId, productId);
                
                // Redirect to the basket page
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                // Log the error
                Debug.WriteLine($"Error decreasing quantity: {ex.Message}");
                
                // Return to the basket page with an error message
                TempData["ErrorMessage"] = "Failed to decrease quantity. Please try again.";
                return RedirectToAction("Index");
            }
        }

        // POST: Basket/ClearBasket
        [HttpPost]
        public IActionResult ClearBasket()
        {
            // For demo purposes, assume the user ID is 1
            int userId = 1;
            
            try
            {
                // Clear the basket
                _basketService.ClearBasket(userId);
                
                // Redirect to the basket page
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                // Log the error
                Debug.WriteLine($"Error clearing basket: {ex.Message}");
                
                // Return to the basket page with an error message
                TempData["ErrorMessage"] = "Failed to clear basket. Please try again.";
                return RedirectToAction("Index");
            }
        }

        // POST: Basket/ApplyPromoCode
        [HttpPost]
        public IActionResult ApplyPromoCode(string promoCode)
        {
            // For demo purposes, assume the user ID is 1
            int userId = 1;
            
            try
            {
                // Get the user's basket
                var user = new User { Id = userId };
                var basket = _basketService.GetBasketByUser(user);
                
                // Apply the promo code
                _basketService.ApplyPromoCode(basket.Id, promoCode);
                
                // Redirect to the basket page with the applied promo code
                return RedirectToAction("Index", new { promoCode });
            }
            catch (InvalidOperationException ex)
            {
                // Log the error
                Debug.WriteLine($"Invalid promo code: {ex.Message}");
                
                // Return to the basket page with an error message
                TempData["ErrorMessage"] = "Invalid promo code. Please try again.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                // Log the error
                Debug.WriteLine($"Error applying promo code: {ex.Message}");
                
                // Return to the basket page with an error message
                TempData["ErrorMessage"] = "Failed to apply promo code. Please try again.";
                return RedirectToAction("Index");
            }
        }

        // POST: Basket/Checkout
        [HttpPost]
        public async Task<IActionResult> Checkout(string promoCode)
        {
            // For demo purposes, assume the user ID is 1
            int userId = 1;
            
            try
            {
                // Get the user's basket
                var user = new User { Id = userId };
                var basket = _basketService.GetBasketByUser(user);
                
                // Validate the basket before checkout
                if (!_basketService.ValidateBasketBeforeCheckOut(basket.Id))
                {
                    TempData["ErrorMessage"] = "Your basket is invalid for checkout. Please ensure all items have valid quantities.";
                    return RedirectToAction("Index");
                }
                
                // Calculate the totals with any applied promo code
                var basketTotals = _basketService.CalculateBasketTotals(basket.Id, promoCode);
                
                // Perform the checkout
                bool success = await _basketService.CheckoutBasketAsync(userId, basket.Id, basketTotals.Discount, basketTotals.TotalAmount);
                
                if (success)
                {
                    // Redirect to a confirmation page
                    TempData["SuccessMessage"] = "Your order has been successfully placed!";
                    return RedirectToAction("OrderConfirmation");
                }
                else
                {
                    // Return to the basket page with an error message
                    TempData["ErrorMessage"] = "Checkout failed. Please try again.";
                    return RedirectToAction("Index");
                }
            }
            catch (InvalidOperationException ex)
            {
                // Log the error
                Debug.WriteLine($"Checkout validation error: {ex.Message}");
                
                // Return to the basket page with the specific error message
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                // Log the error
                Debug.WriteLine($"Error during checkout: {ex.Message}");
                
                // Return to the basket page with an error message
                TempData["ErrorMessage"] = "An error occurred during checkout. Please try again.";
                return RedirectToAction("Index");
            }
        }

        // GET: Basket/OrderConfirmation
        public IActionResult OrderConfirmation()
        {
            return View();
        }
    }
} 