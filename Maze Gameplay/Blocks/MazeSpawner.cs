﻿using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;

namespace Maze
{
    /// <summary>
    /// When activated, it spawns a block based on activation type.
    /// 
    /// Dependencies: MngrLvl, MazeBlock.
    /// 
    /// Activation types: Creates one of the following in spawner's dir:
    /// > 5: The object is the nth - 5 entry of the Type enum.
    /// 
    /// Custom properties of custInt1: none
    /// Custom properties of custInt2: none
    /// Custom properties of custStr: none
    /// </summary>
    public class MazeSpawner : GameObj
    {
        //Relevant assets.
        public static Texture2D TexSpawner { get; private set; }

        //Sprite information.
        private SpriteAtlas spriteAtlas;     

        /// <summary>Sets the block location and default values.</summary>
        /// <param name="x">The column number.</param>
        /// <param name="y">The row number.</param>
        /// <param name="layer">The layer in the maze.</param>
        public MazeSpawner(MainLoop game, int x, int y, int layer) :
            base(game, x, y, layer)
        {
            //Sets default values.
            IsSolid = true;
            BlockType = Type.Spawner;

            //Sets sprite information.
            BlockSprite = new Sprite(true, TexSpawner);
            BlockSprite.depth = 0.402f;
            BlockSprite.drawBehavior = SpriteDraw.all;
            spriteAtlas = new SpriteAtlas(BlockSprite, MainLoop.TileSize, MainLoop.TileSize, 8, 2, 4);

            // Solids generally occlude.
            Lighting = new(null, new(TileHull));
        }

        /// <summary>
        /// Loads relevant graphics into memory.
        /// </summary>
        /// <param name="Content">A game content loader.</param>
        public static void LoadContent(ContentManager Content)
        {
            TexSpawner = Content.Load<Texture2D>("Content/Sprites/Game/sprSpawner");
        }

        /// <summary>
        /// Returns a copy of the object.
        /// </summary>
        public override GameObj Clone()
        {
            MazeSpawner newBlock = new(game, X, Y, Layer);
            newBlock.CopyFrom(this);

            //Sets specific variables.
            newBlock.BlockSprite = BlockSprite;
            newBlock.spriteAtlas = new SpriteAtlas(spriteAtlas, false);
            return newBlock;
        }

        /// <summary>
        /// Updates the atlas. Behavior handled by MngrLvl.cs.
        /// </summary>
        public override void Update()
        {
            //Performs activation behaviors.
            if (IsEnabled && IsActivated)
            {
                if (ActionType > 4)
                {
                    //Deactivates the item and plays a sound.
                    IsActivated = false;

                    //Gets a list of solid objects in the way.
                    List<GameObj> items = game.mngrLvl.items.Where(o =>
                        o.X == X + (int)Utils.DirVector(BlockDir).X &&
                        o.Y == Y + (int)Utils.DirVector(BlockDir).Y &&
                        o.Layer == Layer && o.IsSolid).ToList();

                    #region Interaction: MazeMultiWay.cs
                items = items.Where(o =>
                    !(o.IsEnabled && o.BlockType == Type.MultiWay &&
                    ((o.SlotValueInt1 == 0 && o.BlockDir == BlockDir) ||
                    (o.SlotValueInt1 != 0 && (o.BlockDir == BlockDir ||
                    o.BlockDir == Utils.DirOpp(BlockDir)))))).ToList();
                #endregion

                    //Creates an item if there are no solid objects.
                    if (items.Count == 0)
                    {
                        //Plays a sound when an object is spawned.
                        game.playlist.Play(SndActivated, X, Y);

                        // Animation for when the spawner creates an object
                        FxPoof pickup;
                        for (int i = 0; i < 4; i++)
                        {
                            pickup = new FxPoof(
                                game, X * MainLoop.TileSize, Y * MainLoop.TileSize, Layer,
                                new Microsoft.Xna.Framework.Color(255, 255, 255),
                                (-1 + 2 * Utils.Rng.NextDouble(), -1 + 2 * Utils.Rng.NextDouble()));
                            pickup.X += -MainLoop.TileSizeHalf;
                            game.mngrLvl.AddItem(pickup);
                        }

                        //Creates different blocks based on action type.
                        game.mngrLvl.AddItem(Utils.BlockFromType(game,
                            (Type)(ActionType - 5),
                            X + (int)Utils.DirVector(BlockDir).X,
                            Y + (int)Utils.DirVector(BlockDir).Y, Layer));

                    }
                }
            }

            #region Updates the sprite.
            //Updates the actor sprite by direction.
            //Depends on the texture frames and orientation.
            if (BlockDir == Dir.Right)
            {
                spriteAtlas.frame = 0;
            }
            else if (BlockDir == Dir.Down)
            {
                spriteAtlas.frame = 1;
            }
            else if (BlockDir == Dir.Left)
            {
                spriteAtlas.frame = 2;
            }
            else
            {
                spriteAtlas.frame = 3;
            }
            if (!IsEnabled)
            {
                spriteAtlas.frame += 4;
            }
            #endregion

            spriteAtlas.Update(true);
            base.Update();
        }

        /// <summary>
        /// Draws the spawner. When hovered, draws enabledness/info.
        /// </summary>
        public override void Draw()
        {
            base.Draw();

            //Sets the tooltip to display disabled status and info.
            if (Sprite.IsIntersecting(BlockSprite, new SmoothRect
                (game.mngrLvl.GetCoordsMouse(), 1, 1)) &&
                Layer == game.mngrLvl.actor.Layer)
            {
                game.mngrLvl.tooltip += "Spawner";

                if (!IsEnabled)
                {
                    game.mngrLvl.tooltip += "(disabled)";
                }

                game.mngrLvl.tooltip += " | ";
            }
        }
    }
}