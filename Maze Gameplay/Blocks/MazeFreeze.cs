using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;

namespace Maze
{
    /// <summary>
    /// Disables whichever actor it touches.
    /// Thaw reactivates all disabled actors.
    /// 
    /// Activation types: none
    /// 
    /// Custom properties of custInt1: none
    /// Custom properties of custInt2: none
    /// Custom properties of custStr: none
    /// </summary>
    public class MazeFreeze : GameObj
    {
        //Relevant assets.
        public static SoundEffect sndFreeze;
        public static Texture2D TexFreeze { get; private set; }

        //Sprite information.    
        private SpriteAtlas spriteAtlas;

        /// <summary>Sets the block location and default values.</summary>
        /// <param name="x">The column number.</param>
        /// <param name="y">The row number.</param>
        /// <param name="layer">The layer in the maze.</param>
        public MazeFreeze(MainLoop game, int x, int y, int layer)
            : base(game, x, y, layer)
        {
            //Sets default values.
            BlockType = Type.Freeze;

            //Sets sprite information.
            BlockSprite = new Sprite(true, TexFreeze);
            BlockSprite.depth = 0.203f;
            BlockSprite.doDrawOffset = true;
            BlockSprite.drawBehavior = SpriteDraw.all;
            spriteAtlas = new SpriteAtlas(BlockSprite, MainLoop.TileSize, MainLoop.TileSize, 10, 1, 10);
            spriteAtlas.frameSpeed = 0.4f;
            spriteAtlas.CenterOrigin();
        }

        /// <summary>
        /// Loads relevant graphics into memory.
        /// </summary>
        /// <param name="Content">A game content loader.</param>
        public static void LoadContent(ContentManager Content)
        {
            sndFreeze = Content.Load<SoundEffect>("Content/Sounds/sndFreeze");
            TexFreeze = Content.Load<Texture2D>("Content/Sprites/Game/sprFreeze");
        }

        /// <summary>
        /// Returns a copy of the object.
        /// </summary>
        public override GameObj Clone()
        {
            MazeFreeze newBlock = new(game, X, Y, Layer);
            newBlock.CopyFrom(this);

            //Custom variables.
            newBlock.BlockSprite = BlockSprite;
            newBlock.spriteAtlas = new SpriteAtlas(spriteAtlas);

            return newBlock;
        }

        /// <summary>
        /// If actors are synced, disables them on touch and deletes itself.
        /// </summary>
        public override void Update()
        {
            //Slowly rotates the sprite.
            BlockSprite.angle += 0.05f;

            //If actors are synchronized.
            if (game.mngrLvl.opSyncActors)
            {
                //Disables the first actor touching this.
                var actor = game.mngrLvl.itemsJustActors.FirstOrDefault(o => o.X == X && o.Y == Y && o.Layer == Layer);
                if (actor != null)
                {
                    actor.IsEnabled = false;
                    game.mngrLvl.RemoveItem(this);
                    game.playlist.Play(sndFreeze, X, Y);
                }
            }

            spriteAtlas.Update(true);
            base.Update();
        }

        /// <summary>
        /// Draws the sprite. Sets an informational tooltip.
        /// </summary>
        public override void Draw()
        {
            base.Draw();

            //Sets the tooltip to display information on hover.
            if (Sprite.IsIntersecting(BlockSprite, new SmoothRect
                (game.mngrLvl.GetCoordsMouse(), 1, 1)) &&
                Layer == game.mngrLvl.actor.Layer)
            {
                game.mngrLvl.tooltip += "Freeze | ";
            }
        }
    }
}