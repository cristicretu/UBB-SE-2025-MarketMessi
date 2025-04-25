using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DomainLayer.Domain;
using MarketMinds.Repositories.ChatBotRepository;

namespace MarketMinds.Services.DreamTeam.ChatBotService;

public class ChatBotService : IChatBotService
{
    private readonly IChatBotRepository chatBotRepository;
    private Node currentNode;
    private bool isActive;

    public ChatBotService(IChatBotRepository chatBotRepository)
    {
        this.chatBotRepository = chatBotRepository;
        isActive = false;
    }

    public Node InitializeChat()
    {
        try
        {
            currentNode = chatBotRepository.LoadChatTree();
            if (currentNode == null)
            {
                throw new InvalidOperationException("Chat tree could not be loaded.");
            }

            isActive = true;
            return currentNode;
        }
        catch (Exception ex)
        {
            // Log the exception
            Console.WriteLine($"Error initializing chat: {ex.Message}");
            isActive = false;

            // Create an error node
            currentNode = new Node
            {
                Id = -1,
                Response = "Error: Unable to initialize the chat. Please try again later.",
                ButtonLabel = "Restart",
                LabelText = "Chat Initialization Error",
                Children = new List<Node>()
            };

            return currentNode;
        }
    }

    public bool IsInteractionActive()
    {
        return isActive && currentNode != null;
    }

    public bool SelectOption(Node selectedNode)
    {
        // Validate the selected node
        if (selectedNode == null)
        {
            return false;
        }
        // Check if the selected node is a valid child of the current node
        if (currentNode != null && currentNode.Children.Any(c => c.Id == selectedNode.Id))
        {
            currentNode = selectedNode;

            // Determine if chat is still active based on whether there are more options
            isActive = currentNode != null && currentNode.Children.Count != 0;
            return true;
        }

        // Direct node assignment (for cases where we're navigating in a different way)
        currentNode = selectedNode;
        isActive = currentNode != null && currentNode.Children.Count != 0;

        return true;
    }

    public IEnumerable<Node> GetCurrentOptions()
    {
        if (currentNode == null || currentNode.Children == null)
        {
            return new List<Node>();
        }

        return currentNode.Children;
    }

    public string GetCurrentResponse()
    {
        return currentNode?.Response ?? "Chat not initialized. Please try again.";
    }
}
