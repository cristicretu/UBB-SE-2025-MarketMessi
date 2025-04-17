using DataAccessLayer; 
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using server.Models;
using System.Threading.Tasks;

namespace MarketMinds.Repositories.AuctionProductsRepository
{
    public class AuctionProductsRepository : IAuctionProductsRepository
    {
        private readonly DataBaseConnection _connection; 
        private const int BASE_ID = 0;
        private const int BASE_PRICE = 0;

        
        public AuctionProductsRepository(DataBaseConnection connection)
        {
            _connection = connection;
        }

        public List<AuctionProduct> GetProducts()
        {
            List<AuctionProduct> auctions = new List<AuctionProduct>();
            DataTable productsTable = new DataTable();
            SqlConnection sqlConn = _connection.GetConnection(); 

            string mainQuery = @"
            SELECT 
                ap.id, ap.title, ap.description, ap.seller_id, u.username, u.email,
                ap.condition_id, pc.title AS conditionTitle, pc.description AS conditionDescription,
                ap.category_id, cat.title AS categoryTitle, cat.description AS categoryDescription,
                ap.start_datetime, ap.end_datetime, ap.starting_price, ap.current_price
            FROM AuctionProducts ap
            JOIN Users u ON ap.seller_id = u.id
            JOIN ProductConditions pc ON ap.condition_id = pc.id
            JOIN ProductCategories cat ON ap.category_id = cat.id";

            try
            {
                _connection.OpenConnection(); 

                
                using (SqlCommand cmd = new SqlCommand(mainQuery, sqlConn))
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
                    User seller = new User(sellerId, username, email);

                    int conditionId = (int)row["condition_id"];
                    string conditionTitle = (string)row["conditionTitle"];
                    string conditionDescription = (string)row["conditionDescription"];
                    ProductCondition condition = new ProductCondition(conditionId, conditionTitle, conditionDescription);

                    int categoryId = (int)row["category_id"];
                    string categoryTitle = (string)row["categoryTitle"];
                    string categoryDescription = (string)row["categoryDescription"];
                    ProductCategory category = new ProductCategory(categoryId, categoryTitle, categoryDescription);

                    DateTime start = (DateTime)row["start_datetime"];
                    DateTime end = (DateTime)row["end_datetime"];
                    
                    float startingPrice = Convert.ToSingle(row["starting_price"]); 
                    
                    float currentPrice = row["current_price"] == DBNull.Value ? startingPrice : Convert.ToSingle(row["current_price"]);


                    
                    List<ProductTag> tags = GetProductTags(id, sqlConn);
                    List<Image> images = GetImages(id, sqlConn);

                    AuctionProduct auction = new AuctionProduct(
                        id, title, description, seller, condition, category,
                        tags, images, start, end, startingPrice)
                        {
                            
                             CurrentPrice = currentPrice 
                        }; 

                    auctions.Add(auction);
                }
            }
            catch (Exception ex)
            {
                
                Console.WriteLine($"Error in GetProducts: {ex.Message}");
                throw; 
            }
            finally
            {
                _connection.CloseConnection(); 
            }
            return auctions;
        }

        
        private List<Image> GetImages(int productId, SqlConnection sqlConn)
        {
            List<Image> images = new List<Image>();
            string query = "SELECT url FROM AuctionProductsImages WHERE product_id = @ProductId";

            using (SqlCommand cmd = new SqlCommand(query, sqlConn))
            {
                cmd.Parameters.AddWithValue("@ProductId", productId);
                
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string url = reader.GetString(reader.GetOrdinal("url"));
                        images.Add(new Image(url)); 
                    }
                }
            }
            return images;
        }

        
        private List<ProductTag> GetProductTags(int productId, SqlConnection sqlConn)
        {
            List<ProductTag> tags = new List<ProductTag>();
            string query = @"
            SELECT pt.id, pt.title 
            FROM ProductTags pt
            INNER JOIN AuctionProductProductTags apt ON pt.id = apt.tag_id
            WHERE apt.product_id = @ProductId";

            using (SqlCommand cmd = new SqlCommand(query, sqlConn))
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
            return tags;
        }


        public void DeleteProduct(Product product)
        {
             
            if (!(product is AuctionProduct auction)) 
            {
                 throw new ArgumentException("Product must be of type AuctionProduct for deletion.", nameof(product));
            }

            string deleteProductTagsQuery = "DELETE FROM AuctionProductProductTags WHERE product_id = @Id";
            string deleteProductImagesQuery = "DELETE FROM AuctionProductsImages WHERE product_id = @Id";
            string deleteProductQuery = "DELETE FROM AuctionProducts WHERE id = @Id";
            SqlConnection sqlConn = _connection.GetConnection();

            try
            {
                _connection.OpenConnection();
                
                using (SqlTransaction transaction = sqlConn.BeginTransaction())
                {
                    using (SqlCommand cmdTags = new SqlCommand(deleteProductTagsQuery, sqlConn, transaction))
                    {
                         cmdTags.Parameters.AddWithValue("@Id", auction.Id);
                         cmdTags.ExecuteNonQuery();
                    }
                     using (SqlCommand cmdImages = new SqlCommand(deleteProductImagesQuery, sqlConn, transaction))
                    {
                         cmdImages.Parameters.AddWithValue("@Id", auction.Id);
                         cmdImages.ExecuteNonQuery();
                    }
                    using (SqlCommand cmdProduct = new SqlCommand(deleteProductQuery, sqlConn, transaction))
                    {
                        cmdProduct.Parameters.AddWithValue("@Id", auction.Id);
                        int rowsAffected = cmdProduct.ExecuteNonQuery();
                        if (rowsAffected == 0) {
                             transaction.Rollback(); 
                             throw new KeyNotFoundException($"AuctionProduct with ID {auction.Id} not found for deletion.");
                        }
                    }
                    transaction.Commit();
                }
            }
             catch (Exception ex)
            {
                Console.WriteLine($"Error in DeleteProduct: {ex.Message}");
                throw;
            }
            finally
            {
                _connection.CloseConnection();
            }
        }


        public void AddProduct(Product product)
        {
            if (!(product is AuctionProduct auction))
            {
                 throw new ArgumentException("Product must be of type AuctionProduct for adding.", nameof(product));
            }

            string insertProductQuery = @"
            INSERT INTO AuctionProducts 
            (title, description, seller_id, condition_id, category_id, start_datetime, end_datetime, starting_price, current_price)
            VALUES 
            (@Title, @Description, @SellerId, @ConditionId, @CategoryId, @StartDateTime, @EndDateTime, @StartingPrice, @CurrentPrice);
            SELECT SCOPE_IDENTITY();"; 

            string insertTagQuery = @"
            INSERT INTO AuctionProductProductTags (product_id, tag_id)
            VALUES (@ProductId, @TagId)";

            string insertImageQuery = @"
            INSERT INTO AuctionProductsImages (product_id, url)
            VALUES (@ProductId, @Url)";

            SqlConnection sqlConn = _connection.GetConnection();
            int newProductId;

            try
            {
                _connection.OpenConnection();
                using (SqlTransaction transaction = sqlConn.BeginTransaction())
                {
                    
                    using (SqlCommand cmdProduct = new SqlCommand(insertProductQuery, sqlConn, transaction))
                    {
                        cmdProduct.Parameters.AddWithValue("@Title", auction.Title);
                        cmdProduct.Parameters.AddWithValue("@Description", auction.Description);
                        cmdProduct.Parameters.AddWithValue("@SellerId", auction.Seller.Id);
                        cmdProduct.Parameters.AddWithValue("@ConditionId", auction.Condition.Id);
                        cmdProduct.Parameters.AddWithValue("@CategoryId", auction.Category.Id);
                        cmdProduct.Parameters.AddWithValue("@StartDateTime", auction.StartAuctionDate);
                        cmdProduct.Parameters.AddWithValue("@EndDateTime", auction.EndAuctionDate);
                        cmdProduct.Parameters.AddWithValue("@StartingPrice", auction.StartingPrice);
                        
                        cmdProduct.Parameters.AddWithValue("@CurrentPrice", auction.StartingPrice); 

                        object result = cmdProduct.ExecuteScalar(); 
                         if (result == null || result == DBNull.Value) {
                             transaction.Rollback();
                             throw new Exception("Failed to insert auction product and retrieve new ID.");
                         }
                        newProductId = Convert.ToInt32(result);
                        auction.Id = newProductId; 
                    }

                    
                    foreach (var tag in auction.Tags)
                    {
                        using (SqlCommand cmdTag = new SqlCommand(insertTagQuery, sqlConn, transaction))
                        {
                            cmdTag.Parameters.AddWithValue("@ProductId", newProductId);
                            
                            cmdTag.Parameters.AddWithValue("@TagId", tag.Id); 
                            cmdTag.ExecuteNonQuery();
                        }
                    }

                    
                    foreach (var image in auction.Images)
                    {
                        using (SqlCommand cmdImage = new SqlCommand(insertImageQuery, sqlConn, transaction))
                        {
                            cmdImage.Parameters.AddWithValue("@ProductId", newProductId);
                            cmdImage.Parameters.AddWithValue("@Url", image.Url);
                            cmdImage.ExecuteNonQuery();
                        }
                    }
                    transaction.Commit();
                }
            }
             catch (Exception ex)
            {
                Console.WriteLine($"Error in AddProduct: {ex.Message}");
                throw;
            }
            finally
            {
                _connection.CloseConnection();
            }
        }


        public void UpdateProduct(Product product)
        {
             if (!(product is AuctionProduct auction))
            {
                 throw new ArgumentException("Product must be of type AuctionProduct for update.", nameof(product));
            }

             
             
            string query = "UPDATE AuctionProducts SET current_price = @CurrentPrice WHERE Id = @Id"; 
            

            SqlConnection sqlConn = _connection.GetConnection();
             try
            {
                _connection.OpenConnection();
                 using (SqlCommand cmd = new SqlCommand(query, sqlConn))
                {
                    
                    cmd.Parameters.AddWithValue("@CurrentPrice", auction.CurrentPrice); 
                    cmd.Parameters.AddWithValue("@Id", auction.Id);

                    int rowsAffected = cmd.ExecuteNonQuery();
                     if (rowsAffected == 0) {
                         throw new KeyNotFoundException($"AuctionProduct with ID {auction.Id} not found for update.");
                     }
                }
                 
                 
            }
             catch (Exception ex)
            {
                Console.WriteLine($"Error in UpdateProduct: {ex.Message}");
                throw;
            }
            finally
            {
                _connection.CloseConnection();
            }
        }


        public AuctionProduct GetProductByID(int id)
        {
            AuctionProduct? auction = null;
            SqlConnection sqlConn = _connection.GetConnection();

            string query = @"
            SELECT 
                ap.id, ap.title, ap.description, ap.seller_id, u.username, u.email,
                ap.condition_id, pc.title AS conditionTitle, pc.description AS conditionDescription,
                ap.category_id, cat.title AS categoryTitle, cat.description AS categoryDescription,
                ap.start_datetime, ap.end_datetime, ap.starting_price, ap.current_price
            FROM AuctionProducts ap
            JOIN Users u ON ap.seller_id = u.id
            JOIN ProductConditions pc ON ap.condition_id = pc.id
            JOIN ProductCategories cat ON ap.category_id = cat.id
            WHERE ap.id = @APid";
            
            try
            {
                 _connection.OpenConnection();
                 DataRow? productRow = null;
                 DataTable dt = new DataTable();

                 using (SqlCommand cmd = new SqlCommand(query, sqlConn))
                {
                    cmd.Parameters.AddWithValue("@APid", id);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                         dt.Load(reader);
                         if (dt.Rows.Count > 0) {
                              productRow = dt.Rows[0];
                         }
                    }
                }

                if (productRow != null)
                {
                    int productId = (int)productRow["id"]; 
                    string title = (string)productRow["title"];
                    string description = (string)productRow["description"];

                    int sellerId = (int)productRow["seller_id"];
                    string username = (string)productRow["username"];
                    string email = (string)productRow["email"];
                    User seller = new User(sellerId, username, email);

                    int conditionId = (int)productRow["condition_id"];
                    string conditionTitle = (string)productRow["conditionTitle"];
                    string conditionDescription = (string)productRow["conditionDescription"];
                    ProductCondition condition = new ProductCondition(conditionId, conditionTitle, conditionDescription);

                    int categoryId = (int)productRow["category_id"];
                    string categoryTitle = (string)productRow["categoryTitle"];
                    string categoryDescription = (string)productRow["categoryDescription"];
                    ProductCategory category = new ProductCategory(categoryId, categoryTitle, categoryDescription);

                    DateTime start = (DateTime)productRow["start_datetime"];
                    DateTime end = (DateTime)productRow["end_datetime"];
                    float startingPrice = Convert.ToSingle(productRow["starting_price"]);
                    float currentPrice = productRow["current_price"] == DBNull.Value ? startingPrice : Convert.ToSingle(productRow["current_price"]);

                    
                    List<ProductTag> tags = GetProductTags(productId, sqlConn);
                    List<Image> images = GetImages(productId, sqlConn);

                    auction = new AuctionProduct(
                        productId, title, description, seller, condition, category,
                        tags, images, start, end, startingPrice)
                        {
                             CurrentPrice = currentPrice 
                        };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetProductByID: {ex.Message}");
                throw;
            }
             finally
            {
                _connection.CloseConnection();
            }

             if (auction == null) {
                  throw new KeyNotFoundException($"AuctionProduct with ID {id} not found.");
             }
            return auction;
        }
         
        
        
        
        
        
        
    }
} 