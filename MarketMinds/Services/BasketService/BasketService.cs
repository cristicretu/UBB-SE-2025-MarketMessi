using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using DomainLayer.Domain;
using Microsoft.Extensions.Configuration;

namespace MarketMinds.Services.BasketService
{
    // Custom JSON converter for Basket to handle server/client model differences
    public class BasketConverter : JsonConverter<Basket>
    {
        public override Basket Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                Debug.WriteLine($"[BasketConverter] Expected StartObject token, got {reader.TokenType}");
                throw new JsonException($"Expected StartObject token, got {reader.TokenType}");
            }

            var basket = new Basket();
            basket.Items = new List<BasketItem>(); // Initialize items list

            // Read through all properties of the basket object
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    return basket;
                }

                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    Debug.WriteLine($"[BasketConverter] Expected PropertyName token, got {reader.TokenType}");
                    continue; // Skip unexpected tokens
                }

                string propertyName = reader.GetString();
                Debug.WriteLine($"[BasketConverter] Reading property: {propertyName}");

                // Read the property value
                if (!reader.Read())
                {
                    Debug.WriteLine("[BasketConverter] Unexpected end of JSON");
                    break;
                }

                try
                {
                    switch (propertyName)
                    {
                        case "id":
                            if (reader.TokenType == JsonTokenType.Number)
                            {
                                basket.Id = reader.GetInt32();
                                Debug.WriteLine($"[BasketConverter] Read basket ID: {basket.Id}");
                            }
                            else
                            {
                                Debug.WriteLine($"[BasketConverter] Expected Number for id, got {reader.TokenType}");
                                reader.Skip();
                            }
                            break;
                        case "buyerId":
                            // Just read and ignore
                            reader.Skip();
                            break;
                        case "items":
                            if (reader.TokenType == JsonTokenType.Null)
                            {
                                Debug.WriteLine("[BasketConverter] Items property is null");
                                // Already initialized with empty list
                            }
                            else if (reader.TokenType == JsonTokenType.StartArray)
                            {
                                Debug.WriteLine("[BasketConverter] Reading items array");
                                basket.Items = ReadBasketItemsArray(ref reader, options);
                            }
                            else if (reader.TokenType == JsonTokenType.StartObject)
                            {
                                Debug.WriteLine("[BasketConverter] Reading items as object with $values array");
                                basket.Items = ReadBasketItemsObject(ref reader, options);
                            }
                            else
                            {
                                Debug.WriteLine($"[BasketConverter] Expected StartArray or StartObject for items, got {reader.TokenType}");
                                reader.Skip(); // Skip this property
                            }
                            break;
                        default:
                            Debug.WriteLine($"[BasketConverter] Skipping unknown property: {propertyName}");
                            reader.Skip();
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[BasketConverter] Error reading property {propertyName}: {ex.Message}");
                    // Try to skip this property and continue
                    try { reader.Skip(); } catch { /* ignore */ }
                }
            }

            // If we got here, we hit the end of the JSON without finding the end object
            Debug.WriteLine("[BasketConverter] Warning: Reached end of JSON without proper closure");
            return basket;
        }

        private List<BasketItem> ReadBasketItemsArray(ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            var items = new List<BasketItem>();
            var itemCount = 0;

            // Process the items array
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndArray)
                {
                    Debug.WriteLine($"[BasketConverter] Finished reading {itemCount} items");
                    return items;
                }

                if (reader.TokenType != JsonTokenType.StartObject)
                {
                    Debug.WriteLine($"[BasketConverter] Expected StartObject for basket item, got {reader.TokenType}");
                    continue; // Skip non-object elements
                }

                try
                {
                    var item = ReadBasketItem(ref reader);
                    if (item != null)
                    {
                        items.Add(item);
                        itemCount++;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[BasketConverter] Error reading basket item: {ex.Message}");
                    // Try to skip to the end of this item
                    SkipToEndObject(ref reader);
                }
            }

            Debug.WriteLine("[BasketConverter] Warning: Reached end of JSON without proper array closure");
            return items;
        }

        private BasketItem ReadBasketItem(ref Utf8JsonReader reader)
        {
            Debug.WriteLine("[BasketConverter] Reading basket item");
            var item = new BasketItem();

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    Debug.WriteLine($"[BasketConverter] Finished reading basket item: ID={item.Id}, ProductID={item.ProductId}, Quantity={item.Quantity}");
                    return item;
                }

                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    Debug.WriteLine($"[BasketConverter] Expected PropertyName for basket item property, got {reader.TokenType}");
                    continue;
                }

                string propertyName = reader.GetString();
                Debug.WriteLine($"[BasketConverter] Reading basket item property: {propertyName}");

                // Read the property value
                if (!reader.Read())
                {
                    Debug.WriteLine("[BasketConverter] Unexpected end of JSON in basket item");
                    break;
                }

                try
                {
                    switch (propertyName)
                    {
                        case "$id": // Skip reference ID property
                            reader.Skip();
                            break;
                        case "id":
                            if (reader.TokenType == JsonTokenType.Number)
                            {
                                item.Id = reader.GetInt32();
                                Debug.WriteLine($"[BasketConverter] Set basket item ID: {item.Id}");
                            }
                            else
                            {
                                reader.Skip();
                            }
                            break;
                        case "basketId":
                            if (reader.TokenType == JsonTokenType.Number)
                            {
                                item.BasketId = reader.GetInt32();
                                Debug.WriteLine($"[BasketConverter] Set basket item BasketId: {item.BasketId}");
                            }
                            else
                            {
                                reader.Skip();
                            }
                            break;
                        case "productId":
                            if (reader.TokenType == JsonTokenType.Number)
                            {
                                item.ProductId = reader.GetInt32();
                                Debug.WriteLine($"[BasketConverter] Set basket item ProductId: {item.ProductId}");
                            }
                            else
                            {
                                reader.Skip();
                            }
                            break;
                        case "quantity":
                            if (reader.TokenType == JsonTokenType.Number)
                            {
                                item.Quantity = reader.GetInt32();
                                Debug.WriteLine($"[BasketConverter] Set basket item Quantity: {item.Quantity}");
                            }
                            else
                            {
                                reader.Skip();
                            }
                            break;
                        case "price":
                            if (reader.TokenType == JsonTokenType.Number)
                            {
                                item.Price = reader.GetDouble();
                                Debug.WriteLine($"[BasketConverter] Set basket item Price: {item.Price}");
                            }
                            else
                            {
                                reader.Skip();
                            }
                            break;
                        case "product":
                            if (reader.TokenType == JsonTokenType.Null)
                            {
                                item.Product = null;
                                Debug.WriteLine("[BasketConverter] Product is null");
                            }
                            else if (reader.TokenType == JsonTokenType.StartObject)
                            {
                                Debug.WriteLine("[BasketConverter] Reading product object");
                                // Manually create and configure a product
                                var product = new BuyProduct();
                                item.Product = ReadBuyProduct(ref reader, product);
                                Debug.WriteLine($"[BasketConverter] Product read successfully: ID={item.Product.Id}, Title={item.Product.Title}");
                            }
                            else
                            {
                                Debug.WriteLine($"[BasketConverter] Unexpected token for product: {reader.TokenType}");
                                reader.Skip();
                            }
                            break;
                        default:
                            Debug.WriteLine($"[BasketConverter] Skipping unknown basket item property: {propertyName}");
                            reader.Skip();
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[BasketConverter] Error reading item property {propertyName}: {ex.Message}");
                    try { reader.Skip(); } catch { /* ignore */ }
                }
            }

            Debug.WriteLine("[BasketConverter] Warning: Reached end of JSON without proper item closure");
            return item;
        }

        private BuyProduct ReadBuyProduct(ref Utf8JsonReader reader, BuyProduct product)
        {
            Debug.WriteLine("[BasketConverter] Reading BuyProduct");
            product.Tags = new List<ProductTag>();
            product.Images = new List<Image>();

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    Debug.WriteLine($"[BasketConverter] Finished reading BuyProduct: ID={product.Id}, Title={product.Title}");
                    return product;
                }

                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    continue;
                }

                string propertyName = reader.GetString();
                Debug.WriteLine($"[BasketConverter] Reading BuyProduct property: {propertyName}");

                // Read the property value
                if (!reader.Read())
                {
                    break;
                }

                try
                {
                    switch (propertyName)
                    {
                        case "$id": // Skip reference ID
                            reader.Skip();
                            break;
                        case "id":
                            if (reader.TokenType == JsonTokenType.Number)
                            {
                                product.Id = reader.GetInt32();
                                Debug.WriteLine($"[BasketConverter] Set product ID: {product.Id}");
                            }
                            else
                            {
                                reader.Skip();
                            }
                            break;
                        case "title":
                            if (reader.TokenType == JsonTokenType.String)
                            {
                                product.Title = reader.GetString();
                                Debug.WriteLine($"[BasketConverter] Set product Title: {product.Title}");
                            }
                            else
                            {
                                reader.Skip();
                            }
                            break;
                        case "description":
                            if (reader.TokenType == JsonTokenType.String)
                            {
                                product.Description = reader.GetString();
                            }
                            else
                            {
                                reader.Skip();
                            }
                            break;
                        case "price":
                            if (reader.TokenType == JsonTokenType.Number)
                            {
                                product.Price = reader.GetDouble();
                                Debug.WriteLine($"[BasketConverter] Set product Price: {product.Price}");
                            }
                            else
                            {
                                reader.Skip();
                            }
                            break;
                        case "sellerId":
                            if (reader.TokenType == JsonTokenType.Number)
                            {
                                // Just store the ID if we don't have a seller object yet
                                int sellerId = reader.GetInt32();
                                if (product.Seller == null)
                                {
                                    product.Seller = new User { Id = sellerId };
                                }
                                else
                                {
                                    product.Seller.Id = sellerId;
                                }
                            }
                            else
                            {
                                reader.Skip();
                            }
                            break;
                        case "conditionId":
                            if (reader.TokenType == JsonTokenType.Number)
                            {
                                // Just store the ID if we don't have a condition object yet
                                int conditionId = reader.GetInt32();
                                if (product.Condition == null)
                                {
                                    product.Condition = new ProductCondition { Id = conditionId };
                                }
                                else
                                {
                                    product.Condition.Id = conditionId;
                                }
                            }
                            else
                            {
                                reader.Skip();
                            }
                            break;
                        case "categoryId":
                            if (reader.TokenType == JsonTokenType.Number)
                            {
                                // Just store the ID if we don't have a category object yet
                                int categoryId = reader.GetInt32();
                                if (product.Category == null)
                                {
                                    product.Category = new ProductCategory { Id = categoryId };
                                }
                                else
                                {
                                    product.Category.Id = categoryId;
                                }
                            }
                            else
                            {
                                reader.Skip();
                            }
                            break;
                        case "seller":
                            if (reader.TokenType == JsonTokenType.StartObject)
                            {
                                product.Seller = ReadUserBasic(ref reader);
                            }
                            else
                            {
                                reader.Skip();
                            }
                            break;
                        case "condition":
                            if (reader.TokenType == JsonTokenType.StartObject)
                            {
                                product.Condition = ReadCondition(ref reader);
                            }
                            else
                            {
                                reader.Skip();
                            }
                            break;
                        case "category":
                            if (reader.TokenType == JsonTokenType.StartObject)
                            {
                                product.Category = ReadCategory(ref reader);
                            }
                            else
                            {
                                reader.Skip();
                            }
                            break;
                        case "tags":
                            if (reader.TokenType == JsonTokenType.StartObject)
                            {
                                // The tags might be in an object with a $values array
                                product.Tags = ReadTagsFromObject(ref reader);
                            }
                            else if (reader.TokenType == JsonTokenType.StartArray)
                            {
                                // Or directly as an array
                                product.Tags = ReadTagsArray(ref reader);
                            }
                            else
                            {
                                reader.Skip();
                            }
                            break;
                        case "images":
                            if (reader.TokenType == JsonTokenType.StartObject)
                            {
                                // The images might be in an object with a $values array
                                product.Images = ReadImagesFromObject(ref reader);
                            }
                            else if (reader.TokenType == JsonTokenType.StartArray)
                            {
                                // Or directly as an array
                                product.Images = ReadImagesArray(ref reader);
                            }
                            else
                            {
                                reader.Skip();
                            }
                            break;
                        default:
                            reader.Skip();
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[BasketConverter] Error reading product property {propertyName}: {ex.Message}");
                    try { reader.Skip(); } catch { /* ignore */ }
                }
            }

            return product;
        }

        private List<ProductTag> ReadTagsFromObject(ref Utf8JsonReader reader)
        {
            var tags = new List<ProductTag>();

            // Process the tags object
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    return tags;
                }

                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    continue;
                }

                string propertyName = reader.GetString();

                // Read the property value
                if (!reader.Read())
                {
                    break;
                }

                // Look for the $values property
                if (propertyName == "$values" && reader.TokenType == JsonTokenType.StartArray)
                {
                    // Process the tags array
                    while (reader.Read())
                    {
                        if (reader.TokenType == JsonTokenType.EndArray)
                        {
                            break;
                        }

                        if (reader.TokenType == JsonTokenType.StartObject)
                        {
                            var tag = ReadProductTag(ref reader);
                            if (tag != null)
                            {
                                tags.Add(tag);
                            }
                        }
                        else
                        {
                            reader.Skip();
                        }
                    }
                }
                else
                {
                    reader.Skip();
                }
            }

            return tags;
        }

        private ProductTag ReadProductTag(ref Utf8JsonReader reader)
        {
            var tag = new ProductTag();

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    return tag;
                }

                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    continue;
                }

                string propertyName = reader.GetString();

                if (!reader.Read())
                {
                    break;
                }

                try
                {
                    switch (propertyName)
                    {
                        case "$id": // Skip reference ID
                            reader.Skip();
                            break;
                        case "id":
                            if (reader.TokenType == JsonTokenType.Number)
                            {
                                tag.Id = reader.GetInt32();
                            }
                            break;
                        case "name":
                        case "title":
                        case "displayTitle":
                            if (reader.TokenType == JsonTokenType.String)
                            {
                                tag.DisplayTitle = reader.GetString();
                            }
                            break;
                        default:
                            reader.Skip();
                            break;
                    }
                }
                catch
                {
                    reader.Skip();
                }
            }

            return tag;
        }

        private List<ProductTag> ReadTagsArray(ref Utf8JsonReader reader)
        {
            var tags = new List<ProductTag>();

            // Process the tags array
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndArray)
                {
                    return tags;
                }

                if (reader.TokenType == JsonTokenType.StartObject)
                {
                    var tag = ReadProductTag(ref reader);
                    if (tag != null)
                    {
                        tags.Add(tag);
                    }
                }
                else
                {
                    reader.Skip();
                }
            }

            return tags;
        }

        private List<Image> ReadImagesFromObject(ref Utf8JsonReader reader)
        {
            var images = new List<Image>();

            // Process the images object
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    return images;
                }

                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    continue;
                }

                string propertyName = reader.GetString();

                // Read the property value
                if (!reader.Read())
                {
                    break;
                }

                // Look for the $values property
                if (propertyName == "$values" && reader.TokenType == JsonTokenType.StartArray)
                {
                    // Process the images array
                    while (reader.Read())
                    {
                        if (reader.TokenType == JsonTokenType.EndArray)
                        {
                            break;
                        }

                        if (reader.TokenType == JsonTokenType.StartObject)
                        {
                            var image = ReadImage(ref reader);
                            if (image != null)
                            {
                                images.Add(image);
                            }
                        }
                        else
                        {
                            reader.Skip();
                        }
                    }
                }
                else
                {
                    reader.Skip();
                }
            }

            return images;
        }

        private Image ReadImage(ref Utf8JsonReader reader)
        {
            var image = new Image();

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    return image;
                }

                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    continue;
                }

                string propertyName = reader.GetString();

                if (!reader.Read())
                {
                    break;
                }

                try
                {
                    switch (propertyName)
                    {
                        case "$id": // Skip reference ID
                            reader.Skip();
                            break;
                        case "id":
                            if (reader.TokenType == JsonTokenType.Number)
                            {
                                // No ID property in our Image class, just skip
                                reader.Skip();
                            }
                            break;
                        case "url":
                            if (reader.TokenType == JsonTokenType.String)
                            {
                                image.Url = reader.GetString();
                            }
                            break;
                        default:
                            reader.Skip();
                            break;
                    }
                }
                catch
                {
                    reader.Skip();
                }
            }

            return image;
        }

        private List<Image> ReadImagesArray(ref Utf8JsonReader reader)
        {
            var images = new List<Image>();

            // Process the images array
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndArray)
                {
                    return images;
                }

                if (reader.TokenType == JsonTokenType.StartObject)
                {
                    var image = ReadImage(ref reader);
                    if (image != null)
                    {
                        images.Add(image);
                    }
                }
                else
                {
                    reader.Skip();
                }
            }

            return images;
        }

        private User ReadUserBasic(ref Utf8JsonReader reader)
        {
            var user = new User();

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    return user;
                }

                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    continue;
                }

                string propertyName = reader.GetString();

                // Read the property value
                if (!reader.Read())
                {
                    break;
                }

                try
                {
                    switch (propertyName)
                    {
                        case "$id": // Skip reference ID
                            reader.Skip();
                            break;
                        case "id":
                            if (reader.TokenType == JsonTokenType.Number)
                            {
                                user.Id = reader.GetInt32();
                            }
                            break;
                        case "username":
                            if (reader.TokenType == JsonTokenType.String)
                            {
                                user.Username = reader.GetString();
                            }
                            break;
                        case "email":
                            if (reader.TokenType == JsonTokenType.String)
                            {
                                user.Email = reader.GetString();
                            }
                            break;
                        default:
                            reader.Skip();
                            break;
                    }
                }
                catch
                {
                    try { reader.Skip(); } catch { /* ignore */ }
                }
            }

            return user;
        }

        private ProductCondition ReadCondition(ref Utf8JsonReader reader)
        {
            var condition = new ProductCondition();

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    return condition;
                }

                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    continue;
                }

                string propertyName = reader.GetString();

                // Read the property value
                if (!reader.Read())
                {
                    break;
                }

                try
                {
                    switch (propertyName)
                    {
                        case "$id": // Skip reference ID
                            reader.Skip();
                            break;
                        case "id":
                            if (reader.TokenType == JsonTokenType.Number)
                            {
                                condition.Id = reader.GetInt32();
                            }
                            break;
                        case "displayTitle":
                        case "name":
                            if (reader.TokenType == JsonTokenType.String)
                            {
                                condition.DisplayTitle = reader.GetString();
                            }
                            break;
                        case "description":
                            if (reader.TokenType == JsonTokenType.String)
                            {
                                condition.Description = reader.GetString();
                            }
                            break;
                        default:
                            reader.Skip();
                            break;
                    }
                }
                catch
                {
                    try { reader.Skip(); } catch { /* ignore */ }
                }
            }

            return condition;
        }

        private ProductCategory ReadCategory(ref Utf8JsonReader reader)
        {
            var category = new ProductCategory();

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    return category;
                }

                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    continue;
                }

                string propertyName = reader.GetString();

                // Read the property value
                if (!reader.Read())
                {
                    break;
                }

                try
                {
                    switch (propertyName)
                    {
                        case "$id": // Skip reference ID
                            reader.Skip();
                            break;
                        case "id":
                            if (reader.TokenType == JsonTokenType.Number)
                            {
                                category.Id = reader.GetInt32();
                            }
                            break;
                        case "displayTitle":
                        case "name":
                            if (reader.TokenType == JsonTokenType.String)
                            {
                                category.DisplayTitle = reader.GetString();
                            }
                            break;
                        case "description":
                            if (reader.TokenType == JsonTokenType.String)
                            {
                                category.Description = reader.GetString();
                            }
                            break;
                        default:
                            reader.Skip();
                            break;
                    }
                }
                catch
                {
                    try { reader.Skip(); } catch { /* ignore */ }
                }
            }

            return category;
        }

        // Helper method to skip to the end of an object
        private void SkipToEndObject(ref Utf8JsonReader reader)
        {
            int depth = 1;

            while (reader.Read() && depth > 0)
            {
                if (reader.TokenType == JsonTokenType.StartObject)
                {
                    depth++;
                }
                else if (reader.TokenType == JsonTokenType.EndObject)
                {
                    depth--;
                }
            }
        }

        private List<BasketItem> ReadBasketItemsObject(ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            Debug.WriteLine("[BasketConverter] Entering ReadBasketItemsObject");
            var items = new List<BasketItem>();
            var itemCount = 0;
            var foundValuesArray = false;

            // Process the object containing the items array
            while (reader.Read())
            {
                // We've reached the end of the items object
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    Debug.WriteLine($"[BasketConverter] Finished reading items object with {itemCount} items");
                    return items;
                }

                // Skip any property that is not a property name
                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    Debug.WriteLine($"[BasketConverter] Unexpected token in items object: {reader.TokenType}");
                    continue;
                }

                string propertyName = reader.GetString();
                Debug.WriteLine($"[BasketConverter] Found property in items object: {propertyName}");

                // We're only interested in the $values property
                if (propertyName == "$values")
                {
                    // Move to the value
                    if (!reader.Read())
                    {
                        Debug.WriteLine("[BasketConverter] Unexpected end of JSON in items object");
                        break;
                    }

                    // The $values property should be an array
                    if (reader.TokenType != JsonTokenType.StartArray)
                    {
                        Debug.WriteLine($"[BasketConverter] Expected StartArray for $values, got {reader.TokenType}");
                        reader.Skip();
                        continue;
                    }

                    foundValuesArray = true;
                    Debug.WriteLine("[BasketConverter] Reading $values array");

                    // Process the items array
                    while (reader.Read())
                    {
                        if (reader.TokenType == JsonTokenType.EndArray)
                        {
                            Debug.WriteLine($"[BasketConverter] Finished reading $values array with {itemCount} items");
                            break;
                        }

                        if (reader.TokenType != JsonTokenType.StartObject)
                        {
                            Debug.WriteLine($"[BasketConverter] Expected StartObject for basket item, got {reader.TokenType}");
                            continue; // Skip non-object elements
                        }

                        try
                        {
                            var item = ReadBasketItem(ref reader);
                            if (item != null)
                            {
                                items.Add(item);
                                itemCount++;
                                Debug.WriteLine($"[BasketConverter] Added item {itemCount}: ID={item.Id}, ProductID={item.ProductId}");
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"[BasketConverter] Error reading basket item: {ex.Message}");
                            // Try to skip to the end of this item
                            SkipToEndObject(ref reader);
                        }
                    }
                }
                else
                {
                    // Skip any other property
                    reader.Read();
                    reader.Skip();
                }
            }

            // If we didn't find the $values array, return an empty list
            if (!foundValuesArray)
            {
                Debug.WriteLine("[BasketConverter] Warning: No $values array found in items object");
            }

            Debug.WriteLine("[BasketConverter] Warning: Reached end of JSON without proper closure of items object");
            return items;
        }

        public override void Write(Utf8JsonWriter writer, Basket value, JsonSerializerOptions options)
        {
            throw new NotImplementedException("Serialization is not supported");
        }
    }

    public class BasketService : IBasketService
    {
        public const int MaxQuantityPerItem = 10;
        private const int NOUSER = 0;
        private const int NOITEM = 0;
        private const int NOBASKET = 0;
        private const int NODISCOUNT = 0;
        private const int NOQUANTITY = 0;
        private const int DEFAULTQUANTITY = 1;

        private readonly HttpClient httpClient;
        private readonly string apiBaseUrl;
        private readonly JsonSerializerOptions jsonOptions;

        // Constructor with configuration
        public BasketService(IConfiguration configuration)
        {
            httpClient = new HttpClient();
            apiBaseUrl = configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5000";
            if (!apiBaseUrl.EndsWith("/"))
            {
                apiBaseUrl += "/";
            }
            httpClient.BaseAddress = new Uri(apiBaseUrl + "api/basket/");

            // Configure JSON options with our custom converters
            jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                AllowTrailingCommas = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                NumberHandling = JsonNumberHandling.AllowReadingFromString
            };
            jsonOptions.Converters.Add(new BasketConverter());
            jsonOptions.Converters.Add(new BasketItemConverter());

            // Set longer timeout for HTTP requests
            httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        public void AddProductToBasket(int userId, int productId, int quantity)
        {
            if (userId <= NOUSER)
            {
                throw new ArgumentException("Invalid user ID");
            }
            if (productId <= NOITEM)
            {
                throw new ArgumentException("Invalid product ID");
            }

            try
            {
                // Apply the maximum quantity limit
                int limitedQuantity = Math.Min(quantity, MaxQuantityPerItem);

                // Make the API call to add product to basket
                var response = httpClient.PostAsJsonAsync($"user/{userId}/product/{productId}", limitedQuantity).Result;
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex)
            {
                throw new ApplicationException($"Failed to add product to basket: {ex.Message}", ex);
            }
        }

        public Basket GetBasketByUser(User user)
        {
            Debug.WriteLine($"[Service] GetBasketByUser called for user ID: {user?.Id}");

            if (user == null || user.Id <= NOUSER)
            {
                Debug.WriteLine("[Service] ERROR: Invalid user provided");
                throw new ArgumentException("Valid user must be provided");
            }

            try
            {
                string fullUrl = $"{apiBaseUrl}api/basket/user/{user.Id}";
                Debug.WriteLine($"[Service] Making API request to: {fullUrl}");

                var response = httpClient.GetAsync(fullUrl).Result;
                Debug.WriteLine($"[Service] Response status: {response.StatusCode}");
                response.EnsureSuccessStatusCode();

                // Use the custom JSON converter to properly deserialize the response
                var responseContent = response.Content.ReadAsStringAsync().Result;

                // Log the raw JSON for debugging
                Debug.WriteLine($"[Service] Raw JSON response: {responseContent}");

                try
                {
                    // Create a custom JsonSerializerOptions with more features enabled
                    var debugOptions = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        AllowTrailingCommas = true,
                        ReadCommentHandling = JsonCommentHandling.Skip
                    };
                    debugOptions.Converters.Add(new BasketConverter());

                    var basket = JsonSerializer.Deserialize<Basket>(responseContent, debugOptions);
                    Debug.WriteLine($"[Service] Basket deserialized, ID: {basket.Id}, Items count: {basket.Items?.Count ?? 0}");

                    // Validate product references
                    if (basket.Items != null)
                    {
                        int nullProducts = basket.Items.Count(i => i.Product == null);
                        if (nullProducts > 0)
                        {
                            Debug.WriteLine($"[Service] WARNING: {nullProducts} items have null Product references after deserialization!");
                        }

                        // Check first item
                        if (basket.Items.Count > 0)
                        {
                            var firstItem = basket.Items[0];
                            Debug.WriteLine($"[Service] First item: ID={firstItem.Id}, ProductID={firstItem.ProductId}, Product null? {firstItem.Product == null}");
                        }
                    }

                    return basket;
                }
                catch (JsonException ex)
                {
                    Debug.WriteLine($"[Service] JSON Deserialization Error: {ex.Message}");
                    Debug.WriteLine($"[Service] Stack Trace: {ex.StackTrace}");

                    // Try to fallback to a simpler deserialization
                    var fallbackBasket = new Basket { Id = user.Id, Items = new List<BasketItem>() };
                    Debug.WriteLine($"[Service] Using fallback empty basket after deserialization failure");
                    return fallbackBasket;
                }
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"[Service] ERROR: HttpRequestException - {ex.Message}");
                if (ex.InnerException != null)
                {
                    Debug.WriteLine($"[Service] Inner exception: {ex.InnerException.Message}");
                }
                throw new ApplicationException($"Failed to retrieve user's basket: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Service] ERROR: General exception - {ex.GetType().Name} - {ex.Message}");
                if (ex.InnerException != null)
                {
                    Debug.WriteLine($"[Service] Inner exception: {ex.InnerException.Message}");
                }
                throw new ApplicationException("Failed to retrieve user's basket", ex);
            }
        }

        public void RemoveProductFromBasket(int userId, int productId)
        {
            if (userId <= NOUSER)
            {
                throw new ArgumentException("Invalid user ID");
            }
            if (productId <= NOITEM)
            {
                throw new ArgumentException("Invalid product ID");
            }

            try
            {
                // Make the API call to remove product from basket
                var response = httpClient.DeleteAsync($"user/{userId}/product/{productId}").Result;
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex)
            {
                throw new ApplicationException($"Failed to remove product from basket: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Failed to remove product from basket: {ex.Message}", ex);
            }
        }

        public void UpdateProductQuantity(int userId, int productId, int quantity)
        {
            if (userId <= NOUSER)
            {
                throw new ArgumentException("Invalid user ID");
            }
            if (productId <= NOITEM)
            {
                throw new ArgumentException("Invalid product ID");
            }
            if (quantity < NOQUANTITY)
            {
                throw new ArgumentException("Quantity cannot be negative");
            }

            try
            {
                // Apply the maximum quantity limit
                int limitedQuantity = Math.Min(quantity, MaxQuantityPerItem);

                // Make the API call to update product quantity
                var response = httpClient.PutAsJsonAsync($"user/{userId}/product/{productId}", limitedQuantity).Result;
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex)
            {
                throw new ApplicationException($"Failed to update product quantity: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Failed to update product quantity: {ex.Message}", ex);
            }
        }

        public bool ValidateQuantityInput(string quantityText, out int quantity)
        {
            // Initialize output parameter
            quantity = NOQUANTITY;

            // Check if the input is empty
            if (string.IsNullOrWhiteSpace(quantityText))
            {
                return false;
            }

            // Try to parse the input string to an integer
            if (!int.TryParse(quantityText, out quantity))
            {
                return false;
            }

            // Check if quantity is non-negative
            if (quantity < NOQUANTITY)
            {
                return false;
            }

            return true;
        }

        public int GetLimitedQuantity(int quantity)
        {
            return Math.Min(quantity, MaxQuantityPerItem);
        }

        public void ClearBasket(int userId)
        {
            if (userId <= NOUSER)
            {
                throw new ArgumentException("Invalid user ID");
            }

            try
            {
                // Clear the basket through API
                var response = httpClient.DeleteAsync($"user/{userId}/clear").Result;
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex)
            {
                throw new InvalidOperationException($"Could not clear basket: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Could not clear basket: {ex.Message}", ex);
            }
        }

        public bool ValidateBasketBeforeCheckOut(int basketId)
        {
            if (basketId <= NOBASKET)
            {
                throw new ArgumentException("Invalid basket ID");
            }

            try
            {
                // Validate basket through API
                var response = httpClient.GetAsync($"{basketId}/validate").Result;
                response.EnsureSuccessStatusCode();

                var responseContent = response.Content.ReadAsStringAsync().Result;
                Debug.WriteLine($"[Service] ValidateBasketBeforeCheckOut response: {responseContent}");

                try
                {
                    return JsonSerializer.Deserialize<bool>(responseContent, jsonOptions);
                }
                catch (JsonException ex)
                {
                    Debug.WriteLine($"[Service] JSON Error in ValidateBasketBeforeCheckOut: {ex.Message}");
                    // Try a simple parsing approach as fallback
                    responseContent = responseContent.Trim().ToLower();
                    if (responseContent == "true")
                    {
                        return true;
                    }

                    return false;
                }
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"[Service] HTTP Error in ValidateBasketBeforeCheckOut: {ex.Message}");
                throw new InvalidOperationException($"Could not validate basket: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Service] General Error in ValidateBasketBeforeCheckOut: {ex.Message}");
                throw new InvalidOperationException($"Could not validate basket: {ex.Message}", ex);
            }
        }

        public void ApplyPromoCode(int basketId, string code)
        {
            if (basketId <= NOBASKET)
            {
                throw new ArgumentException("Invalid basket ID");
            }
            if (string.IsNullOrWhiteSpace(code))
            {
                throw new ArgumentException("Promo code cannot be empty");
            }

            try
            {
                // Apply promo code through API
                var response = httpClient.PostAsJsonAsync($"{basketId}/promocode", code).Result;

                if (!response.IsSuccessStatusCode)
                {
                    throw new InvalidOperationException("Invalid promo code");
                }
            }
            catch (HttpRequestException ex)
            {
                throw new InvalidOperationException($"Could not apply promo code: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        // Class for deserializing discount rate response
        private class DiscountResponse
        {
            public double DiscountRate { get; set; }
        }

        // Add a new method to get the discount for a promo code
        public double GetPromoCodeDiscount(string code, double subtotal)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return NODISCOUNT;
            }

            try
            {
                // This is just to get the discount rate, we use a temporary basket ID of 1
                // In a real implementation, this should be changed to a dedicated endpoint
                var response = httpClient.PostAsJsonAsync($"1/promocode", code).Result;

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = response.Content.ReadAsStringAsync().Result;
                    var result = JsonSerializer.Deserialize<DiscountResponse>(responseContent, jsonOptions);
                    return subtotal * result.DiscountRate;
                }

                return NODISCOUNT;
            }
            catch
            {
                return NODISCOUNT;
            }
        }

        // Add a new method to calculate basket totals
        public BasketTotals CalculateBasketTotals(int basketId, string promoCode)
        {
            if (basketId <= NOBASKET)
            {
                throw new ArgumentException("Invalid basket ID");
            }

            try
            {
                // Get totals from API
                string endpoint = $"{basketId}/totals";
                if (!string.IsNullOrEmpty(promoCode))
                {
                    endpoint += $"?promoCode={promoCode}";
                }

                var response = httpClient.GetAsync(endpoint).Result;
                response.EnsureSuccessStatusCode();

                var responseContent = response.Content.ReadAsStringAsync().Result;
                Debug.WriteLine($"[Service] CalculateBasketTotals response: {responseContent}");

                try
                {
                    return JsonSerializer.Deserialize<BasketTotals>(responseContent, jsonOptions);
                }
                catch (JsonException ex)
                {
                    Debug.WriteLine($"[Service] JSON Error in CalculateBasketTotals: {ex.Message}");
                    // Create a default totals object
                    return new BasketTotals();
                }
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"[Service] HTTP Error in CalculateBasketTotals: {ex.Message}");
                throw new InvalidOperationException($"Could not calculate basket totals: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Service] General Error in CalculateBasketTotals: {ex.Message}");
                throw new InvalidOperationException($"Could not calculate basket totals: {ex.Message}", ex);
            }
        }

        public void DecreaseProductQuantity(int userId, int productId)
        {
            if (userId <= NOUSER)
            {
                throw new ArgumentException("Invalid user ID");
            }
            if (productId <= NOITEM)
            {
                throw new ArgumentException("Invalid product ID");
            }

            try
            {
                // Decrease quantity through API
                var response = httpClient.PutAsync($"user/{userId}/product/{productId}/decrease", null).Result;
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex)
            {
                throw new InvalidOperationException($"Could not decrease quantity: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Could not decrease quantity: {ex.Message}", ex);
            }
        }

        public void IncreaseProductQuantity(int userId, int productId)
        {
            if (userId <= NOUSER)
            {
                throw new ArgumentException("Invalid user ID");
            }
            if (productId <= NOITEM)
            {
                throw new ArgumentException("Invalid product ID");
            }

            try
            {
                // Increase quantity through API
                var response = httpClient.PutAsync($"user/{userId}/product/{productId}/increase", null).Result;
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex)
            {
                throw new InvalidOperationException($"Could not increase quantity: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Could not increase quantity: {ex.Message}", ex);
            }
        }
    }
}