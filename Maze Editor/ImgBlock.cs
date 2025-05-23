﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Maze
{
    /// <summary>
    /// Visually represents an item, but doesn't simulate behavior.
    /// </summary>
    public class ImgBlock
    {
        //Refers to the game instance.
        protected MainLoop game;

        //Contains a sprite.
        public Sprite BlockSprite { get; protected set; }
        public SpriteAtlas BlockSpriteAtlas { get; protected set; }

        //Block location.
        public int X { get; internal set; }
        public int Y { get; internal set; }
        public int Layer { get; protected set; }

        //Block identity by type.
        public Type BlockType { get; set; }

        //Block's facing direction.
        public Dir BlockDir { get; internal set; }

        //If the block is enabled or not.
        public bool IsEnabled { get; internal set; }

        //Block activation.
        public int SignalListenChannel { get; internal set; } //The activation channel.
        public int SignalSendChannel { get; internal set; } //Actuator channels.
        public int ActionType { get; internal set; } //The activation behavior.

        //Custom block properties.
        public int SlotValueInt1 { get; internal set; }
        public int SlotValueInt2 { get; internal set; }

        public Dictionary<string, object> Properties { get; internal set; }

        private bool isShrinking;

        /// <summary>
        /// Sets the block's basic values.
        /// </summary>
        /// <param name="x">The column number.</param>
        /// <param name="y">The row number.</param>
        /// <param name="layer">The layer in the maze.</param>
        public ImgBlock(MainLoop game, Type type, int x, int y, int layer)
        {
            this.game = game;
            BlockType = type;
            X = x;
            Y = y;
            Layer = layer;

            //Sets default values.
            SignalListenChannel = 0;
            SignalSendChannel = 0;
            ActionType = 0;
            SlotValueInt1 = 0;
            SlotValueInt2 = 0;
            BlockDir = Dir.Right;
            IsEnabled = true;
            Properties = new()
            {
                { Utils.PropertyNameCustomString, "" }
            };

            //Sets up the appearance information.
            AdjustSprite();

            //Sets custom variable info.
            isShrinking = true; //Used to draw MazeClick.
        }

        /// <summary>
        /// Updates the sprite atlas for sprites, esp. animated ones.
        /// </summary>
        public virtual void Update()
        {
            if (BlockSpriteAtlas != null)
            {
                BlockSpriteAtlas.Update(true);
            }

            //Handles constant sprite animations.
            if (BlockType == Type.Click && IsEnabled)
            {
                if (isShrinking)
                {
                    BlockSprite.scaleX -= 0.01f;
                    BlockSprite.scaleY -= 0.01f;
                    if (BlockSprite.scaleX <= 0.5f)
                    {
                        isShrinking = false;
                    }
                }
                else
                {
                    BlockSpriteAtlas.frame = 0;
                    BlockSprite.scaleX += 0.01f;
                    BlockSprite.scaleY += 0.01f;
                    if (BlockSprite.scaleX >= 1)
                    {
                        isShrinking = true;
                    }
                }
            }
            else if (BlockType == Type.Freeze)
            {
                BlockSprite.angle += 0.05f;
            }
            else if (BlockType == Type.Spike)
            {
                BlockSprite.angle += 0.02f;
            }
        }

        /// <summary>
        /// Draws the sprite and any GUI aspects.
        /// </summary>
        public virtual void Draw()
        {
            BlockSprite.Draw(game.GameSpriteBatch);

            // Draws the message text.
            if (BlockType == Type.Message)
            {
                SpriteText txt = new(game.fntBoldBig, Properties[Utils.PropertyNameCustomString].ToString())
                {
                    color = Color.Black,
                    depth = 0.009f,
                    drawBehavior = SpriteDraw.all,
                    position = BlockSprite.rectDest.Center
                };

                txt.CenterOriginVert();
                txt.Draw(game.GameSpriteBatch);
            }
        }

        /// <summary>
        /// Adjusts the sprite to match the type, dir, etc. This is called
        /// whenever the sprite is updated.
        /// </summary>
        public void AdjustSprite()
        {
            BlockSprite = new Sprite();

            switch (BlockType)
            {
                case Type.Actor:
                    BlockSprite = new Sprite(true, MazeActor.TexActor);
                    BlockSprite.depth = 0.1f;
                    BlockSprite.drawBehavior = SpriteDraw.all;
                    BlockSpriteAtlas = new SpriteAtlas(BlockSprite, MainLoop.TileSize, MainLoop.TileSize, 6, 2, 3);
                    #region Chooses sprite by direction.
                    if (BlockDir == Dir.Right)
                    {
                        BlockSpriteAtlas.frame = 0;
                        BlockSprite.spriteEffects = SpriteEffects.None;
                    }
                    else if (BlockDir == Dir.Down)
                    {
                        BlockSpriteAtlas.frame = 1;
                        BlockSprite.spriteEffects = SpriteEffects.None;
                    }
                    else if (BlockDir == Dir.Left)
                    {
                        BlockSpriteAtlas.frame = 0;
                        BlockSprite.spriteEffects = SpriteEffects.FlipHorizontally;
                    }
                    else
                    {
                        BlockSpriteAtlas.frame = 2;
                        BlockSprite.spriteEffects = SpriteEffects.None;
                    }
                    #endregion
                    break;
                case Type.Belt:
                    BlockSprite = new Sprite(true, MazeBelt.TexBelt);
                    BlockSprite.depth = 0.401f;
                    BlockSprite.doDrawOffset = true;
                    BlockSprite.drawBehavior = SpriteDraw.all;
                    BlockSpriteAtlas = new SpriteAtlas(BlockSprite, MainLoop.TileSize, MainLoop.TileSize, 9, 1, 9);
                    BlockSpriteAtlas.CenterOrigin();
                    #region Chooses sprite by direction and isEnabled.
                    if (BlockDir == Dir.Right)
                    {
                        BlockSprite.angle = 0;
                    }
                    else if (BlockDir == Dir.Down)
                    {
                        BlockSprite.angle = (float)(Math.PI / 2);
                    }
                    else if (BlockDir == Dir.Left)
                    {
                        BlockSprite.angle = (float)(Math.PI);
                    }
                    else
                    {
                        BlockSprite.angle = (float)(-Math.PI / 2);
                    }

                    //Determines the belt's image speed.
                    if (IsEnabled)
                    {
                        BlockSpriteAtlas.frameSpeed = 0.25f;
                    }
                    else
                    {
                        BlockSpriteAtlas.frameSpeed = 0;
                    }
                    #endregion
                    break;
                case Type.Checkpoint:
                    //Sets sprite information.
                    BlockSprite = new Sprite(true, MazeCheckpoint.TexCheckpoint);
                    BlockSprite.depth = 0.208f;
                    BlockSpriteAtlas = new SpriteAtlas(BlockSprite, MainLoop.TileSize, MainLoop.TileSize, 19, 2, 10);
                    BlockSpriteAtlas.frameSpeed = 0.35f;
                    BlockSprite.color = SlotValueInt1 == 1 ? MazeCheckpoint.colorChkptOneUse : MazeCheckpoint.colorChkptMultiUse;
                    break;
                case Type.Click:
                    //Sets sprite information.
                    BlockSprite = new Sprite(true, MazeClick.TexClick);
                    BlockSprite.depth = 0.201f;
                    BlockSprite.drawBehavior = SpriteDraw.all;
                    BlockSpriteAtlas = new SpriteAtlas(BlockSprite, MainLoop.TileSize, MainLoop.TileSize, 2, 1, 2);
                    #region Adjusts sprite and handles growing/shrinking animation.
                    if (IsEnabled)
                    {
                        BlockSpriteAtlas.frame = 0;
                    }
                    else
                    {
                        BlockSpriteAtlas.frame = 1;
                    }
                    #endregion
                    break;
                case Type.Coin:
                    BlockSprite = new Sprite(true, MazeCoin.TexCoin);
                    BlockSprite.depth = 0.205f;
                    BlockSpriteAtlas = new SpriteAtlas(BlockSprite, MainLoop.TileSize, MainLoop.TileSize, 19, 2, 10);
                    BlockSpriteAtlas.frameSpeed = 0.2f;
                    break;
                case Type.CoinLock:
                    BlockSprite = new Sprite(true, MazeCoinLock.TexCoinLock);
                    BlockSprite.depth = 0.410f;
                    break;
                case Type.Crate:
                    BlockSprite = new Sprite(true, MazeCrate.TexCrate);
                    BlockSprite.depth = 0.3f;
                    BlockSprite.rectSrc = new SmoothRect(0, 0, MainLoop.TileSize, MainLoop.TileSize);
                    BlockSprite.rectDest.Width = MainLoop.TileSize;
                    BlockSprite.rectDest.Height = MainLoop.TileSize;
                    BlockSprite.drawBehavior = SpriteDraw.basicAnimated;
                    break;
                case Type.CrateHole:
                    BlockSprite = new Sprite(true, MazeCrateHole.TexCrateHole);
                    BlockSprite.depth = 0.403f;
                    BlockSpriteAtlas = new SpriteAtlas(BlockSprite, MainLoop.TileSize, MainLoop.TileSize, 2, 1, 2);
                    break;
                case Type.EAuto:
                    //Sets sprite information.
                    BlockSprite = new Sprite(true, MazeEAuto.TexEAuto);
                    BlockSprite.depth = 0.417f;
                    BlockSpriteAtlas = new SpriteAtlas(BlockSprite, MainLoop.TileSize, MainLoop.TileSize, 3, 2, 3);
                    #region Adjusts sprite.
                    if (IsEnabled)
                    {
                        BlockSpriteAtlas.frameSpeed = 0.2f;
                    }
                    else
                    {
                        BlockSpriteAtlas.frameSpeed = 0;
                    }
                    #endregion
                    break;
                case Type.ELight:
                    BlockSprite = new Sprite(true, MazeELight.TexELight);
                    BlockSprite.depth = 0.416f;
                    BlockSpriteAtlas = new SpriteAtlas(BlockSprite, MainLoop.TileSize, MainLoop.TileSize, 2, 1, 2);
                    BlockSpriteAtlas.frame = 1;
                    break;
                case Type.Enemy:
                    BlockSprite = new Sprite(true, MazeEnemy.TexEnemy);
                    BlockSprite.depth = 0.4f;
                    BlockSprite.drawBehavior = SpriteDraw.all;
                    BlockSpriteAtlas = new SpriteAtlas(BlockSprite, MainLoop.TileSize, MainLoop.TileSize, 4, 1, 4);
                    #region Chooses sprite by direction and isEnabled.
                    if (Utils.DirCardinal(BlockDir))
                    {
                        if (IsEnabled)
                        {
                            BlockSpriteAtlas.frame = 0;
                        }
                        else
                        {
                            BlockSpriteAtlas.frame = 1;
                        }
                    }
                    else
                    {
                        if (IsEnabled)
                        {
                            BlockSpriteAtlas.frame = 2;
                        }
                        else
                        {
                            BlockSpriteAtlas.frame = 3;
                        }
                    }
                    #endregion
                    break;
                case Type.EPusher:
                    BlockSprite = new Sprite(true, MazeEPusher.TexEPusher);
                    BlockSprite.depth = 0.415f;
                    BlockSprite.drawBehavior = SpriteDraw.all;
                    BlockSpriteAtlas = new SpriteAtlas(BlockSprite, MainLoop.TileSize * 2, MainLoop.TileSize, 3, 1, 3);
                    BlockSprite.doDrawOffset = true;
                    BlockSprite.origin.X = MainLoop.TileSizeHalf;
                    BlockSprite.origin.Y = MainLoop.TileSizeHalf;
                    #region Adjusts sprite.
                    if (BlockDir == Dir.Right)
                    {
                        BlockSprite.angle = 0;
                    }
                    else if (BlockDir == Dir.Down)
                    {
                        BlockSprite.angle = (float)(Math.PI / 2);
                    }
                    else if (BlockDir == Dir.Left)
                    {
                        BlockSprite.angle = (float)(Math.PI);
                    }
                    else
                    {
                        BlockSprite.angle = (float)(-Math.PI / 2);
                    }         
                    if (IsEnabled)
                    {
                        BlockSpriteAtlas.frame = 0;
                    }
                    else
                    {
                        BlockSpriteAtlas.frame = 2;
                    }
                    #endregion
                    break;
                case Type.Filter:
                    BlockSprite = new Sprite(true, MazeFilter.TexFilter);
                    BlockSprite.depth = 0.405f;
                    BlockSprite.doDrawOffset = true;
                    BlockSprite.drawBehavior = SpriteDraw.all;
                    BlockSpriteAtlas = new SpriteAtlas(BlockSprite, MainLoop.TileSize, MainLoop.TileSize, 19, 2, 10);
                    BlockSpriteAtlas.CenterOrigin();
                    #region Chooses frame speed by isEnabled.
                    if (IsEnabled)
                    {
                        BlockSpriteAtlas.frameSpeed = 0.35f;
                    }
                    else
                    {
                        BlockSpriteAtlas.frame = 0;
                        BlockSpriteAtlas.frameSpeed = 0;
                    }
                    #endregion
                    break;
                case Type.Finish:
                    BlockSprite = new Sprite(true, MazeFinish.TexFinish);
                    BlockSprite.depth = 0.417f;
                    break;
                case Type.Floor:
                    BlockSprite = new Sprite(true, MazeFloor.TexFloor);
                    BlockSprite.depth = 0.6f;
                    break;
                case Type.Freeze:
                    BlockSprite = new Sprite(true, MazeFreeze.TexFreeze);
                    BlockSprite.depth = 0.203f;
                    BlockSprite.doDrawOffset = true;
                    BlockSprite.drawBehavior = SpriteDraw.all;
                    BlockSpriteAtlas = new SpriteAtlas(BlockSprite, MainLoop.TileSize, MainLoop.TileSize, 10, 1, 10);
                    BlockSpriteAtlas.frameSpeed = 0.4f;
                    BlockSpriteAtlas.CenterOrigin();
                    break;
                case Type.Gate:
                    BlockSprite = new Sprite(true, MazeGate.TexGate);
                    BlockSprite.depth = 0.102f;
                    BlockSpriteAtlas = new SpriteAtlas(BlockSprite, MainLoop.TileSize, MainLoop.TileSize, 2, 1, 2);
                    if (SlotValueInt2 == 1)
                    {
                        BlockSpriteAtlas.frame = 1;
                    }
                    break;
                case Type.Goal:
                    BlockSprite = new Sprite(true, MazeGoal.TexGoal);
                    BlockSprite.depth = 0.202f;
                    BlockSprite.doDrawOffset = true;
                    BlockSprite.drawBehavior = SpriteDraw.all;
                    BlockSpriteAtlas = new SpriteAtlas(BlockSprite, MainLoop.TileSize, MainLoop.TileSize, 9, 1, 9);
                    BlockSpriteAtlas.frameSpeed = 0.2f;
                    BlockSpriteAtlas.CenterOrigin();
                    break;
                case Type.Health:
                    BlockSprite = new Sprite(true, MazeHealth.TexHealth);
                    BlockSprite.depth = 0.206f;
                    BlockSpriteAtlas = new SpriteAtlas(BlockSprite, MainLoop.TileSize, MainLoop.TileSize, 19, 2, 10);
                    BlockSpriteAtlas.frameSpeed = 0.2f;
                    break;
                case Type.Ice:
                    BlockSprite = new Sprite(true, MazeIce.TexIce);
                    BlockSprite.depth = 0.5f;
                    break;
                case Type.Key:
                    BlockSprite = new Sprite(true, MazeKey.TexKey);
                    BlockSprite.depth = 0.207f;
                    BlockSprite.drawBehavior = SpriteDraw.all;
                    BlockSpriteAtlas = new SpriteAtlas(BlockSprite, MainLoop.TileSize, MainLoop.TileSize, 19, 2, 10);
                    BlockSpriteAtlas.frameSpeed = 0.2f;
                    #region Chooses key color by custInt1.
                    switch (SlotValueInt1)
                    {
                        case (0):
                            BlockSprite.color = Color.Blue;
                            break;
                        case (1):
                            BlockSprite.color = Color.Red;
                            break;
                        case (2):
                            BlockSprite.color = Color.Goldenrod;
                            break;
                        case (3):
                            BlockSprite.color = Color.Purple;
                            break;
                        case (4):
                            BlockSprite.color = Color.Orange;
                            break;
                        case (5):
                            BlockSprite.color = Color.Black;
                            break;
                        case (6):
                            BlockSprite.color = Color.DarkBlue;
                            break;
                        case (7):
                            BlockSprite.color = Color.DarkRed;
                            break;
                        case (8):
                            BlockSprite.color = Color.DarkGoldenrod;
                            break;
                        case (9):
                            BlockSprite.color = Color.DarkOrange;
                            break;
                    }
                    #endregion
                    break;
                case Type.LaserActuator:
                    BlockSprite = new Sprite(true, MazeLaserActuator.TexLaserActuator);
                    BlockSprite.depth = 0.421f;
                    BlockSpriteAtlas = new SpriteAtlas(BlockSprite, MainLoop.TileSize, MainLoop.TileSize, 5, 1, 5);
                    #region Adjusts sprite.
                    if (IsEnabled)
                    {
                        BlockSpriteAtlas.frame = 1;
                    }
                    #endregion
                    break;
                case Type.Lock:
                    BlockSprite = new Sprite(true, MazeLock.TexLock);
                    BlockSprite.depth = 0.407f;
                    BlockSprite.drawBehavior = SpriteDraw.all;
                    #region Chooses lock color by custInt1.
                    switch (SlotValueInt1)
                    {
                        case (0):
                            BlockSprite.color = Color.Blue;
                            break;
                        case (1):
                            BlockSprite.color = Color.Red;
                            break;
                        case (2):
                            BlockSprite.color = Color.Goldenrod;
                            break;
                        case (3):
                            BlockSprite.color = Color.Purple;
                            break;
                        case (4):
                            BlockSprite.color = Color.Orange;
                            break;
                        case (5):
                            BlockSprite.color = Color.Black;
                            break;
                        case (6):
                            BlockSprite.color = Color.DarkBlue;
                            break;
                        case (7):
                            BlockSprite.color = Color.DarkRed;
                            break;
                        case (8):
                            BlockSprite.color = Color.DarkGoldenrod;
                            break;
                        case (9):
                            BlockSprite.color = Color.DarkOrange;
                            break;
                    }
                    #endregion
                    break;
                case Type.Message:
                    BlockSprite = new Sprite(true, MazeMessage.TexMessage);
                    BlockSprite.depth = 0.209f;
                    break;
                case Type.Mirror:
                    BlockSprite = new Sprite(true, MazeMirror.TexMirror);
                    BlockSprite.drawBehavior = SpriteDraw.all;
                    BlockSprite.depth = 0.420f;
                    BlockSpriteAtlas = new SpriteAtlas(BlockSprite, MainLoop.TileSize, MainLoop.TileSize, 4, 1, 4);
                    if (BlockDir == Dir.Right)
                    {
                        BlockSpriteAtlas.frame = 0;
                    }
                    else if (BlockDir == Dir.Down)
                    {
                        BlockSpriteAtlas.frame = 1;
                    }
                    else if (BlockDir == Dir.Left)
                    {
                        BlockSpriteAtlas.frame = 2;
                    }
                    else
                    {
                        BlockSpriteAtlas.frame = 3;
                    }
                    break;
                case Type.MultiWay:
                    BlockSprite = new Sprite(true, MazeMultiWay.TexMultiWay);
                    BlockSprite.depth = 0.408f;
                    BlockSprite.doDrawOffset = true;
                    BlockSprite.drawBehavior = SpriteDraw.all;
                    BlockSpriteAtlas = new SpriteAtlas(BlockSprite, MainLoop.TileSize, MainLoop.TileSize, 4, 1, 4);
                    BlockSpriteAtlas.CenterOrigin();
                    #region Chooses sprite by direction and frame.
                    //Updates the sprite by direction.
                    if (BlockDir == Dir.Right)
                    {
                        BlockSprite.angle = 0;
                    }
                    else if (BlockDir == Dir.Down)
                    {
                        BlockSprite.angle = (float)(Math.PI / 2);
                    }
                    else if (BlockDir == Dir.Left)
                    {
                        BlockSprite.angle = (float)(Math.PI);
                    }
                    else
                    {
                        BlockSprite.angle = (float)(-Math.PI / 2);
                    }

                    //Determines the frame used.
                    //Dependent on frame order.
                    if (SlotValueInt1 == 0)
                    {
                        BlockSpriteAtlas.frame = 0;
                    }
                    else
                    {
                        BlockSpriteAtlas.frame = 2;
                    }
                    if (!IsEnabled)
                    {
                        BlockSpriteAtlas.frame += 1;
                    }
                    #endregion
                    break;
                case Type.Panel:
                    BlockSprite = new Sprite(true, MazePanel.TexPanel);
                    BlockSprite.depth = 0.414f;
                    BlockSpriteAtlas = new SpriteAtlas(BlockSprite, MainLoop.TileSize, MainLoop.TileSize, 4, 1, 4);
                    break;
                case Type.Rotate:
                    BlockSprite = new Sprite(true, MazeRotate.TexRotate);
                    BlockSprite.depth = 0.418f;
                    BlockSprite.doDrawOffset = true;
                    BlockSprite.drawBehavior = SpriteDraw.all;
                    BlockSpriteAtlas = new SpriteAtlas(BlockSprite, MainLoop.TileSize, MainLoop.TileSize, 2, 1, 2);
                    BlockSpriteAtlas.CenterOrigin();
                    #region Adjusts sprite.
                    if (IsEnabled)
                    {
                        BlockSpriteAtlas.frame = 0;
                    }
                    else
                    {
                        BlockSpriteAtlas.frame = 1;
                    }
                    #endregion
                    break;
                case Type.Spawner:
                    BlockSprite = new Sprite(true, MazeSpawner.TexSpawner);
                    BlockSprite.depth = 0.402f;
                    BlockSprite.drawBehavior = SpriteDraw.all;
                    BlockSpriteAtlas = new SpriteAtlas(BlockSprite, MainLoop.TileSize, MainLoop.TileSize, 8, 2, 4);
                    #region Chooses sprite by direction and isEnabled.
                    if (BlockDir == Dir.Right)
                    {
                        BlockSpriteAtlas.frame = 0;
                    }
                    else if (BlockDir == Dir.Down)
                    {
                        BlockSpriteAtlas.frame = 1;
                    }
                    else if (BlockDir == Dir.Left)
                    {
                        BlockSpriteAtlas.frame = 2;
                    }
                    else
                    {
                        BlockSpriteAtlas.frame = 3;
                    }
                    if (!IsEnabled)
                    {
                        BlockSpriteAtlas.frame += 4;
                    }
                    #endregion
                    break;
                case Type.Spike:
                    BlockSprite = new Sprite(true, MazeSpike.TexSpike);
                    BlockSprite.depth = 0.409f;
                    BlockSprite.drawBehavior = SpriteDraw.all;
                    BlockSprite.doDrawOffset = true;
                    BlockSpriteAtlas = new SpriteAtlas(BlockSprite, MainLoop.TileSize, MainLoop.TileSize, 19, 2, 10);
                    BlockSpriteAtlas.frameSpeed = 0.2f;
                    BlockSpriteAtlas.CenterOrigin();
                    break;
                case Type.Stairs:
                    BlockSprite = new Sprite(true, MazeStairs.TexStairs);
                    BlockSprite.depth = 0.406f;
                    BlockSpriteAtlas = new SpriteAtlas(BlockSprite, MainLoop.TileSize, MainLoop.TileSize, 2, 1, 2);
                    #region Chooses frame by custInt1.
                    //Adjusts the sprite frame.
                    if (SlotValueInt1 == 0)
                    {
                        BlockSpriteAtlas.frame = 0; //up.
                    }
                    else
                    {
                        BlockSpriteAtlas.frame = 1; //down.
                    }
                    #endregion
                    break;
                case Type.Teleporter:
                    BlockSprite = new Sprite(true, MazeTeleporter.TexTeleporter);
                    BlockSprite.depth = 0.412f;
                    BlockSpriteAtlas = new SpriteAtlas(BlockSprite, MainLoop.TileSize, MainLoop.TileSize, 4, 1, 4);
                    #region Chooses sprite by frame and isEnabled.
                    //Adjusts the sprite frame.
                    if (SlotValueInt1 == 0)
                    {
                        BlockSpriteAtlas.frame = 0; //Sender.
                    }
                    else
                    {
                        BlockSpriteAtlas.frame = 2; //Receiver.
                    }
                    //Depends on frame positions and texture.
                    if (!IsEnabled)
                    {
                        BlockSpriteAtlas.frame++;
                    }
                    #endregion
                    break;
                case Type.Thaw:
                    BlockSprite = new Sprite(true, MazeThaw.TexThaw);
                    BlockSprite.depth = 0.204f;
                    BlockSpriteAtlas = new SpriteAtlas(BlockSprite, MainLoop.TileSize, MainLoop.TileSize, 13, 1, 13);
                    BlockSpriteAtlas.frameSpeed = 0.25f;
                    break;
                case Type.Turret:
                    BlockSprite = new Sprite(true, MazeTurret.TexTurret);
                    BlockSprite.depth = 0.419f;
                    BlockSprite.drawBehavior = SpriteDraw.all;
                    BlockSpriteAtlas = new SpriteAtlas(BlockSprite, MainLoop.TileSize, MainLoop.TileSize, 8, 2, 4);
                    #region Chooses sprite by direction and isEnabled.
                    if (BlockDir == Dir.Right)
                    {
                        BlockSpriteAtlas.frame = 0;
                    }
                    else if (BlockDir == Dir.Down)
                    {
                        BlockSpriteAtlas.frame = 1;
                    }
                    else if (BlockDir == Dir.Left)
                    {
                        BlockSpriteAtlas.frame = 2;
                    }
                    else
                    {
                        BlockSpriteAtlas.frame = 3;
                    }
                    if (!IsEnabled)
                    {
                        BlockSpriteAtlas.frame += 4;
                    }
                    #endregion
                    break;
                case Type.Wall:
                    BlockSprite = new Sprite(true, MazeWall.TexWall);
                    BlockSprite.depth = 0.413f;
                    break;
            }

            //Synchronizes sprite position to location.
            BlockSprite.rectDest.X = X * MainLoop.TileSize - MainLoop.TileSizeHalf; //-half for camera offset.
            BlockSprite.rectDest.Y = Y * MainLoop.TileSize - MainLoop.TileSizeHalf; //-half for camera offset.
            if (BlockSpriteAtlas != null)
            {
                BlockSpriteAtlas.Update(true);
            }
        }
    }
}