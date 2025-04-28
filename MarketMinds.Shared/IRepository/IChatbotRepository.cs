using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MarketMinds.Shared.Models;

namespace MarketMinds.Shared.IRepository
{
    public interface IChatbotRepository
    {
        Task<string> GetBotResponseAsync(string userMessage, int? userId = null);
    }
} 