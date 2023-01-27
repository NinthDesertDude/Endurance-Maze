using ImpossiMaze;
using Microsoft.Xna.Framework;

namespace EnduranceTheMaze
{
    /// <summary>
    /// Visual effect for when a receiver is used.
    /// Dependencies: MngrLvl for the sprFx texture.
    /// </summary>
    public class FxReceived : GameObj
    {
        private SpriteAtlas spriteAtlas;

        /// <summary>Sets the block location and default values.</summary>
        /// <param name="x">The column number.</param>
        /// <param name="y">The row number.</param>
        /// <param name="layer">The layer in the maze.</param>
        public FxReceived(MainLoop game, int x, int y, int layer)
            : base(game, x, y, layer)
        {
            //Sets default values.
            isSynchronized = true;

            //Sets sprite information.
            BlockSprite = new Sprite(true, MngrLvl.TexFx);
            BlockSprite.depth = 0.001f;
            BlockSprite.rectSrc = new SmoothRect(0, 0, 32, 32);
            BlockSprite.rectDest.Width = 32;
            BlockSprite.rectDest.Height = 32;
            BlockSprite.drawBehavior = SpriteDraw.all;

            spriteAtlas = new SpriteAtlas(BlockSprite, 32, 32, 2, 1, 2);

            BlockSprite.scaleX = 0.1f;
            BlockSprite.scaleY = 0.1f;
            BlockSprite.alpha = 0.5f;
            spriteAtlas.CenterOrigin();
        }

        /// <summary>
        /// Returns an exact copy of the object.
        /// </summary>
        public override GameObj Clone()
        {
            //Sets common variables.
            FxReceived newBlock = new FxReceived(game, X, Y, Layer);
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
            base.Update();

            BlockSprite.scaleX *= 1.5f;
            BlockSprite.scaleY *= 1.5f;

            if (BlockSprite.scaleX >= 0.8f)
            {
                game.mngrLvl.RemoveDecor(this);
                return;
            }

            spriteAtlas.Update(false);
        }

        public override void Draw()
        {
            base.Draw();
        }
    }
}