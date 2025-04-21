using Microsoft.Xna.Framework;

namespace Maze
{
    /// <summary>
    /// A list of all colors used by key objects.
    /// </summary>
    public static class KeyColor
    {
        /// <summary>
        /// Returns colors based on the given index, or White if there is no match.
        /// </summary>
        public static Color GetColorByIndex(int index)
        {
            return index switch
            {
                0 => Color.Blue,
                1 => Color.Red,
                2 => Color.Goldenrod,
                3 => Color.Purple,
                4 => Color.Orange,
                5 => Color.Black,
                6 => Color.DarkBlue,
                7 => Color.DarkRed,
                8 => Color.DarkGoldenrod,
                9 => Color.DarkOrange,
                _ => Color.Blue
            };
        }

        /// <summary>
        /// Returns a string containing the color's name based on the given index, or an empty string if no match.
        /// </summary>
        public static string GetColorNameByIndex(int index)
        {
            return index switch
            {
                0 => "Blue",
                1 => "Red",
                2 => "Yellow",
                3 => "Purple",
                4 => "Orange",
                5 => "Black",
                6 => "Dark blue",
                7 => "Dark red",
                8 => "Dark yellow",
                9 => "Dark orange",
                _ => ""
            };
        }

        /// <summary>
        /// Returns an index based on the given color, or -1 if there is no match.
        /// </summary>
        public static int GetIndexByColor(Color color)
        {
            if (color == Color.Blue) { return 0; }
            if (color == Color.Red) { return 1; }
            if (color == Color.Goldenrod) { return 2; }
            if (color == Color.Purple) { return 3; }
            if (color == Color.Orange) { return 4; }
            if (color == Color.Black) { return 5; }
            if (color == Color.DarkBlue) { return 6; }
            if (color == Color.DarkRed) { return 7; }
            if (color == Color.DarkGoldenrod) { return 8; }
            if (color == Color.DarkOrange) { return 9; }

            return -1;
        }
    }
}