using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccessLayer;
using DomainLayer.Domain;
using Microsoft.Data.SqlClient;

namespace MarketMinds.Repositories.ChatBotRepository;

public class ChatBotRepository : IChatBotRepository
{
    private readonly DataBaseConnection dbConnection;
    public ChatBotRepository(DataBaseConnection databaseConnection)
    {
        dbConnection = databaseConnection;
    }
    public Node LoadChatTree()
    {
        Dictionary<int, Node> nodes = new Dictionary<int, Node>();
        try
        {
            dbConnection.OpenConnection();

            nodes = FetchChatBotNodes();

            LoadRelationships(nodes);
        }
        catch (SqlException ex)
        {
            // Handle SQL exceptions
            Console.WriteLine($"SQL Error: {ex.Message}");
            return CreateErrorNode("Error: Unable to load chat tree.");
        }
        catch (Exception ex)
        {
            // Handle general exceptions
            Console.WriteLine($"Error: {ex.Message}");
            return CreateErrorNode("Error: Unable to load chat tree.");
        }
        finally
        {
            dbConnection.CloseConnection();
        }

        if (nodes.TryGetValue(1, out Node rootNode))
        {
            return rootNode;
        }
        else
        {
            // Handle case where root node is not found
            return CreateErrorNode("Error: Root node not found.");
        }
    }

    private Dictionary<int, Node> FetchChatBotNodes()
    {
        var nodes = new Dictionary<int, Node>();
        string? queryNodes = "SELECT pid, button_label, label_text, response_text FROM ChatBotNodes";

        // Use using statements for disposable objects (SqlCommand, SqlDataAdapter, DataTable)
        using (SqlCommand command = new SqlCommand(queryNodes, dbConnection.GetConnection()))
        using (SqlDataAdapter adapter = new SqlDataAdapter(command))
        using (DataTable dataTable = new DataTable())
        {
            // Fill the DataTable
            adapter.Fill(dataTable);

            // Create nodes with data
            foreach (DataRow row in dataTable.Rows)
            {
                int id = Convert.ToInt32(row["pid"]);
                // Use IsDBNull check for robustness before converting/accessing
                string? buttonLabel = row.IsNull("button_label") ? string.Empty : row["button_label"].ToString();
                string? labelText = row.IsNull("label_text") ? string.Empty : row["label_text"].ToString();
                string? response = row.IsNull("response_text") ? string.Empty : row["response_text"].ToString();

                nodes[id] = new Node
                {
                    Id = id,
                    ButtonLabel = buttonLabel,
                    LabelText = labelText,
                    Response = response
                };
            }
        }
        return nodes;
    }

    private void LoadRelationships(Dictionary<int, Node> nodes)
    {
        string? queryRelationships = "SELECT ParentID, ChildID FROM ChatBotChildren";

        using (SqlCommand command = new SqlCommand(queryRelationships, dbConnection.GetConnection()))
        using (SqlDataAdapter adapter = new SqlDataAdapter(command))
        using (DataTable dataTable = new DataTable())
        {
            adapter.Fill(dataTable);

            // Create relationships between nodes
            foreach (DataRow row in dataTable.Rows)
            {
                // Ensure ParentID and ChildID are not null before converting
                if (!row.IsNull("ParentID") && !row.IsNull("ChildID"))
                {
                    int parentId = Convert.ToInt32(row["ParentID"]);
                    int childId = Convert.ToInt32(row["ChildID"]);

                    // Check if both parent and child nodes were successfully loaded previously
                    if (nodes.TryGetValue(parentId, out Node parentNode) && nodes.TryGetValue(childId, out Node childNode))
                    {
                        parentNode.Children.Add(childNode);
                    }
                    else
                    {
                        // Log a warning or handle cases where relationships point to non-existent nodes
                        Console.WriteLine($"Warning: Relationship ({parentId} -> {childId}) involves a node not found in ChatBotNodes.");
                    }
                }
                else
                {
                    Console.WriteLine($"Warning: Found null ParentID or ChildID in ChatBotChildren relationship.");
                }
            }
        }
        // No need to return nodes, modifications are done on the objects within the dictionary
    }

    private Node CreateErrorNode(string errorMessage)
    {
        return new Node { Id = -1, ButtonLabel = string.Empty, LabelText = string.Empty, Response = errorMessage };
    }
}
