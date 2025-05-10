using System;

namespace MarketMinds.Shared.Models.Helpers
{
    public static class IdConverter
    {
        /// <summary>
        /// Converts a string ID to an integer
        /// </summary>
        public static int ToInt(string id)
        {
            if (string.IsNullOrEmpty(id))
                return 0;
                
            return int.TryParse(id, out int result) ? result : 0;
        }
        
        /// <summary>
        /// Converts an integer ID to a string
        /// </summary>
        public static string ToString(int id)
        {
            return id.ToString();
        }
        
        /// <summary>
        /// Compares a string ID with an int ID
        /// </summary>
        public static bool Equals(string stringId, int intId)
        {
            return ToInt(stringId) == intId;
        }
        
        /// <summary>
        /// Compares a string ID with an int ID using less than or equal to
        /// </summary>
        public static bool LessThanOrEqual(string stringId, int intId)
        {
            return ToInt(stringId) <= intId;
        }
        
        /// <summary>
        /// Returns defaultValue if the string ID is null or empty
        /// </summary>
        public static int DefaultIfEmpty(string stringId, int defaultValue)
        {
            return string.IsNullOrEmpty(stringId) ? defaultValue : ToInt(stringId);
        }
    }
} 