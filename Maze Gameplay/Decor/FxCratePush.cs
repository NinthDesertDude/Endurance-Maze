using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Maze
{
    /// <summary>
    /// Visual effect for when a crate is pushed by an actor.
    /// </summary>
    public class FxCratePush : GameObj
    {
        //Relevant assets.
        public static Texture2D TexCratePush { get; private set; }

        private SpriteAtlas spriteAtlas;

        /// <summary>Sets the block location and default values.</summary>
        /// <param name="x">The column number.</param>
        /// <param name="y">The row number.</param>
        /// <param name="layer">The layer in the maze.</param>
        public FxCratePush(MainLoop game, int x, int y, int layer)
            : base(game, x, y, layer, true)
        {
            //Sets sprite information.
            BlockSprite = new Sprite(true, TexCratePush);
            BlockSprite.depth = 0.001f;
            BlockSprite.rectSrc = new SmoothRect(0, 0, MainLoop.TileSize, MainLoop.TileSize);
            BlockSprite.rectDest.Width = MainLoop.TileSize;
            BlockSprite.rectDest.Height = MainLoop.TileSize;
            BlockSprite.drawBehavior = SpriteDraw.all;
            spriteAtlas = new SpriteAtlas(BlockSprite, MainLoop.TileSize, MainLoop.TileSize, 4, 1, 4);
            spriteAtlas.CenterOrigin();
            spriteAtlas.frame = 1;
            spriteAtlas.frameSpeed = 0.5f;
            spriteAtlas.frameEndBehavior = FrameEnd.End;
            spriteAtlas.OnAnimationEnd += RemoveAfterPlayback;
        }

        private void RemoveAfterPlayback()
        {
            game.mngrLvl.RemoveItem(this);
        }

        /// <summary>
        /// Loads relevant graphics into memory.
        /// </summary>
        /// <param name="Content">A game content loader.</param>
        public static void LoadContent(ContentManager Content)
        {
            TexCratePush = Content.Load<Texture2D>("Content/Sprites/Game/sprCratePush");
        }

        /// <summary>
        /// Returns a copy of the object.
        /// </summary>
        public override GameObj Clone()
        {
            FxCratePush newBlock = new(game, X, Y, Layer);
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