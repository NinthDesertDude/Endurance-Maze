using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Maze
{
    /// <summary>
    /// Defines a simple region, and visibility color.
    /// </summary>
    public class Region
    {
        /// <summary>
        /// The location of the region in tiles, and its layer.
        /// </summary>
        public (int X, int Y, int Layer) Position;

        /// <summary>
        /// If both are non-null, the region is a rectangle of this width and height.
        /// If Height is null, the region is a circle of this radius.
        /// </summary>
        public (int WidthOrRadius, int? Height) Dimensions;

        /// <summary>
        /// Whether or not this region exists across all layers.
        /// </summary>
        public bool LayerIndependent = false;

        /// <summary>
        /// The color to display with the region, if any. Used by the level editor.
        /// </summary>
        public Color? VisibilityColor;

        /// <summary>
        /// Creates a rectangular region.
        /// </summary>
        public Region(int x, int y, int layer, int width, int height)
        {
            Position = new(x, y, layer);
            Dimensions = new(width, height);
        }

        /// <summary>
        /// Creates a circular region.
        /// </summary>
        public Region(int x, int y, int layer, int radius)
        {
            Position = new(x, y, layer);
            Dimensions = new(radius, null);
        }

        /// <summary>
        /// Counts all tiles that intersect, optionally performing an action for each one, and returns the count.
        /// </summary>
        public int PerIntersect(IEnumerable<GameObj> candidates, Action onIntersect)
        {
            int count = 0;
            var bounds = new SmoothRect(
                Position.X,
                Position.Y,
                Dimensions.WidthOrRadius,
                Dimensions.Height ?? Dimensions.WidthOrRadius);

            foreach (GameObj candidate in candidates)
            {
                if (!LayerIndependent && Position.Layer != candidate.Layer) { continue; }
                if (SmoothRect.ContainsPoint(bounds, candidate.BlockSprite.rectDest.Center))
                {
                    count++;
                    onIntersect?.Invoke();
                }
            }

            return count;
        }

        /// <summary>
        /// Finds all tiles that intersect and returns a list of them.
        /// </summary>
        public IEnumerable<GameObj> FindIntersects(IEnumerable<GameObj> candidates)
        {
            var bounds = new SmoothRect(
                Position.X,
                Position.Y,
                Dimensions.WidthOrRadius,
                Dimensions.Height ?? Dimensions.WidthOrRadius);

            List<GameObj> intersectingObjects = new();
            foreach (GameObj candidate in candidates)
            {
                if (!LayerIndependent && Position.Layer != candidate.Layer) { continue; }
                if (SmoothRect.ContainsPoint(bounds, candidate.BlockSprite.rectDest.Center))
                {
                    intersectingObjects.Add(candidate);
                }
            }

            return intersectingObjects;
        }
    }
}