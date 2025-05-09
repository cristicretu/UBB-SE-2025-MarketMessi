using System;
using System.Linq;
using System.Collections.Generic;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.ProxyRepository;
using MarketMinds.Shared.Services.ProductTagService;

namespace MarketMinds.Shared.Services.BorrowProductsService
{
    public class BorrowProductsService : IBorrowProductsService, IProductService
    {
        private readonly BorrowProductsProxyRepository borrowProductsRepository;

        private const int DEFAULT_PRODUCT_COUNT = 0;

        public BorrowProductsService(BorrowProductsProxyRepository borrowProductsRepository)
        {
            this.borrowProductsRepository = borrowProductsRepository;
        }

        public void CreateListing(Product product)
        {
            if (!(product is BorrowProduct borrowProduct))
            {
                throw new ArgumentException("Product must be a BorrowProduct.", nameof(product));
            }

            if (borrowProduct.StartDate == default(DateTime))
            {
                borrowProduct.StartDate = DateTime.Now;
            }

            if (borrowProduct.EndDate == default(DateTime))
            {
                borrowProduct.EndDate = DateTime.Now.AddDays(7);
            }

            if (borrowProduct.TimeLimit == default(DateTime))
            {
                borrowProduct.TimeLimit = DateTime.Now.AddDays(7);
            }

            borrowProductsRepository.CreateListing(borrowProduct);
        }

        public void DeleteListing(Product product)
        {
            if (product.Id == 0)
            {
                throw new ArgumentException("Product ID must be set for delete.", nameof(product.Id));
            }
            
            borrowProductsRepository.DeleteListing(product);
        }

        public List<Product> GetProducts()
        {
            return borrowProductsRepository.GetProducts();
        }

        public Product GetProductById(int id)
        {
            try
            {
                var product = borrowProductsRepository.GetProductById(id);
                if (product == null)
                {
                    throw new KeyNotFoundException($"Borrow product with ID {id} not found.");
                }
                return product;
            }
            catch (KeyNotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new KeyNotFoundException($"Borrow product with ID {id} not found: {ex.Message}");
            }
        }

        public List<Product> GetSortedFilteredProducts(List<Condition> selectedConditions, List<Category> selectedCategories, List<ProductTag> selectedTags, ProductSortType sortCondition, string searchQuery)
        {
            List<Product> products = GetProducts();
            List<Product> productResultSet = new List<Product>();
            foreach (Product product in products)
            {
                bool matchesConditions = selectedConditions == null || selectedConditions.Count == DEFAULT_PRODUCT_COUNT || selectedConditions.Any(condition => condition.Id == product.Condition.Id);
                bool matchesCategories = selectedCategories == null || selectedCategories.Count == DEFAULT_PRODUCT_COUNT || selectedCategories.Any(category => category.Id == product.Category.Id);
                bool matchesTags = selectedTags == null || selectedTags.Count == DEFAULT_PRODUCT_COUNT || selectedTags.Any(tag => product.Tags.Any(productTag => productTag.Id == tag.Id));
                bool matchesSearchQuery = string.IsNullOrEmpty(searchQuery) || product.Title.ToLower().Contains(searchQuery.ToLower());

                if (matchesConditions && matchesCategories && matchesTags && matchesSearchQuery)
                {
                    productResultSet.Add(product);
                }
            }

            if (sortCondition != null)
            {
                if (sortCondition.IsAscending)
                {
                    productResultSet = productResultSet.OrderBy(
                        product =>
                        {
                            var property = product?.GetType().GetProperty(sortCondition.InternalAttributeFieldTitle);
                            return property?.GetValue(product);
                        }).ToList();
                }
                else
                {
                    productResultSet = productResultSet.OrderByDescending(
                        product =>
                        {
                            var property = product?.GetType().GetProperty(sortCondition.InternalAttributeFieldTitle);
                            return property?.GetValue(product);
                        }).ToList();
                }
            }
            return productResultSet;
        }
    }
}