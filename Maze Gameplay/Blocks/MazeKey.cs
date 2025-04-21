using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;

namespace Maze
{
    /// <summary>
    /// A colored key that corresponds with colored locks. Pick them up and
    /// move into locks to "unlock" (delete) them.
    /// 
    /// Activation types: none
    /// 
    /// Custom properties of custInt1: Key colors:
    /// 0: Blue
    /// 1: Red
    /// 2: Goldenrod
    /// 3: Purple
    /// 4: Orange
    /// 5: Black
    /// 6: Dark blue
    /// 7: Dark red
    /// 8: Dark goldenrod
    /// 9: Dark orange
    /// Custom properties of custInt2: none
    /// Custom properties of custStr: none
    /// </summary>
    public class MazeKey : GameObj
    {
        //Relevant assets.
        public static SoundEffect sndCollectKey;
        public static Texture2D TexKey { get; private set; }

        //Sprite information.    
        private SpriteAtlas spriteAtlas;

        /// <summary>Sets the block location and default values.</summary>
        /// <param name="x">The column number.</param>
        /// <param name="y">The row number.</param>
        /// <param name="layer">The layer in the maze.</param>
        public MazeKey(MainLoop game, int x, int y, int layer)
            : base(game, x, y, layer)
        {
            //Sets default values.
            BlockType = Type.Key;

            //Sets sprite information.
            BlockSprite = new Sprite(true, TexKey);
            BlockSprite.depth = 0.207f;
            BlockSprite.drawBehavior = SpriteDraw.all;
            spriteAtlas = new SpriteAtlas(BlockSprite, MainLoop.TileSize, MainLoop.TileSize, 19, 2, 10);
            spriteAtlas.frameSpeed = 0.2f;
        }

        /// <summary>
        /// Loads relevant graphics into memory.
        /// </summary>
        /// <param name="Content">A game content loader.</param>
        public static void LoadContent(ContentManager Content)
        {
            sndCollectKey = Content.Load<SoundEffect>("Content/Sounds/sndCollectKey");
            TexKey = Content.Load<Texture2D>("Content/Sprites/Game/sprKey");
        }

        /// <summary>
        /// Returns a copy of the object.
        /// </summary>
        public override GameObj Clone()
        {
            MazeKey newBlock = new(game, X, Y, Layer);
            newBlock.CopyFrom(this);

            //Sets specific variables.
            newBlock.BlockSprite = BlockSprite;
            newBlock.spriteAtlas = new SpriteAtlas(spriteAtlas, false);
            return newBlock;
        }

        /// <summary>
        /// Adds to the actor's list of keys on contact.
        /// </summary>
        public override void Update()
        {
            BlockSprite.color = KeyColor.GetColorByIndex(SlotValueInt1);

            // Adds to the list of keys the first actor to touch it has.
            var firstActor = game.mngrLvl.itemsJustActors.FirstOrDefault(o => o.X == X && o.Y == Y && o.Layer == Layer);
            if (firstActor != null)
            {
                firstActor.keys.Add(BlockSprite.color);
                game.mngrLvl.RemoveItem(this);
                game.playlist.Play(sndCollectKey, X, Y);

                // Animation for picking up the key
                FxPickup pickup;
                int sparkles = 4 + Utils.Rng.Next(4);
                for (int i = 0; i < sparkles; i++)
                {
                    pickup = new FxPickup(game, X * MainLoop.TileSize, Y * MainLoop.TileSize, Layer, BlockSprite.color);
                    pickup.X += -MainLoop.TileSizeHalf + Utils.Rng.Next(MainLoop.TileSize);
                    pickup.Y -= Utils.Rng.Next(20);
                    game.mngrLvl.AddItem(pickup);
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
                game.mngrLvl.tooltip += "Key | ";
            }
        }
    }
}