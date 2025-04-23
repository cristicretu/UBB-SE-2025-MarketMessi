using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using server.Models;

namespace server.Models
{
    // Represents an item in the shopping basket with its associated product, quantity, and price
    public class BasketItem
    {
        public int Id { get; set; }
        public BuyProduct Product { get; set; }
        public int Quantity { get; set; }
        public double Price { get; set; }

        public bool HasValidPrice { get; private set; }

        public string FormattedPrice => $"${Price:F2}";

        private const int DEFAULT_PRICE = 0;

        public BasketItem(int id, BuyProduct product, int quantity)
        {
            this.Id = id;
            this.Product = product;
            this.Quantity = quantity;

            // Set the price directly from the BuyProduct
            this.Price = product.Price;
            this.HasValidPrice = true;
        }

        // Default constructor for serialization
        public BasketItem()
        {
            this.Id = 0;
            this.Product = null;
            this.Quantity = 0;
            this.Price = DEFAULT_PRICE;
            this.HasValidPrice = false;
        }

        public float GetPrice()
        {
            // Calculates the total price for this basket item (unit price × quantity)
            return (float)(Price * Quantity);
        }
    }
}
