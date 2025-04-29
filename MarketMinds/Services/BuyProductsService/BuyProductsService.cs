using System.Linq;
using System.Collections.Generic;
using MarketMinds.Shared.Models;
using MarketMinds.ServiceProxy;
using MarketMinds.Services.ProductTagService;

namespace MarketMinds.Services.BuyProductsService
{
    public class BuyProductsService : IBuyProductsService, IProductService
    {
        private readonly BuyProductsServiceProxy buyProductsRepository;

        private const int NOCOUNT = 0;

        public BuyProductsService(BuyProductsServiceProxy buyProductsRepository)
        {
            this.buyProductsRepository = buyProductsRepository;
        }

        public List<Product> GetProducts()
        {
            return buyProductsRepository.GetProducts();
        }

        public void CreateListing(BuyProduct product)
        {
            buyProductsRepository.CreateListing(product);
        }

        public void DeleteListing(Product product)
        {
            buyProductsRepository.DeleteListing(product);
        }

        public BuyProduct GetProductById(int id)
        {
            return buyProductsRepository.GetProductById(id);
        }

        public List<Product> GetSortedFilteredProducts(List<Condition> selectedConditions, List<Category> selectedCategories, List<ProductTag> selectedTags, ProductSortType sortCondition, string searchQuery)
        {
            List<Product> products = GetProducts();
            List<Product> productResultSet = new List<Product>();
            foreach (Product product in products)
            {
                bool matchesConditions = selectedConditions == null || selectedConditions.Count == NOCOUNT || selectedConditions.Any(c => c.Id == product.Condition.Id);
                bool matchesCategories = selectedCategories == null || selectedCategories.Count == NOCOUNT || selectedCategories.Any(c => c.Id == product.Category.Id);
                bool matchesTags = selectedTags == null || selectedTags.Count == NOCOUNT || selectedTags.Any(t => product.Tags.Any(pt => pt.Id == t.Id));
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
                        p =>
                        {
                            var prop = p?.GetType().GetProperty(sortCondition.InternalAttributeFieldTitle);
                            return prop?.GetValue(p);
                        }).ToList();
                }
                else
                {
                    productResultSet = productResultSet.OrderByDescending(
                        p =>
                        {
                            var prop = p?.GetType().GetProperty(sortCondition.InternalAttributeFieldTitle);
                            return prop?.GetValue(p);
                        }).ToList();
                }
            }
            return productResultSet;
        }
    }
}