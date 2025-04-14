using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EnduranceTheMaze
{
    /// <summary>
    /// Saves the current progress of the player.
    /// 
    /// Custom properties of custInt1:
    /// 0: Saves every time an actor occupies the area.
    /// 1: Saves once and vanishes.
    /// 
    /// Custom properties of custInt2: none
    /// Custom properties of custStr: none
    /// </summary>
    public class MazeCheckpoint : GameObj
    {
        //Relevant assets.
        public static Texture2D TexCheckpoint { get; private set; }
        public static readonly Color colorChkptOneUse = new(0, 255, 255);
        public static readonly Color colorChkptMultiUse = new(0, 255, 0);

        //Sprite information.    
        private SpriteAtlas spriteAtlas;

        //Contains whether the checkpoint has been activated or not.
        bool hasActivated;

        /// <summary>Sets the block location and default values.</summary>
        /// <param name="x">The column number.</param>
        /// <param name="y">The row number.</param>
        /// <param name="layer">The layer in the maze.</param>
        public MazeCheckpoint(MainLoop game, int x, int y, int layer)
            : base(game, x, y, layer)
        {
            //Sets default values.
            BlockType = Type.Checkpoint;

            //Sets sprite information.
            BlockSprite = new Sprite(true, TexCheckpoint);
            BlockSprite.depth = 0.208f;
            spriteAtlas = new SpriteAtlas(BlockSprite, MainLoop.TileSize, MainLoop.TileSize, 19, 2, 10);
            spriteAtlas.frameSpeed = 0.35f;
        }

        /// <summary>
        /// Loads relevant graphics into memory.
        /// </summary>
        /// <param name="Content">A game content loader.</param>
        public static void LoadContent(ContentManager Content)
        {
            TexCheckpoint = Content.Load<Texture2D>("Content/Sprites/Game/sprCheckpoint");
        }

        /// <summary>
        /// Returns a copy of the object.
        /// </summary>
        public override GameObj Clone()
        {
            MazeCheckpoint newBlock = new(game, X, Y, Layer);
            newBlock.CopyFrom(this);

            //Custom variables.
            newBlock.BlockSprite = BlockSprite;
            newBlock.hasActivated = hasActivated;
            newBlock.spriteAtlas = new SpriteAtlas(spriteAtlas, false);
            return newBlock;
        }

        /// <summary>
        /// Saves if touched by an actor, deleting itself if custInt1 == 1.
        /// </summary>
        public override void Update()
        {
            // Sets color based on checkpoint behavior.
            if (CustInt1 == 1) { BlockSprite.color = colorChkptOneUse; }
            else { BlockSprite.color = colorChkptMultiUse; }

            //Attempts to save if at least one actor is touching the checkpoint.
            if (game.mngrLvl.itemsJustActors.Any(o => o.X == X && o.Y == Y && o.Layer == Layer))
            {
                if (!hasActivated)
                {
                    game.mngrLvl.doCheckpoint = true;

                    if (CustInt1 == 1)
                    {
                        game.mngrLvl.RemoveItem(this);
                    }

                    // Animation for activating the checkpoint
                    FxRing pickup;
                    int sparkles = 6 + Utils.Rng.Next(4);
                    double angleDifference = (2 * Math.PI) / sparkles;

                    for (int i = 0; i < sparkles; i++)
                    {
                        double radianAngle = angleDifference * i;
                        double ySpeed = Math.Sin(radianAngle);
                        double xSpeed = Math.Cos(radianAngle);

                        pickup = new FxRing(game,
                            X * MainLoop.TileSize + MainLoop.TileSizeHalf,
                            Y * MainLoop.TileSize + MainLoop.TileSizeHalf,
                            Layer, (xSpeed, ySpeed),
                            CustInt1 == 1 ? colorChkptOneUse : colorChkptMultiUse);

                        game.mngrLvl.AddItem(pickup);
                    }
                }

                hasActivated = true;
            }
            else //Doesn't attempt to save.
            {
                hasActivated = false;
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
                game.mngrLvl.tooltip += "Checkpoint";
                
                if (CustInt1 == 1)
                {
                    game.mngrLvl.tooltip += "(disappears on touch)";
                }
                if (!IsEnabled)
                {
                    game.mngrLvl.tooltip += "(disabled)";
                }

                game.mngrLvl.tooltip += " | ";
            }
        }
    }
}