using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DomainLayer.Domain;
using MarketMinds.Services.DreamTeam.ChatService;

namespace MarketMinds.ViewModels;

public class ChatViewModel
{
    private readonly IChatService chatService;

    public ChatViewModel(IChatService chatService)
    {
        this.chatService = chatService;
    }

    public void InitializeChat(User currentUser, User targetUser)
    {
        chatService.Initialize(currentUser, targetUser);
    }

    public bool SendTextMessage(string textMessage)
    {
        return chatService.SendTextMessage(textMessage);
    }

    public bool SendImageMessage(byte[] imageData)
    {
        return chatService.SendImageMessage(imageData);
    }

    public List<Message> GetInitialMessages()
    {
        return chatService.GetInitialMessages();
    }

    public List<Message> CheckForNewMessages()
    {
        return chatService.CheckForNewMessages();
    }

    public Conversation GetConversation()
    {
        return chatService.GetConversation();
    }
}