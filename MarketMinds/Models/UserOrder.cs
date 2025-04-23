using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer.Domain;

public class UserOrder
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public float Cost { get; set; }
    public ulong Created { get; set; }
    public int SellerId { get; set; }
    public int BuyerId { get; set; }
    public string OrderStatus { get; set; } = string.Empty;
}
