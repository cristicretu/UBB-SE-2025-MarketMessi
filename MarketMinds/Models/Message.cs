using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Marketplace_SE.Objects
{
    public class Message
    {
        public int id;
        public int conversationId;
        public int creator;
        public long timestamp;
        public string contentType;
        public string content;
    }
}
