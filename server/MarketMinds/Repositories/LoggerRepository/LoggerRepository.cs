using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using server.Models;
using server.DataAccessLayer;
using Microsoft.EntityFrameworkCore;

namespace MarketMinds.Repositories.LoggerRepository
{
    public class LoggerRepository : ILoggerRepository
    {
        private readonly ApplicationDbContext context;

        public LoggerRepository(ApplicationDbContext dbContext)
        {
            context = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }
        public void LogInfo(string message)
        {
            SaveLog("INFO", message);
        }

        public void LogError(string message)
        {
            SaveLog("ERROR", message);
        }
        
        public void LogWarning(string message)
        {
            SaveLog("WARNING", message);
        }

        private void SaveLog(string logLevel, string message)
        {
            try
            {
                var log = new Log(logLevel, message);
                context.Logs.Add(log);
                
                var saveResult = context.SaveChanges();
                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving log to database: {ex.Message}");
                Console.WriteLine($"Exception type: {ex.GetType().Name}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                Console.WriteLine($"Original log message: [{logLevel}] {message}");
                throw;
            }
        }
        
        public List<Log> GetRecentLogs(int count = 100)
        {
            try
            {
                var query = context.Logs
                    .AsNoTracking()
                    .OrderByDescending(l => l.Timestamp)
                    .Take(count);
           
                var logs = query.ToList();
                return logs;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving logs: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }
        
        public List<Log> GetLogsByLevel(string logLevel, int count = 100)
        {
            try
            {
                return context.Logs
                    .AsNoTracking()
                    .Where(l => l.LogLevel == logLevel)
                    .OrderByDescending(l => l.Timestamp)
                    .Take(count)
                    .ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving logs by level: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }
        
        private string MaskConnectionString(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
                return "(null)";
                
            if (connectionString.Contains("Password=") || connectionString.Contains("password="))
                return "[MASKED CONNECTION STRING]";
                
            return connectionString;
        }
    }
}
