using System;
using System.IO;
using System.Text.Json;

namespace Maze
{
    public static class FileUtils
    {
        private static readonly string userPreferencesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "userpreferences.json");

        /// <summary>
        /// Returns loaded user preferences or saves and returns an empty preferences object if none exists.
        /// Returns null on failure.
        /// </summary>
        public static Preferences LoadPreferences()
        {
            try
            {
                using FileStream stream = new FileStream(userPreferencesPath, FileMode.OpenOrCreate, FileAccess.ReadWrite);

                if (new FileInfo(userPreferencesPath).Length != 0 && File.Exists(userPreferencesPath))
                {
                    return (Preferences)JsonSerializer.Deserialize(stream, typeof(Preferences));
                }

                Preferences prefs = new Preferences();
                JsonSerializer.Serialize(stream, prefs, typeof(Preferences));
                return prefs;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Saves the given user preferences to its expected location, returning true/false for success.
        /// </summary>
        public static bool SavePreferences(Preferences prefs)
        {
            try
            {
                using FileStream stream = new FileStream(userPreferencesPath, FileMode.Create, FileAccess.Write);
                JsonSerializer.Serialize(stream, prefs, typeof(Preferences));
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}