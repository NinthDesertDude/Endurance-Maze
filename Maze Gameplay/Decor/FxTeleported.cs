using ImpossiMaze;
using Microsoft.Xna.Framework;

namespace EnduranceTheMaze
{
    /// <summary>
    /// Visual effect for when a teleporter activates.
    /// Dependencies: MazeTeleporter and its texture.
    /// </summary>
    public class FxTeleported : GameObj
    {
        private SpriteAtlas spriteAtlas;

        /// <summary>Sets the block location and default values.</summary>
        /// <param name="x">The column number.</param>
        /// <param name="y">The row number.</param>
        /// <param name="layer">The layer in the maze.</param>
        public FxTeleported(MainLoop game, int x, int y, int layer, Color color)
            : base(game, x, y, layer, true)
        {
            //Sets default values.
            isSynchronized = true;

            //Sets sprite information.
            BlockSprite = new Sprite(true, MazeTeleporter.TexTeleporter);
            BlockSprite.depth = 0.001f;
            BlockSprite.rectSrc = new SmoothRect(0, 0, 32, 32);
            BlockSprite.rectDest.Width = 32;
            BlockSprite.rectDest.Height = 32;
            BlockSprite.drawBehavior = SpriteDraw.all;

            spriteAtlas = new SpriteAtlas(BlockSprite, 32, 32, 7, 1, 7);
            spriteAtlas.frame = 4;
            spriteAtlas.frameSpeed = 0.5f;
            spriteAtlas.frameEndBehavior = FrameEnd.End;
            spriteAtlas.OnAnimationEnd += RemoveAfterPlayback;

            BlockSprite.scaleX = 2;
            BlockSprite.scaleY = 2;
            BlockSprite.color = color;
            BlockSprite.alpha = 0.5f;
            spriteAtlas.CenterOrigin();
        }

        private void RemoveAfterPlayback()
        {
            game.mngrLvl.RemoveItem(this);
        }

        /// <summary>
        /// Returns an exact copy of the object.
        /// </summary>
        public override GameObj Clone()
        {
            //Sets common variables.
            FxTeleported newBlock = new FxTeleported(game, X, Y, Layer, BlockSprite.color);
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
            spriteAtlas.Update(false);
        }

        public override void Draw()
        {
            base.Draw();
        }
    }
}