using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DomainLayer.Domain;
using MarketMinds.Services.DreamTeam.ChatbotService;

namespace MarketMinds.ViewModels;

public class ChatBotViewModel
{
    private readonly IChatbotService chatBotService;
    private string currentResponse;
    private ObservableCollection<Node> currentOptions;
    private bool isActive;

    public ChatBotViewModel(IChatbotService chatBotService)
    {
        this.chatBotService = chatBotService;
        currentOptions = new ObservableCollection<Node>();
    }

    public void InitializeChat()
    {
        chatBotService.InitializeChat();
        UpdateState();
    }
    public bool SelectOption(Node selectedNode)
    {
        bool result = chatBotService.SelectOption(selectedNode);
        UpdateState();
        return result;
    }
    public string GetCurrentResponse()
    {
        return currentResponse;
    }
    public IEnumerable<Node> GetCurrentOptions()
    {
        return currentOptions;
    }
    public bool IsChatInteractionActive()
    {
        return isActive;
    }
    private void UpdateState()
    {
        // Update state properties
        currentResponse = chatBotService.GetCurrentResponse();
        currentOptions.Clear();
        var options = chatBotService.GetCurrentOptions();
        if (options != null)
        {
            foreach (var option in options)
            {
                currentOptions.Add(option);
            }
        }
        isActive = chatBotService.IsInteractionActive();
    }
}
