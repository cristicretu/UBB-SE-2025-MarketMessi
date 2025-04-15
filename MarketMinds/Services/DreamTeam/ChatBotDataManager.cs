using Marketplace_SE.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static Azure.Core.HttpHeader;


namespace Marketplace_SE.Service
{
    public static class ChatBotDataManager
    {
        public static Node LoadTree()
        {
            // Create database connection
            Database.database = new Database(@"Integrated Security=True;TrustServerCertificate=True;data source=DESKTOP-45FVE4D\SQLEXPRESS;initial catalog=Marketplace_SE_UserGetHelp;trusted_connection=true");
            Database.database.Connect();

            // Fetch the data and insert it into each node
            Dictionary<int, Node> nodes = FetchChatBotData();

            // Fetch the relationship data and create the relations
            nodes = LoadRelationships(nodes);

            if (nodes.ContainsKey(1))
            {
                Node root = nodes[1];
                Database.database.Close();
                return root;
            }
            else
            {
                Node root = new Node { ButtonLabel = "", LabelText = "", Response = "Error: Chat has failed to load. Please restart the service." };
                Database.database.Close();
                return root;
            }
        }

        private static Dictionary<int, Node> FetchChatBotData()
        {
            Database.database = new Database(@"Integrated Security=True;TrustServerCertificate=True;data source=DESKTOP-45FVE4D\SQLEXPRESS;initial catalog=Marketplace_SE_UserGetHelp;trusted_connection=true");
            Database.database.Connect();

            // Fetch data for all nodes
            string queryNodes = "SELECT pid, button_label, label_text, response_text FROM ChatBotNodes";
            var nodeRows = Database.database.Get(queryNodes);
            Dictionary<int, Node> nodes = new Dictionary<int, Node>();

            // Create nodes with data
            foreach (var row in nodeRows)
            {
                int id = Convert.ToInt32(row["pid"]);
                string buttonLabel = row["button_label"]?.ToString() ?? string.Empty; // if ToString() returns null we assign an empty string
                string labelText = row["label_text"]?.ToString() ?? string.Empty;
                string response = row["response_text"]?.ToString() ?? string.Empty;

                nodes[id] = new Node
                {
                    Id = id,
                    ButtonLabel = buttonLabel,
                    LabelText = labelText,
                    Response = response
                };
            }

            Database.database.Close();
            return nodes;
        }

        private static Dictionary<int, Node> LoadRelationships(Dictionary<int, Node> nodes)
        {
            Database.database = new Database(@"Integrated Security=True;TrustServerCertificate=True;data source=DESKTOP-45FVE4D\SQLEXPRESS;initial catalog=Marketplace_SE_UserGetHelp;trusted_connection=true");
            Database.database.Connect();

            // Fetch relationship data
            string queryRelationships = "SELECT ParentID, ChildID FROM ChatBotChildren";
            var relRows = Database.database.Get(queryRelationships);

            // Create relationships between nodes
            foreach (var row in relRows)
            {
                int parentId = Convert.ToInt32(row["ParentID"]);
                int childId = Convert.ToInt32(row["ChildID"]);

                if (nodes.ContainsKey(parentId) && nodes.ContainsKey(childId))
                {
                    nodes[parentId].Children.Add(nodes[childId]);
                }
            }

            Database.database.Close();
            return nodes;
        }
    }
}