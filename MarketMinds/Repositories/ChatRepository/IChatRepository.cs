using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using DomainLayer.Domain;

namespace MarketMinds.Repositories.ChatRepository;

public interface IChatRepository
{
    Conversation? GetConversation(int userId1, int userId2);
    Conversation CreateConversation(int userId1, int userId2);
    List<Message> GetMessages(int conversationid, long sinceTimestamp = 0);
    bool AddMessage(Message message);
}
