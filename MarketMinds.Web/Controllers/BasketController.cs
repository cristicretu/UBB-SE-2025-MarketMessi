using Microsoft.AspNetCore.Mvc;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.Services.BasketService;
using System;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using MarketMinds.Web.Models;

namespace MarketMinds.Web.Controllers
{
    [Authorize]
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
            // Get the current user's ID from claims
            int userId = User.GetCurrentUserId();
            
            try
            {
                // Get the user's basket
                var user = new User { Id = userId };
                var basket = _basketService.GetBasketByUser(user);
                
                // Calculate basket totals with no promo code
                var basketTotals = _basketService.CalculateBasketTotals(basket.Id, null);
                
                // Add debug info to diagnose the issue with basketTotals
                if (basketTotals == null)
                {
                    Debug.WriteLine("ERROR: basketTotals is null");
                    basketTotals = new BasketTotals(); // Create a default object to avoid null reference
                }
                else
                {
                    Debug.WriteLine($"DEBUG: basketTotals - Subtotal: {basketTotals.Subtotal}, Discount: {basketTotals.Discount}, Total: {basketTotals.TotalAmount}");
                }
                
                // Recalculate subtotal if needed by summing basket items
                if (basketTotals.Subtotal <= 0 && basket.Items != null && basket.Items.Count > 0)
                {
                    double calculatedSubtotal = 0;
                    foreach (var item in basket.Items)
                    {
                        calculatedSubtotal += (item.Price * item.Quantity);
                    }
                    
                    Debug.WriteLine($"DEBUG: Had to recalculate subtotal: {calculatedSubtotal}");
                    basketTotals.Subtotal = calculatedSubtotal;
                    basketTotals.TotalAmount = calculatedSubtotal - basketTotals.Discount;
                }
                
                // Pass the basket and totals to the view
                ViewBag.BasketTotals = basketTotals;
                
                return View(basket);
            }
            catch (Exception ex)
            {
                // Log the error
                Debug.WriteLine($"Error getting basket: {ex.Message}");
                
                // Return an empty basket if there was an error
                var emptyBasket = new Basket { Id = 0, Items = new System.Collections.Generic.List<BasketItem>() };
                
                // Create default basket totals to avoid null reference in the view
                ViewBag.BasketTotals = new BasketTotals();
                
                return View(emptyBasket);
            }
        }

        // POST: Basket/AddItem
        [HttpPost]
        public IActionResult AddItem(int productId, int quantity = 1)
        {
            // Get the current user's ID from claims
            int userId = User.GetCurrentUserId();
            
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
            // Get the current user's ID from claims
            int userId = User.GetCurrentUserId();
            
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
            // Get the current user's ID from claims
            int userId = User.GetCurrentUserId();
            
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
            // Get the current user's ID from claims
            int userId = User.GetCurrentUserId();
            
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
            // Get the current user's ID from claims
            int userId = User.GetCurrentUserId();
            
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
            // Get the current user's ID from claims
            int userId = User.GetCurrentUserId();
            
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
            // Get the current user's ID from claims
            int userId = User.GetCurrentUserId();
            
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
            // Get the current user's ID from claims
            int userId = User.GetCurrentUserId();
            
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
                
                // Manually calculate the subtotal and total to ensure accuracy
                double calculatedSubtotal = 0;
                if (basket.Items != null && basket.Items.Count > 0)
                {
                    foreach (var item in basket.Items)
                    {
                        calculatedSubtotal += (item.Price * item.Quantity);
                    }
                    
                    Debug.WriteLine($"DEBUG: Manually calculated subtotal: {calculatedSubtotal}");
                    basketTotals.Subtotal = calculatedSubtotal;
                    basketTotals.TotalAmount = calculatedSubtotal - basketTotals.Discount;
                }
                
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