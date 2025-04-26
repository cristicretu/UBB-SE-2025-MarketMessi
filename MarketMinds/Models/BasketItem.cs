using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer.Domain
{
    // Represents an item in the shopping basket with its associated product, quantity, and price
    public class BasketItem
    {
        public int Id { get; set; }
        public BuyProduct Product { get; set; }

        // Add missing properties to match server model
        public int ProductId { get; set; }
        public int BasketId { get; set; }

        public int Quantity { get; set; }
        public double Price { get; set; }

        public string FormattedPrice => $"${Price:F2}";

        private const int DEFAULT_PRICE = 0;

        public BasketItem(int id, BuyProduct product, int quantity)
        {
            this.Id = id;
            this.Product = product;
            this.ProductId = product.Id; // Set ProductId from the product
            this.Quantity = quantity;

            // Set the price based on the product
            this.Price = product.Price;
        }

        // Default constructor for serialization
        public BasketItem()
        {
            this.Id = 0;
            this.Product = null;
            this.ProductId = 0;
            this.BasketId = 0;
            this.Quantity = 0;
            this.Price = DEFAULT_PRICE;
        }

        public double GetPrice()
        {
            // Calculates the total price for this basket item (unit price × quantity)
            return Price * Quantity;
        }
    }
}