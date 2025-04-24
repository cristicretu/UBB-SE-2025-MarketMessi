using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer.Domain
{
    // Represents a shopping basket which contains basket items
    public class Basket
    {
        public int Id { get; set; }
        public int BuyerId { get; set; }
        public List<BasketItem> Items { get; set; }

        public Basket(int id)
        {
            this.Id = id;
            this.BuyerId = 0;
            this.Items = new List<BasketItem>();
        }

        // Default constructor for serialization
        public Basket()
        {
            this.Id = 0;
            this.BuyerId = 0;
            this.Items = new List<BasketItem>();
        }

        public void AddItem(BuyProduct product, int quantity)
        {
            // Adds a product to the basket with the specified quantity
            // If the product already exists in the basket, its quantity is increased

            // Check if the product is already in the basket
            BasketItem existingItem = Items.FirstOrDefault(i => i.Product.Id == product.Id);

            if (existingItem != null)
            {
                // Update quantity of existing item
                existingItem.Quantity += quantity;
            }
            else
            {
                // Add new item
                BasketItem newItem = new(Items.Count + 1, product, quantity);
                Items.Add(newItem);
            }
        }

        public List<BasketItem> GetItems()
        {
            // Gets a copy of all items in the basket
            // Return a copy to prevent external modification
            return new List<BasketItem>(Items);
        }
    }
}
