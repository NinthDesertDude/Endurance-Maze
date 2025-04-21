using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;

namespace Maze
{
    /// <summary>
    /// A hole that can be filled by a crate. Acts like a wall.
    /// 
    /// Activation types: none
    /// 
    /// Custom properties of custInt1: none
    /// Custom properties of custInt2: none
    /// Custom properties of custStr: none
    /// </summary>
    public class MazeCrateHole : GameObj
    {
        //Relevant assets.
        public static SoundEffect sndCrateHole;
        public static Texture2D TexCrateHole { get; private set; }

        private SpriteAtlas spriteAtlas;

        /// <summary>Sets the block location and default values.</summary>
        /// <param name="x">The column number.</param>
        /// <param name="y">The row number.</param>
        /// <param name="layer">The layer in the maze.</param>
        public MazeCrateHole(MainLoop game, int x, int y, int layer)
            : base(game, x, y, layer)
        {
            //Sets default values.
            IsSolid = true;
            BlockType = Type.CrateHole;

            //Sets sprite information.
            BlockSprite = new Sprite(true, TexCrateHole);
            BlockSprite.depth = 0.403f;
            spriteAtlas = new SpriteAtlas(BlockSprite, MainLoop.TileSize, MainLoop.TileSize, 2, 1, 2);

            // Solids generally occlude.
            Lighting = new(null, new(TileHull));
        }

        /// <summary>
        /// Loads relevant graphics into memory.
        /// </summary>
        /// <param name="Content">A game content loader.</param>
        public static void LoadContent(ContentManager Content)
        {
            sndCrateHole = Content.Load<SoundEffect>("Content/Sounds/sndCrateHole");
            TexCrateHole = Content.Load<Texture2D>("Content/Sprites/Game/sprCrateHole");
        }

        /// <summary>
        /// Returns a copy of the object.
        /// </summary>
        public override GameObj Clone()
        {
            MazeCrateHole newBlock = new(game, X, Y, Layer);
            newBlock.CopyFrom(this);

            //Sets custom variables.
            newBlock.BlockSprite = BlockSprite;
            newBlock.spriteAtlas = new SpriteAtlas(spriteAtlas, false);
            return newBlock;
        }

        /// <summary>
        /// Deletes crates on the hole (moved to hole by MngrLvl). Controls
        /// sprite frame used.
        /// </summary>
        public override void Update()
        {
            //If a crate is on the hole, fills it and deletes the crate.
            if (IsSolid)
            {
                //Gets a list of all crates on the hole.
                List<GameObj> items = game.mngrLvl.items.Where(o =>
                    o.X == X && o.Y == Y && o.Layer == Layer &&
                    o.BlockType == Type.Crate).ToList();

                //Removes the first crate and fills the hole.
                if (items.Count != 0)
                {
                    game.mngrLvl.RemoveItem(items[0]);
                    game.playlist.Play(sndCrateHole, X, Y);

                    spriteAtlas.frame = 1;
                    IsSolid = false;
                    UpdateLighting();
                }
            }

            spriteAtlas.Update(true);
            base.Update();
        }

        /// <summary>
        /// Requires the hole to be "solid" to render a hull.
        /// </summary>
        public override void UpdateLighting(bool lightDefaultVis = true, bool shadowDefaultVis = true)
        {
            base.UpdateLighting(lightDefaultVis, shadowDefaultVis && IsSolid);
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
                game.mngrLvl.tooltip += "Hole | ";
            }
        }
    }
}