using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace EnduranceTheMaze
{
    /// <summary>
    /// The user preferences format.
    /// </summary>
    public class Preferences
    {
        /// <summary>
        /// The preferred musicVolume for Song effects.
        /// </summary>
        [JsonPropertyName("VolumeSfx")]
        public float VolumeSfx { get; set; } = 0.4f;

        /// <summary>
        /// The preferred musicVolume for the music.
        /// </summary>
        [JsonPropertyName("VolumeMusic")]
        public float VolumeMusic { get; set; } = 0.3f;

        /// <summary>
        /// The absolute path to the user's custom levels folder, if any is set.
        /// </summary>
        [JsonPropertyName("CustomLevelsPath")]
        public string CustomLevelsPath { get; set; } = "";

        /// <summary>
        /// A list of all completed levels by name.
        /// </summary>
        public List<string> CompletedLevels { get; set; }
    }
}