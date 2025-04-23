using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using DomainLayer.Domain;

namespace MarketMinds.Repositories.ChatRepository;

/// <summary>
/// This interface defines the contract for the Chat Repository.
/// </summary>
public interface IChatRepository
{
    /// <summary>
    /// Retrieves the current active conversation metadata.
    /// </summary>
    /// <param name="userId1">The ID of the first user in the conversation.</param>
    /// <param name="userId2">The ID of the second user in the conversation.</param>
    /// <returns>The Conversation object if a conversation exists between the specified users; otherwise, null.</returns>
    Conversation? GetConversation(int userId1, int userId2);

    /// <summary>
    /// Creates a new conversation between two users if it doesn't already exist.
    /// </summary>
    /// <param name="userId1">The ID of the first user in the conversation.</param>
    /// <param name="userId2">The ID of the second user in the conversation.</param>
    /// <returns>A new Conversation object representing the created conversation.</returns>
    Conversation CreateConversation(int userId1, int userId2);

    /// <summary>
    /// Retrieves the initial set of messages for the current conversation.
    /// </summary>
    /// <param name="conversationid">The ID of the conversation whose messages should be retrieved.</param>
    /// <param name="sinceTimestamp">
    /// Optional. When provided, only retrieves messages with a timestamp greater than this value.
    /// Default is 0, which retrieves all messages.</param>
    /// <returns>A list of Message objects from the specified conversation.</returns>
    List<Message> GetMessages(int conversationid, long sinceTimestamp = 0);

    /// <summary>
    /// Adds a new message to the conversation.
    /// </summary>
    /// <param name="message">
    /// The Message object to add, containing details such as the conversation ID,
    /// creator ID, content, and timestamp.</param>
    /// <returns>True if the message was successfully added; otherwise, false.</returns>
    bool AddMessage(Message message);
}
