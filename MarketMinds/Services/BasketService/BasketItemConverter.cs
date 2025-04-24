using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using DomainLayer.Domain;

namespace MarketMinds.Services.BasketService
{
    /// <summary>
    /// Converter for deserializing basket items from the server to client models
    /// </summary>
    public class BasketItemConverter : JsonConverter<BasketItem>
    {
        public override BasketItem Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                Debug.WriteLine($"[BasketItemConverter] Expected StartObject token, got {reader.TokenType}");
                // Return an empty item instead of throwing
                return new BasketItem();
            }

            var item = new BasketItem();

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    return item;
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
                        case "id":
                            if (reader.TokenType == JsonTokenType.Number)
                            {
                                item.Id = reader.GetInt32();
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
                            }
                            else
                            {
                                reader.Skip();
                            }
                            break;
                        case "product":
                            if (reader.TokenType != JsonTokenType.Null)
                            {
                                if (reader.TokenType == JsonTokenType.StartObject)
                                {
                                    // Manually create and configure a product
                                    var product = new BuyProduct();
                                    item.Product = ReadBuyProduct(ref reader, product);
                                }
                                else
                                {
                                    reader.Skip();
                                }
                            }
                            break;
                        default:
                            reader.Skip();
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[BasketItemConverter] Error reading property {propertyName}: {ex.Message}");
                    try
                    {
                        reader.Skip();
                    }
                    catch
                    {
                        // ignore
                    }
                }
            }

            return item;
        }

        private BuyProduct ReadBuyProduct(ref Utf8JsonReader reader, BuyProduct product)
        {
            product.Tags = new List<ProductTag>();
            product.Images = new List<Image>();

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    return product;
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
                        case "id":
                            if (reader.TokenType == JsonTokenType.Number)
                            {
                                product.Id = reader.GetInt32();
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
                    Debug.WriteLine($"[BasketItemConverter] Error reading product property {propertyName}: {ex.Message}");
                    try
                    {
                        reader.Skip();
                    }
                    catch
                    {
                        // ignore
                    }
                }
            }

            return product;
        }

        public override void Write(Utf8JsonWriter writer, BasketItem value, JsonSerializerOptions options)
        {
            throw new NotImplementedException("Serialization not supported");
        }
    }
}