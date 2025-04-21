using Microsoft.Xna.Framework;

namespace Maze
{
    /// <summary>
    /// A list of all colors used available as ambient lighting with the lighting engine.
    /// </summary>
    public static class LightEngineColor
    {
        public static readonly Color Black = Color.Black;
        public static readonly Color Dark = new(20, 20, 40);
        public static readonly Color Dim = new(62, 62, 81);
        public static readonly Color DimRed = new(81, 62, 62);
        public static readonly Color DimGreen = new(62, 81, 62);
        public static readonly Color Bright = new(180, 180, 180);
        public static readonly Color BrightBlue = new(150, 180, 180);
        public static readonly Color BrightRed = new(180, 150, 150);
        public static readonly Color BrightGreen = new(150, 180, 150);
        public static readonly Color BrightYellow = new(180, 180, 150);

        /// <summary>
        /// Returns colors based on the given index, or Black if there is no match.
        /// </summary>
        public static Color GetColorByIndex(int index)
        {
            return index switch
            {
                0 => Black,
                1 => Dark,
                2 => Dim,
                3 => DimRed,
                4 => DimGreen,
                5 => Bright,
                6 => BrightBlue,
                7 => BrightRed,
                8 => BrightGreen,
                9 => BrightYellow,
                _ => Black,
            };
        }

        /// <summary>
        /// Returns a string containing the name of the color for the given index, or an empty string if no match.
        /// </summary>
        public static string GetColorNameByIndex(int index)
        {
            return index switch
            {
                0 => "Black",
                1 => "Dark",
                2 => "Dim",
                3 => "DimRed",
                4 => "DimGreen",
                5 => "Bright",
                6 => "BrightBlue",
                7 => "BrightRed",
                8 => "BrightGreen",
                9 => "BrightYellow",
                _ => ""
            };
        }

        /// <summary>
        /// Returns an index based on the given color, or -1 if there is no match.
        /// </summary>
        public static int GetIndexByColor(Color color)
        {
            if (color == Black) { return 0; }
            if (color == Dark) { return 1; }
            if (color == Dim) { return 2; }
            if (color == DimRed) { return 3; }
            if (color == DimGreen) { return 4; }
            if (color == Bright) { return 5; }
            if (color == BrightBlue) { return 6; }
            if (color == BrightRed) { return 7; }
            if (color == BrightGreen) { return 8; }
            if (color == BrightYellow) { return 9; }

            return -1;
        }
    }
}