using System;
using System.Collections.Generic;
using NUnit.Framework;
using DomainLayer.Domain;
using MarketMinds.Services.BasketService;
using MarketMinds.Repositories.BasketRepository;
using MarketMinds.Test.Services.BasketServiceTest;

namespace MarketMinds.Tests.Services.BasketServiceTest
{
    [TestFixture]
    public class BasketServiceTest
    {
        // Constants for user details
        private const int TestUserId = 1;
        private const string TestUserName = "Test User";
        private const string TestUserEmail = "test@example.com";

        // Constants for product details
        private const int TestProductId = 10;
        private const int InvalidId = 0;
        private const int NegativeId = -1;

        // Constants for quantities
        private const int DefaultQuantity = 1;
        private const int StandardQuantity = 3;
        private const int UpdatedQuantity = 5;
        private const int ZeroQuantity = 0;
        private const int NegativeQuantity = -1;
        private const int ExcessiveQuantity = BasketService.MaxQuantityPerItem + 5;

        // Constants for basket IDs
        private const int BasketId = 1;
        private const int EmptyBasketId = 2;

        // Constants for promo codes
        private const string ValidPromoCode = "DISCOUNT10";
        private const string InvalidPromoCode = "INVALID";
        private const string EmptyCode = "";
        private const string WhitespaceCode = "   ";

        // Constants for expected values
        private const int ExpectedSingleItem = 1;
        private const int ExpectedZeroItems = 0;
        private const float ExpectedSubtotal = 150f;
        private const float ExpectedDiscount = 15f;
        private const float ExpectedTotalWithDiscount = 135f;
        private const float ExpectedZeroValue = 0f;
        private const float StandardSubtotal = 100f;
        private const float StandardDiscount = 10f;

        // Constants for error messages
        private const string InvalidUserIdMessage = "Invalid user ID";
        private const string InvalidProductIdMessage = "Invalid product ID";
        private const string ValidUserRequiredMessage = "Valid user must be provided";
        private const string InvalidBasketIdMessage = "Invalid basket ID";
        private const string EmptyPromoCodeMessage = "Promo code cannot be empty";
        private const string InvalidPromoCodeMessage = "Invalid promo code";
        private const string NegativeQuantityMessage = "Quantity cannot be negative";
        private const string UpdateQuantityErrorPrefix = "Could not update quantity:";
        private const string RemoveProductErrorPrefix = "Could not remove product:";
        private const string RetrieveBasketErrorMessage = "Failed to retrieve user's basket";

        private BasketService _basketService;
        private BasketRepositoryMock _basketRepositoryMock;
        private User _testUser;

        [SetUp]
        public void Setup()
        {
            _basketRepositoryMock = new BasketRepositoryMock();
            _basketService = new BasketService(_basketRepositoryMock);
            _testUser = new User(TestUserId, TestUserName, TestUserEmail);
        }

        [Test]
        public void TestAddProductToBasket_ValidParameters_AddsProduct()
        {
            // Act
            _basketService.AddProductToBasket(TestUserId, TestProductId, StandardQuantity);

            // Assert
            Assert.That(_basketRepositoryMock.GetAddItemCount(), Is.EqualTo(ExpectedSingleItem));
        }

        [Test]
        public void TestAddProductToBasket_QuantityExceedsMax_LimitsQuantity()
        {
            // Act
            _basketService.AddProductToBasket(TestUserId, TestProductId, ExcessiveQuantity);

            // Assert
            Assert.That(_basketRepositoryMock.GetAddItemCount(), Is.EqualTo(ExpectedSingleItem));
        }

        [Test]
        public void TestAddProductToBasket_InvalidUserId_ThrowsException()
        {
            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() =>
                _basketService.AddProductToBasket(InvalidId, TestProductId, DefaultQuantity));

            Assert.That(ex.Message, Is.EqualTo(InvalidUserIdMessage));
            Assert.That(_basketRepositoryMock.GetAddItemCount(), Is.EqualTo(ExpectedZeroItems));
        }

        [Test]
        public void TestAddProductToBasket_InvalidProductId_ThrowsException()
        {
            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() =>
                _basketService.AddProductToBasket(TestUserId, InvalidId, DefaultQuantity));

            Assert.That(ex.Message, Is.EqualTo(InvalidProductIdMessage));
            Assert.That(_basketRepositoryMock.GetAddItemCount(), Is.EqualTo(ExpectedZeroItems));
        }

        [Test]
        public void TestGetBasketByUser_ValidUser_ReturnsBasket()
        {
            // Act
            var basket = _basketService.GetBasketByUser(_testUser);

            // Assert
            Assert.That(basket, Is.Not.Null);
        }

        [Test]
        public void TestGetBasketByUser_NullUser_ThrowsException()
        {
            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => _basketService.GetBasketByUser(null));

            Assert.That(ex.Message, Is.EqualTo(ValidUserRequiredMessage));
        }

        [Test]
        public void TestRemoveProductFromBasket_ValidParameters_RemovesProduct()
        {
            // Act
            _basketService.RemoveProductFromBasket(TestUserId, TestProductId);

            // Assert
            Assert.That(_basketRepositoryMock.GetRemoveItemCount(), Is.EqualTo(ExpectedSingleItem));
        }

        [Test]
        public void TestRemoveProductFromBasket_InvalidUserId_ThrowsException()
        {
            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() =>
                _basketService.RemoveProductFromBasket(InvalidId, TestProductId));

            Assert.That(ex.Message, Is.EqualTo(InvalidUserIdMessage));
            Assert.That(_basketRepositoryMock.GetRemoveItemCount(), Is.EqualTo(ExpectedZeroItems));
        }

        [Test]
        public void TestUpdateProductQuantity_ValidParameters_UpdatesQuantity()
        {
            // Arrange
            _basketService.AddProductToBasket(TestUserId, TestProductId, DefaultQuantity);

            int addCount = _basketRepositoryMock.GetAddItemCount();
            Assert.That(addCount, Is.EqualTo(ExpectedSingleItem));

            // Act
            _basketService.UpdateProductQuantity(TestUserId, TestProductId, UpdatedQuantity);

            // Assert
            Assert.That(_basketRepositoryMock.GetUpdateItemCount(), Is.EqualTo(ExpectedSingleItem));
        }

        [Test]
        public void TestUpdateProductQuantity_QuantityExceedsMax_LimitsQuantity()
        {
            // Arrange
            _basketService.AddProductToBasket(TestUserId, TestProductId, DefaultQuantity);

            int addCount = _basketRepositoryMock.GetAddItemCount();
            Assert.That(addCount, Is.EqualTo(ExpectedSingleItem));

            // Act
            _basketService.UpdateProductQuantity(TestUserId, TestProductId, ExcessiveQuantity);

            // Assert
            Assert.That(_basketRepositoryMock.GetUpdateItemCount(), Is.EqualTo(ExpectedSingleItem));
        }

        [Test]
        public void TestUpdateProductQuantity_QuantityZero_RemovesProduct()
        {
            // Act
            _basketService.UpdateProductQuantity(TestUserId, TestProductId, ZeroQuantity);

            // Assert
            Assert.That(_basketRepositoryMock.GetRemoveItemCount(), Is.EqualTo(ExpectedSingleItem));
            Assert.That(_basketRepositoryMock.GetUpdateItemCount(), Is.EqualTo(ExpectedZeroItems));
        }

        [Test]
        public void TestUpdateProductQuantity_InvalidUserId_ThrowsException()
        {
            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() =>
                _basketService.UpdateProductQuantity(InvalidId, TestProductId, DefaultQuantity));

            Assert.That(ex.Message, Is.EqualTo(InvalidUserIdMessage));
            Assert.That(_basketRepositoryMock.GetUpdateItemCount(), Is.EqualTo(ExpectedZeroItems));
        }

        [Test]
        public void TestClearBasket_ValidUserId_ClearsBasket()
        {
            // Act
            _basketService.ClearBasket(TestUserId);

            // Assert
            Assert.That(_basketRepositoryMock.GetClearBasketCount(), Is.EqualTo(ExpectedSingleItem));
        }

        [Test]
        public void TestClearBasket_InvalidUserId_ThrowsException()
        {
            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => _basketService.ClearBasket(InvalidId));

            Assert.That(ex.Message, Is.EqualTo(InvalidUserIdMessage));
            Assert.That(_basketRepositoryMock.GetClearBasketCount(), Is.EqualTo(ExpectedZeroItems));
        }

        [Test]
        public void TestValidateBasketBeforeCheckOut_EmptyBasket_ReturnsFalse()
        {
            // Act
            bool result = _basketService.ValidateBasketBeforeCheckOut(BasketId);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void TestApplyPromoCode_ValidCode_AppliesDiscount()
        {
            // Act
            _basketService.ApplyPromoCode(BasketId, ValidPromoCode);
            float discount = _basketService.GetPromoCodeDiscount(ValidPromoCode, StandardSubtotal);

            // Assert
            Assert.That(discount, Is.EqualTo(StandardDiscount));
        }

        [Test]
        public void TestApplyPromoCode_InvalidCode_ThrowsException()
        {
            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() =>
                _basketService.ApplyPromoCode(BasketId, InvalidPromoCode));

            Assert.That(ex.Message, Is.EqualTo(InvalidPromoCodeMessage));
        }

        [Test]
        public void TestValidateBasketBeforeCheckOut_InvalidBasketId_ThrowsException()
        {
            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() =>
                _basketService.ValidateBasketBeforeCheckOut(InvalidId));

            Assert.That(ex.Message, Is.EqualTo(InvalidBasketIdMessage));
        }

        [Test]
        public void TestValidateBasketBeforeCheckOut_ItemWithZeroQuantity_ReturnsFalse()
        {
            // Arrange
            _basketRepositoryMock.SetupInvalidItemQuantity(BasketId);

            // Act
            bool result = _basketService.ValidateBasketBeforeCheckOut(BasketId);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void TestValidateBasketBeforeCheckOut_ItemWithInvalidPrice_ReturnsFalse()
        {
            // Arrange
            _basketRepositoryMock.SetupInvalidItemPrice(BasketId);

            // Act
            bool result = _basketService.ValidateBasketBeforeCheckOut(BasketId);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void TestValidateBasketBeforeCheckOut_ValidBasket_ReturnsTrue()
        {
            // Arrange
            _basketRepositoryMock.SetupValidBasket(BasketId);

            // Act
            bool result = _basketService.ValidateBasketBeforeCheckOut(BasketId);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void TestApplyPromoCode_InvalidBasketId_ThrowsException()
        {
            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() =>
                _basketService.ApplyPromoCode(InvalidId, ValidPromoCode));

            Assert.That(ex.Message, Is.EqualTo(InvalidBasketIdMessage));
        }

        [Test]
        public void TestApplyPromoCode_EmptyCode_ThrowsException()
        {
            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() =>
                _basketService.ApplyPromoCode(BasketId, EmptyCode));

            Assert.That(ex.Message, Is.EqualTo(EmptyPromoCodeMessage));
        }

        [Test]
        public void TestApplyPromoCode_NullCode_ThrowsException()
        {
            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() =>
                _basketService.ApplyPromoCode(BasketId, null));

            Assert.That(ex.Message, Is.EqualTo(EmptyPromoCodeMessage));
        }

        [Test]
        public void TestApplyPromoCode_WhitespaceCode_ThrowsException()
        {
            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() =>
                _basketService.ApplyPromoCode(BasketId, WhitespaceCode));

            Assert.That(ex.Message, Is.EqualTo(EmptyPromoCodeMessage));
        }

        [Test]
        public void TestUpdateProductQuantity_InvalidProductId_ThrowsException()
        {
            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() =>
                _basketService.UpdateProductQuantity(TestUserId, InvalidId, DefaultQuantity));

            Assert.That(ex.Message, Is.EqualTo(InvalidProductIdMessage));
            Assert.That(_basketRepositoryMock.GetUpdateItemCount(), Is.EqualTo(ExpectedZeroItems));
        }

        [Test]
        public void TestUpdateProductQuantity_NegativeQuantity_ThrowsException()
        {
            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() =>
                _basketService.UpdateProductQuantity(TestUserId, TestProductId, NegativeQuantity));

            Assert.That(ex.Message, Is.EqualTo(NegativeQuantityMessage));
            Assert.That(_basketRepositoryMock.GetUpdateItemCount(), Is.EqualTo(ExpectedZeroItems));
        }

        [Test]
        public void TestUpdateProductQuantity_RepositoryThrowsException_ThrowsInvalidOperationException()
        {
            // Arrange
            _basketRepositoryMock.SetupUpdateQuantityException();

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() =>
                _basketService.UpdateProductQuantity(TestUserId, TestProductId, DefaultQuantity));

            Assert.That(ex.Message, Does.Contain(UpdateQuantityErrorPrefix));
        }

        [Test]
        public void TestRemoveProductFromBasket_InvalidProductId_ThrowsException()
        {
            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() =>
                _basketService.RemoveProductFromBasket(TestUserId, InvalidId));

            Assert.That(ex.Message, Is.EqualTo(InvalidProductIdMessage));
            Assert.That(_basketRepositoryMock.GetRemoveItemCount(), Is.EqualTo(ExpectedZeroItems));
        }

        [Test]
        public void TestRemoveProductFromBasket_RepositoryThrowsException_ThrowsInvalidOperationException()
        {
            // Arrange
            _basketRepositoryMock.SetupRemoveItemException();

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() =>
                _basketService.RemoveProductFromBasket(TestUserId, TestProductId));

            Assert.That(ex.Message, Does.Contain(RemoveProductErrorPrefix));
        }

        [Test]
        public void TestGetBasketByUser_RepositoryThrowsException_ThrowsApplicationException()
        {
            // Arrange
            _basketRepositoryMock.SetupGetBasketException();

            // Act & Assert
            var ex = Assert.Throws<ApplicationException>(() => _basketService.GetBasketByUser(_testUser));

            Assert.That(ex.Message, Is.EqualTo(RetrieveBasketErrorMessage));
        }

        [Test]
        public void TestGetPromoCodeDiscount_EmptyCode_ReturnsZero()
        {
            // Act
            float discount = _basketService.GetPromoCodeDiscount(EmptyCode, StandardSubtotal);

            // Assert
            Assert.That(discount, Is.EqualTo(ExpectedZeroValue));
        }

        [Test]
        public void TestGetPromoCodeDiscount_NullCode_ReturnsZero()
        {
            // Act
            float discount = _basketService.GetPromoCodeDiscount(null, StandardSubtotal);

            // Assert
            Assert.That(discount, Is.EqualTo(ExpectedZeroValue));
        }

        [Test]
        public void TestGetPromoCodeDiscount_WhitespaceCode_ReturnsZero()
        {
            // Act
            float discount = _basketService.GetPromoCodeDiscount(WhitespaceCode, StandardSubtotal);

            // Assert
            Assert.That(discount, Is.EqualTo(ExpectedZeroValue));
        }

        [Test]
        public void TestCalculateBasketTotals_ValidBasket_ReturnsCorrectTotals()
        {
            // Arrange
            _basketRepositoryMock.SetupValidBasket(BasketId);

            // Act
            BasketTotals totals = _basketService.CalculateBasketTotals(BasketId, null);

            // Assert
            Assert.That(totals, Is.Not.Null);
            Assert.That(totals.Subtotal, Is.EqualTo(ExpectedSubtotal));
            Assert.That(totals.Discount, Is.EqualTo(ExpectedZeroValue));
            Assert.That(totals.TotalAmount, Is.EqualTo(ExpectedSubtotal));
        }

        [Test]
        public void TestCalculateBasketTotals_WithValidPromoCode_AppliesDiscount()
        {
            // Arrange
            _basketRepositoryMock.SetupValidBasket(BasketId);

            // Act
            BasketTotals totals = _basketService.CalculateBasketTotals(BasketId, ValidPromoCode);

            // Assert
            Assert.That(totals, Is.Not.Null);
            Assert.That(totals.Subtotal, Is.EqualTo(ExpectedSubtotal));
            Assert.That(totals.Discount, Is.EqualTo(ExpectedDiscount));
            Assert.That(totals.TotalAmount, Is.EqualTo(ExpectedTotalWithDiscount));
        }

        [Test]
        public void TestCalculateBasketTotals_WithInvalidPromoCode_NoDiscount()
        {
            // Arrange
            _basketRepositoryMock.SetupValidBasket(BasketId);

            // Act
            BasketTotals totals = _basketService.CalculateBasketTotals(BasketId, InvalidPromoCode);

            // Assert
            Assert.That(totals, Is.Not.Null);
            Assert.That(totals.Subtotal, Is.EqualTo(ExpectedSubtotal));
            Assert.That(totals.Discount, Is.EqualTo(ExpectedZeroValue));
            Assert.That(totals.TotalAmount, Is.EqualTo(ExpectedSubtotal));
        }

        [Test]
        public void TestCalculateBasketTotals_EmptyBasket_ReturnsZeroTotals()
        {
            // Act
            BasketTotals totals = _basketService.CalculateBasketTotals(EmptyBasketId, null);

            // Assert
            Assert.That(totals, Is.Not.Null);
            Assert.That(totals.Subtotal, Is.EqualTo(ExpectedZeroValue));
            Assert.That(totals.Discount, Is.EqualTo(ExpectedZeroValue));
            Assert.That(totals.TotalAmount, Is.EqualTo(ExpectedZeroValue));
        }

        [Test]
        public void TestCalculateBasketTotals_InvalidBasketId_ThrowsException()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() =>
                _basketService.CalculateBasketTotals(NegativeId, null));
        }
    }
}