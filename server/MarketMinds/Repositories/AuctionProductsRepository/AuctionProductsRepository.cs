using DataAccessLayer; 
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using server.Models;
using System.Threading.Tasks;
using server.DataAccessLayer;
using Microsoft.EntityFrameworkCore;

namespace MarketMinds.Repositories.AuctionProductsRepository
{
    public class AuctionProductsRepository : IAuctionProductsRepository
    {
        private readonly DataBaseConnection _connection; 
        private readonly ApplicationDbContext _context;
        private const int BASE_ID = 0;
        private const int BASE_PRICE = 0;

        
        public AuctionProductsRepository(DataBaseConnection connection, ApplicationDbContext context)
        {
            _connection = connection;
            _context = context;
        }

        public List<AuctionProduct> GetProducts()
        {
            try
            {
                // Use Entity Framework to get all auction products with related data
                var products = _context.AuctionProducts
                    .Include(p => p.Seller)
                    .Include(p => p.Condition)
                    .Include(p => p.Category)
                    .Include(p => p.Bids)
                        .ThenInclude(b => b.Bidder)
                    .Include(p => p.Images)
                    .ToList();

                return products;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetProducts using EF: {ex.Message}");
                throw;
            }
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


        public void DeleteProduct(AuctionProduct product)
        {
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
                         cmdTags.Parameters.AddWithValue("@Id", product.Id);
                         cmdTags.ExecuteNonQuery();
                    }
                     using (SqlCommand cmdImages = new SqlCommand(deleteProductImagesQuery, sqlConn, transaction))
                    {
                         cmdImages.Parameters.AddWithValue("@Id", product.Id);
                         cmdImages.ExecuteNonQuery();
                    }
                    using (SqlCommand cmdProduct = new SqlCommand(deleteProductQuery, sqlConn, transaction))
                    {
                        cmdProduct.Parameters.AddWithValue("@Id", product.Id);
                        int rowsAffected = cmdProduct.ExecuteNonQuery();
                        if (rowsAffected == 0) {
                             transaction.Rollback(); 
                             throw new KeyNotFoundException($"AuctionProduct with ID {product.Id} not found for deletion.");
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


        public void AddProduct(AuctionProduct product)
        {
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
                        cmdProduct.Parameters.AddWithValue("@Title", product.Title);
                        cmdProduct.Parameters.AddWithValue("@Description", product.Description ?? (object)DBNull.Value);
                        cmdProduct.Parameters.AddWithValue("@SellerId", product.SellerId);
                        cmdProduct.Parameters.AddWithValue("@ConditionId", product.ConditionId ?? (object)DBNull.Value);
                        cmdProduct.Parameters.AddWithValue("@CategoryId", product.CategoryId ?? (object)DBNull.Value);
                        cmdProduct.Parameters.AddWithValue("@StartDateTime", product.StartTime);
                        cmdProduct.Parameters.AddWithValue("@EndDateTime", product.EndTime);
                        cmdProduct.Parameters.AddWithValue("@StartingPrice", product.StartPrice);
                        cmdProduct.Parameters.AddWithValue("@CurrentPrice", product.StartPrice);

                        object result = cmdProduct.ExecuteScalar(); 
                        if (result == null || result == DBNull.Value) {
                            transaction.Rollback();
                            throw new Exception("Failed to insert auction product and retrieve new ID.");
                        }
                        newProductId = Convert.ToInt32(result);
                        product.Id = newProductId; 
                    }

                    // Handle image references
                    if (product.Images != null)
                    {
                        foreach (var image in product.Images)
                        {
                            using (SqlCommand cmdImage = new SqlCommand(insertImageQuery, sqlConn, transaction))
                            {
                                cmdImage.Parameters.AddWithValue("@ProductId", newProductId);
                                cmdImage.Parameters.AddWithValue("@Url", image.Url);
                                cmdImage.ExecuteNonQuery();
                            }
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


        public void UpdateProduct(AuctionProduct product)
        {
            string query = "UPDATE AuctionProducts SET current_price = @CurrentPrice WHERE Id = @Id"; 
            SqlConnection sqlConn = _connection.GetConnection();
            
            try
            {
                _connection.OpenConnection();
                using (SqlCommand cmd = new SqlCommand(query, sqlConn))
                {
                    cmd.Parameters.AddWithValue("@CurrentPrice", product.CurrentPrice); 
                    cmd.Parameters.AddWithValue("@Id", product.Id);

                    int rowsAffected = cmd.ExecuteNonQuery();
                    if (rowsAffected == 0) {
                        throw new KeyNotFoundException($"AuctionProduct with ID {product.Id} not found for update.");
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
            try
            {
                // Use Entity Framework to get the auction product by ID with related data
                var product = _context.AuctionProducts
                    .Include(p => p.Seller)
                    .Include(p => p.Condition)
                    .Include(p => p.Category)
                    .Include(p => p.Bids)
                        .ThenInclude(b => b.Bidder)
                    .Include(p => p.Images)
                    .FirstOrDefault(p => p.Id == id);

                if (product == null)
                {
                    throw new KeyNotFoundException($"AuctionProduct with ID {id} not found.");
                }

                return product;
            }
            catch (KeyNotFoundException)
            {
                // Rethrow KeyNotFoundException to be handled by the controller
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetProductByID using EF: {ex.Message}");
                throw;
            }
        }
         
        
        
        
        
        
        
    }
} 