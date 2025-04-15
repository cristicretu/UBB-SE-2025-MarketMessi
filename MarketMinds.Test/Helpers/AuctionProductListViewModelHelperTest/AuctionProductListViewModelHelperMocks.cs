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

namespace MarketMinds.Test.Helpers.AuctionProductListViewModelHelperTest
{
    public static class AuctionProductListViewModelHelperMocks
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
        /// Creates a list of test AuctionProduct objects
        /// </summary>
        public static List<AuctionProduct> CreateTestAuctionProducts(int count)
        {
            var testProducts = new List<AuctionProduct>();
            var testSeller = new User(1, "Test Seller", "seller@test.com");
            var testCondition = new ProductCondition(1, "New", "Brand new item");
            var testCategory = new ProductCategory(1, "Electronics", "Electronic devices");
            var testTags = new List<ProductTag>();
            var testImages = new List<Image>();

            for (int i = 1; i <= count; i++)
            {
                testProducts.Add(new AuctionProduct(
                    i,
                    $"Test Auction Product {i}",
                    $"Test Description {i}",
                    testSeller,
                    testCondition,
                    testCategory,
                    testTags,
                    testImages,
                    DateTime.Now.AddDays(1),
                    DateTime.Now.AddDays(7),
                    99.99f + i));
            }
            
            return testProducts;
        }
    }
} 