﻿using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Maze
{
    /// <summary>
    /// Sends/receives objects on contact.
    /// 
    /// Activation types: none
    /// 
    /// Custom properties of custInt1:
    /// 0: Sender node.
    /// 1: Receiver node.
    /// Custom properties of custInt2:
    /// The number is the teleporting channel.
    /// Custom properties of custStr: none
    /// </summary>
    public class MazeTeleporter : GameObj
    {
        //Relevant assets.
        public static SoundEffect sndTeleport;
        public static Texture2D TexTeleporter { get; private set; }

        //Sprite information.    
        private SpriteAtlas spriteAtlas;

        /// <summary>Sets the block location and default values.</summary>
        /// <param name="x">The column number.</param>
        /// <param name="y">The row number.</param>
        /// <param name="layer">The layer in the maze.</param>
        public MazeTeleporter(MainLoop game, int x, int y, int layer)
            : base(game, x, y, layer)
        {
            //Sets default values.
            BlockType = Type.Teleporter;

            //Sets sprite information.
            BlockSprite = new Sprite(true, TexTeleporter);
            BlockSprite.depth = 0.412f;
            spriteAtlas = new SpriteAtlas(BlockSprite, MainLoop.TileSize, MainLoop.TileSize, 4, 1, 4);
        }

        /// <summary>
        /// Loads relevant graphics into memory.
        /// </summary>
        /// <param name="Content">A game content loader.</param>
        public static void LoadContent(ContentManager Content)
        {
            sndTeleport = Content.Load<SoundEffect>("Content/Sounds/sndTeleport");
            TexTeleporter = Content.Load<Texture2D>("Content/Sprites/Game/sprTeleport");
        }

        /// <summary>
        /// Returns a copy of the object.
        /// </summary>
        public override GameObj Clone()
        {
            MazeTeleporter newBlock = new(game, X, Y, Layer);
            newBlock.CopyFrom(this);

            //Sets specific variables.
            newBlock.BlockSprite = BlockSprite;
            newBlock.spriteAtlas = new SpriteAtlas(spriteAtlas, true);
            return newBlock;
        }

        /// <summary>
        /// Transfers between senders and receivers on contact if possible.
        /// </summary>
        public override void Update()
        {
            #region Adjusts sprite.
            //Adjusts the sprite frame.
            if (SlotValueInt1 == 0)
            {
                spriteAtlas.frame = 0; //Sender.
            }
            else
            {
                spriteAtlas.frame = 2; //Receiver.
            }
            //Depends on frame positions and texture.
            if (!IsEnabled)
            {
                spriteAtlas.frame++;
            }
            #endregion

            //Sender logic.
            if (IsEnabled && SlotValueInt1 == 0)
            {
                //Blocks on this block, blocks on receivers, and receivers.
                List<GameObj> itemsTop = new List<GameObj>();
                List<GameObj> itemsDestTop = new List<GameObj>();
                List<GameObj> itemsNodes = new List<GameObj>();

                //Gets a list of all blocks on the sender.
                itemsTop = game.mngrLvl.items.Where(o =>
                    o.X == X && o.Y == Y && o.Layer == Layer &&
                    (o.BlockSprite.depth < BlockSprite.depth) &&
                    o.BlockType != Type.MultiWay).ToList(); // don't teleport multi-ways.

                #region Interaction: MazeTurretBullet
                itemsTop.AddRange(game.mngrLvl.items.Where(o =>
                    o.BlockType == Type.TurretBullet &&
                    Math.Abs(X * MainLoop.TileSize + MainLoop.TileSizeHalf - o.X) < MainLoop.TileSizeHalf &&
                    Math.Abs(Y * MainLoop.TileSize + MainLoop.TileSizeHalf - o.Y) < MainLoop.TileSizeHalf &&
                    o.Layer == Layer));
                #endregion

                //Gets a list of all enabled receivers.
                itemsNodes = game.mngrLvl.items.Where(o =>
                    o.BlockType == Type.Teleporter &&
                    o.IsEnabled &&
                    o.SlotValueInt1 != 0 &&
                    o.SlotValueInt2 == SlotValueInt2).ToList();

                //Teleports blocks if receivers are available.
                foreach (GameObj item in itemsTop)
                {
                    //Filters out all incapable receivers.
                    for (int i = itemsNodes.Count - 1; i >= 0; i--)
                    {
                        //Gets a list of all solid blocks on the receiver.
                        itemsDestTop = game.mngrLvl.items.Where(o =>
                            o.X == itemsNodes[i].X &&
                            o.Y == itemsNodes[i].Y && o.Layer ==
                            itemsNodes[i].Layer && o.IsSolid).ToList();

                        //Iterates through each block on the receiver.
                        for (int j = itemsDestTop.Count - 1; j >= 0; j--)
                        {
                            #region Interaction: MazeCrate
                            if (itemsDestTop[j].BlockType == Type.Crate)
                            {
                                //Gets a list of all solid blocks in front.
                                List<GameObj> itemsDestFront =
                                    game.mngrLvl.items.Where(o => o.IsSolid &&
                                    o.X == itemsNodes[i].X +
                                    (int)Utils.DirVector(item.BlockDir).X &&
                                    o.Y == itemsNodes[i].Y +
                                    (int)Utils.DirVector(item.BlockDir).Y &&
                                    o.Layer == itemsNodes[i].Layer).ToList();

                                //Removes valid multiways from the list.
                                #region Interaction: MazeMultiWay.cs
                                itemsDestFront = itemsDestFront.Where(o =>
                                    !(o.IsEnabled &&
                                    o.BlockType == Type.MultiWay &&
                                    ((o.SlotValueInt1 == 0 && o.BlockDir == item.BlockDir) ||
                                    (o.SlotValueInt1 != 0 && (o.BlockDir == item.BlockDir ||
                                    o.BlockDir == Utils.DirOpp(BlockDir)))))).ToList();
                                #endregion

                                /*Allows a block to enter the teleporter if
                                  the crate blocking the receiver can be
                                  pushed out of the way in the direction the
                                  block is traveling.*/
                                if (itemsDestFront.Count == 0)
                                {
                                    //Moves the crate; removes it from list.
                                    itemsDestTop[j].X +=
                                        (int)Utils.DirVector(item.BlockDir).X;
                                    itemsDestTop[j].Y +=
                                        (int)Utils.DirVector(item.BlockDir).Y;
                                    itemsDestTop[j].BlockDir = item.BlockDir;
                                    itemsDestTop.RemoveAt(j);
                                }
                            }
                            #endregion
                        }

                        //Removes the receiver for being incapable.
                        if (itemsDestTop.Count != 0)
                        {
                            itemsNodes.RemoveAt(i);
                        }
                    }

                    if (itemsNodes.Count != 0)
                    {
                        //Selects a receiver at random.
                        GameObj receiver =
                            itemsNodes[Utils.Rng.Next(itemsNodes.Count)];

                        game.playlist.Play(sndTeleport, X, Y);

                        // Animation for the teleporter activating
                        var teleportEffect = new FxTeleported(game, X, Y, Layer, new(0, 128, 0));
                        game.mngrLvl.AddItem(teleportEffect);

                        #region Interaction: MazeTurretBullet
                        if (item.BlockType == Type.TurretBullet)
                        {
                            item.X = (int)Math.IEEERemainder(item.X, MainLoop.TileSize);
                            item.Y = (int)Math.IEEERemainder(item.Y, MainLoop.TileSize);
                            item.X += receiver.X * MainLoop.TileSize;
                            item.Y += receiver.Y * MainLoop.TileSize;
                        }
                        else
                        {
                            item.X = receiver.X;
                            item.Y = receiver.Y;
                        }
                        #endregion

                        item.Layer = receiver.Layer;

                        // Animation for the teleporter receiving
                        var receivedEffect = new FxReceived(game, receiver.X, receiver.Y, receiver.Layer);
                        game.mngrLvl.AddItem(receivedEffect);
                    }
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
                if (SlotValueInt1 == 0)
                {
                    game.mngrLvl.tooltip += "Sender " +
                        "(channel " + SlotValueInt2 + ")";
                }
                else
                {
                    game.mngrLvl.tooltip += "Receiver " +
                        "(channel " + SlotValueInt2 + ")";
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