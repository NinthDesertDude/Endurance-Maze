using Microsoft.Xna.Framework;
using System;

namespace EnduranceTheMaze
{
    /// <summary>
    /// Visual effect for when a coin is collected.
    /// Dependencies: MngrLvl for loading sprFx.
    /// </summary>
    public class FxRing : GameObj
    {
        (double, double) xySpeed;
        double xFraction = 0d;
        double yFraction = 0d;
        float decaySpeed;

        /// <summary>Sets the block location and default values.</summary>
        /// <param name="x">The column number.</param>
        /// <param name="y">The row number.</param>
        /// <param name="layer">The layer in the maze.</param>
        public FxRing(MainLoop game, int x, int y, int layer, (double, double) xySpeed, Color color, float decaySpeed = 0.02f)
            : base(game, x, y, layer, true)
        {
            //Sets default values.
            IsSynchronized = false;

            //Sets sprite information.
            BlockSprite = new Sprite(true, MngrLvl.TexFx);
            BlockSprite.depth = 0.001f;
            BlockSprite.rectSrc = new SmoothRect(0, 0, MainLoop.TileSize, MainLoop.TileSize);
            BlockSprite.rectDest.Width = MainLoop.TileSize;
            BlockSprite.rectDest.Height = MainLoop.TileSize;
            BlockSprite.drawBehavior = SpriteDraw.all;

            BlockSprite.scaleX = 0.25f;
            BlockSprite.scaleY = 0.25f;
            BlockSprite.color = color;

            this.xySpeed = xySpeed;
            this.decaySpeed = decaySpeed;
        }

        /// <summary>
        /// Returns a copy of the object.
        /// </summary>
        public override GameObj Clone()
        {
            FxRing newBlock = new(game, X, Y, Layer, xySpeed, BlockSprite.color);
            newBlock.CopyFrom(this);
            return newBlock;
        }

        public override void Update()
        {
            BlockSprite.alpha -= decaySpeed;
            xFraction += xySpeed.Item1;
            yFraction += xySpeed.Item2;

            if (Math.Abs(xFraction) > 1)
            {
                int amountToAdd = (int)xFraction;
                xFraction -= amountToAdd;
                X += amountToAdd;
            }
            if (Math.Abs(yFraction) > 1)
            {
                int amountToAdd = (int)yFraction;
                yFraction -= amountToAdd;
                Y += amountToAdd;
            }

            if (BlockSprite.alpha <= 0)
            {
                game.mngrLvl.RemoveItem(this);
            }

            base.Update();
        }

        public override void Draw()
        {
            base.Draw();
        }
    }
}