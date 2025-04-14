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
            IsSynchronized = true;

            //Sets sprite information.
            BlockSprite = new Sprite(true, MazeTeleporter.TexTeleporter);
            BlockSprite.depth = 0.001f;
            BlockSprite.rectSrc = new SmoothRect(0, 0, MainLoop.TileSize, MainLoop.TileSize);
            BlockSprite.rectDest.Width = MainLoop.TileSize;
            BlockSprite.rectDest.Height = MainLoop.TileSize;
            BlockSprite.drawBehavior = SpriteDraw.all;

            spriteAtlas = new SpriteAtlas(BlockSprite, MainLoop.TileSize, MainLoop.TileSize, 7, 1, 7);
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
        /// Returns a copy of the object.
        /// </summary>
        public override GameObj Clone()
        {
            FxTeleported newBlock = new(game, X, Y, Layer, BlockSprite.color);
            newBlock.CopyFrom(this);
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