using System;
using System.Collections.Generic;
using System.Linq;
using DomainLayer.Domain;
using ViewModelLayer.ViewModel;
using BusinessLogicLayer.ViewModel;
using Moq;
using MarketMinds.Services;
using MarketMinds.Services.ProductTagService;
using MarketMinds.Repositories;

namespace MarketMinds.Test.Helpers.BuyProductListViewModelHelperTest
{
    public static class BuyProductListViewModelHelperMocks
    {
        /// <summary>
        /// Creates a SortAndFilterViewModel with a product service that returns the specified products
        /// </summary>
        public static SortAndFilterViewModel CreateSortAndFilterViewModel(List<Product> products)
        {
            // Create a mock product repository
            var mockRepository = new Mock<IProductsRepository>();
            mockRepository.Setup(r => r.GetProducts())
                .Returns(products);
            
            // Create real ProductService with mocked repository
            var productService = new ProductService(mockRepository.Object);

            // Create actual SortAndFilterViewModel with our ProductService
            return new SortAndFilterViewModel(productService);
        }
        
        /// <summary>
        /// Creates a list of test BuyProduct objects
        /// </summary>
        public static List<BuyProduct> CreateTestBuyProducts(int count)
        {
            var testProducts = new List<BuyProduct>();
            var testSeller = new User(1, "Test Seller", "seller@test.com");
            var testCondition = new ProductCondition(1, "New", "Brand new item");
            var testCategory = new ProductCategory(1, "Electronics", "Electronic devices");
            var testTags = new List<ProductTag>();
            var testImages = new List<Image>();

            for (int i = 1; i <= count; i++)
            {
                testProducts.Add(new BuyProduct(
                    i,
                    $"Test Buy Product {i}",
                    $"Test Description {i}",
                    testSeller,
                    testCondition,
                    testCategory,
                    testTags,
                    testImages,
                    99.99f + i));
            }
            
            return testProducts;
        }
    }
} 