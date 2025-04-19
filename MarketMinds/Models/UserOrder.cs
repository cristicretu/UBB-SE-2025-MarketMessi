using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Marketplace_SE.Objects
{
    public class UserOrder
    {
        public int id;
        public string name;
        public string description;
        public float cost;
        public ulong created;
        public int sellerId;
        public int buyerId;
        public string orderStatus;
    }
}
