using DataAccessLayer;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using server.Models;
using System.Threading.Tasks;

namespace MarketMinds.Repositories.BorrowProductsRepository
{
    public class BorrowProductsRepository : IBorrowProductsRepository
    {
        private readonly DataBaseConnection _connection;

        public BorrowProductsRepository(DataBaseConnection connection)
        {
            _connection = connection;
        }

        public List<BorrowProduct> GetProducts()
        {
            var borrows = new List<BorrowProduct>();
            DataTable productsTable = new DataTable();
            SqlConnection sqlConn = _connection.GetConnection();

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
                bp.daily_rate,
                bp.time_limit,
                bp.start_date,
                bp.end_date,
                bp.is_borrowed
            FROM BorrowProducts bp
            JOIN Users u ON bp.seller_id = u.id
            JOIN ProductConditions pc ON bp.condition_id = pc.id
            JOIN ProductCategories cat ON bp.category_id = cat.id";

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

                    double dailyRateDouble = (double)row["daily_rate"];
                    float dailyRate = (float)dailyRateDouble;

                    DateTime timeLimit = (DateTime)row["time_limit"];
                    DateTime startDate = (DateTime)row["start_date"];
                    DateTime endDate = (DateTime)row["end_date"];
                    bool isBorrowed = (bool)row["is_borrowed"];

                    List<ProductTag> tags = GetProductTags(id);
                    List<Image> images = GetProductImages(id);

                    BorrowProduct borrow = new BorrowProduct(
                        id, title, description, seller, condition,
                        category, tags, images, timeLimit,
                        startDate, endDate, dailyRate, isBorrowed);
                    borrows.Add(borrow);
                }
            }
            finally
            {
                _connection.CloseConnection();
            }

            return borrows;
        }

        private List<ProductTag> GetProductTags(int productId)
        {
            var tags = new List<ProductTag>();

            string query = @"
            SELECT pt.id, pt.title
            FROM ProductTags pt
            INNER JOIN BorrowProductProductTags bpt ON pt.id = bpt.tag_id
            WHERE bpt.product_id = @ProductId";

            try
            {
                _connection.OpenConnection();
                using (SqlCommand cmd = new SqlCommand(query, _connection.GetConnection()))
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
            finally
            {
                _connection.CloseConnection();
            }

            return tags;
        }

        private List<Image> GetProductImages(int productId)
        {
            var images = new List<Image>();

            string query = @"
            SELECT url
            FROM BorrowProductImages
            WHERE product_id = @ProductId";

            try
            {
                _connection.OpenConnection();
                using (SqlCommand cmd = new SqlCommand(query, _connection.GetConnection()))
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
            }
            finally
            {
                _connection.CloseConnection();
            }

            return images;
        }

        public BorrowProduct GetProductByID(int productId)
        {
            BorrowProduct? borrow = null;
            SqlConnection sqlConn = _connection.GetConnection();

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
                bp.daily_rate,
                bp.time_limit,
                bp.start_date,
                bp.end_date,
                bp.is_borrowed
            FROM BorrowProducts bp
            JOIN Users u ON bp.seller_id = u.id
            JOIN ProductConditions pc ON bp.condition_id = pc.id
            JOIN ProductCategories cat ON bp.category_id = cat.id
            WHERE bp.id = @productID";

            try
            {
                _connection.OpenConnection();
                DataRow? productRow = null;
                DataTable dt = new DataTable();

                using (SqlCommand cmd = new SqlCommand(query, sqlConn))
                {
                    cmd.Parameters.AddWithValue("@productID", productId);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        dt.Load(reader);
                        if (dt.Rows.Count > 0)
                        {
                            productRow = dt.Rows[0];
                        }
                    }
                }

                if (productRow != null)
                {
                    int id = (int)productRow["id"];
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

                    float dailyRate = (float)(double)productRow["daily_rate"];

                    DateTime timeLimit = (DateTime)productRow["time_limit"];
                    DateTime startDate = (DateTime)productRow["start_date"];
                    DateTime endDate = (DateTime)productRow["end_date"];
                    bool isBorrowed = (bool)productRow["is_borrowed"];

                    List<ProductTag> tags = GetProductTags(id);
                    List<Image> images = GetProductImages(id);

                    borrow = new BorrowProduct(
                        id,
                        title,
                        description,
                        seller,
                        condition,
                        category,
                        tags,
                        images,
                        timeLimit,
                        startDate,
                        endDate,
                        dailyRate,
                        isBorrowed);
                }
            }
            finally
            {
                _connection.CloseConnection();
            }

            return borrow!;
        }

        public void AddProduct(BorrowProduct product)
        {
            if (product == null)
            {
                throw new ArgumentException("Product must not be null.");
            }

            string insertProductQuery = @"
            INSERT INTO BorrowProducts 
            (title, description, seller_id, condition_id, category_id, time_limit, start_date, end_date, daily_rate, is_borrowed)
            VALUES 
            (@Title, @Description, @SellerId, @ConditionId, @CategoryId, @TimeLimit, @StartDate, @EndDate, @DailyRate, @IsBorrowed);
            SELECT SCOPE_IDENTITY();";

            SqlConnection sqlConn = _connection.GetConnection();
            try
            {
                _connection.OpenConnection();
                using (SqlTransaction transaction = sqlConn.BeginTransaction())
                {
                    int newProductId;
                    using (SqlCommand cmdProduct = new SqlCommand(insertProductQuery, sqlConn, transaction))
                    {
                        cmdProduct.Parameters.AddWithValue("@Title", product.Title);
                        cmdProduct.Parameters.AddWithValue("@Description", product.Description);
                        cmdProduct.Parameters.AddWithValue("@SellerId", product.Seller.Id);
                        cmdProduct.Parameters.AddWithValue("@ConditionId", product.Condition.Id);
                        cmdProduct.Parameters.AddWithValue("@CategoryId", product.Category.Id);
                        cmdProduct.Parameters.AddWithValue("@TimeLimit", product.TimeLimit);
                        cmdProduct.Parameters.AddWithValue("@StartDate", product.StartDate);
                        cmdProduct.Parameters.AddWithValue("@EndDate", product.EndDate);
                        cmdProduct.Parameters.AddWithValue("@DailyRate", product.DailyRate);
                        cmdProduct.Parameters.AddWithValue("@IsBorrowed", product.IsBorrowed);

                        object result = cmdProduct.ExecuteScalar();
                        if (result == null || result == DBNull.Value)
                        {
                            transaction.Rollback();
                            throw new Exception("Failed to insert borrow product and retrieve new ID.");
                        }
                        newProductId = Convert.ToInt32(result);
                        product.Id = newProductId;
                    }

                    foreach (var tag in product.Tags)
                    {
                        string insertTagQuery = @"
                        INSERT INTO BorrowProductProductTags (product_id, tag_id)
                        VALUES (@ProductId, @TagId)";

                        using (SqlCommand cmd = new SqlCommand(insertTagQuery, sqlConn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@ProductId", newProductId);
                            cmd.Parameters.AddWithValue("@TagId", tag.Id);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    foreach (var image in product.Images)
                    {
                        string insertImageQuery = @"
                        INSERT INTO BorrowProductImages (url, product_id)
                        VALUES (@Url, @ProductId)";

                        using (SqlCommand cmd = new SqlCommand(insertImageQuery, sqlConn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@Url", image.Url);
                            cmd.Parameters.AddWithValue("@ProductId", newProductId);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    transaction.Commit();
                }
            }
            finally
            {
                _connection.CloseConnection();
            }
        }

        public void DeleteProduct(BorrowProduct product)
        {
            if (product == null)
            {
                throw new ArgumentException("Product must not be null.");
            }

            SqlConnection sqlConn = _connection.GetConnection();
            try
            {
                _connection.OpenConnection();
                using (SqlTransaction transaction = sqlConn.BeginTransaction())
                {
                    // Delete associated images
                    string deleteImagesQuery = "DELETE FROM BorrowProductImages WHERE product_id = @ProductId";
                    using (SqlCommand cmd = new SqlCommand(deleteImagesQuery, sqlConn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@ProductId", product.Id);
                        cmd.ExecuteNonQuery();
                    }

                    // Delete associated tags
                    string deleteTagsQuery = "DELETE FROM BorrowProductProductTags WHERE product_id = @ProductId";
                    using (SqlCommand cmd = new SqlCommand(deleteTagsQuery, sqlConn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@ProductId", product.Id);
                        cmd.ExecuteNonQuery();
                    }

                    // Delete the product
                    string deleteProductQuery = "DELETE FROM BorrowProducts WHERE id = @ProductId";
                    using (SqlCommand cmd = new SqlCommand(deleteProductQuery, sqlConn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@ProductId", product.Id);
                        cmd.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }
            }
            finally
            {
                _connection.CloseConnection();
            }
        }

        public void UpdateProduct(BorrowProduct product)
        {
            if (product == null)
            {
                throw new ArgumentException("Product must not be null.");
            }

            string updateQuery = @"
            UPDATE BorrowProducts 
            SET title = @Title,
                description = @Description,
                seller_id = @SellerId,
                condition_id = @ConditionId,
                category_id = @CategoryId,
                time_limit = @TimeLimit,
                start_date = @StartDate,
                end_date = @EndDate,
                daily_rate = @DailyRate,
                is_borrowed = @IsBorrowed
            WHERE id = @Id";

            try
            {
                _connection.OpenConnection();
                using (SqlCommand cmd = new SqlCommand(updateQuery, _connection.GetConnection()))
                {
                    cmd.Parameters.AddWithValue("@Id", product.Id);
                    cmd.Parameters.AddWithValue("@Title", product.Title);
                    cmd.Parameters.AddWithValue("@Description", product.Description);
                    cmd.Parameters.AddWithValue("@SellerId", product.Seller.Id);
                    cmd.Parameters.AddWithValue("@ConditionId", product.Condition.Id);
                    cmd.Parameters.AddWithValue("@CategoryId", product.Category.Id);
                    cmd.Parameters.AddWithValue("@TimeLimit", product.TimeLimit);
                    cmd.Parameters.AddWithValue("@StartDate", product.StartDate);
                    cmd.Parameters.AddWithValue("@EndDate", product.EndDate);
                    cmd.Parameters.AddWithValue("@DailyRate", product.DailyRate);
                    cmd.Parameters.AddWithValue("@IsBorrowed", product.IsBorrowed);

                    cmd.ExecuteNonQuery();
                }
            }
            finally
            {
                _connection.CloseConnection();
            }
        }
    }
} 