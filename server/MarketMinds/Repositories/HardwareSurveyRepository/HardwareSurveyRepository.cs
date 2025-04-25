using System;
using System.Collections.Generic;
using System.Linq;
using server.Models;
using server.DataAccessLayer;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace MarketMinds.Repositories.HardwareSurveyRepository
{
    public class HardwareSurveyRepository : IHardwareSurveyRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public HardwareSurveyRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            
            // Don't test table existence here - will be shown in logs if there's an issue
            Console.WriteLine("HardwareSurveyRepository initialized");
        }

        public void SaveHardwareData(HardwareSurvey hardwareData)
        {
            if (hardwareData == null)
            {
                throw new ArgumentNullException(nameof(hardwareData));
            }

            try
            {
                // Ensure timestamp is set
                if (hardwareData.SurveyTimestamp == default)
                {
                    hardwareData.SurveyTimestamp = DateTime.UtcNow;
                }

                // Debug logging
                Console.WriteLine($"Attempting to save hardware data: {JsonSerializer.Serialize(hardwareData)}");
                
                // Reset ID to 0 to let the database assign the identity value
                if (hardwareData.Id != 0)
                {
                    Console.WriteLine($"Resetting ID from {hardwareData.Id} to 0 to allow identity insert");
                    hardwareData.Id = 0;
                }
                
                _dbContext.HardwareSurveys.Add(hardwareData);
                _dbContext.SaveChanges();
                
                Console.WriteLine($"Successfully saved hardware data with ID: {hardwareData.Id}");
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine($"EF Core SaveHardwareData Error: {ex.InnerException?.Message ?? ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                
                // Log more detailed information about the database context
                Console.WriteLine($"Database connection string: {_dbContext.Database.GetConnectionString()}");
                Console.WriteLine($"Database provider: {_dbContext.Database.ProviderName}");
                
                throw new Exception("Failed to save hardware data to database", ex);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving hardware data to database: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        public List<HardwareSurvey> GetAllHardwareSurveys()
        {
            try
            {
                Console.WriteLine("Starting GetAllHardwareSurveys");
                
                if (_dbContext?.HardwareSurveys == null)
                {
                    Console.WriteLine("HardwareSurveys DbSet is null");
                    return new List<HardwareSurvey>();
                }

                try
                {
                    // Log connection information
                    var connString = _dbContext.Database.GetConnectionString();
                    if (!string.IsNullOrEmpty(connString))
                    {
                        var maskedConnString = connString.Contains("Password=") || connString.Contains("password=") ? 
                            "Connection string contains sensitive data (masked)" : 
                            connString;
                        Console.WriteLine($"Database connection string: {maskedConnString}");
                    }
                    
                    // Use to list directly without Count first to avoid unnecessary query
                    var surveys = _dbContext.HardwareSurveys.AsNoTracking().ToList();
                    Console.WriteLine($"Retrieved {surveys.Count} hardware surveys");
                    return surveys;
                }
                catch (Exception countEx)
                {
                    Console.WriteLine($"Error during query: {countEx.Message}");
                    
                    // Safe fallback - empty list
                    return new List<HardwareSurvey>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving hardware surveys: {ex.Message}");
                Console.WriteLine($"Exception type: {ex.GetType().Name}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                
                // Return empty list instead of throwing to avoid 500 error
                return new List<HardwareSurvey>();
            }
        }
    }
}
