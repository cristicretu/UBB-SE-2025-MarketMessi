using System;
using System.Collections.Generic;
using System.Linq;
using DomainLayer.Domain;
using ViewModelLayer.ViewModel;
using MarketMinds.Services.ProductTagService;
using MarketMinds.Repositories.ProductTagRepository;

namespace MarketMinds.Test.Helpers.TagManagementViewModelHelperTest
{
    /// <summary>
    /// Test implementation of ProductTagRepository for mocking
    /// </summary>
    public class TestProductTagRepository : IProductTagRepository
    {
        private List<ProductTag> _tags = new List<ProductTag>();
        private int _nextId = 1;

        public TestProductTagRepository(List<ProductTag> initialTags = null)
        {
            if (initialTags != null)
            {
                _tags = new List<ProductTag>(initialTags);
                if (_tags.Any())
                {
                    _nextId = _tags.Max(t => t.Id) + 1;
                }
            }
        }

        public List<ProductTag> GetAllProductTags()
        {
            return _tags;
        }

        public ProductTag CreateProductTag(string displayTitle)
        {
            var tag = new ProductTag(_nextId++, displayTitle);
            _tags.Add(tag);
            return tag;
        }

        public void DeleteProductTag(string displayTitle)
        {
            _tags.RemoveAll(t => t.DisplayTitle == displayTitle);
        }
    }

    /// <summary>
    /// Mock implementation of ProductTagService for testing
    /// </summary>
    public class ProductTagServiceMock : ProductTagService
    {
        // Store the repository directly for testing
        private readonly TestProductTagRepository testRepository;

        public ProductTagServiceMock(List<ProductTag> initialTags = null)
            : base(new TestProductTagRepository(initialTags))
        {
            // We need to create a separate reference to the repository for testing
            // This means we have two repositories, one in the base class and one here
            // which is not ideal, but necessary since we can't access the private field
            testRepository = new TestProductTagRepository(initialTags);
        }

        // Override methods to use our test repository
        public override List<ProductTag> GetAllProductTags()
        {
            return testRepository.GetAllProductTags();
        }

        public override ProductTag CreateProductTag(string displayTitle)
        {
            return testRepository.CreateProductTag(displayTitle);
        }

        public override void DeleteProductTag(string displayTitle)
        {
            testRepository.DeleteProductTag(displayTitle);
        }

        // Helper method for test verification
        public List<ProductTag> GetAllTags()
        {
            return testRepository.GetAllProductTags();
        }
    }

    /// <summary>
    /// A test-friendly implementation of ProductTagViewModel for use in tests
    /// </summary>
    public class TestProductTagViewModel : ProductTagViewModel
    {
        private readonly ProductTagServiceMock _productTagService;

        public TestProductTagViewModel(ProductTagServiceMock productTagService) 
            : base(productTagService)
        {
            _productTagService = productTagService;
        }

        // Helper method to get the underlying tags for test verification
        public List<ProductTag> GetAllProductTags()
        {
            return _productTagService.GetAllTags();
        }
    }

    public static class TagManagementViewModelHelperMocks
    {
        /// <summary>
        /// Creates a test-specific ProductTagViewModel for testing
        /// </summary>
        public static TestProductTagViewModel CreateTestProductTagViewModel()
        {
            return new TestProductTagViewModel(new ProductTagServiceMock());
        }
        
        /// <summary>
        /// Creates test ProductTag objects
        /// </summary>
        public static List<ProductTag> CreateTestProductTags()
        {
            return new List<ProductTag>
            {
                new ProductTag(1, "Tag1"),
                new ProductTag(2, "Tag2"),
                new ProductTag(3, "Tag3")
            };
        }
        
        /// <summary>
        /// Sets up a TestProductTagViewModel with the given existing tags
        /// </summary>
        public static TestProductTagViewModel SetupTagViewModel(List<ProductTag> existingTags = null)
        {
            var tagService = new ProductTagServiceMock(existingTags);
            return new TestProductTagViewModel(tagService);
        }
    }
} 