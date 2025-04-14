using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EnduranceTheMaze
{
    /// <summary>
    /// Adds to the colliding actor's health.
    /// 
    /// Activation types: none
    /// 
    /// Custom properties of custInt1: none
    /// Custom properties of custInt2: none
    /// Custom properties of custStr: none
    /// </summary>
    public class MazeHealth : GameObj
    {
        //Relevant assets.
        public static SoundEffect sndCollectHealth;
        public static Texture2D TexHealth { get; private set; }

        //Sprite information.    
        private SpriteAtlas spriteAtlas;

        /// <summary>Sets the block location and default values.</summary>
        /// <param name="x">The column number.</param>
        /// <param name="y">The row number.</param>
        /// <param name="layer">The layer in the maze.</param>
        public MazeHealth(MainLoop game, int x, int y, int layer)
            : base(game, x, y, layer)
        {
            //Sets default values.
            BlockType = Type.Health;

            //Sets sprite information.
            BlockSprite = new Sprite(true, TexHealth);
            BlockSprite.depth = 0.206f;
            spriteAtlas = new SpriteAtlas(BlockSprite, MainLoop.TileSize, MainLoop.TileSize, 19, 2, 10);
            spriteAtlas.frameSpeed = 0.2f;
        }

        /// <summary>
        /// Loads relevant graphics into memory.
        /// </summary>
        /// <param name="Content">A game content loader.</param>
        public static void LoadContent(ContentManager Content)
        {
            sndCollectHealth = Content.Load<SoundEffect>("Content/Sounds/sndCollectHealth");
            TexHealth = Content.Load<Texture2D>("Content/Sprites/Game/sprHealth");
        }

        /// <summary>
        /// Returns a copy of the object.
        /// </summary>
        public override GameObj Clone()
        {
            MazeHealth newBlock = new(game, X, Y, Layer);
            newBlock.CopyFrom(this);

            //Sets specific variables.
            newBlock.BlockSprite = BlockSprite;
            newBlock.spriteAtlas = new SpriteAtlas(spriteAtlas, false);
            return newBlock;
        }

        /// <summary>
        /// Adds to the actor's health and deletes itself on contact.
        /// </summary>
        public override void Update()
        {
            // The first actor (if any) touching the health gains 25, capped at 100.
            var firstActor = game.mngrLvl.itemsJustActors.FirstOrDefault(o => o.X == X && o.Y == Y && o.Layer == Layer);
            if (firstActor != null)
            {
                firstActor.hp = Math.Min(firstActor.hp + 25, 100);

                game.mngrLvl.RemoveItem(this);
                game.playlist.Play(sndCollectHealth, X, Y);

                // Animation for picking up health
                FxPickup pickup;
                int sparkles = 4 + Utils.Rng.Next(4);
                for (int i = 0; i < sparkles; i++)
                {
                    pickup = new FxPickup(game, X * MainLoop.TileSize, Y * MainLoop.TileSize, Layer, new Microsoft.Xna.Framework.Color(255, 0, 0));
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
                game.mngrLvl.tooltip += "Health | ";
            }
        }
    }
}