using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer.Domain
{
    /// <summary>
    /// Represents the calculated totals for a basket.
    /// </summary>
    public class BasketTotals
    {
        /// <summary>
        /// Gets or sets the subtotal amount before discount.
        /// </summary>
        public double Subtotal { get; set; }

        /// <summary>
        /// Gets or sets the discount amount.
        /// </summary>
        public double Discount { get; set; }

        /// <summary>
        /// Gets or sets the total amount after discount.
        /// </summary>
        public double TotalAmount { get; set; }

        /// <summary>
        /// Gets the formatted subtotal with currency symbol.
        /// </summary>
        public string FormattedSubtotal => $"${Subtotal:F2}";

        /// <summary>
        /// Gets the formatted discount with currency symbol.
        /// </summary>
        public string FormattedDiscount => $"${Discount:F2}";

        /// <summary>
        /// Gets the formatted total amount with currency symbol.
        /// </summary>
        public string FormattedTotalAmount => $"${TotalAmount:F2}";

        /// <summary>
        /// Initializes a new instance of the BasketTotals class.
        /// </summary>
        public BasketTotals()
        {
            Subtotal = 0;
            Discount = 0;
            TotalAmount = 0;
        }

        /// <summary>
        /// Initializes a new instance of the BasketTotals class with the specified values.
        /// </summary>
        /// <param name="subtotal">The subtotal amount.</param>
        /// <param name="discount">The discount amount.</param>
        /// <param name="totalAmount">The total amount.</param>
        public BasketTotals(double subtotal, double discount, double totalAmount)
        {
            Subtotal = subtotal;
            Discount = discount;
            TotalAmount = totalAmount;
        }
    }
}