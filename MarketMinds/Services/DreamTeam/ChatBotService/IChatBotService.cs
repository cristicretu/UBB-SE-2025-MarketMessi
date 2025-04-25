using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DomainLayer.Domain;

namespace MarketMinds.Services.DreamTeam.ChatBotService;

/// <summary>
/// This interface defines the contract for the Chat Bot Service.
/// </summary>
public interface IChatBotService
{
    /// <summary>
    /// Initializes the chat bot interaction.
    /// </summary>
    /// <returns>The initial node to begin the conversation</returns>
    Node InitializeChat();

    /// <summary>
    /// Processes the user's input and returns the next node in the conversation tree.
    /// </summary>
    /// <param name="selectedNode">The Node selected by the user from the available options.</param>
    /// <returns>True if the selection was processed successfully; otherwise, false</returns>
    bool SelectOption(Node selectedNode);

    /// <summary>
    /// Retrieves the current response text from the chatbot based on the conversation state.
    /// </summary>
    /// <returns>A string containing the chatbot's current response.</returns>
    string GetCurrentResponse();

    /// <summary>
    /// Retrieves the current options available for the user to select from.
    /// </summary>
    /// <returns>An enumerable collection of Node objects representing available choices.</returns>
    IEnumerable<Node> GetCurrentOptions();

    /// <summary>
    /// Determines whether the chatbot interaction is currently active.
    /// </summary>
    /// <returns>True if a conversation is in progress; otherwise, false.</returns>
    bool IsInteractionActive();
}
