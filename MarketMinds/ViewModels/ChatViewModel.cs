using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DomainLayer.Domain;
using MarketMinds.Services.DreamTeam.ChatService;

namespace MarketMinds.ViewModels;

public class ChatViewModel
{
    private readonly IChatService chatService;
    private int currentUserId;
    private int currentConversationId;

    public ChatViewModel(IChatService chatService)
    {
        this.chatService = chatService;
    }

    public async Task InitializeAsync(int userId)
    {
        currentUserId = userId;
        var conversations = await chatService.GetUserConversationsAsync(userId);

        if (conversations != null && conversations.Count > 0)
        {
            currentConversationId = conversations[0].Id;
        }
        else
        {
            var newConversation = await chatService.CreateConversationAsync(userId);
            currentConversationId = newConversation.Id;
        }
    }

    public async Task<Message> SendMessageAsync(string content)
    {
        return await chatService.SendMessageAsync(currentConversationId, currentUserId, content);
    }

    public async Task<List<Message>> GetMessagesAsync()
    {
        return await chatService.GetMessagesAsync(currentConversationId);
    }
    public async Task<Conversation> GetCurrentConversationAsync()
    {
        return await chatService.GetConversationAsync(currentConversationId);
    }

    public int CurrentConversationId => currentConversationId;
    public int CurrentUserId => currentUserId;
}