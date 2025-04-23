using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DomainLayer.Domain;

namespace MarketMinds.Services.DreamTeam.ChatService;

public interface IChatService
{
    void Initialize(User currentUser, User targetUser);
    bool SendTextMessage(string textMessage);
    bool SendImageMessage(byte[] imageData);
    List<Message> GetInitialMessages();
    List<Message> CheckForNewMessages();
    Conversation? GetConversation();
}
