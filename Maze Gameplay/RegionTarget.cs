using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Maze
{
    /// <summary>
    /// Defines a simple region, and visibility color.
    /// </summary>
    public static class RegionTarget
    {
        /// <summary>
        /// Properties by name.
        /// </summary>
        public enum Property
        {
            /// <summary>
            /// X and Y grid position, which is multiplied by tile size. Edits to position cannot
            /// disalign non-grid-aligned objects.
            /// </summary>
            Position = 0,
            IsVisible = 1,
            IsEnabled = 2,
            SlotValueInt1 = 3,
            SlotValueInt2 = 4,
            SlotValueString = 5,
            IsActive = 6,
            BlockDir = 7,
            SignalListenChannel = 8,
            SignalSendChannel = 9,

            /// <summary>
            /// Vector angle, in radians. For blocks with any non-grid-aligned angle property.
            /// </summary>
            MotionAngle = 10,

            /// <summary>
            /// Vector speed, in radians. For blocks with any non-grid-aligned speed property.
            /// </summary>
            MotionSpeed = 11
        }

        /// <summary>
        /// Associates properties to their type.
        /// </summary>
        public static readonly Dictionary<Property, System.Type> PropertyTypes = new()
        {
            { Property.Position, typeof(Vector2) },
            { Property.IsVisible, typeof(bool) },
            { Property.IsEnabled, typeof(bool) },
            { Property.SlotValueInt1, typeof(int) },
            { Property.SlotValueInt2, typeof(int) },
            { Property.SlotValueString, typeof(string) },
            { Property.IsActive, typeof(bool) },
            { Property.BlockDir, typeof(Dir) },
            { Property.SignalListenChannel, typeof(int) },
            { Property.SignalSendChannel, typeof(int) },
        };
    }    
}