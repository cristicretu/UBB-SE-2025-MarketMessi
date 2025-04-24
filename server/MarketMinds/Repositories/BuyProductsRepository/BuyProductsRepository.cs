using System.Collections.Generic;
using System;
using System.Data;
using Microsoft.Data.SqlClient;
using server.Models;
using DataAccessLayer;

namespace MarketMinds.Repositories.BuyProductsRepository
{
    public class BuyProductsRepository: IBuyProductsRepository
    {
        private readonly DataBaseConnection connection;

        public BuyProductsRepository(DataBaseConnection connection)
        {
            this.connection = connection ?? throw new ArgumentNullException(nameof(connection));
        }

        public void AddProduct(BuyProduct product)
        {
            if (product == null)
            {
                throw new ArgumentNullException(nameof(product));
            }

            string insertProductQuery = @"
            INSERT INTO BuyProducts 
            (title, description, seller_id, condition_id, category_id, price)
            VALUES 
            (@Title, @Description, @SellerId, @ConditionId, @CategoryId, @Price);
            SELECT SCOPE_IDENTITY();";

            connection.OpenConnection();

            try
            {
                int newProductId;
                using (SqlCommand cmd = new SqlCommand(insertProductQuery, connection.GetConnection()))
                {
                    cmd.Parameters.AddWithValue("@Title", product.Title);
                    cmd.Parameters.AddWithValue("@Description", product.Description);
                    cmd.Parameters.AddWithValue("@SellerId", product.Seller.Id);
                    cmd.Parameters.AddWithValue("@ConditionId", product.Condition.Id);
                    cmd.Parameters.AddWithValue("@CategoryId", product.Category.Id);
                    cmd.Parameters.AddWithValue("@Price", product.Price);

                    object result = cmd.ExecuteScalar();
                    newProductId = Convert.ToInt32(result);
                }

                if (product.ProductTags != null)
                {
                    foreach (var tag in product.ProductTags)
                    {
                        string insertTagQuery = @"
                        INSERT INTO BuyProductProductTags (product_id, tag_id)
                        VALUES (@ProductId, @TagId)";

                        using (SqlCommand cmd = new SqlCommand(insertTagQuery, connection.GetConnection()))
                        {
                            cmd.Parameters.AddWithValue("@ProductId", newProductId);
                            cmd.Parameters.AddWithValue("@TagId", tag.Id);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }

                if (product.Images != null)
                {
                    foreach (var image in product.Images)
                    {
                        string insertImageQuery = @"
                        INSERT INTO BuyProductImages (url, product_id)
                        VALUES (@Url, @ProductId)";

                        using (SqlCommand cmd = new SqlCommand(insertImageQuery, connection.GetConnection()))
                        {
                            cmd.Parameters.AddWithValue("@Url", image.Url);
                            cmd.Parameters.AddWithValue("@ProductId", newProductId);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to add product to the database", ex);
            }
            finally
            {
                connection.CloseConnection();
            }
        }

        public void DeleteProduct(BuyProduct product)
        {
            if (product == null)
            {
                throw new ArgumentNullException(nameof(product));
            }

            string query = "DELETE FROM BuyProducts WHERE id = @Id";

            connection.OpenConnection();
            try
            {
                using (SqlCommand cmd = new SqlCommand(query, connection.GetConnection()))
                {
                    cmd.Parameters.AddWithValue("@Id", product.Id);
                    int affectedRows = cmd.ExecuteNonQuery();
                    
                    if (affectedRows == 0)
                    {
                        throw new KeyNotFoundException($"Product with ID {product.Id} not found.");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to delete product from the database", ex);
            }
            finally
            {
                connection.CloseConnection();
            }
        }

        public List<BuyProduct> GetProducts()
        {
            List<BuyProduct> products = new List<BuyProduct>();
            DataTable productsTable = new DataTable();

            string mainQuery = @"
            SELECT 
                bp.id,
                bp.title,
                bp.description,
                bp.seller_id,
                u.username,
                u.email,
                bp.condition_id,
                pc.title AS conditionTitle,
                pc.description AS conditionDescription,
                bp.category_id,
                cat.title AS categoryTitle,
                cat.description AS categoryDescription,
                bp.price
            FROM BuyProducts bp
            JOIN Users u ON bp.seller_id = u.id
            JOIN ProductConditions pc ON bp.condition_id = pc.id
            JOIN ProductCategories cat ON bp.category_id = cat.id";

            connection.OpenConnection();
            try
            {
                using (SqlCommand cmd = new SqlCommand(mainQuery, connection.GetConnection()))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    productsTable.Load(reader);
                }

                foreach (DataRow row in productsTable.Rows)
                {
                    int id = (int)row["id"];
                    string title = (string)row["title"];
                    string description = (string)row["description"];

                    int sellerId = (int)row["seller_id"];
                    string username = (string)row["username"];
                    string email = (string)row["email"];
                    User seller = new User { Id = sellerId, Username = username, Email = email };

                    int conditionId = (int)row["condition_id"];
                    string conditionTitle = (string)row["conditionTitle"];
                    string conditionDescription = (string)row["conditionDescription"];
                    Condition condition = new Condition(conditionTitle) { Id = conditionId };

                    int categoryId = (int)row["category_id"];
                    string categoryTitle = (string)row["categoryTitle"];
                    string categoryDescription = (string)row["categoryDescription"];
                    Category category = new Category(categoryTitle, categoryDescription) { Id = categoryId };

                    double priceDouble = (double)row["price"];
                    float price = (float)priceDouble;

                    BuyProduct product = new BuyProduct(
                        title: title,
                        description: description,
                        sellerId: sellerId,
                        conditionId: conditionId,
                        categoryId: categoryId,
                        price: price);
                    product.Id = id;
                    product.Seller = seller;
                    product.Condition = condition;
                    product.Category = category;

                    List<ProductTag> tags = GetProductTags(id);
                    List<BuyProductImage> images = GetProductImages(id);

                    foreach (var tag in tags)
                    {
                        product.ProductTags.Add(new BuyProductProductTag { Product = product, Tag = tag });
                    }
                    
                    foreach (var image in images)
                    {
                        product.Images.Add(image);
                    }

                    products.Add(product);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to retrieve products from the database", ex);
            }
            finally
            {
                connection.CloseConnection();
            }

            return products;
        }

        private List<ProductTag> GetProductTags(int productId)
        {
            var tags = new List<ProductTag>();

            string query = @"
            SELECT pt.id, pt.title
            FROM ProductTags pt
            INNER JOIN BuyProductProductTags bpt ON pt.id = bpt.tag_id
            WHERE bpt.product_id = @ProductId";

            connection.OpenConnection();
            try
            {
                using (SqlCommand cmd = new SqlCommand(query, connection.GetConnection()))
                {
                    cmd.Parameters.AddWithValue("@ProductId", productId);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int tagId = reader.GetInt32(reader.GetOrdinal("id"));
                            string tagTitle = reader.GetString(reader.GetOrdinal("title"));
                            tags.Add(new ProductTag(tagId, tagTitle));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to retrieve tags for product {productId}", ex);
            }
            finally
            {
                connection.CloseConnection();
            }
            return tags;
        }

        private List<BuyProductImage> GetProductImages(int productId)
        {
            var images = new List<BuyProductImage>();

            string query = @"
            SELECT url
            FROM BuyProductImages
            WHERE product_id = @ProductId";

            connection.OpenConnection();
            try
            {
                using (SqlCommand cmd = new SqlCommand(query, connection.GetConnection()))
                {
                    cmd.Parameters.AddWithValue("@ProductId", productId);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string url = reader.GetString(reader.GetOrdinal("url"));
                            images.Add(new BuyProductImage { Url = url });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to retrieve images for product {productId}", ex);
            }
            finally
            {
                connection.CloseConnection();
            }
            return images;
        }

        public BuyProduct GetProductByID(int productId)
        {
            string query = @"
            SELECT 
                bp.id,
                bp.title,
                bp.description,
                bp.seller_id,
                u.username,
                u.email,
                bp.condition_id,
                pc.title AS conditionTitle,
                pc.description AS conditionDescription,
                bp.category_id,
                cat.title AS categoryTitle,
                cat.description AS categoryDescription,
                bp.price
            FROM BuyProducts bp
            JOIN Users u ON bp.seller_id = u.id
            JOIN ProductConditions pc ON bp.condition_id = pc.id
            JOIN ProductCategories cat ON bp.category_id = cat.id
            WHERE bp.id = @ProductId";

            connection.OpenConnection();
            try
            {
                using (SqlCommand cmd = new SqlCommand(query, connection.GetConnection()))
                {
                    cmd.Parameters.AddWithValue("@ProductId", productId);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            throw new KeyNotFoundException($"Product with ID {productId} not found.");
                        }

                        int id = reader.GetInt32(reader.GetOrdinal("id"));
                        string title = reader.GetString(reader.GetOrdinal("title"));
                        string description = reader.GetString(reader.GetOrdinal("description"));

                        int sellerId = reader.GetInt32(reader.GetOrdinal("seller_id"));
                        string username = reader.GetString(reader.GetOrdinal("username"));
                        string email = reader.GetString(reader.GetOrdinal("email"));
                        User seller = new User { Id = sellerId, Username = username, Email = email };

                        int conditionId = reader.GetInt32(reader.GetOrdinal("condition_id"));
                        string conditionTitle = reader.GetString(reader.GetOrdinal("conditionTitle"));
                        string conditionDescription = reader.GetString(reader.GetOrdinal("conditionDescription"));
                        Condition condition = new Condition(conditionTitle) { Id = conditionId };

                        int categoryId = reader.GetInt32(reader.GetOrdinal("category_id"));
                        string categoryTitle = reader.GetString(reader.GetOrdinal("categoryTitle"));
                        string categoryDescription = reader.GetString(reader.GetOrdinal("categoryDescription"));
                        Category category = new Category(categoryTitle, categoryDescription) { Id = categoryId };

                        double priceDouble = reader.GetDouble(reader.GetOrdinal("price"));
                        float price = (float)priceDouble;

                        BuyProduct product = new BuyProduct(
                            title: title,
                            description: description,
                            sellerId: sellerId,
                            conditionId: conditionId,
                            categoryId: categoryId,
                            price: price);
                        product.Id = id;
                        product.Seller = seller;
                        product.Condition = condition;
                        product.Category = category;

                        List<ProductTag> tags = GetProductTags(id);
                        List<BuyProductImage> images = GetProductImages(id);

                        foreach (var tag in tags)
                        {
                            product.ProductTags.Add(new BuyProductProductTag { Product = product, Tag = tag });
                        }
                        
                        foreach (var image in images)
                        {
                            product.Images.Add(image);
                        }

                        return product;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to retrieve product with ID {productId}", ex);
            }
            finally
            {
                connection.CloseConnection();
            }
        }

        public void UpdateProduct(BuyProduct product)
        {
            if (product == null)
            {
                throw new ArgumentNullException(nameof(product));
            }

            string query = @"
            UPDATE BuyProducts 
            SET title = @Title,
                description = @Description,
                seller_id = @SellerId,
                condition_id = @ConditionId,
                category_id = @CategoryId,
                price = @Price
            WHERE id = @Id";

            connection.OpenConnection();
            try
            {
                using (SqlCommand cmd = new SqlCommand(query, connection.GetConnection()))
                {
                    cmd.Parameters.AddWithValue("@Id", product.Id);
                    cmd.Parameters.AddWithValue("@Title", product.Title);
                    cmd.Parameters.AddWithValue("@Description", product.Description);
                    cmd.Parameters.AddWithValue("@SellerId", product.Seller.Id);
                    cmd.Parameters.AddWithValue("@ConditionId", product.Condition.Id);
                    cmd.Parameters.AddWithValue("@CategoryId", product.Category.Id);
                    cmd.Parameters.AddWithValue("@Price", product.Price);

                    int affectedRows = cmd.ExecuteNonQuery();
                    if (affectedRows == 0)
                    {
                        throw new KeyNotFoundException($"Product with ID {product.Id} not found.");
                    }
                }

                // Update tags
                string deleteTagsQuery = "DELETE FROM BuyProductProductTags WHERE product_id = @ProductId";
                using (SqlCommand cmd = new SqlCommand(deleteTagsQuery, connection.GetConnection()))
                {
                    cmd.Parameters.AddWithValue("@ProductId", product.Id);
                    cmd.ExecuteNonQuery();
                }

                if (product.ProductTags != null)
                {
                    foreach (var tag in product.ProductTags)
                    {
                        string insertTagQuery = @"
                        INSERT INTO BuyProductProductTags (product_id, tag_id)
                        VALUES (@ProductId, @TagId)";

                        using (SqlCommand cmd = new SqlCommand(insertTagQuery, connection.GetConnection()))
                        {
                            cmd.Parameters.AddWithValue("@ProductId", product.Id);
                            cmd.Parameters.AddWithValue("@TagId", tag.Id);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }

                // Update images
                string deleteImagesQuery = "DELETE FROM BuyProductImages WHERE product_id = @ProductId";
                using (SqlCommand cmd = new SqlCommand(deleteImagesQuery, connection.GetConnection()))
                {
                    cmd.Parameters.AddWithValue("@ProductId", product.Id);
                    cmd.ExecuteNonQuery();
                }

                if (product.Images != null)
                {
                    foreach (var image in product.Images)
                    {
                        string insertImageQuery = @"
                        INSERT INTO BuyProductImages (url, product_id)
                        VALUES (@Url, @ProductId)";

                        using (SqlCommand cmd = new SqlCommand(insertImageQuery, connection.GetConnection()))
                        {
                            cmd.Parameters.AddWithValue("@Url", image.Url);
                            cmd.Parameters.AddWithValue("@ProductId", product.Id);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to update product with ID {product.Id}", ex);
            }
            finally
            {
                connection.CloseConnection();
            }
        }
    }
}
