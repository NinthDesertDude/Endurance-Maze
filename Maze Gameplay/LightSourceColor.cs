using Microsoft.Xna.Framework;

namespace Maze
{
    /// <summary>
    /// A list of all colors used by object light sources in the game.
    /// </summary>
    public static class LightSourceColor
    {
        public static readonly int Count = 32;
        public static readonly Color White = new(255, 255, 255);
        public static readonly Color BrightPink = new(255, 191, 207);
        public static readonly Color Peach = new(255, 223, 191);
        public static readonly Color PalestYellow = new(255, 255, 191);
        public static readonly Color PaleGreen = new(191, 255, 191);
        public static readonly Color PalestAqua = new(191, 255, 255);
        public static readonly Color PaleBlue = new(191, 191, 255);
        public static readonly Color Salmon = new(255, 130, 130);
        public static readonly Color PaleOrange = new(255, 192, 130);
        public static readonly Color PaleYellow = new(255, 255, 130);
        public static readonly Color PaleYellowGreen = new (192, 255, 130);
        public static readonly Color SpringGreen = new(130, 250, 130);
        public static readonly Color PaleAqua = new(130, 255, 255);
        public static readonly Color LightBlue = new(130, 161, 255);
        public static readonly Color LightBluePurple = new (161, 130, 255);
        public static readonly Color Lavender = new(192, 130, 255);
        public static readonly Color Rose = new(255, 130, 161);
        public static readonly Color RosyRed = new(255, 130, 130);
        public static readonly Color Red = new(255, 0, 0);
        public static readonly Color Orange = new(255, 127, 0);
        public static readonly Color Gold = new(255, 191, 0);
        public static readonly Color Yellow = new(255, 255, 0);
        public static readonly Color YellowGreen = new (191,255,0);
        public static readonly Color Green = new(0, 255, 0);
        public static readonly Color Turquoise = new(0, 255, 191);
        public static readonly Color Aqua = new(0, 255, 255);
        public static readonly Color MediumBlue = new(0, 127, 255);
        public static readonly Color Blue = new(0, 0, 255);
        public static readonly Color Purple = new(127, 0, 255);
        public static readonly Color BrightPurple = new(191, 0, 255);
        public static readonly Color Fuchsia = new(255, 0, 255);
        public static readonly Color BurntPink = new(255, 0, 127);

        /// <summary>
        /// Returns colors based on the given index, or White if there is no match.
        /// </summary>
        public static Color GetColorByIndex(int index)
        {
            return index switch
            {
                0 => White,
                1 => BrightPink,
                2 => Peach,
                3 => PalestYellow,
                4 => PaleGreen,
                5 => PalestAqua,
                6 => PaleBlue,
                7 => Salmon,
                8 => PaleOrange,
                9 => PaleYellow,
                10 => PaleYellowGreen,
                11 => SpringGreen,
                12 => PaleAqua,
                13 => LightBlue,
                14 => LightBluePurple,
                15 => Lavender,
                16 => Rose,
                17 => RosyRed,
                18 => Red,
                19 => Orange,
                20 => Gold,
                21 => Yellow,
                22 => YellowGreen,
                23 => Green,
                24 => Turquoise,
                25 => Aqua,
                26 => MediumBlue,
                27 => Blue,
                28 => Purple,
                29 => BrightPurple,
                30 => Fuchsia,
                31 => BurntPink,
                _ => White,
            };
        }

        /// <summary>
        /// Returns a string containing the name of the color for the given index, or an empty string if no match.
        /// </summary>
        public static string GetColorNameByIndex(int index)
        {
            return index switch
            {
                 0 => "White",
                 1 => "Bright Pink",
                 2 => "Peach",
                 3 => "Palest Yellow",
                 4 => "Pale Green",
                 5 => "Palest Aqua",
                 6 => "Pale Blue",
                 7 => "Salmon",
                 8 => "Pale Orange",
                 9 => "Pale Yellow",
                10 => "Pale Yellow-green",
                11 => "Spring Green",
                12 => "Pale Aqua",
                13 => "Light Blue",
                14 => "Light Blue-purple",
                15 => "Lavender",
                16 => "Rose",
                17 => "Rosy Red",
                18 => "Red",
                19 => "Orange",
                20 => "Gold",
                21 => "Yellow",
                22 => "Yellow Green",
                23 => "Green",
                24 => "Turquoise",
                25 => "Aqua",
                26 => "Medium Blue",
                27 => "Blue",
                28 => "Purple",
                29 => "Bright Purple",
                30 => "Fuchsia",
                31 => "Burnt Pink",
                _ => "",
            };
        }

        /// <summary>
        /// Returns an index based on the given color, or -1 if there is no match.
        /// </summary>
        public static int GetIndexByColor(Color color)
        {
            if (color == White) { return 0; }
            if (color == BrightPink) { return 1; }
            if (color == Peach) { return 2; }
            if (color == PalestYellow) { return 3; }
            if (color == PaleGreen) { return 4; }
            if (color == PalestAqua) { return 5; }
            if (color == PaleBlue) { return 6; }
            if (color == Salmon) { return 7; }
            if (color == PaleOrange) { return 8; }
            if (color == PaleYellow) { return 9; }
            if (color == PaleYellowGreen) { return 10; }
            if (color == SpringGreen) { return 11; }
            if (color == PaleAqua) { return 12; }
            if (color == LightBlue) { return 13; }
            if (color == LightBluePurple) { return 14; }
            if (color == Lavender) { return 15; }
            if (color == Rose) { return 16; }
            if (color == RosyRed) { return 17; }
            if (color == Red) { return 18; }
            if (color == Orange) { return 19; }
            if (color == Gold) { return 20; }
            if (color == Yellow) { return 21; }
            if (color == YellowGreen) { return 22; }
            if (color == Green) { return 23; }
            if (color == Turquoise) { return 24; }
            if (color == Aqua) { return 25; }
            if (color == MediumBlue) { return 26; }
            if (color == Blue) { return 27; }
            if (color == Purple) { return 28; }
            if (color == BrightPurple) { return 29; }
            if (color == Fuchsia) { return 30; }
            if (color == BurntPink) { return 31; }

            return -1;
        }
    }
}