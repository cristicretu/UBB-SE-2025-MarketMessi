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
    private readonly DataBaseConnection dataBaseConnection;
    public ChatBotRepository(DataBaseConnection dataBaseConnection)
    {
        this.dataBaseConnection = dataBaseConnection;
    }
    public Node LoadChatTree()
    {
        Dictionary<int, Node> nodes = new Dictionary<int, Node>();
        try
        {
            dataBaseConnection.OpenConnection();

            nodes = FetchChatBotNodes();

            LoadRelationships(nodes);
        }
        catch (SqlException exception)
        {
            Console.WriteLine($"SQL Error: {exception.Message}");
            return CreateErrorNode("Error: Unable to load chat tree.");
        }
        catch (Exception exception)
        {
            Console.WriteLine($"Error: {exception.Message}");
            return CreateErrorNode("Error: Unable to load chat tree.");
        }
        finally
        {
            dataBaseConnection.CloseConnection();
        }

        if (nodes.TryGetValue(1, out Node rootNode))
        {
            return rootNode;
        }
        else
        {
            return CreateErrorNode("Error: Root node not found.");
        }
    }

    private Dictionary<int, Node> FetchChatBotNodes()
    {
        var nodes = new Dictionary<int, Node>();
        string? queryNodes = "SELECT pid, button_label, label_text, response_text FROM ChatBotNodes";

        using (SqlCommand command = new SqlCommand(queryNodes, dataBaseConnection.GetConnection()))
        using (SqlDataAdapter adapter = new SqlDataAdapter(command))
        using (DataTable dataTable = new DataTable())
        {
            adapter.Fill(dataTable);
            foreach (DataRow row in dataTable.Rows)
            {
                int id = Convert.ToInt32(row["pid"]);
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
        string? queryRelationships = "SELECT parentID, childID FROM ChatBotChildren";

        using (SqlCommand command = new SqlCommand(queryRelationships, dataBaseConnection.GetConnection()))
        using (SqlDataAdapter adapter = new SqlDataAdapter(command))
        using (DataTable dataTable = new DataTable())
        {
            adapter.Fill(dataTable);

            foreach (DataRow row in dataTable.Rows)
            {
                if (!row.IsNull("parentID") && !row.IsNull("childID"))
                {
                    int parentId = Convert.ToInt32(row["parentID"]);
                    int childId = Convert.ToInt32(row["childID"]);

                    if (nodes.TryGetValue(parentId, out Node parentNode) && nodes.TryGetValue(childId, out Node childNode))
                    {
                        parentNode.Children.Add(childNode);
                    }
                    else
                    {
                        Console.WriteLine($"Warning: Relationship ({parentId} -> {childId}) involves a node not found in ChatBotNodes.");
                    }
                }
                else
                {
                    Console.WriteLine($"Warning: Found null parentID or childID in ChatBotChildren relationship.");
                }
            }
        }
    }

    private Node CreateErrorNode(string errorMessage)
    {
        return new Node { Id = -1, ButtonLabel = string.Empty, LabelText = string.Empty, Response = errorMessage };
    }
}
