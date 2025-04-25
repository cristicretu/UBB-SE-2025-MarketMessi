using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer.Domain
{
    public class Message
    {
        public int Id { get; set; }
        public int ConversationId { get; set; }
        public int Creator { get; set; }
        public long Timestamp { get; set; }
        public string ContentType { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }
}
