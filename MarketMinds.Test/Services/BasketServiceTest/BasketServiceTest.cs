using System;
using System.Collections.Generic;
using NUnit.Framework;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.Services.BasketService;
using MarketMinds.Repositories.BasketRepository;
using MarketMinds.Test.Services.BasketServiceTest;

namespace MarketMinds.Tests.Services.BasketServiceTest
{
    [TestFixture]
    public class BasketServiceTest
    {
        // Constants for user details
        private const string TEST_USER_ID = "1";
        private const string TEST_USER_NAME = "Test User";
        private const string TEST_USER_EMAIL = "test@example.com";

        // Constants for product details
        private const int TEST_PRODUCT_ID = 10;
        private const int INVALID_ID = 0;
        private const int NEGATIVE_ID = -1;

        // Constants for quantities
        private const int DEFAULT_QUANTITY = 1;
        private const int STANDARD_QUANTITY = 3;
        private const int UPDATED_QUANTITY = 5;
        private const int ZERO_QUANTITY = 0;
        private const int NEGATIVE_QUANTITY = -1;
        private const int EXCESSIVE_QUANTITY = BasketService.MAXIMUM_QUANTITY_PER_ITEM + 5;

        // Constants for basket IDs
        private const int BASKET_ID = 1;
        private const int EMPTY_BASKET_ID = 2;

        // Constants for promo codes
        private const string VALID_PROMO_CODE = "DISCOUNT10";
        private const string INVALID_PROMO_CODE = "INVALID";
        private const string EMPTY_CODE = "";
        private const string WHITESPACE_CODE = "   ";

        // Constants for expected values
        private const int EXPECTED_SINGLE_ITEM = 1;
        private const int EXPECTED_ZERO_ITEMS = 0;
        private const float EXPECTED_SUBTOTAL = 150f;
        private const float EXPECTED_DISCOUNT = 15f;
        private const float EXPECTED_TOTAL_DISCOUNT = 135f;
        private const float EXPECTED_ZERO_VALUE = 0f;
        private const float STANDARD_SUBTOTAL = 100f;
        private const float STANDARD_DISCOUNT = 10f;

        // Constants for error messages
        private const string INVALID_USER_ID_MESSAGE = "Invalid user ID";
        private const string INVALID_PRODUCT_ID_MESSAGE = "Invalid product ID";
        private const string VALID_USER_ID_MESSAGE = "Valid user must be provided";
        private const string INVALID_BASKET_ID_MESSAGE = "Invalid basket ID";
        private const string EMPTY_PROMO_CODE_MESSAGE = "Promo code cannot be empty";
        private const string INVALID_PROMO_CODE_MESSAGE = "Invalid promo code";
        private const string NEGATIVE_QUANTITY_MESSAGE = "Quantity cannot be negative";
        private const string UPDATE_QUANTITY_ERROR = "Could not update quantity:";
        private const string REMOVE_PRODUCT_ERROR_PREFIX = "Could not remove product:";
        private const string RETRIVE_BASKET_ERROR_MESSAGE = "Failed to retrieve user's basket";

        private BasketService _basketService;
        private BasketRepositoryMock _basketRepositoryMock;
        private User _testUser;

        [SetUp]
        public void Setup()
        {
            _basketRepositoryMock = new BasketRepositoryMock();
            _basketService = new BasketService(_basketRepositoryMock);
            _testUser = new User(TEST_USER_ID, TEST_USER_NAME, TEST_USER_EMAIL);
        }

        [Test]
        public void TestAddProductToBasket_ValidParameters_AddsProduct()
        {
            // Act
            _basketService.AddProductToBasket(TEST_USER_ID, TEST_PRODUCT_ID, STANDARD_QUANTITY);

            // Assert
            Assert.That(_basketRepositoryMock.GetAddItemCount(), Is.EqualTo(EXPECTED_SINGLE_ITEM));
        }

        [Test]
        public void TestAddProductToBasket_QuantityExceedsMax_LimitsQuantity()
        {
            // Act
            _basketService.AddProductToBasket(TEST_USER_ID, TEST_PRODUCT_ID, EXCESSIVE_QUANTITY);

            // Assert
            Assert.That(_basketRepositoryMock.GetAddItemCount(), Is.EqualTo(EXPECTED_SINGLE_ITEM));
        }

        [Test]
        public void TestAddProductToBasket_InvalidUserId_ThrowsException()
        {
            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() =>
                _basketService.AddProductToBasket(INVALID_ID, TEST_PRODUCT_ID, DEFAULT_QUANTITY));

            Assert.That(ex.Message, Is.EqualTo(INVALID_USER_ID_MESSAGE));
            Assert.That(_basketRepositoryMock.GetAddItemCount(), Is.EqualTo(EXPECTED_ZERO_ITEMS));
        }

        [Test]
        public void TestAddProductToBasket_InvalidProductId_ThrowsException()
        {
            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() =>
                _basketService.AddProductToBasket(TEST_USER_ID, INVALID_ID, DEFAULT_QUANTITY));

            Assert.That(ex.Message, Is.EqualTo(INVALID_PRODUCT_ID_MESSAGE));
            Assert.That(_basketRepositoryMock.GetAddItemCount(), Is.EqualTo(EXPECTED_ZERO_ITEMS));
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

            Assert.That(ex.Message, Is.EqualTo(VALID_USER_ID_MESSAGE));
        }

        [Test]
        public void TestRemoveProductFromBasket_ValidParameters_RemovesProduct()
        {
            // Act
            _basketService.RemoveProductFromBasket(TEST_USER_ID, TEST_PRODUCT_ID);

            // Assert
            Assert.That(_basketRepositoryMock.GetRemoveItemCount(), Is.EqualTo(EXPECTED_SINGLE_ITEM));
        }

        [Test]
        public void TestRemoveProductFromBasket_InvalidUserId_ThrowsException()
        {
            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() =>
                _basketService.RemoveProductFromBasket(INVALID_ID, TEST_PRODUCT_ID));

            Assert.That(ex.Message, Is.EqualTo(INVALID_USER_ID_MESSAGE));
            Assert.That(_basketRepositoryMock.GetRemoveItemCount(), Is.EqualTo(EXPECTED_ZERO_ITEMS));
        }

        [Test]
        public void TestUpdateProductQuantity_ValidParameters_UpdatesQuantity()
        {
            // Arrange
            _basketService.AddProductToBasket(TEST_USER_ID, TEST_PRODUCT_ID, DEFAULT_QUANTITY);

            int addCount = _basketRepositoryMock.GetAddItemCount();
            Assert.That(addCount, Is.EqualTo(EXPECTED_SINGLE_ITEM));

            // Act
            _basketService.UpdateProductQuantity(TEST_USER_ID, TEST_PRODUCT_ID, UPDATED_QUANTITY);

            // Assert
            Assert.That(_basketRepositoryMock.GetUpdateItemCount(), Is.EqualTo(EXPECTED_SINGLE_ITEM));
        }

        [Test]
        public void TestUpdateProductQuantity_QuantityExceedsMax_LimitsQuantity()
        {
            // Arrange
            _basketService.AddProductToBasket(TEST_USER_ID, TEST_PRODUCT_ID, DEFAULT_QUANTITY);

            int addCount = _basketRepositoryMock.GetAddItemCount();
            Assert.That(addCount, Is.EqualTo(EXPECTED_SINGLE_ITEM));

            // Act
            _basketService.UpdateProductQuantity(TEST_USER_ID, TEST_PRODUCT_ID, EXCESSIVE_QUANTITY);

            // Assert
            Assert.That(_basketRepositoryMock.GetUpdateItemCount(), Is.EqualTo(EXPECTED_SINGLE_ITEM));
        }

        [Test]
        public void TestUpdateProductQuantity_QuantityZero_RemovesProduct()
        {
            // Act
            _basketService.UpdateProductQuantity(TEST_USER_ID, TEST_PRODUCT_ID, ZERO_QUANTITY);

            // Assert
            Assert.That(_basketRepositoryMock.GetRemoveItemCount(), Is.EqualTo(EXPECTED_SINGLE_ITEM));
            Assert.That(_basketRepositoryMock.GetUpdateItemCount(), Is.EqualTo(EXPECTED_ZERO_ITEMS));
        }

        [Test]
        public void TestUpdateProductQuantity_InvalidUserId_ThrowsException()
        {
            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() =>
                _basketService.UpdateProductQuantity(INVALID_ID, TEST_PRODUCT_ID, DEFAULT_QUANTITY));

            Assert.That(ex.Message, Is.EqualTo(INVALID_USER_ID_MESSAGE));
            Assert.That(_basketRepositoryMock.GetUpdateItemCount(), Is.EqualTo(EXPECTED_ZERO_ITEMS));
        }

        [Test]
        public void TestClearBasket_ValidUserId_ClearsBasket()
        {
            // Act
            _basketService.ClearBasket(TEST_USER_ID);

            // Assert
            Assert.That(_basketRepositoryMock.GetClearBasketCount(), Is.EqualTo(EXPECTED_SINGLE_ITEM));
        }

        [Test]
        public void TestClearBasket_InvalidUserId_ThrowsException()
        {
            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => _basketService.ClearBasket(INVALID_ID));

            Assert.That(ex.Message, Is.EqualTo(INVALID_USER_ID_MESSAGE));
            Assert.That(_basketRepositoryMock.GetClearBasketCount(), Is.EqualTo(EXPECTED_ZERO_ITEMS));
        }

        [Test]
        public void TestValidateBasketBeforeCheckOut_EmptyBasket_ReturnsFalse()
        {
            // Act
            bool result = _basketService.ValidateBasketBeforeCheckOut(BASKET_ID);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void TestApplyPromoCode_ValidCode_AppliesDiscount()
        {
            // Act
            _basketService.ApplyPromoCode(BASKET_ID, VALID_PROMO_CODE);
            float discount = _basketService.GetPromoCodeDiscount(VALID_PROMO_CODE, STANDARD_SUBTOTAL);

            // Assert
            Assert.That(discount, Is.EqualTo(STANDARD_DISCOUNT));
        }

        [Test]
        public void TestApplyPromoCode_InvalidCode_ThrowsException()
        {
            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() =>
                _basketService.ApplyPromoCode(BASKET_ID, INVALID_PROMO_CODE));

            Assert.That(ex.Message, Is.EqualTo(INVALID_PROMO_CODE_MESSAGE));
        }

        [Test]
        public void TestValidateBasketBeforeCheckOut_InvalidBasketId_ThrowsException()
        {
            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() =>
                _basketService.ValidateBasketBeforeCheckOut(INVALID_ID));

            Assert.That(ex.Message, Is.EqualTo(INVALID_BASKET_ID_MESSAGE));
        }

        [Test]
        public void TestValidateBasketBeforeCheckOut_ItemWithZeroQuantity_ReturnsFalse()
        {
            // Arrange
            _basketRepositoryMock.SetupInvalidItemQuantity(BASKET_ID);

            // Act
            bool result = _basketService.ValidateBasketBeforeCheckOut(BASKET_ID);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void TestValidateBasketBeforeCheckOut_ItemWithInvalidPrice_ReturnsFalse()
        {
            // Arrange
            _basketRepositoryMock.SetupInvalidItemPrice(BASKET_ID);

            // Act
            bool result = _basketService.ValidateBasketBeforeCheckOut(BASKET_ID);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void TestValidateBasketBeforeCheckOut_ValidBasket_ReturnsTrue()
        {
            // Arrange
            _basketRepositoryMock.SetupValidBasket(BASKET_ID);

            // Act
            bool result = _basketService.ValidateBasketBeforeCheckOut(BASKET_ID);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void TestApplyPromoCode_InvalidBasketId_ThrowsException()
        {
            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() =>
                _basketService.ApplyPromoCode(INVALID_ID, VALID_PROMO_CODE));

            Assert.That(ex.Message, Is.EqualTo(INVALID_BASKET_ID_MESSAGE));
        }

        [Test]
        public void TestApplyPromoCode_EmptyCode_ThrowsException()
        {
            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() =>
                _basketService.ApplyPromoCode(BASKET_ID, EMPTY_CODE));

            Assert.That(ex.Message, Is.EqualTo(EMPTY_PROMO_CODE_MESSAGE));
        }

        [Test]
        public void TestApplyPromoCode_NullCode_ThrowsException()
        {
            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() =>
                _basketService.ApplyPromoCode(BASKET_ID, null));

            Assert.That(ex.Message, Is.EqualTo(EMPTY_PROMO_CODE_MESSAGE));
        }

        [Test]
        public void TestApplyPromoCode_WhitespaceCode_ThrowsException()
        {
            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() =>
                _basketService.ApplyPromoCode(BASKET_ID, WHITESPACE_CODE));

            Assert.That(ex.Message, Is.EqualTo(EMPTY_PROMO_CODE_MESSAGE));
        }

        [Test]
        public void TestUpdateProductQuantity_InvalidProductId_ThrowsException()
        {
            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() =>
                _basketService.UpdateProductQuantity(TEST_USER_ID, INVALID_ID, DEFAULT_QUANTITY));

            Assert.That(ex.Message, Is.EqualTo(INVALID_PRODUCT_ID_MESSAGE));
            Assert.That(_basketRepositoryMock.GetUpdateItemCount(), Is.EqualTo(EXPECTED_ZERO_ITEMS));
        }

        [Test]
        public void TestUpdateProductQuantity_NegativeQuantity_ThrowsException()
        {
            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() =>
                _basketService.UpdateProductQuantity(TEST_USER_ID, TEST_PRODUCT_ID, NEGATIVE_QUANTITY));

            Assert.That(ex.Message, Is.EqualTo(NEGATIVE_QUANTITY_MESSAGE));
            Assert.That(_basketRepositoryMock.GetUpdateItemCount(), Is.EqualTo(EXPECTED_ZERO_ITEMS));
        }

        [Test]
        public void TestUpdateProductQuantity_RepositoryThrowsException_ThrowsInvalidOperationException()
        {
            // Arrange
            _basketRepositoryMock.SetupUpdateQuantityException();

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() =>
                _basketService.UpdateProductQuantity(TEST_USER_ID, TEST_PRODUCT_ID, DEFAULT_QUANTITY));

            Assert.That(ex.Message, Does.Contain(UPDATE_QUANTITY_ERROR));
        }

        [Test]
        public void TestRemoveProductFromBasket_InvalidProductId_ThrowsException()
        {
            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() =>
                _basketService.RemoveProductFromBasket(TEST_USER_ID, INVALID_ID));

            Assert.That(ex.Message, Is.EqualTo(INVALID_PRODUCT_ID_MESSAGE));
            Assert.That(_basketRepositoryMock.GetRemoveItemCount(), Is.EqualTo(EXPECTED_ZERO_ITEMS));
        }

        [Test]
        public void TestRemoveProductFromBasket_RepositoryThrowsException_ThrowsInvalidOperationException()
        {
            // Arrange
            _basketRepositoryMock.SetupRemoveItemException();

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() =>
                _basketService.RemoveProductFromBasket(TEST_USER_ID, TEST_PRODUCT_ID));

            Assert.That(ex.Message, Does.Contain(REMOVE_PRODUCT_ERROR_PREFIX));
        }

        [Test]
        public void TestGetBasketByUser_RepositoryThrowsException_ThrowsApplicationException()
        {
            // Arrange
            _basketRepositoryMock.SetupGetBasketException();

            // Act & Assert
            var ex = Assert.Throws<ApplicationException>(() => _basketService.GetBasketByUser(_testUser));

            Assert.That(ex.Message, Is.EqualTo(RETRIVE_BASKET_ERROR_MESSAGE));
        }

        [Test]
        public void TestGetPromoCodeDiscount_EmptyCode_ReturnsZero()
        {
            // Act
            float discount = _basketService.GetPromoCodeDiscount(EMPTY_CODE, STANDARD_SUBTOTAL);

            // Assert
            Assert.That(discount, Is.EqualTo(EXPECTED_ZERO_VALUE));
        }

        [Test]
        public void TestGetPromoCodeDiscount_NullCode_ReturnsZero()
        {
            // Act
            float discount = _basketService.GetPromoCodeDiscount(null, STANDARD_SUBTOTAL);

            // Assert
            Assert.That(discount, Is.EqualTo(EXPECTED_ZERO_VALUE));
        }

        [Test]
        public void TestGetPromoCodeDiscount_WhitespaceCode_ReturnsZero()
        {
            // Act
            float discount = _basketService.GetPromoCodeDiscount(WHITESPACE_CODE, STANDARD_SUBTOTAL);

            // Assert
            Assert.That(discount, Is.EqualTo(EXPECTED_ZERO_VALUE));
        }

        [Test]
        public void TestCalculateBasketTotals_ValidBasket_ReturnsCorrectTotals()
        {
            // Arrange
            _basketRepositoryMock.SetupValidBasket(BASKET_ID);

            // Act
            BasketTotals totals = _basketService.CalculateBasketTotals(BASKET_ID, null);

            // Assert
            Assert.That(totals, Is.Not.Null);
            Assert.That(totals.Subtotal, Is.EqualTo(EXPECTED_SUBTOTAL));
            Assert.That(totals.Discount, Is.EqualTo(EXPECTED_ZERO_VALUE));
            Assert.That(totals.TotalAmount, Is.EqualTo(EXPECTED_SUBTOTAL));
        }

        [Test]
        public void TestCalculateBasketTotals_WithValidPromoCode_AppliesDiscount()
        {
            // Arrange
            _basketRepositoryMock.SetupValidBasket(BASKET_ID);

            // Act
            BasketTotals totals = _basketService.CalculateBasketTotals(BASKET_ID, VALID_PROMO_CODE);

            // Assert
            Assert.That(totals, Is.Not.Null);
            Assert.That(totals.Subtotal, Is.EqualTo(EXPECTED_SUBTOTAL));
            Assert.That(totals.Discount, Is.EqualTo(EXPECTED_DISCOUNT));
            Assert.That(totals.TotalAmount, Is.EqualTo(EXPECTED_TOTAL_DISCOUNT));
        }

        [Test]
        public void TestCalculateBasketTotals_WithInvalidPromoCode_NoDiscount()
        {
            // Arrange
            _basketRepositoryMock.SetupValidBasket(BASKET_ID);

            // Act
            BasketTotals totals = _basketService.CalculateBasketTotals(BASKET_ID, INVALID_PROMO_CODE);

            // Assert
            Assert.That(totals, Is.Not.Null);
            Assert.That(totals.Subtotal, Is.EqualTo(EXPECTED_SUBTOTAL));
            Assert.That(totals.Discount, Is.EqualTo(EXPECTED_ZERO_VALUE));
            Assert.That(totals.TotalAmount, Is.EqualTo(EXPECTED_SUBTOTAL));
        }

        [Test]
        public void TestCalculateBasketTotals_EmptyBasket_ReturnsZeroTotals()
        {
            // Act
            BasketTotals totals = _basketService.CalculateBasketTotals(EMPTY_BASKET_ID, null);

            // Assert
            Assert.That(totals, Is.Not.Null);
            Assert.That(totals.Subtotal, Is.EqualTo(EXPECTED_ZERO_VALUE));
            Assert.That(totals.Discount, Is.EqualTo(EXPECTED_ZERO_VALUE));
            Assert.That(totals.TotalAmount, Is.EqualTo(EXPECTED_ZERO_VALUE));
        }

        [Test]
        public void TestCalculateBasketTotals_InvalidBasketId_ThrowsException()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() =>
                _basketService.CalculateBasketTotals(NEGATIVE_ID, null));
        }
    }
}