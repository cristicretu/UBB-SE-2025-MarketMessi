using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Server.DataAccessLayer;
using Server.Models;

namespace MarketMinds.Repositories.ProductCategoryRepository
{
    public class ProductCategoryRepository : IProductCategoryRepository
    {
        private readonly ApplicationDbContext databaseContext;

        public ProductCategoryRepository(ApplicationDbContext context)
        {
            databaseContext = context;
        }

        public List<Category> GetAllProductCategories()
        {
            try
            {
                var allCategories = databaseContext.ProductCategories.ToList();
                return allCategories;
            }
            catch (Exception exception)
            {
                Console.WriteLine($"Error in GetAllProductCategories using EF: {exception.Message}");
                throw;
            }
        }

        public Category CreateProductCategory(string displayTitle, string? description)
        {
            try
            {
                var categoryToCreate = new Category(displayTitle, description);

                databaseContext.ProductCategories.Add(categoryToCreate);
                databaseContext.SaveChanges();

                return categoryToCreate;
            }
            catch (Exception exception)
            {
                Console.WriteLine($"Error in CreateProductCategory using EF: {exception.Message}");
                throw;
            }
        }

        public void DeleteProductCategory(string displayTitle)
        {
            try
            {
                var categoryToDelete = databaseContext.ProductCategories.FirstOrDefault(category => category.Name == displayTitle);

                if (categoryToDelete == null)
                {
                    throw new KeyNotFoundException($"Category with title '{displayTitle}' not found.");
                }

                databaseContext.ProductCategories.Remove(categoryToDelete);
                databaseContext.SaveChanges();
            }
            catch (Exception exception)
            {
                Console.WriteLine($"Error in DeleteProductCategory using EF: {exception.Message}");
                throw;
            }
        }
    }
}