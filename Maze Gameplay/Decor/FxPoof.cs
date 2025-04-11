using Microsoft.Xna.Framework;
using System;

namespace EnduranceTheMaze
{
    /// <summary>
    /// Visual effect for when a coin is collected.
    /// Dependencies: MngrLvl for loading sprFx.
    /// </summary>
    public class FxPoof : GameObj
    {
        readonly float alphaDegradeSpeed;
        (double, double) xySpeed = (0, 0);
        double xFraction = 0d;
        double yFraction = 0d;

        /// <summary>Sets the block location and default values.</summary>
        /// <param name="x">The column number.</param>
        /// <param name="y">The row number.</param>
        /// <param name="layer">The layer in the maze.</param>
        public FxPoof(MainLoop game, int x, int y, int layer, Color color, (double, double) xySpeed)
            : base(game, x, y, layer, true)
        {
            //Sets default values.
            isSynchronized = false;

            //Sets sprite information.
            BlockSprite = new Sprite(true, MngrLvl.TexFx);
            BlockSprite.depth = 0.001f;
            BlockSprite.rectSrc = new SmoothRect(MainLoop.TileSize * 2, 0, MainLoop.TileSize, MainLoop.TileSize);
            BlockSprite.rectDest.Width = MainLoop.TileSize;
            BlockSprite.rectDest.Height = MainLoop.TileSize;
            BlockSprite.drawBehavior = SpriteDraw.all;

            alphaDegradeSpeed = 0.05f + (Utils.Rng.Next(3) / 20f);
            this.xySpeed = xySpeed;

            float randomSize = Utils.Rng.Next(0, 2) / 10f;
            BlockSprite.scaleX = 0.6f + randomSize;
            BlockSprite.scaleY = 0.6f + randomSize;
            BlockSprite.color = color;
            BlockSprite.CenterOrigin();
        }

        /// <summary>
        /// Returns an exact copy of the object.
        /// </summary>
        public override GameObj Clone()
        {
            //Sets common variables.
            FxPoof newBlock = new FxPoof(game, X, Y, Layer, BlockSprite.color, xySpeed);
            newBlock.ActionIndex = ActionIndex;
            newBlock.ActionIndex2 = ActionIndex2;
            newBlock.ActionType = ActionType;
            newBlock.CustInt1 = CustInt1;
            newBlock.CustInt2 = CustInt2;
            newBlock.CustStr = CustStr;
            newBlock.BlockDir = BlockDir;
            newBlock.IsActivated = IsActivated;
            newBlock.IsEnabled = IsEnabled;
            newBlock.IsVisible = IsVisible;
            return newBlock;
        }

        public override void Update()
        {
            BlockSprite.alpha -= alphaDegradeSpeed;
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