using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DomainLayer.Domain;

namespace MarketMinds.Services.DreamTeam.ChatService;

/// <summary>
/// This interface defines the contract for the Chat Service.
/// </summary>
public interface IChatService
{
    /// <summary>
    /// Initializes the chat service with the current user and the target user for the conversation.
    /// </summary>
    /// <param name="currentUser">The user who is initiating or participating in the conversation. </param>
    /// <param name="targetUser">The recipient or other participant in the conversation.</param>
    void Initialize(User currentUser, User targetUser);

    /// <summary>
    /// Sends a text message to the target user in the active conversation.
    /// </summary>
    /// <param name="textMessage">The content of the text message to be sent.</param>
    /// <returns>True if the message was sent successfully; otherwise, false.</returns>
    bool SendTextMessage(string textMessage);

    /// <summary>
    /// Sends an image message from the current user to the target user in the active conversation.
    /// </summary>
    /// <param name="imageData">The binary data of the image to be sent.</param>
    /// <returns>True if the image was sent successfully; otherwise, false.</returns>
    bool SendImageMessage(byte[] imageData);

    /// <summary>
    /// Retrieves the initial set of messages for the current conversation.
    /// </summary>
    /// <returns>A list of Message objects representing the conversation history.</returns>
    List<Message> GetInitialMessages();

    /// <summary>
    /// Checks for new messages in the conversation since the last retrieved timestamp.
    /// </summary>
    /// <returns>A list of new Message objects, or an empty list if no new messages are available.</returns>
    List<Message> CheckForNewMessages();

    /// <summary>
    /// Retrieves the current active conversation metadata.
    /// </summary>
    /// <returns>The Conversation object containing metadata about the current chat session, or null if no active conversation exists.</returns>
    Conversation? GetConversation();
}
