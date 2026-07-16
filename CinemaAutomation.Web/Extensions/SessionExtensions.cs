using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace CinemaAutomation.Web
{
    public static class SessionExtensions // Extension methods for ISession
    {
        // Stores a complex object in session by serializing it to JSON
        public static void SetObject<T>(this ISession session, string key, T value)
        {
            var json = JsonSerializer.Serialize(value); // Serialize object to JSON string
            session.SetString(key, json); // Save JSON string into session
        }

        // Retrieves a complex object from session by deserializing JSON
        public static T? GetObject<T>(this ISession session, string key)
        {
            var json = session.GetString(key); // Read JSON string from session
            if (json == null) // Check if session key does not exist
                return default; // Return default value if missing

            return JsonSerializer.Deserialize<T>(json); // Deserialize JSON back to object
        }
    }
}


