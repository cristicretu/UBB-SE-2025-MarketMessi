using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccessLayer;
using DomainLayer.Domain;
using Marketplace_SE.Data;
using Microsoft.Data.SqlClient;

namespace MarketMinds.Repositories.ChatRepository;

public class ChatRepository : IChatRepository
{
    private readonly DataBaseConnection dataBaseConnection;

    public ChatRepository(DataBaseConnection dataBaseConnection)
    {
        this.dataBaseConnection = dataBaseConnection;
    }

    public Conversation? GetConversation(int userId1, int userId2)
    {
        Conversation conversation = null;

        string getConversationQuery = @"
        SELECT id, user1, user2 
        FROM Conversations
        WHERE (user1 = @UserId1 AND user2 = @UserId2) OR (user1 = @UserId2 AND user2 = @UserId1)";

        try
        {
            dataBaseConnection.OpenConnection();
            using (SqlCommand getConversationCommand = new SqlCommand(getConversationQuery, dataBaseConnection.GetConnection()))
            {
                getConversationCommand.Parameters.AddWithValue("@UserId1", userId1);
                getConversationCommand.Parameters.AddWithValue("@UserId2", userId2);

                using (SqlDataReader reader = getConversationCommand.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        conversation = MapReaderToConversation(reader);
                    }
                }
            }
        }
        catch (Exception exception)
        {
            throw new Exception($"Error retrieving conversation: {exception.Message}");
        }
        finally
        {
            dataBaseConnection.CloseConnection();
        }

        return conversation;
    }

    public Conversation CreateConversation(int userId1, int userId2)
    {
        Conversation newConversation = null;
        string createConversationQuery = @"
        INSERT INTO Conversations (user1, user2) 
        VALUES (@UserId1, @UserId2)";
        try
        {
            dataBaseConnection.OpenConnection();
            using (SqlCommand createConversationCommand = new SqlCommand(createConversationQuery, dataBaseConnection.GetConnection()))
            {
                createConversationCommand.Parameters.AddWithValue("@UserId1", userId1);
                createConversationCommand.Parameters.AddWithValue("@UserId2", userId2);

                using (SqlDataReader reader = createConversationCommand.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        int conversationId = Convert.ToInt32(reader[0]);
                        newConversation = MapReaderToConversation(reader);
                    }
                }
            }
        }
        catch (Exception exception)
        {
            throw new Exception($"Error creating conversation: {exception.Message}");
        }
        finally
        {
            dataBaseConnection.CloseConnection();
        }
        return newConversation;
    }

    public List<Message> GetMessages(int conversationId, long sinceTimestamp = 0)
    {
        List<Message> messages = new List<Message>();
        string getMessagesQuery = @"
        SELECT id, conversationId, creator, timestamp, contentType, content
        FROM Messages
        WHERE ConversationId = @ConversationId";

        if (sinceTimestamp > 0)
        {
            getMessagesQuery += " AND Timestamp > @SinceTimestamp";
        }

        getMessagesQuery += " ORDER BY Timestamp ASC";

        try
        {
            dataBaseConnection.OpenConnection();
            using (SqlCommand getMessagesCommand = new SqlCommand(getMessagesQuery, dataBaseConnection.GetConnection()))
            {
                getMessagesCommand.Parameters.AddWithValue("@ConversationId", conversationId);
                getMessagesCommand.Parameters.AddWithValue("@SinceTimestamp", sinceTimestamp);

                using (SqlDataReader reader = getMessagesCommand.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        messages.Add(MapReaderToMessage(reader));
                    }
                }
            }
        }
        catch (Exception exception)
        {
            throw new Exception($"Error retrieving messages: {exception.Message}");
        }
        finally
        {
            dataBaseConnection.CloseConnection();
        }
        return messages;
    }

    public bool AddMessage(Message message)
    {
        string addMessageQuery = @"
        INSERT INTO Messages (conversationId, creator, timestamp, contentType, content) 
        VALUES (@ConversationId, @Creator, @Timestamp, @ContentType, @Content)";

        int rowsAffected = 0;

        try
        {
            dataBaseConnection.OpenConnection();
            using (SqlCommand addMessageCommand = new SqlCommand(addMessageQuery, dataBaseConnection.GetConnection()))
            {
                addMessageCommand.Parameters.AddWithValue("@ConversationId", message.ConversationId);
                addMessageCommand.Parameters.AddWithValue("@Creator", message.Creator);
                addMessageCommand.Parameters.AddWithValue("@Timestamp", message.Timestamp);
                addMessageCommand.Parameters.AddWithValue("@ContentType", message.ContentType);
                addMessageCommand.Parameters.AddWithValue("@Content", message.Content);

                rowsAffected = addMessageCommand.ExecuteNonQuery();
            }
        }
        catch (Exception exception)
        {
            throw new Exception($"Error adding message: {exception.Message}");
        }
        finally
        {
            dataBaseConnection.CloseConnection();
        }

        return rowsAffected > 0;
    }

    // --- Helper Mapping Methods ---
    private Conversation MapReaderToConversation(SqlDataReader reader)
    {
        return new Conversation
        {
            Id = reader.GetInt32(reader.GetOrdinal("id")),
            UserId1 = reader.GetInt32(reader.GetOrdinal("user1")),
            UserId2 = reader.GetInt32(reader.GetOrdinal("user2"))
        };
    }

    private Message MapReaderToMessage(SqlDataReader reader)
    {
        return new Message
        {
            Id = reader.GetInt32(reader.GetOrdinal("id")),
            ConversationId = reader.GetInt32(reader.GetOrdinal("conversationId")),
            Creator = reader.GetInt32(reader.GetOrdinal("creator")),
            Timestamp = reader.GetInt64(reader.GetOrdinal("timestamp")),
            ContentType = reader.GetString(reader.GetOrdinal("contentType")),
            Content = reader.GetString(reader.GetOrdinal("content"))
        };
    }
}
