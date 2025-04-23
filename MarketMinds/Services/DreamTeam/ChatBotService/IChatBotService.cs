using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DomainLayer.Domain;

namespace MarketMinds.Services.DreamTeam.ChatBotService;

public interface IChatBotService
{
    Node InitializeChat();
    bool SelectOption(Node selectedNode);
    string GetCurrentResponse();
    IEnumerable<Node> GetCurrentOptions();
    bool IsInteractionActive();
}
