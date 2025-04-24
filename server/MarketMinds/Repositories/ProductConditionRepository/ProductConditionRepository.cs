using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using server.DataAccessLayer;
using server.Models;

namespace MarketMinds.Repositories.ProductConditionRepository
{
    public class ProductConditionRepository : IProductConditionRepository
    {
        private readonly ApplicationDbContext databaseContext;

        public ProductConditionRepository(ApplicationDbContext context)
        {
            databaseContext = context;
        }

        public List<Condition> GetAllProductConditions()
        {
            try
            {
                var allConditions = databaseContext.ProductConditions.ToList();
                return allConditions;
            }
            catch (Exception exception)
            {
                Console.WriteLine($"Error in GetAllProductConditions using EF: {exception.Message}");
                throw;
            }
        }

        public Condition CreateProductCondition(string displayTitle, string description)
        {
            try
            {
                var conditionToCreate = new Condition(displayTitle);
                databaseContext.ProductConditions.Add(conditionToCreate);
                databaseContext.SaveChanges();
                return conditionToCreate;
            }
            catch (Exception exception)
            {
                Console.WriteLine($"Error in CreateProductCondition using EF: {exception.Message}");
                throw;
            }
        }

        public void DeleteProductCondition(string displayTitle)
        {
            try
            {
                // Debug: Log all conditions in the database
                var allConditions = databaseContext.ProductConditions.ToList();
                Console.WriteLine($"DEBUG: Found {allConditions.Count} conditions in database");
                foreach (var condition in allConditions)
                {
                    Console.WriteLine($"DEBUG: Condition ID: {condition.Id}, Name: '{condition.Name}'");
                }
                Console.WriteLine($"DEBUG: Looking for condition with title: '{displayTitle}'");
                
                var conditionToDelete = databaseContext.ProductConditions.FirstOrDefault(condition => condition.Name == displayTitle);
                
                if (conditionToDelete == null)
                {
                    throw new KeyNotFoundException($"Product condition with title '{displayTitle}' not found.");
                }
                
                databaseContext.ProductConditions.Remove(conditionToDelete);
                databaseContext.SaveChanges();
            }
            catch (Exception exception)
            {
                Console.WriteLine($"Error in DeleteProductCondition using EF: {exception.Message}");
                throw;
            }
        }
    }
}
