using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DomainLayer.Domain;

namespace MarketMinds.Repositories.ChatBotRepository;

/// <summary>
/// This interface defines the contract for the ChatBot Repository.
/// </summary>
public interface IChatBotRepository
{
    /// <summary>
    /// Initializes the chat tree with the root node and sets up the conversation context.
    /// </summary>
    /// <returns>The root Node of the conversation tree containing the complete structure of dialogue paths and responses for the chatbot interaction. </returns>
    Node LoadChatTree();
}