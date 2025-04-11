using Microsoft.Xna.Framework;

namespace EnduranceTheMaze
{
    /// <summary>
    /// Visual effect for when a coin is collected.
    /// Dependencies: MngrLvl for loading sprFx.
    /// </summary>
    public class FxPickup : GameObj
    {
        /// <summary>Sets the block location and default values.</summary>
        /// <param name="x">The column number.</param>
        /// <param name="y">The row number.</param>
        /// <param name="layer">The layer in the maze.</param>
        public FxPickup(MainLoop game, int x, int y, int layer, Color color)
            : base(game, x, y, layer, true)
        {
            //Sets default values.
            isSynchronized = false;

            //Sets sprite information.
            BlockSprite = new Sprite(true, MngrLvl.TexFx);
            BlockSprite.depth = 0.001f;
            BlockSprite.rectSrc = new SmoothRect(MainLoop.TileSize, 0, MainLoop.TileSize, MainLoop.TileSize);
            BlockSprite.rectDest.Width = MainLoop.TileSize;
            BlockSprite.rectDest.Height = MainLoop.TileSize;
            BlockSprite.drawBehavior = SpriteDraw.all;

            float randomSize = Utils.Rng.Next(0, 2) / 10f;
            BlockSprite.scaleX = 0.3f + randomSize;
            BlockSprite.scaleY = 0.3f + randomSize;
            BlockSprite.color = color;
        }

        /// <summary>
        /// Returns an exact copy of the object.
        /// </summary>
        public override GameObj Clone()
        {
            //Sets common variables.
            FxPickup newBlock = new FxPickup(game, X, Y, Layer, BlockSprite.color);
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
            BlockSprite.alpha -= 0.1f;
            Y -= 2;

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