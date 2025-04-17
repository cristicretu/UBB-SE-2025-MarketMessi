using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DomainLayer.Domain;
using MarketMinds.Repositories;

namespace MarketMinds.Services.ProductTagService
{
    public class ProductService : IProductService
    {
        private const int NOCOUNT = 0;
        private IProductsRepository productRepository;

        public ProductService(IProductsRepository repository)
        {
            productRepository = repository;
        }

        public virtual List<Product> GetProducts()
        {
            if (productRepository == null) 
                throw new InvalidOperationException("Product repository is not initialized in the base class.");
            return productRepository.GetProducts();
        }

        public virtual Product GetProductById(int id)
        {
            if (productRepository == null) 
                throw new InvalidOperationException("Product repository is not initialized in the base class.");
            return productRepository.GetProductByID(id);
        }

        public virtual void AddProduct(Product product)
        {
            if (productRepository == null) 
                throw new InvalidOperationException("Product repository is not initialized in the base class.");
            productRepository.AddProduct(product);
        }

        public virtual void UpdateProduct(Product product)
        {
            if (productRepository == null) 
                throw new InvalidOperationException("Product repository is not initialized in the base class.");
            productRepository.UpdateProduct(product);
        }

        public virtual void DeleteProduct(Product product)
        {
            if (productRepository == null) 
                throw new InvalidOperationException("Product repository is not initialized in the base class.");
            productRepository.DeleteProduct(product);
        }

        public List<Product> GetSortedFilteredProducts(List<ProductCondition> selectedConditions, List<ProductCategory> selectedCategories, List<ProductTag> selectedTags, ProductSortType sortCondition, string searchQuery)
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

        public static List<Product> GetSortedFilteredProducts(List<Product> products, List<ProductCondition> selectedConditions, List<ProductCategory> selectedCategories, List<ProductTag> selectedTags, ProductSortType sortCondition, string searchQuery)
        {
            var tempService = new ProductService(null);
            List<Product> filteredProducts = new List<Product>();
            foreach (Product product in products)
            {
                bool matchesConditions = selectedConditions == null || selectedConditions.Count == NOCOUNT || selectedConditions.Any(c => c.Id == product.Condition.Id);
                bool matchesCategories = selectedCategories == null || selectedCategories.Count == NOCOUNT || selectedCategories.Any(c => c.Id == product.Category.Id);
                bool matchesTags = selectedTags == null || selectedTags.Count == NOCOUNT || selectedTags.Any(t => product.Tags.Any(pt => pt.Id == t.Id));
                bool matchesSearchQuery = string.IsNullOrEmpty(searchQuery) || product.Title.ToLower().Contains(searchQuery.ToLower());

                if (matchesConditions && matchesCategories && matchesTags && matchesSearchQuery)
                {
                    filteredProducts.Add(product);
                }
            }

            if (sortCondition != null)
            {
                if (sortCondition.IsAscending)
                {
                    filteredProducts = filteredProducts.OrderBy(
                        p =>
                        {
                            var prop = p?.GetType().GetProperty(sortCondition.InternalAttributeFieldTitle);
                            return prop?.GetValue(p);
                        }).ToList();
                }
                else
                {
                    filteredProducts = filteredProducts.OrderByDescending(
                        p =>
                        {
                            var prop = p?.GetType().GetProperty(sortCondition.InternalAttributeFieldTitle);
                            return prop?.GetValue(p);
                        }).ToList();
                }
            }
            return filteredProducts;
        }
    }
}
