using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;

namespace EnduranceTheMaze
{
    /// <summary>
    /// Adds to the level score.
    /// 
    /// Activation types: none
    /// 
    /// Custom properties of custInt1: none
    /// Custom properties of custInt2: none
    /// Custom properties of custStr: none
    /// </summary>
    public class MazeCoin : GameObj
    {
        //Relevant assets.
        public static SoundEffect sndCollectCoin;
        public static Texture2D TexCoin { get; private set; }

        //Sprite information.    
        private SpriteAtlas spriteAtlas;

        /// <summary>Sets the block location and default values.</summary>
        /// <param name="x">The column number.</param>
        /// <param name="y">The row number.</param>
        /// <param name="layer">The layer in the maze.</param>
        public MazeCoin(MainLoop game, int x, int y, int layer)
            : base(game, x, y, layer)
        {
            //Sets default values.
            BlockType = Type.Coin;

            //Sets sprite information.
            BlockSprite = new Sprite(true, TexCoin);
            BlockSprite.depth = 0.205f;
            spriteAtlas = new SpriteAtlas(BlockSprite, MainLoop.TileSize, MainLoop.TileSize, 19, 2, 10);
            spriteAtlas.frameSpeed = 0.2f;
        }

        /// <summary>
        /// Loads relevant graphics into memory.
        /// </summary>
        /// <param name="Content">A game content loader.</param>
        public static void LoadContent(ContentManager Content)
        {
            sndCollectCoin = Content.Load<SoundEffect>("Content/Sounds/sndCollectCoin");
            TexCoin = Content.Load<Texture2D>("Content/Sprites/Game/sprCoin");
        }

        /// <summary>
        /// Returns a copy of the object.
        /// </summary>
        public override GameObj Clone()
        {
            MazeCoin newBlock = new(game, X, Y, Layer);
            newBlock.CopyFrom(this);

            //Sets specific variables.
            newBlock.BlockSprite = BlockSprite;
            newBlock.spriteAtlas = new SpriteAtlas(spriteAtlas, false);
            return newBlock;
        }

        /// <summary>
        /// Adds to the score and deletes itself on contact.
        /// </summary>
        public override void Update()
        {
            //If there is at least one actor touching the coin.
            if (game.mngrLvl.itemsJustActors.Any(o => o.X == X && o.Y == Y && o.Layer == Layer))
            {
                game.mngrLvl.ActorCoins++;
                game.mngrLvl.RemoveItem(this);
                game.playlist.Play(sndCollectCoin, X, Y);

                // Animation for picking up a coin
                FxPickup pickup;
                int sparkles = 4 + Utils.Rng.Next(4);
                for (int i = 0; i < sparkles; i++)
                {
                    pickup = new FxPickup(game, X * MainLoop.TileSize, Y * MainLoop.TileSize, Layer, new Microsoft.Xna.Framework.Color(255, 255, 0));
                    pickup.X += -MainLoop.TileSizeHalf + Utils.Rng.Next(MainLoop.TileSize);
                    pickup.Y -= Utils.Rng.Next(10);
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
                game.mngrLvl.tooltip += "Coin | ";
            }
        }
    }
}