﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using ButtonState = Microsoft.Xna.Framework.Input.ButtonState;
using Keys = Microsoft.Xna.Framework.Input.Keys;

namespace Maze
{
    /// <summary>
    /// The level editor. Handles level design logic and UI.
    /// 
    /// Dependencies: MainLoop.cs, MngrLvl, All MazeBlock children.
    /// </summary>
    public class MngrEditor
    {
        //Refers to the game instance.
        private readonly MainLoop game;

        //The bottom toolbar, left sidebar, and block selector.
        public static Texture2D TexInGameLevelEditorBg { get; private set; }
        private Sprite sprInGameLevelEditorBg, sprToolbar, sprSidebar;
        public int sidebarScroll = 0; //Amount of vertical sidebar scrolling.

        //Toolbar objects.
        public string tooltip; //Informational tip.
        private PropButton bttnSignalListenChannel, bttnSignalSendChannel,
            bttnActionType, bttnSlotValueInt1, bttnSlotValueInt2, bttnSlotValueString, bttnDir,
            bttnIsEnabled, bttnGameDelay, bttnLvlLink, bttnMaxSteps,
            bttnMinGoals, bttnSyncActors, bttnSyncDeath;
        private const int sidebarWidth = 64;
        private const int bottomBarHeight = 64;

        //Sidebar objects.
        public List<ImgType> itemTypes; //Items by type.

        //All positions occupied as a result of this mouse click + drag.
        public List<Vector2> itemDragPos;

        public Type activeType; //The active block type selected.
        public ImgBlock activeItem; //The active existing block selected.
        public List<ImgBlock> items; //All items in the level.
        private Dir lastDirStrict = Dir.Right; // The most recent direction used/set on a block. New blocks paste in this dir.
        private Dir lastDirEnemy = Dir.Right; // The most recent direction used/set on a block including diagonals.

        //Contains the camera position and zoom.
        public Matrix Camera { get; private set; }
        public int camX, camY, camLayer;
        private float camZoom;

        //Contains the level settings.
        internal int opGameDelay, opMaxSteps, opMinGoals;
        internal string opLvlLink;
        internal bool opSyncActors, opSyncDeath;

        //Copied properties. These become default for new blocks.
        private int copySignalListenChannel, copySignalSendChannel, copySlotValueInt1, copySlotValueInt2, copyActType;
        private Type copyType;
        private Dir copyDir;
        private bool copyIsEnabled;
        private string copyCustStr;

        /// <summary>
        /// Sets the game instance and default level options.
        /// </summary>
        /// <param name="game">The game instance to use.</param>
        public MngrEditor(MainLoop game)
        {
            this.game = game;

            //Sets up toolbar objects.
            tooltip = "";

            itemDragPos = new List<Vector2>();

            activeType = Type.Actor;
            activeItem = null;
            items = new List<ImgBlock>();

            //Sets up the camera.
            camX = 0;
            camY = 0;
            camLayer = 0;
            camZoom = 1f;

            //Updates the camera position.
            Camera = Matrix.CreateTranslation(new Vector3(-camX, -camY, 0)) *
                Matrix.CreateScale(new Vector3(camZoom, camZoom, 1)) *
                Matrix.CreateTranslation(
                new Vector3(game.GetScreenSize().X * 0.5f,
                            game.GetScreenSize().Y * 0.5f, 0));

            //Sets the default level settings.
            opGameDelay = 8;
            opMaxSteps = 0;
            opMinGoals = 0;
            opLvlLink = "";
            opSyncActors = false;
            opSyncDeath = false;

            //Sets the default copy values.
            copySignalListenChannel = copySignalSendChannel = 0;
            copySlotValueInt1 = copySlotValueInt2 = 0;
            copyActType = 0;
            copyDir = Dir.Right;
            copyIsEnabled = true;
            copyCustStr = "";
            copyType = 0;
        }

        private void Window_ClientSizeChanged(object sender, EventArgs e)
        {
            var screenSize = game.GetScreenSize();
            float yPos = screenSize.Y - bottomBarHeight;

            sprInGameLevelEditorBg.rectDest.X = screenSize.X / 2 - sprInGameLevelEditorBg.texture.Width / 2;
            sprInGameLevelEditorBg.rectDest.Y = screenSize.Y / 2 - sprInGameLevelEditorBg.texture.Height / 2;

            sprToolbar.rectDest = new SmoothRect(new Vector2(sidebarWidth, screenSize.Y - bottomBarHeight),
                screenSize.X,
                bottomBarHeight);

            bttnGameDelay.Pos = new Vector2(bttnGameDelay.Pos.X, yPos);
            bttnLvlLink.Pos = new Vector2(bttnLvlLink.Pos.X, yPos);
            bttnMaxSteps.Pos = new Vector2(bttnMaxSteps.Pos.X, yPos);
            bttnMinGoals.Pos = new Vector2(bttnMinGoals.Pos.X, yPos);
            bttnSyncActors.Pos = new Vector2(bttnSyncActors.Pos.X, yPos);
            bttnSyncDeath.Pos = new Vector2(bttnSyncDeath.Pos.X, yPos);

            bttnSignalListenChannel.Pos = new Vector2(bttnSignalListenChannel.Pos.X, yPos);
            bttnSignalSendChannel.Pos = new Vector2(bttnSignalSendChannel.Pos.X, yPos);
            bttnActionType.Pos = new Vector2(bttnActionType.Pos.X, yPos);
            bttnSlotValueInt1.Pos = new Vector2(bttnSlotValueInt1.Pos.X, yPos);
            bttnSlotValueInt2.Pos = new Vector2(bttnSlotValueInt2.Pos.X, yPos);
            bttnSlotValueString.Pos = new Vector2(bttnSlotValueString.Pos.X, yPos);
            bttnIsEnabled.Pos = new Vector2(bttnIsEnabled.Pos.X, yPos);
            bttnDir.Pos = new Vector2(bttnDir.Pos.X, yPos);
            sprSidebar.rectDest.Height = screenSize.Y;
        }

        /// <summary>
        /// Sets the default block properties based on the block type.
        /// </summary>
        private void SetDefaults(ImgBlock block)
        {
            switch (block.BlockType)
            {
                case Type.Click:
                case Type.Panel:
                    block.SignalSendChannel = 1;
                    block.ActionType = 5;
                    break;
                case Type.EAuto:
                    block.SignalSendChannel = 1;
                    block.ActionType = 5;
                    block.SlotValueInt1 = 10;
                    break;
                case Type.ELight:
                    block.ActionType = 5;
                    block.SlotValueInt2 = 1;
                    break;
                case Type.EPusher:
                    block.ActionType = 5;
                    break;
                case Type.Gate:
                    block.ActionType = 5;
                    block.SlotValueInt2 = 1;
                    break;
                case Type.Rotate:
                    block.ActionType = 5;
                    block.SlotValueInt1 = 2;
                    break;
                case Type.Turret:
                    block.ActionType = 5;
                    block.SlotValueInt1 = 10;
                    block.SlotValueInt2 = 4;
                    break;
            }
        }

        ///<summary>
        ///Loads relevant graphics into memory.
        ///
        /// Dependencies: MainLoop.cs, MngrLvl.cs
        /// </summary>
        public void LoadContent()
        {
            //Loads sidebar assets.
            PropButton.LoadContent(game.Content);

            TexInGameLevelEditorBg = game.Content.Load<Texture2D>("Content/Sprites/Gui/sprInGameLevelEditorBg");
            sprInGameLevelEditorBg = new Sprite(true, TexInGameLevelEditorBg);

            //Sets up the bottom toolbar.
            sprToolbar = new Sprite(true, MngrLvl.TexPixel);
            sprToolbar.color = Color.Gray;
            sprToolbar.alpha = 0.5f;

            //Sets up toolbar buttons.
            static int GetXPos(int i) => MainLoop.TileSize + (MainLoop.TileSize + 2) * i; // 2 is padding
            float yPos = game.GetScreenSize().Y - bottomBarHeight;

            //Level settings.
            bttnGameDelay = new PropButton(game, new Sprite(true, PropButton.TexOpGameDelay), new Vector2(GetXPos(0), yPos));
            bttnLvlLink = new PropButton(game, new Sprite(true, PropButton.TexOpLvlLink), new Vector2(GetXPos(1), yPos));
            bttnMaxSteps = new PropButton(game, new Sprite(true, PropButton.TexOpMaxSteps), new Vector2(GetXPos(2), yPos));
            bttnMinGoals = new PropButton(game, new Sprite(true, PropButton.TexOpMinGoals), new Vector2(GetXPos(3), yPos));
            bttnSyncActors = new PropButton(game, new Sprite(true, PropButton.TexOpSyncActors), new Vector2(GetXPos(4), yPos));
            bttnSyncDeath = new PropButton(game, new Sprite(true, PropButton.TexOpSyncDeath), new Vector2(GetXPos(5), yPos));

            //Active item properties.
            bttnSignalListenChannel = new PropButton(game, new Sprite(true, PropButton.TexPropActionInd1), new Vector2(GetXPos(0), yPos));
            bttnSignalSendChannel = new PropButton(game, new Sprite(true, PropButton.TexPropActionInd2), new Vector2(GetXPos(1), yPos));
            bttnActionType = new PropButton(game, new Sprite(true, PropButton.TexPropActionType), new Vector2(GetXPos(2), yPos));
            bttnSlotValueInt1 = new PropButton(game, new Sprite(true, PropButton.TexPropCustInt1), new Vector2(GetXPos(3), yPos));
            bttnSlotValueInt2 = new PropButton(game, new Sprite(true, PropButton.TexPropCustInt2), new Vector2(GetXPos(4), yPos));
            bttnSlotValueString = new PropButton(game, new Sprite(true, PropButton.TexPropCustStr), new Vector2(GetXPos(5), yPos));
            bttnIsEnabled = new PropButton(game, new Sprite(true, PropButton.TexPropIsEnabled), new Vector2(GetXPos(6), yPos));
            bttnDir = new PropButton(game, new Sprite(true, PropButton.TexPropDir), new Vector2(GetXPos(7), yPos));

            //Sets up the sidebar.
            sprSidebar = new Sprite(true, MngrLvl.TexPixel)
            {
                color = Color.Gray,
                alpha = 0.5f,
                rectDest = new SmoothRect(Vector2.Zero, sidebarWidth, game.GetScreenSize().Y)
            };

            //Sets up the selectable blocks.
            itemTypes = new List<ImgType>
            {
                new(game, Type.Actor),
                new(game, Type.Wall),

                new(game, Type.Finish),
                new(game, Type.Goal),

                new(game, Type.Floor),

                new(game, Type.Teleporter),
                new(game, Type.Key),
                new(game, Type.Lock),
                new(game, Type.Ice),
                new(game, Type.Crate),
                new(game, Type.CrateHole),

                new(game, Type.Spike),
                new(game, Type.Enemy),

                new(game, Type.Turret),
                new(game, Type.Mirror),
                new(game, Type.LaserActuator),

                new(game, Type.Panel),
                new(game, Type.EAuto),
                new(game, Type.Click),

                new(game, Type.MultiWay),
                new(game, Type.Stairs),

                new(game, Type.Belt),
                new(game, Type.Coin),
                new(game, Type.CoinLock),
                new(game, Type.Health),

                new(game, Type.Gate),
                new(game, Type.ELight),
                new(game, Type.EPusher),
                new(game, Type.Filter),
                new(game, Type.Spawner),

                new(game, Type.Checkpoint),

                new(game, Type.Freeze),
                new(game, Type.Thaw),
                new(game, Type.Rotate),
                new(game, Type.Message),
            };

            game.Window.ClientSizeChanged += Window_ClientSizeChanged;
            Window_ClientSizeChanged(null, null);
        }

        /// <summary>
        /// Returns the given coordinates converted from view space to
        /// world space, to work with the camera matrix.
        /// </summary>
        public Vector2 GetCoords(float x, float y)
        {
            return Vector2.Transform
                (new Vector2(x, y), Matrix.Invert(Camera));
        }

        /// <summary>
        /// Returns the mouse coordinates converted from view space to
        /// world space, to work with the camera matrix.
        /// </summary>
        public Vector2 GetCoordsMouse()
        {
            return Vector2.Transform(new Vector2(game.MsState.X,
                game.MsState.Y), Matrix.Invert(Camera));
        }

        /// <summary>
        /// Updates all block logic.
        /// </summary>
        public void Update()
        {
            //Records mouse positions for convenience.
            int mouseX = (int)GetCoordsMouse().X;
            int mouseY = (int)GetCoordsMouse().Y;

            //Resets the tooltip.
            tooltip = "";

            #region Handles input controls
            if (game.IsActive)
            {
                //Enables scrolling the sidebar.
                if (game.MsState.X <= sidebarWidth)
                {
                    if (game.MsState.ScrollWheelValue <
                        game.MsStateOld.ScrollWheelValue)
                    {
                        sidebarScroll -= MainLoop.TileSize;

                        //Clamps the scrolling to the last item in the list.
                        if (sidebarScroll < itemTypes.Count * -MainLoop.TileSize + (int)game.GetScreenSize().Y)
                        {
                            sidebarScroll = itemTypes.Count * -MainLoop.TileSize + (int)game.GetScreenSize().Y;
                        }
                    }
                    else if (game.MsState.ScrollWheelValue >
                        game.MsStateOld.ScrollWheelValue)
                    {
                        sidebarScroll += MainLoop.TileSize;
                        if (sidebarScroll > 0)
                        {
                            sidebarScroll = 0;
                        }
                    }
                }

                //If the mouse isn't over the toolbar or sidebar.
                if (game.MsState.X > sidebarWidth && game.MsState.Y <
                    game.GetScreenSize().Y - bottomBarHeight)
                {
                    //When the mouse is no longer held, resets holding list.
                    if (game.MsState.LeftButton == ButtonState.Released &&
                        game.MsState.RightButton == ButtonState.Released)
                    {
                        itemDragPos = new List<Vector2>();
                    }

                    #region Creating blocks.
                    //Whether the player holds Ctrl + V.
                    bool isPasting =
                        (game.KbState.IsKeyDown(Keys.LeftControl) ||
                        game.KbState.IsKeyDown(Keys.RightControl)) &&
                        game.KbState.IsKeyDown(Keys.V);

                    //If clicking to set a block or pressing Ctrl + V.
                    if ((game.MsState.LeftButton == ButtonState.Pressed &&
                        game.KbState.IsKeyUp(Keys.LeftControl) &&
                        game.KbState.IsKeyUp(Keys.RightControl)) ||
                        isPasting)
                    {
                        //Gets a list of all blocks in the current position.
                        List<GameObj> blocks = new List<GameObj>();
                        foreach (ImgBlock block in items.Where(o =>
                            o.X == (int)Math.Round(mouseX / (float)MainLoop.TileSize) &&
                            o.Y == (int)Math.Round(mouseY / (float)MainLoop.TileSize) &&
                            o.Layer == camLayer))
                        {
                            blocks.Add(Utils.BlockFromType
                                (game, block.BlockType, 0, 0, 0));
                        }

                        //If the position is open for the mouse.
                        if (!itemDragPos.Any(o =>
                            o.X == (int)Math.Round(mouseX / (float)MainLoop.TileSize) &&
                            o.Y == (int)Math.Round(mouseY / (float)MainLoop.TileSize)))
                        {
                            //If the block to be placed is not solid.
                            Type typeToUse = isPasting ? copyType : activeType;

                            bool isSpeciallyStackable = typeToUse == Type.Filter
                                || typeToUse == Type.Teleporter
                                || typeToUse == Type.Rotate;

                            bool sameTypeInLoc = blocks.Any(o => o.BlockType == typeToUse);
                            bool firstPress = isPasting
                                ? game.KbStateOld.IsKeyUp(Keys.V)
                                : game.MsStateOld.LeftButton == ButtonState.Released;

                            if (!(Utils.BlockFromType
                                (game, typeToUse, 0, 0, 0).IsSolid &&
                                blocks.Any(o => o.IsSolid)) &&
                                ((isSpeciallyStackable && firstPress) || !sameTypeInLoc))
                            {
                                //Adds the position.
                                itemDragPos.Add(new Vector2(
                                    (int)Math.Round(mouseX / (float)MainLoop.TileSize),
                                    (int)Math.Round(mouseY / (float)MainLoop.TileSize)));

                                //Adds the item.
                                items.Add(new ImgBlock(game, typeToUse,
                                    (int)Math.Round(mouseX / (float)MainLoop.TileSize),
                                    (int)Math.Round(mouseY / (float)MainLoop.TileSize), camLayer));

                                // Sets the block direction.
                                items[^1].BlockDir = items[^1].BlockType == Type.Enemy
                                    ? lastDirEnemy
                                    : lastDirStrict;

                                //Sets the new item as the active one.
                                activeItem = items[^1];

                                //Copies saved properties over it.
                                if (isPasting)
                                {
                                    activeItem.SignalListenChannel = copySignalListenChannel;
                                    activeItem.SignalSendChannel = copySignalSendChannel;
                                    activeItem.SlotValueInt1 = copySlotValueInt1;
                                    activeItem.SlotValueInt2 = copySlotValueInt2;
                                    activeItem.ActionType = copyActType;
                                    activeItem.BlockDir = copyDir;
                                    activeItem.IsEnabled = copyIsEnabled;
                                    activeItem.Properties[Utils.PropertyNameCustomString] = copyCustStr;
                                    activeItem.AdjustSprite();
                                    activeItem.BlockType = typeToUse;
                                }
                                else
                                {
                                    SetDefaults(activeItem);
                                }

                                //Adds a floor panel if there aren't any.
                                if (typeToUse != Type.Floor && !blocks.Any(o => o.BlockType == Type.Floor))
                                {
                                    items.Add(new ImgBlock(game, Type.Floor,
                                        (int)Math.Round(mouseX / (float)MainLoop.TileSize),
                                        (int)Math.Round(mouseY / (float)MainLoop.TileSize),
                                        camLayer));
                                }

                                //Resets all button visibility.
                                bttnSignalListenChannel.IsVisible = true;
                                bttnSignalSendChannel.IsVisible = true;
                                bttnActionType.IsVisible = true;
                                bttnDir.IsVisible = true;
                                bttnSlotValueInt1.IsVisible = true;
                                bttnSlotValueInt2.IsVisible = true;
                                bttnIsEnabled.IsVisible = true;
                                bttnSlotValueString.IsVisible = true;
                            }
                        }
                    }
                    #endregion
                    #region Deleting blocks.
                    if (game.MsState.RightButton == ButtonState.Pressed)
                    {
                        //Gets a list of all items in the grid location.
                        List<ImgBlock> tempList = items.Where(o =>
                            o.X == (int)Math.Round(mouseX / (float)MainLoop.TileSize) &&
                            o.Y == (int)Math.Round(mouseY / (float)MainLoop.TileSize) &&
                            o.Layer == camLayer).ToList();

                        //If the position is open, removes the item.
                        if (!itemDragPos.Any(o =>
                            o.X == (int)Math.Round(mouseX / (float)MainLoop.TileSize) &&
                            o.Y == (int)Math.Round(mouseY / (float)MainLoop.TileSize)))
                        {
                            //Adds the position.
                            itemDragPos.Add(new Vector2(
                                (int)Math.Round(mouseX / (float)MainLoop.TileSize),
                                (int)Math.Round(mouseY / (float)MainLoop.TileSize)));

                            //Removes the topmost item in the list.
                            if (tempList.Count > 0)
                            {
                                //Organizes by depth so top item is topmost.
                                tempList = tempList.OrderBy(o => o.BlockSprite.depth).ToList();

                                //Removes the item.
                                items.Remove(tempList[0]);

                                //Removes the active item status as necessary.
                                if (activeItem == tempList[0])
                                {
                                    activeItem = null;
                                }

                                //Removes the floor block if it stands alone.
                                tempList.RemoveAt(0);
                                if (!tempList.Any(o => o.BlockType != Type.Floor))
                                {
                                    foreach (ImgBlock item in tempList)
                                    {
                                        items.Remove(item);

                                        //Removes the active item status as necessary.
                                        if (activeItem == item)
                                        {
                                            activeItem = null;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    #endregion
                    #region Selecting blocks.
                    if (game.MsState.LeftButton == ButtonState.Pressed &&
                        game.MsStateOld.LeftButton == ButtonState.Released &&
                        (game.KbState.IsKeyDown(Keys.LeftControl) ||
                        game.KbState.IsKeyDown(Keys.RightControl)))
                    {
                        //Gets a list of all objects under the mouse.
                        List<ImgBlock> tempList = items.Where(o =>
                            o.X == (int)Math.Round(mouseX / (float)MainLoop.TileSize) &&
                            o.Y == (int)Math.Round(mouseY / (float)MainLoop.TileSize) &&
                            o.Layer == camLayer).ToList();

                        //Organizes the list by depth.
                        if (tempList.Count != 0)
                        {
                            tempList.OrderBy(o => o.BlockSprite.depth);
                            tempList.Reverse();

                            //Toggles active item status on the block.
                            if (tempList[0] != activeItem)
                            {
                                activeItem = tempList[0];
                                bttnSignalListenChannel.IsVisible = true;
                                bttnSignalSendChannel.IsVisible = true;
                                bttnActionType.IsVisible = true;
                                bttnDir.IsVisible = true;
                                bttnSlotValueInt1.IsVisible = true;
                                bttnSlotValueInt2.IsVisible = true;
                                bttnIsEnabled.IsVisible = true;
                                bttnSlotValueString.IsVisible = true;
                            }
                            else
                            {
                                activeItem = null;
                            }
                        }
                    }
                    #endregion
                    #region Copying block properties.
                    if (activeItem != null)
                    {
                        //If Ctrl + C is pressed.
                        if ((game.KbState.IsKeyDown(Keys.LeftControl) ||
                            game.KbState.IsKeyDown(Keys.RightControl)) &&
                            game.KbState.IsKeyDown(Keys.C) &&
                            game.KbStateOld.IsKeyUp(Keys.C))
                        {
                            copySignalListenChannel = activeItem.SignalListenChannel;
                            copySignalSendChannel = activeItem.SignalSendChannel;
                            copySlotValueInt1 = activeItem.SlotValueInt1;
                            copySlotValueInt2 = activeItem.SlotValueInt2;
                            copyActType = activeItem.ActionType;
                            copyDir = activeItem.BlockDir;
                            copyIsEnabled = activeItem.IsEnabled;
                            copyCustStr = (string)activeItem.Properties[Utils.PropertyNameCustomString];
                            copyType = activeItem.BlockType;
                        }
                    }
                    #endregion
                }

                //Handles moving up/down layers.
                if (!bttnSlotValueString.IsHovered) //If not typing text.
                {
                    if ((game.KbState.IsKeyDown(Keys.OemPlus) &&
                        game.KbStateOld.IsKeyUp(Keys.OemPlus)))
                    {
                        camLayer++;
                        itemDragPos = new List<Vector2>(); //resets drag list.
                    }
                    if ((game.KbState.IsKeyDown(Keys.OemMinus) &&
                        game.KbStateOld.IsKeyUp(Keys.OemMinus)))
                    {
                        camLayer--;
                        itemDragPos = new List<Vector2>(); //resets drag list.
                    }
                }
            }
            #endregion
            #region Handles camera functionality.
            if (game.IsActive)
            {
                //Enables basic zooming.
                if (game.MsState.X > sidebarWidth && game.MsState.Y <
                    game.GetScreenSize().Y - bottomBarHeight)
                {
                    if (game.MsState.ScrollWheelValue >
                        game.MsStateOld.ScrollWheelValue)
                    {
                        if (camZoom >= 2)
                        {
                            camZoom = 2;
                        }
                        else
                        {
                            //Works around inherent floating point error.
                            camZoom = ((int)Math.Round(10 *
                                (camZoom + 0.1))) / 10f;
                        }
                    }
                    else if (game.MsState.ScrollWheelValue <
                        game.MsStateOld.ScrollWheelValue)
                    {
                        if (camZoom <= 0.5)
                        {
                            camZoom = 0.5f;
                        }
                        else
                        {
                            //Works around inherent floating point error.
                            camZoom = ((int)Math.Round(10 *
                                (camZoom - 0.1))) / 10f;
                        }
                    }
                }

                //Handles moving the camera.
                if (!bttnSlotValueString.IsHovered)
                {
                    if ((game.KbState.IsKeyDown(Keys.D) ||
                        (game.KbState.IsKeyDown(Keys.Right))))
                    {
                        camX += 8;
                    }
                    else if ((game.KbState.IsKeyDown(Keys.A) ||
                        (game.KbState.IsKeyDown(Keys.Left))))
                    {
                        camX -= 8;
                    }
                    if ((game.KbState.IsKeyDown(Keys.S) ||
                        (game.KbState.IsKeyDown(Keys.Down))))
                    {
                        camY += 8;
                    }
                    else if ((game.KbState.IsKeyDown(Keys.W) ||
                        (game.KbState.IsKeyDown(Keys.Up))))
                    {
                        camY -= 8;
                    }
                }
            }

            //Updates the camera position.
            Camera = Matrix.CreateTranslation(new Vector3(-camX, -camY, 0)) *
                Matrix.CreateScale(new Vector3(camZoom, camZoom, 1)) *
                Matrix.CreateTranslation(
                new Vector3(game.GetScreenSize().X * 0.5f,
                            game.GetScreenSize().Y * 0.5f, 0));
            #endregion
            #region Handles toolbar, sidebar, and block properties.
            //Always updates the active item's sprite to account for changes.
            if (activeItem == null)
            {
                //Updates toolbar buttons.
                bttnGameDelay.Update();
                bttnLvlLink.Update();
                bttnMaxSteps.Update();
                bttnMinGoals.Update();
                bttnSyncActors.Update();
                bttnSyncDeath.Update();

                #region Handles changing level settings / block properties.
                if (bttnGameDelay.IsHovered)
                {
                    //Left clicking or scrolling up.
                    if ((game.MsState.LeftButton == ButtonState.Pressed &&
                        game.MsStateOld.LeftButton == ButtonState.Released) ||
                        (game.MsState.ScrollWheelValue >
                        game.MsStateOld.ScrollWheelValue))
                    {
                        opGameDelay++;
                    }

                    //Right clicking or scrolling down.
                    if ((game.MsState.RightButton == ButtonState.Pressed &&
                        game.MsStateOld.RightButton == ButtonState.Released) ||
                        (game.MsState.ScrollWheelValue <
                        game.MsStateOld.ScrollWheelValue))
                    {
                        opGameDelay--;
                    }
                }
                else if (bttnLvlLink.IsHovered)
                {
                    //Left clicking or scrolling up.
                    if ((game.MsState.LeftButton == ButtonState.Pressed &&
                        game.MsStateOld.LeftButton == ButtonState.Released) ||
                        (game.MsState.ScrollWheelValue >
                        game.MsStateOld.ScrollWheelValue))
                    {
                        // If the window is fullscreened and OpenFileDialog is used, the game hangs (white screen, unresponsive).
                        bool abortFullscreen = game.FullscreenHandler.IsFullscreen;
                        if (abortFullscreen) { game.FullscreenHandler.ToggleFullscreen(); }

                        //Opens a file to determine it exists. If so, uses
                        OpenFileDialog dlg = new();

                        //Sets the initial directory for user friendliness.
                        if (opLvlLink != "")
                        {
                            dlg.InitialDirectory = Path.GetDirectoryName(opLvlLink);
                        }

                        //Shows the dialog and sets the level link if it can.
                        if (dlg.ShowDialog() == DialogResult.OK)
                        {
                            opLvlLink = dlg.FileName;
                        }

                        if (abortFullscreen) { game.FullscreenHandler.ToggleFullscreen(); }
                    }

                    //Right clicking or scrolling down.
                    if ((game.MsState.RightButton == ButtonState.Pressed &&
                        game.MsStateOld.RightButton == ButtonState.Released) ||
                        (game.MsState.ScrollWheelValue < game.MsStateOld.ScrollWheelValue))
                    {
                        opLvlLink = "";
                    }
                }
                else if (bttnMaxSteps.IsHovered)
                {
                    //Left clicking or scrolling up.
                    if ((game.MsState.LeftButton == ButtonState.Pressed &&
                        game.MsStateOld.LeftButton == ButtonState.Released) ||
                        (game.MsState.ScrollWheelValue > game.MsStateOld.ScrollWheelValue))
                    {
                        opMaxSteps++;
                    }

                    //Right clicking or scrolling down.
                    if ((game.MsState.RightButton == ButtonState.Pressed &&
                        game.MsStateOld.RightButton == ButtonState.Released) ||
                        (game.MsState.ScrollWheelValue < game.MsStateOld.ScrollWheelValue))
                    {
                        opMaxSteps--;
                    }
                }
                else if (bttnMinGoals.IsHovered)
                {
                    //Left clicking or scrolling up.
                    if ((game.MsState.LeftButton == ButtonState.Pressed &&
                        game.MsStateOld.LeftButton == ButtonState.Released) ||
                        (game.MsState.ScrollWheelValue > game.MsStateOld.ScrollWheelValue))
                    {
                        opMinGoals++;
                    }

                    //Right clicking or scrolling down.
                    if ((game.MsState.RightButton == ButtonState.Pressed &&
                        game.MsStateOld.RightButton == ButtonState.Released) ||
                        (game.MsState.ScrollWheelValue < game.MsStateOld.ScrollWheelValue))
                    {
                        opMinGoals--;
                    }
                }
                else if (bttnSyncActors.IsHovered)
                {
                    //Left clicking or scrolling up.
                    if ((game.MsState.LeftButton == ButtonState.Pressed &&
                        game.MsStateOld.LeftButton == ButtonState.Released) ||
                        (game.MsState.ScrollWheelValue > game.MsStateOld.ScrollWheelValue))
                    {
                        opSyncActors = true;
                    }

                    //Right clicking or scrolling down.
                    if ((game.MsState.RightButton == ButtonState.Pressed &&
                        game.MsStateOld.RightButton == ButtonState.Released) ||
                        (game.MsState.ScrollWheelValue < game.MsStateOld.ScrollWheelValue))
                    {
                        opSyncActors = false;
                    }
                }
                else if (bttnSyncDeath.IsHovered)
                {
                    //Left clicking or scrolling up.
                    if ((game.MsState.LeftButton == ButtonState.Pressed &&
                        game.MsStateOld.LeftButton == ButtonState.Released) ||
                        (game.MsState.ScrollWheelValue > game.MsStateOld.ScrollWheelValue))
                    {
                        opSyncDeath = true;
                    }

                    //Right clicking or scrolling down.
                    if ((game.MsState.RightButton == ButtonState.Pressed &&
                        game.MsStateOld.RightButton == ButtonState.Released) ||
                        (game.MsState.ScrollWheelValue < game.MsStateOld.ScrollWheelValue))
                    {
                        opSyncDeath = false;
                    }
                }
                #endregion
                #region Sets the bounds for block properties.
                if (opGameDelay < 2)
                {
                    opGameDelay = 2;
                }
                if (!File.Exists(opLvlLink))
                {
                    opLvlLink = "";
                }
                if (opMaxSteps < 0)
                {
                    opMaxSteps = 0;
                }
                if (opMinGoals < 0)
                {
                    opMinGoals = 0;
                }
                #endregion
            }
            else
            {
                //Updates toolbar buttons.
                bttnSignalListenChannel.Update();
                bttnSignalSendChannel.Update();
                bttnActionType.Update();
                bttnDir.Update();
                bttnSlotValueInt1.Update();
                bttnSlotValueInt2.Update();
                bttnIsEnabled.Update();
                bttnSlotValueString.Update();

                #region Handles changing active block values.
                if (bttnSignalListenChannel.IsHovered && bttnSignalListenChannel.IsVisible)
                {
                    //Left clicking or scrolling up.
                    if ((game.MsState.LeftButton == ButtonState.Pressed &&
                        game.MsStateOld.LeftButton == ButtonState.Released) ||
                        (game.MsState.ScrollWheelValue > game.MsStateOld.ScrollWheelValue))
                    {
                        activeItem.SignalListenChannel++;
                    }

                    //Right clicking or scrolling down.
                    if ((game.MsState.RightButton == ButtonState.Pressed &&
                        game.MsStateOld.RightButton == ButtonState.Released) ||
                        (game.MsState.ScrollWheelValue < game.MsStateOld.ScrollWheelValue))
                    {
                        activeItem.SignalListenChannel--;
                    }
                }
                else if (bttnSignalSendChannel.IsHovered &&
                    bttnSignalSendChannel.IsVisible)
                {
                    //Left clicking or scrolling up.
                    if ((game.MsState.LeftButton == ButtonState.Pressed &&
                        game.MsStateOld.LeftButton == ButtonState.Released) ||
                        (game.MsState.ScrollWheelValue > game.MsStateOld.ScrollWheelValue))
                    {
                        activeItem.SignalSendChannel++;
                    }

                    //Right clicking or scrolling down.
                    if ((game.MsState.RightButton == ButtonState.Pressed &&
                        game.MsStateOld.RightButton == ButtonState.Released) ||
                        (game.MsState.ScrollWheelValue < game.MsStateOld.ScrollWheelValue))
                    {
                        activeItem.SignalSendChannel--;
                    }
                }
                else if (bttnActionType.IsHovered && bttnActionType.IsVisible)
                {
                    //Left clicking or scrolling up.
                    if ((game.MsState.LeftButton == ButtonState.Pressed &&
                        game.MsStateOld.LeftButton == ButtonState.Released) ||
                        (game.MsState.ScrollWheelValue > game.MsStateOld.ScrollWheelValue))
                    {
                        activeItem.ActionType++;
                    }

                    //Right clicking or scrolling down.
                    if ((game.MsState.RightButton == ButtonState.Pressed &&
                        game.MsStateOld.RightButton == ButtonState.Released) ||
                        (game.MsState.ScrollWheelValue < game.MsStateOld.ScrollWheelValue))
                    {
                        activeItem.ActionType--;
                    }
                }
                else if (bttnDir.IsHovered && bttnDir.IsVisible)
                {
                    //Left clicking or scrolling up.
                    if ((game.MsState.LeftButton == ButtonState.Pressed &&
                        game.MsStateOld.LeftButton == ButtonState.Released) ||
                        (game.MsState.ScrollWheelValue > game.MsStateOld.ScrollWheelValue))
                    {
                        if (activeItem.BlockType == Type.Enemy)
                        {
                            activeItem.BlockDir = Utils.DirNextAll(activeItem.BlockDir);
                        }
                        else
                        {
                            activeItem.BlockDir = Utils.DirNext(activeItem.BlockDir);
                            lastDirStrict = activeItem.BlockDir;
                        }

                        lastDirEnemy = activeItem.BlockDir;
                    }

                    //Right clicking or scrolling down.
                    if ((game.MsState.RightButton == ButtonState.Pressed &&
                        game.MsStateOld.RightButton == ButtonState.Released) ||
                        (game.MsState.ScrollWheelValue < game.MsStateOld.ScrollWheelValue))
                    {
                        if (activeItem.BlockType == Type.Enemy)
                        {
                            activeItem.BlockDir = Utils.DirPrevAll(activeItem.BlockDir);
                        }
                        else
                        {
                            activeItem.BlockDir = Utils.DirPrev(activeItem.BlockDir);
                            lastDirStrict = activeItem.BlockDir;
                        }

                        lastDirEnemy = activeItem.BlockDir;
                    }
                }
                else if (bttnSlotValueInt1.IsHovered && bttnSlotValueInt1.IsVisible)
                {
                    //Left clicking or scrolling up.
                    if ((game.MsState.LeftButton == ButtonState.Pressed &&
                        game.MsStateOld.LeftButton == ButtonState.Released) ||
                        (game.MsState.ScrollWheelValue > game.MsStateOld.ScrollWheelValue))
                    {
                        activeItem.SlotValueInt1++;
                        if (activeItem.BlockType == Type.Filter && activeItem.SlotValueInt1 == 0)
                        {
                            activeItem.SlotValueInt1 = 1;
                        }
                    }

                    //Right clicking or scrolling down.
                    if ((game.MsState.RightButton == ButtonState.Pressed &&
                        game.MsStateOld.RightButton == ButtonState.Released) ||
                        (game.MsState.ScrollWheelValue < game.MsStateOld.ScrollWheelValue))
                    {
                        activeItem.SlotValueInt1--;
                        if (activeItem.BlockType == Type.Filter &&
                            activeItem.SlotValueInt1 == 0)
                        {
                            activeItem.SlotValueInt1 = -1;
                        }
                    }
                }
                else if (bttnSlotValueInt2.IsHovered && bttnSlotValueInt2.IsVisible)
                {
                    //Left clicking or scrolling up.
                    if ((game.MsState.LeftButton == ButtonState.Pressed &&
                        game.MsStateOld.LeftButton == ButtonState.Released) ||
                        (game.MsState.ScrollWheelValue >
                        game.MsStateOld.ScrollWheelValue))
                    {
                        activeItem.SlotValueInt2++;
                    }

                    //Right clicking or scrolling down.
                    if ((game.MsState.RightButton == ButtonState.Pressed &&
                        game.MsStateOld.RightButton == ButtonState.Released) ||
                        (game.MsState.ScrollWheelValue <
                        game.MsStateOld.ScrollWheelValue))
                    {
                        activeItem.SlotValueInt2--;
                    }
                }
                else if (bttnIsEnabled.IsHovered && bttnIsEnabled.IsVisible)
                {
                    //Left clicking or scrolling up.
                    if ((game.MsState.LeftButton == ButtonState.Pressed &&
                        game.MsStateOld.LeftButton == ButtonState.Released) ||
                        (game.MsState.ScrollWheelValue >
                        game.MsStateOld.ScrollWheelValue))
                    {
                        activeItem.IsEnabled = true;
                    }

                    //Right clicking or scrolling down.
                    if ((game.MsState.RightButton == ButtonState.Pressed &&
                        game.MsStateOld.RightButton == ButtonState.Released) ||
                        (game.MsState.ScrollWheelValue <
                        game.MsStateOld.ScrollWheelValue))
                    {
                        activeItem.IsEnabled = false;
                    }
                }
                else if (bttnSlotValueString.IsHovered && bttnSlotValueString.IsVisible)
                {
                    //Gets all keys and whether shift is pressed.
                    Keys[] keys = game.KbState.GetPressedKeys();
                    bool shiftPressed = false;

                    //Determines if shift is pressed.
                    if (game.KbState.IsKeyDown(Keys.LeftShift) ||
                        game.KbState.IsKeyDown(Keys.RightShift))
                    {
                        shiftPressed = true;
                    }

                    //Iterates through each held key.
                    foreach (Keys key in keys)
                    {
                        //If the key was just pressed.
                        if (game.KbState.IsKeyDown(key) &&
                            game.KbStateOld.IsKeyUp(key))
                        {
                            //Adds the smart string representation.
                            activeItem.Properties[Utils.PropertyNameCustomString] = KeyboardStateExt.KeyToString(
                                (string)activeItem.Properties[Utils.PropertyNameCustomString], key, shiftPressed);
                        }
                    }
                }
                #endregion
                #region Sets block property ranges and button visibility.
                //signal listen channel.
                if (activeItem.SignalListenChannel < 0)
                {
                    activeItem.SignalListenChannel = 0;
                }

                //signal send channel.
                if (activeItem.SignalSendChannel < 0)
                {
                    activeItem.SignalSendChannel = 0;
                }
                if (activeItem.BlockType != Type.EAuto &&
                    activeItem.BlockType != Type.Panel &&
                    activeItem.BlockType != Type.Click &&
                    activeItem.BlockType != Type.LaserActuator)
                {
                    bttnSignalSendChannel.IsVisible = false;
                }

                //action type.
                if (activeItem.ActionType < 0)
                {
                    activeItem.ActionType = 0;
                }
                else if (activeItem.BlockType == Type.Crate)
                {
                    if (activeItem.ActionType > 5) { activeItem.ActionType = 5; }
                    if (activeItem.SlotValueInt1 > 31) { activeItem.SlotValueInt1 = 31; }
                }
                else if (activeItem.BlockType == Type.EPusher)
                {
                    if (activeItem.ActionType > 5) { activeItem.ActionType = 5; }
                }
                else if (activeItem.BlockType == Type.ELight)
                {
                    if (activeItem.ActionType > 13) { activeItem.ActionType = 13; }
                }
                else if (activeItem.BlockType == Type.Gate)
                {
                    if (activeItem.ActionType > 7) { activeItem.ActionType = 7; }
                }
                else if (activeItem.BlockType == Type.Click ||
                    activeItem.BlockType == Type.EAuto ||
                    activeItem.BlockType == Type.Rotate)
                {
                    if (activeItem.ActionType > 7) { activeItem.ActionType = 7; }
                }
                else if (activeItem.BlockType == Type.Panel)
                {
                    if (activeItem.ActionType > 9) { activeItem.ActionType = 9; }
                }
                else if (activeItem.BlockType == Type.LaserActuator)
                {
                    if (activeItem.ActionType > 8) { activeItem.ActionType = 8; }
                }
                else if (activeItem.BlockType == Type.Spawner ||
                    activeItem.BlockType == Type.Filter)
                {
                    if (activeItem.ActionType - 4 > Enum.GetNames(typeof(Type)).Length)
                    {
                        activeItem.ActionType = Enum.GetNames(typeof(Type)).Length + 4;
                    }
                }
                else if (activeItem.BlockType == Type.Turret)
                {
                    if (activeItem.ActionType > 5) { activeItem.ActionType = 5; }
                }
                else if (activeItem.ActionType > 4)
                {
                    activeItem.ActionType = 4;
                }

                //slot value int 1.
                if (activeItem.BlockType == Type.Crate)
                {
                    activeItem.SlotValueInt1 = Math.Min(activeItem.SlotValueInt1, Enum.GetNames(typeof(Type)).Length);
                    if (activeItem.SlotValueInt1 < 0) { activeItem.SlotValueInt1 = 0; }
                }
                else if (activeItem.BlockType == Type.Rotate)
                {
                    //No maximum limit on rotation grid's size.
                    if (activeItem.SlotValueInt1 < 0) { activeItem.SlotValueInt1 = 0; }
                }
                else if (activeItem.BlockType == Type.LaserActuator)
                {
                    //Requires a positive amount of bullets to activate.
                    if (activeItem.SlotValueInt1 < 1) { activeItem.SlotValueInt1 = 1; }
                }
                else if (activeItem.BlockType == Type.CoinLock)
                {
                    //No maximum limit on number of coins required.
                    if (activeItem.SlotValueInt1 < 1) { activeItem.SlotValueInt1 = 0; }
                }
                else if (activeItem.BlockType == Type.EAuto)
                {
                    if (activeItem.SlotValueInt1 < 2) { activeItem.SlotValueInt1 = 2; }
                }
                else if (activeItem.BlockType == Type.ELight)
                {
                    activeItem.SlotValueInt1 = Math.Clamp(activeItem.SlotValueInt1, 0, LightSourceColor.Count);
                }
                else if (activeItem.BlockType == Type.Filter)
                {
                    if (activeItem.SlotValueInt1 == 0) { activeItem.SlotValueInt1 = 1; }
                    if (activeItem.SlotValueInt1 < -1) { activeItem.SlotValueInt1 = -1; }
                }
                else if (activeItem.BlockType == Type.Checkpoint ||
                    activeItem.BlockType == Type.Click ||
                    activeItem.BlockType == Type.Gate ||
                    activeItem.BlockType == Type.Stairs ||
                    activeItem.BlockType == Type.Panel ||
                    activeItem.BlockType == Type.MultiWay ||
                    activeItem.BlockType == Type.Teleporter)
                {
                    if (activeItem.SlotValueInt1 > 1) { activeItem.SlotValueInt1 = 1; }
                    else if (activeItem.SlotValueInt1 < 0) { activeItem.SlotValueInt1 = 0; }
                }
                else if (activeItem.BlockType == Type.Key ||
                    activeItem.BlockType == Type.Lock)
                {
                    if (activeItem.SlotValueInt1 > 9) { activeItem.SlotValueInt1 = 9; }
                    else if (activeItem.SlotValueInt1 < 0) { activeItem.SlotValueInt1 = 0; }
                }
                else if (activeItem.BlockType == Type.Turret)
                {
                    if (activeItem.SlotValueInt1 < 2) { activeItem.SlotValueInt1 = 2; }
                    else if (activeItem.SlotValueInt1 < 0) { activeItem.SlotValueInt1 = 0; }
                }
                else
                {
                    if (activeItem.SlotValueInt1 != 0) { activeItem.SlotValueInt1 = 0; }
                    bttnSlotValueInt1.IsVisible = false;
                }

                //slot value int 2.
                if (activeItem.SlotValueInt2 < 0) { activeItem.SlotValueInt2 = 0; }
                if (activeItem.BlockType == Type.Teleporter)
                {
                    //No max limit on teleporter channel.
                }
                else if (activeItem.BlockType == Type.Click ||
                    activeItem.BlockType == Type.CoinLock ||
                    activeItem.BlockType == Type.Filter ||
                    activeItem.BlockType == Type.EAuto ||
                    activeItem.BlockType == Type.Gate ||
                    activeItem.BlockType == Type.LaserActuator)
                {
                    if (activeItem.SlotValueInt2 > 1) { activeItem.SlotValueInt2 = 1; }
                }
                else if (activeItem.BlockType == Type.ELight)
                {
                    if (activeItem.SlotValueInt2 > 9) { activeItem.SlotValueInt2 = 9; }
                }
                else if (activeItem.BlockType == Type.Turret)
                {
                    if (activeItem.SlotValueInt2 < 3) { activeItem.SlotValueInt2 = 3; }
                    else if (activeItem.SlotValueInt2 > 15) { activeItem.SlotValueInt2 = 15; }
                }
                else
                {
                    if (activeItem.SlotValueInt2 > 0) { activeItem.SlotValueInt2 = 0; }
                    bttnSlotValueInt2.IsVisible = false;
                }

                //block direction.
                if (activeItem.BlockType != Type.Actor &&
                    activeItem.BlockType != Type.Belt &&
                    activeItem.BlockType != Type.Crate &&
                    activeItem.BlockType != Type.Enemy &&
                    activeItem.BlockType != Type.EPusher &&
                    activeItem.BlockType != Type.Mirror &&
                    activeItem.BlockType != Type.MultiWay &&
                    activeItem.BlockType != Type.Spawner &&
                    activeItem.BlockType != Type.Turret)
                {
                    activeItem.BlockDir = Dir.Right;
                    bttnDir.IsVisible = false;
                }

                // Only enemy blocks can have diagonal directions.
                if (activeItem.BlockType != Type.Enemy &&
                    !Utils.DirCardinal(activeItem.BlockDir))
                {
                    activeItem.BlockDir = Utils.DirNextAll(activeItem.BlockDir);
                }

                // These aren't affected by enabledness.
                if (activeItem.BlockType == Type.Checkpoint ||
                    activeItem.BlockType == Type.Coin ||
                    activeItem.BlockType == Type.Crate ||
                    activeItem.BlockType == Type.CrateHole ||
                    activeItem.BlockType == Type.Finish ||
                    activeItem.BlockType == Type.Floor ||
                    activeItem.BlockType == Type.Freeze ||
                    activeItem.BlockType == Type.Goal ||
                    activeItem.BlockType == Type.Health ||
                    activeItem.BlockType == Type.Ice ||
                    activeItem.BlockType == Type.Key ||
                    activeItem.BlockType == Type.Lock ||
                    activeItem.BlockType == Type.Mirror ||
                    activeItem.BlockType == Type.Spike ||
                    activeItem.BlockType == Type.Stairs ||
                    activeItem.BlockType == Type.Thaw ||
                    activeItem.BlockType == Type.Wall)
                {
                    activeItem.IsEnabled = true;
                    bttnIsEnabled.IsVisible = false;
                }

                // Slot value string.
                if (activeItem.BlockType != Type.Message)
                {
                    bttnSlotValueString.IsVisible = false;
                }
                #endregion

                activeItem.AdjustSprite();
            }

            //Updates the sidebar position on the far left.
            sprSidebar.rectDest.X = 0;
            sprSidebar.rectDest.Y = 0;
            
            //Updates the positions of all selectable block types.
            for (int i = 0; i < itemTypes.Count; i++)
            {
                itemTypes[i].X = 0;
                itemTypes[i].Y = i;
                itemTypes[i].Update();
            }
            #endregion
            #region Handles updating blocks.
            //Updates each item.
            foreach (ImgBlock item in items)
            {
                item.Update();
            }
            #endregion

            //Adds some level information.
            if (activeItem == null)
            {
                tooltip += $"Sidebar item: {Utils.GetBlockName(activeType)}"
                    + $". Blocks on layer {camLayer}: {items.Count((o) => o.Layer == camLayer)}";
            }
            else
            {
                tooltip += $"{activeItem.BlockType} is selected.";
            }
        }

        /// <summary>
        /// Draws blocks, including adjacent layers at 25% alpha.
        /// </summary>
        public void Draw()
        {
            sprInGameLevelEditorBg.rectDest = game.GetVisibleBounds(Camera, camZoom);
            sprInGameLevelEditorBg.Draw(game.GameSpriteBatch);

            //Organizes all items by sprite depth.
            items = items.OrderByDescending(o => o.BlockSprite.depth).ToList();

            //Draws each item.
            foreach (ImgBlock item in items)
            {
                //Renders above/below layers at 25% alpha.
                if (item.Layer == camLayer + 1 ||
                    item.Layer == camLayer - 1)
                {
                    item.BlockSprite.alpha = 0.25f;
                }
                else
                {
                    item.BlockSprite.alpha = 1;
                }

                //Only draws the current, below, and above layers.
                if (item.Layer == camLayer ||
                    item.Layer == camLayer + 1 ||
                    item.Layer == camLayer - 1)
                {
                    item.Draw();
                }
            }

            //Draws the active item indicator.
            if (activeItem != null)
            {
                game.GameSpriteBatch.Draw(MngrLvl.TexPixel, new Rectangle(
                    activeItem.X * MainLoop.TileSize - MainLoop.TileSizeHalf, activeItem.Y * MainLoop.TileSize - MainLoop.TileSizeHalf,
                    MainLoop.TileSize, MainLoop.TileSize), Color.Yellow * 0.5f);
            }

            //Draws the selector.
            if (game.MsState.X > sidebarWidth &&
                game.MsState.Y < game.GetScreenSize().Y - bottomBarHeight)
            {
                int selX = (int)Math.Round((int)GetCoordsMouse().X / (float)MainLoop.TileSize);
                int selY = (int)Math.Round((int)GetCoordsMouse().Y / (float)MainLoop.TileSize);

                ImgBlock blk;

                if (game.KbState.IsKeyDown(Keys.LeftControl) ||
                    game.KbState.IsKeyDown(Keys.RightControl))
                {
                    blk = new ImgBlock(game, copyType, selX, selY, 0)
                    {
                        SignalListenChannel = copySignalListenChannel,
                        SignalSendChannel = copySignalSendChannel,
                        ActionType = copyActType,
                        SlotValueInt1 = copySlotValueInt1,
                        SlotValueInt2 = copySlotValueInt2,
                        BlockDir = copyDir,
                        IsEnabled = copyIsEnabled
                    };
                }
                else
                {
                    blk = new ImgBlock(game, activeType, selX, selY, 0);
                    blk.BlockDir = blk.BlockType == Type.Enemy ? lastDirEnemy : lastDirStrict;

                    SetDefaults(blk);
                }

                blk.AdjustSprite();
                blk.BlockSprite.alpha = 0.5f;
                if (blk.BlockType != Type.Key && blk.BlockType != Type.Lock)
                {
                    blk.BlockSprite.color = Color.Green;
                }

                blk.Draw();
            }
        }

        /// <summary>
        /// Draws all sprites which do not shift with the camera.
        /// </summary>
        public void DrawHud()
        {
            //Draws the toolbar, sidebar, and selector.
            sprToolbar.Draw(game.GameSpriteBatch);
            sprSidebar.Draw(game.GameSpriteBatch);

            //Draws all selectable block types in the sidebar.
            for (int i = 0; i < itemTypes.Count; i++)
            {
                itemTypes[i].Draw();
            }

            //Draws toolbar buttons and information for active item.
            //Sets up a text outline to be used by all buttons.
            SpriteText tempText = new SpriteText(game.fntBold, "");
            tempText.color = Color.Black;
            tempText.drawBehavior = SpriteDraw.all;

            if (activeItem == null)
            {
                #region Draws level settings + text.
                bttnGameDelay.Draw(); //Delay in game timer.
                tempText.text = opGameDelay.ToString();
                tempText.position = new Vector2(
                    bttnGameDelay.Pos.X + MainLoop.TileSizeHalf,
                    bttnGameDelay.Pos.Y + MainLoop.TileSizeHalf);
                tempText.CenterOrigin();
                tempText.Draw(game.GameSpriteBatch);
                bttnLvlLink.Draw(); //Next level to play when completed.
                bttnMaxSteps.Draw(); //Maximum steps allowed.
                tempText.text = opMaxSteps.ToString();
                tempText.position = new Vector2(
                    bttnMaxSteps.Pos.X + MainLoop.TileSizeHalf,
                    bttnMaxSteps.Pos.Y + MainLoop.TileSizeHalf);
                tempText.CenterOrigin();
                tempText.Draw(game.GameSpriteBatch);
                bttnMinGoals.Draw(); //Minimum goals required.
                tempText.text = opMinGoals.ToString();
                tempText.position = new Vector2(
                    bttnMinGoals.Pos.X + MainLoop.TileSizeHalf,
                    bttnMinGoals.Pos.Y + MainLoop.TileSizeHalf);
                tempText.CenterOrigin();
                tempText.Draw(game.GameSpriteBatch);
                bttnSyncActors.Draw(); //If all actors copy active movements.
                tempText.text = opSyncActors ? "on" : "off";
                tempText.position = new Vector2(
                    bttnSyncActors.Pos.X + MainLoop.TileSizeHalf,
                    bttnSyncActors.Pos.Y + MainLoop.TileSizeHalf);
                tempText.CenterOrigin();
                tempText.Draw(game.GameSpriteBatch);
                bttnSyncDeath.Draw(); //If any actor death reverts to chkpt.
                tempText.text = opSyncDeath ? "on" : "off";
                tempText.position = new Vector2(
                    bttnSyncDeath.Pos.X + MainLoop.TileSizeHalf,
                    bttnSyncDeath.Pos.Y + MainLoop.TileSizeHalf);
                tempText.CenterOrigin();
                tempText.Draw(game.GameSpriteBatch);
                #endregion
                #region Draws level settings based on values.
                if (bttnGameDelay.IsHovered)
                    { tooltip = "The game timer delay. Lower values make moving objects faster, and vice versa."; }
                if (bttnLvlLink.IsHovered)
                {
                    if (opLvlLink == "") { tooltip = "The next level to play when this one is completed (if any)"; }
                    else { tooltip = $"Play this level on completion: {opLvlLink}"; }
                }
                if (bttnMaxSteps.IsHovered)
                    if (opMaxSteps == 0) { tooltip = "Change this number to limit how many steps a player is allowed to take."; }
                    else { tooltip = $"The player is limited to {opMaxSteps} or fewer steps in order to win."; }
                if (bttnMinGoals.IsHovered)
                    { tooltip = "The number of goal objects required to beat the level."; }
                if (bttnSyncActors.IsHovered)
                    { tooltip = "If on, all actors try to copy your movements."; }
                if (bttnSyncDeath.IsHovered)
                    { tooltip = "If on, the player loses when any actor loses."; }

                //Draws the tooltip in the toolbar.
                game.GameSpriteBatch.DrawString(game.fntBold, tooltip,
                    new Vector2(461, (int)game.GetScreenSize().Y - 43), // 461 and 43 are ad-hoc for visual balance.
                    Color.Black);
                #endregion
            }
            else
            {
                #region Draws active item properties + text.

                //Draws all toolbar buttons with text.
                bttnSignalListenChannel.Draw();
                bttnSignalSendChannel.Draw();
                bttnDir.Draw();
                bttnActionType.Draw();
                bttnSlotValueInt1.Draw();
                bttnSlotValueInt2.Draw();
                bttnIsEnabled.Draw();
                bttnSlotValueString.Draw();

                if (bttnSignalListenChannel.IsVisible)
                {
                    tempText.text = activeItem.SignalListenChannel.ToString();
                    tempText.position = new Vector2(
                        bttnSignalListenChannel.Pos.X + MainLoop.TileSizeHalf,
                        bttnSignalListenChannel.Pos.Y + MainLoop.TileSizeHalf);
                    tempText.CenterOrigin();
                    tempText.Draw(game.GameSpriteBatch);
                }
                if (bttnSignalSendChannel.IsVisible)
                {
                    tempText.text = activeItem.SignalSendChannel.ToString();
                    tempText.position = new Vector2(
                        bttnSignalSendChannel.Pos.X + MainLoop.TileSizeHalf,
                        bttnSignalSendChannel.Pos.Y + MainLoop.TileSizeHalf);
                    tempText.CenterOrigin();
                    tempText.Draw(game.GameSpriteBatch);
                }
                if (bttnActionType.IsVisible)
                {
                    tempText.text = activeItem.ActionType.ToString();
                    tempText.position = new Vector2(
                        bttnActionType.Pos.X + MainLoop.TileSizeHalf,
                        bttnActionType.Pos.Y + MainLoop.TileSizeHalf);
                    tempText.CenterOrigin();
                    tempText.Draw(game.GameSpriteBatch);
                }
                if (bttnSlotValueInt1.IsVisible)
                {
                    tempText.text = activeItem.SlotValueInt1.ToString();
                    tempText.position = new Vector2(
                        bttnSlotValueInt1.Pos.X + MainLoop.TileSizeHalf,
                        bttnSlotValueInt1.Pos.Y + MainLoop.TileSizeHalf);
                    tempText.CenterOrigin();
                    tempText.Draw(game.GameSpriteBatch);
                }
                if (bttnSlotValueInt2.IsVisible)
                {
                    tempText.text = activeItem.SlotValueInt2.ToString();
                    tempText.position = new Vector2(
                        bttnSlotValueInt2.Pos.X + MainLoop.TileSizeHalf,
                        bttnSlotValueInt2.Pos.Y + MainLoop.TileSizeHalf);
                    tempText.CenterOrigin();
                    tempText.Draw(game.GameSpriteBatch);
                }
                if (bttnIsEnabled.IsVisible)
                {
                    tempText.text = activeItem.IsEnabled ? "on" : "off";
                    tempText.position = new Vector2(
                        bttnIsEnabled.Pos.X + MainLoop.TileSizeHalf,
                        bttnIsEnabled.Pos.Y + MainLoop.TileSizeHalf);
                    tempText.CenterOrigin();
                    tempText.Draw(game.GameSpriteBatch);
                }
                #endregion
                #region Draws active block properties based on values.
                #region Handles SignalListenChannel
                if (bttnSignalListenChannel.IsHovered && bttnSignalListenChannel.IsVisible)
                    { tooltip = "The channel to listen for signals. This block gets activated by signals sent on that channel."; }
                #endregion
                #region Handles Signal send channel
                else if (bttnSignalSendChannel.IsHovered &&
                    bttnSignalSendChannel.IsVisible)
                    { tooltip = "The channel to raise a signal on, from blocks that can send signals."; }
                #endregion
                #region Handles ActionType
                else if (bttnActionType.IsHovered && bttnActionType.IsVisible)
                {
                    tooltip = "When activated: ";

                    if (activeItem.ActionType < 5)
                    {
                        switch (activeItem.ActionType)
                        {
                            case 0:
                                tooltip += "toggles block visibility.";
                                break;
                            case 1:
                                tooltip += "toggle block enabled. No effect for blocks that can't be disabled.";
                                break;
                            case 2:
                                tooltip += "rotates clockwise. No effect for directionless blocks.";
                                break;
                            case 3:
                                tooltip += "rotates counter-clockwise. No effect for directionless blocks.";
                                break;
                            case 4:
                                tooltip += "destroys the block.";
                                break;
                        }
                    }
                    else
                    {
                        switch (activeItem.BlockType)
                        {
                            case Type.Click:
                                if (activeItem.ActionType == 5)
                                    { tooltip += "activates linked items on trigger."; }
                                else if (activeItem.ActionType == 6)
                                    { tooltip += "deactivates linked items on trigger."; }
                                else if (activeItem.ActionType == 7)
                                    { tooltip += "de/activates linked items each other trigger."; }
                                break;
                            case Type.Crate:
                                if (activeItem.ActionType == 5)
                                    { tooltip += "breaks open."; }
                                break;
                            case Type.LaserActuator:
                                if (activeItem.ActionType == 5)
                                    { tooltip += $"activates linked items every {activeItem.SlotValueInt1} bullets."; }
                                else if (activeItem.ActionType == 6)
                                    { tooltip += $"activates/deactivates linked items every {activeItem.SlotValueInt1} bullets"; }
                                else if (activeItem.ActionType == 7)
                                    { tooltip += $"Activates linked items after {activeItem.SlotValueInt1} bullets, then deactivates after {activeItem.SlotValueInt2} frames without a bullet"; }
                                else if (activeItem.ActionType == 8)
                                    { tooltip += $"Activates after {activeItem.SlotValueInt1} bullets and doesn't deactivate."; }
                                break;
                            case Type.EAuto:
                                if (activeItem.ActionType == 5) { tooltip += "activates linked items on trigger."; }
                                else if (activeItem.ActionType == 6) { tooltip += "deactivates linked items on trigger."; }
                                else if (activeItem.ActionType == 7) { tooltip += "de/activates linked items each other trigger."; }
                                break;
                            case Type.ELight:
                                if (activeItem.ActionType == 5) { tooltip += "Brightens fast, dims fast."; }
                                if (activeItem.ActionType == 6) { tooltip += "Brightens fast, dims slower."; }
                                if (activeItem.ActionType == 7) { tooltip += "Brightens fast, dims slow."; }
                                if (activeItem.ActionType == 8) { tooltip += "Brightens slower, dims fast."; }
                                if (activeItem.ActionType == 9) { tooltip += "Brightens slower, dims slower."; }
                                if (activeItem.ActionType == 10) { tooltip += "Brightens slower, dims slow."; }
                                if (activeItem.ActionType == 11) { tooltip += "Brightens slow, dims fast."; }
                                if (activeItem.ActionType == 12) { tooltip += "Brightens slow, dims slower."; }
                                if (activeItem.ActionType == 13) { tooltip += "Brightens slow, dims slow."; }
                                break;
                            case Type.EPusher:
                                if (activeItem.ActionType == 5) { tooltip += "pushes blocks if possible."; }
                                break;
                            case Type.Filter:
                                tooltip += "becomes " +
                                    Utils.GetBlockName((Type)(activeItem.ActionType - 5)) +
                                    " if possible.";
                                break;
                            case Type.Gate:
                                if (activeItem.ActionType == 5) { tooltip += "toggles block solidity."; }
                                else if (activeItem.ActionType == 6) { tooltip += "is solid, but otherwise isn't."; }
                                else if (activeItem.ActionType == 7) { tooltip += "isn't solid, but otherwise is."; }
                                break;
                            case Type.Panel:
                                if (activeItem.ActionType == 5) { tooltip += "activates and deactivates linked items when pressed."; }
                                else if (activeItem.ActionType == 6) { tooltip += "activates linked items when pressed."; }
                                else if (activeItem.ActionType == 7) { tooltip += "activates linked items when pressed, then disables itself."; }
                                else if (activeItem.ActionType == 8) { tooltip += "deactivates linked items when pressed."; }
                                else if (activeItem.ActionType == 9) { tooltip += "deactivates linked items when pressed, then disables itself."; }
                                break;
                            case Type.Rotate:
                                if (activeItem.ActionType == 5) { tooltip += "rotates 90 degrees clockwise."; }
                                else if (activeItem.ActionType == 6) { tooltip += "rotates 90 degrees counter-clockwise."; }
                                else if (activeItem.ActionType == 7) { tooltip += "rotates 180 degrees."; }
                                break;
                            case Type.Spawner:
                                tooltip += "creates one " +
                                    Utils.GetBlockName((Type)(activeItem.ActionType - 5)) +
                                    " in the pointed direction if possible.";
                                break;
                            case Type.Turret:
                                if (activeItem.ActionType == 5) { tooltip += "fires a bullet."; }
                                break;
                        }
                    }
                }
                #endregion
                #region Handles CustDir (custom direction)
                else if (bttnDir.IsHovered && bttnDir.IsVisible)
                {
                    tooltip = activeItem.BlockDir.ToString();
                }
                #endregion
                #region Handles SlotValueInt1
                else if (bttnSlotValueInt1.IsHovered && bttnSlotValueInt1.IsVisible)
                {
                    switch (activeItem.BlockType)
                    {
                        case Type.Checkpoint:
                            if (activeItem.SlotValueInt1 == 0) { tooltip = "Saves every time an actor checkpoints."; }
                            else if (activeItem.SlotValueInt1 == 1) { tooltip = "Saves and disappears when an actor checkpoints."; }
                            break;
                        case Type.Click:
                            if (activeItem.SlotValueInt1 == 0) { tooltip = "Functions normally."; }
                            if (activeItem.SlotValueInt1 == 1) { tooltip = "Deletes itself after one use."; }
                            break;
                        case Type.CoinLock:
                            tooltip = $"Opens on contact when the player has {activeItem.SlotValueInt1} coins.";
                            break;
                        case Type.Crate:
                            if (activeItem.SlotValueInt1 > 0) { tooltip = "When crate breaks, contains: "
                                + Utils.GetBlockName((Type)(activeItem.SlotValueInt1 - 1)); }
                            else { tooltip = "Contains nothing when broken."; }
                            break;
                        case Type.EAuto:
                            tooltip = $"Triggers every {activeItem.SlotValueInt1} frames.";
                            break;
                        case Type.ELight:
                            tooltip = $"Color is: {LightSourceColor.GetColorNameByIndex(activeItem.SlotValueInt1).ToLower()}.";
                            break;
                        case Type.Filter:
                            if (activeItem.SlotValueInt1 > 0) { tooltip = $"If passed over {activeItem.SlotValueInt1} times, it activates."; }
                            else if (activeItem.SlotValueInt1 == -1) { tooltip = "Cannot be activated by passing over it."; }
                            break;
                        case Type.Gate:
                            if (activeItem.SlotValueInt1 == 0) { tooltip = "Can't close on solid objects."; }
                            else if (activeItem.SlotValueInt1 == 1) { tooltip = "May close on actors. Trapped actors automatically lose all health."; }
                            break;
                        case Type.LaserActuator:
                            tooltip = $"Absorbs {activeItem.SlotValueInt1} bullets before triggering.";
                            break;
                        case Type.Key:
                        case Type.Lock:
                            tooltip = $"Color is: {KeyColor.GetColorNameByIndex(activeItem.SlotValueInt1).ToLower()}.";
                            break;
                        case Type.MultiWay:
                            if (activeItem.SlotValueInt1 == 0) { tooltip = "The block is one-way."; }
                            else if (activeItem.SlotValueInt1 == 1) { tooltip = "The block is two-way."; }
                            break;
                        case Type.Panel:
                            if (activeItem.SlotValueInt1 == 0) { tooltip = "Works for linked items regardless of layer."; }
                            else if (activeItem.SlotValueInt1 == 1) { tooltip = "Works for linked items on the same layer only."; }
                            break;
                        case Type.Rotate:
                                tooltip = $"Rotates {activeItem.SlotValueInt1} * {activeItem.SlotValueInt1} blocks.";
                            break;
                        case Type.Stairs:
                            if (activeItem.SlotValueInt1 == 0) { tooltip = "Stairs lead upwards."; }
                            else if (activeItem.SlotValueInt1 == 1) { tooltip = "Stairs lead downwards."; }
                            break;
                        case Type.Teleporter:
                            if (activeItem.SlotValueInt1 == 0) { tooltip = "Teleporter is a sender. Blocks enter this to teleport."; }
                            else if (activeItem.SlotValueInt1 == 1) { tooltip = "Teleporter is a receiver. Blocks appear here when they teleport."; }
                            break;
                        case Type.Turret:
                            tooltip = $"Shoots bullets every {activeItem.SlotValueInt1} frames.";
                            break;
                    }
                }
                #endregion
                #region Handles SlotValueInt2
                else if (bttnSlotValueInt2.IsHovered && bttnSlotValueInt2.IsVisible)
                {
                    switch (activeItem.BlockType)
                    {
                        case Type.CoinLock:
                            if (activeItem.SlotValueInt2 == 0) { tooltip = "Doesn't subtract coins on unlocking."; }
                            else if (activeItem.SlotValueInt2 == 1) { tooltip = "Subtracts coins on unlocking."; }
                            break;
                        case Type.Click:
                        case Type.EAuto:
                            if (activeItem.SlotValueInt2 == 0) { tooltip = "Works for linked items regardless of layer."; }
                            else if (activeItem.SlotValueInt2 == 1) { tooltip = "Works for linked items on the same layer only."; }
                            break;
                        case Type.ELight:
                            if (activeItem.SlotValueInt2 == 0) { tooltip = "The light travels 1/2 tile."; }
                            else if (activeItem.SlotValueInt2 == 1) { tooltip = "The light travels 1 tile."; }
                            else if (activeItem.SlotValueInt2 == 2) { tooltip = "The light travels 2 tiles."; }
                            else if (activeItem.SlotValueInt2 == 3) { tooltip = "The light travels 3 tiles."; }
                            else if (activeItem.SlotValueInt2 == 4) { tooltip = "The light travels 4 tiles."; }
                            else if (activeItem.SlotValueInt2 == 5) { tooltip = "The light travels 5 tiles."; }
                            else if (activeItem.SlotValueInt2 == 6) { tooltip = "The light travels 8 tiles."; }
                            else if (activeItem.SlotValueInt2 == 7) { tooltip = "The light travels 10 tiles."; }
                            else if (activeItem.SlotValueInt2 == 8) { tooltip = "The light travels 20 tiles."; }
                            else { tooltip = "The light travels 40 tiles."; }
                            break;
                        case Type.Filter:
                            if (activeItem.SlotValueInt2 == 0) { tooltip = "Objects can pass through."; }
                            else if (activeItem.SlotValueInt2 == 1) { tooltip = "Objects can not pass through."; }
                            break;
                        case Type.Gate:
                            if (activeItem.SlotValueInt2 == 0) { tooltip = "Isn't solid."; }
                            else if (activeItem.SlotValueInt2 == 1) { tooltip = "Is solid."; }
                            break;
                        case Type.LaserActuator:
                            tooltip = $"Turns off after {activeItem.SlotValueInt2} frames.";
                            break;
                        case Type.Teleporter:
                            tooltip = $"Teleport channel: {activeItem.SlotValueInt2}. Senders link to receivers on the same channel to function.";
                            break;
                        case Type.Turret:
                            tooltip = $"bullet speed: {activeItem.SlotValueInt2}.";
                            break;
                    }
                }
                #endregion
                #region Handles CustStr (custom string)
                else if (bttnSlotValueString.IsHovered && bttnSlotValueString.IsVisible)
                {
                    if ((activeItem.Properties[Utils.PropertyNameCustomString] as string).Length == 0)
                    { tooltip = "Type text while hovered over button for custom text."; }
                    else
                    { tooltip = (string)activeItem.Properties[Utils.PropertyNameCustomString]; }
                }
                #endregion
                #region Handles IsEnabled.
                else if (bttnIsEnabled.IsHovered && bttnIsEnabled.IsVisible)
                {
                    if (activeItem.IsEnabled) { tooltip = "Block is enabled."; }
                    else { tooltip = "Block is disabled."; }
                }
                #endregion
                #endregion

                //Draws the tooltip in the toolbar after buttons (2 is tile padding, 8 is the tile count, 43 is ad-hoc for visual balance).
                game.GameSpriteBatch.DrawString(game.fntBold, tooltip,
                    new Vector2(MainLoop.TileSize + (MainLoop.TileSize + 2) * 8, (int)game.GetScreenSize().Y - 43),
                    Color.Black);
            }
        }

        /// <summary>
        /// Returns an equivalent MazeBlock list to the representative
        /// ImgBlock list for actual use.
        /// </summary>
        public void LoadTest()
        {
            //Loads the level from the editor for testing.
            List<GameObj> instancedBlocks = new();
            List<GameObj> instancedDecor = new();

            foreach (ImgBlock item in items)
            {
                GameObj block = Utils.BlockFromType(game, item.BlockType, item.X, item.Y, item.Layer);
                block.SignalListenChannel = item.SignalListenChannel;
                block.SignalSendChannel = item.SignalSendChannel;
                block.ActionType = item.ActionType;
                block.BlockDir = item.BlockDir;
                block.SlotValueInt1 = item.SlotValueInt1;
                block.SlotValueInt2 = item.SlotValueInt2;
                block.SlotValueString = item.Properties[Utils.PropertyNameCustomString].ToString();
                block.IsEnabled = item.IsEnabled;
                block.Properties = new(item.Properties);

                if (block.IsDecor)
                {
                    instancedDecor.Add(block);
                }
                else
                {
                    instancedBlocks.Add(block);
                }
                
            }

            game.mngrLvl.LevelStart(instancedBlocks, instancedDecor);
        }

        /// <summary>
        /// Loads a level from a file with a given path.
        /// Returns true if loaded, false otherwise.
        /// </summary>
        /// <param name="path">The path and filename.</param>
        public bool LoadEdit(string path)
        {
            if (File.Exists(path))
            {
                //Creates a stream object to the file.
                Stream stream = File.OpenRead(path);

                //Opens a text reader on the file.
                TextReader txtRead = new StreamReader(stream);

                //Gets all block items with each entry as a block.
                //Includes level settings.
                List<string> strItems =
                    txtRead.ReadToEnd().Split('|').ToList();

                //Closes the text reader.
                txtRead.Close();

                //Clears the level if the loaded level has content.
                if (strItems.Count > 0)
                {
                    items.Clear();
                }
                else
                {
                    return false;
                }

                for (int i = 0; i < strItems.Count; i++)
                {
                    //Gets all parts of the string separately.
                    List<string> strBlock =
                        strItems[i].Split(',').ToList();

                    //If there is content.
                    if (strBlock.Count != 0)
                    {
                        //If the current object is level settings.
                        if (strBlock[0] == "ops")
                        {
                            //The format must have 7 parts.
                            if (strBlock.Count != 7)
                            {
                                continue;
                            }

                            int.TryParse(strBlock[1], out opGameDelay);
                            opLvlLink = strBlock[2];
                            int.TryParse(strBlock[3], out opMaxSteps);
                            int.TryParse(strBlock[4], out opMinGoals);
                            Boolean.TryParse(strBlock[5], out opSyncActors);
                            Boolean.TryParse(strBlock[6], out opSyncDeath);
                        }

                        //If the current object is a block.
                        else if (strBlock[0] == "blk")
                        {
                            //The format must have 13 parts.
                            if (strBlock.Count != 13)
                            {
                                continue;
                            }

                            //Sets up value containers.
                            ImgBlock tempBlock;
                            Type tempType;
                            int tempX, tempY, tempLayer;
                            int tempAInd, tempAInd2, tempAType;
                            int tempInt1, tempInt2;
                            bool tempEnabled;

                            //Gets all values.
                            Enum.TryParse(strBlock[1], out tempType);
                            int.TryParse(strBlock[2], out tempX);
                            int.TryParse(strBlock[3], out tempY);
                            int.TryParse(strBlock[4], out tempLayer);
                            int.TryParse(strBlock[5], out tempAInd);
                            int.TryParse(strBlock[6], out tempAInd2);
                            int.TryParse(strBlock[7], out tempAType);
                            int.TryParse(strBlock[8], out tempInt1);
                            int.TryParse(strBlock[9], out tempInt2);
                            Boolean.TryParse(strBlock[11],
                                out tempEnabled);

                            //Creates and adds the block with the values.
                            tempBlock = new ImgBlock(game, tempType,
                                tempX, tempY, tempLayer);
                            tempBlock.SignalListenChannel = tempAInd;
                            tempBlock.SignalSendChannel = tempAInd2;
                            tempBlock.ActionType = tempAType;
                            tempBlock.SlotValueInt1 = tempInt1;
                            tempBlock.SlotValueInt2 = tempInt2;
                            tempBlock.BlockDir = (Dir)Enum.Parse(typeof(Dir),
                                strBlock[10]);
                            tempBlock.IsEnabled = tempEnabled;
                            tempBlock.Properties[Utils.PropertyNameCustomString] =
                                strBlock[12].Replace("\t", ",");
                            items.Add(tempBlock);

                            //Adjusts the block sprite.
                            tempBlock.AdjustSprite();
                        }
                    }
                }

                //Closes resources.
                stream.Close();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Loads a level from a file, returning true if successful, false otherwise.
        /// </summary>
        public bool LoadEdit()
        {
            // If the window is fullscreened and OpenFileDialog is used, the game hangs (white screen, unresponsive).
            bool abortFullscreen = game.FullscreenHandler.IsFullscreen;
            if (abortFullscreen) { game.FullscreenHandler.ToggleFullscreen(); }

            //Creates a dialog to display.
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Level files (*.lvl)|*.lvl|txt files (*.txt)|*.txt";

            //If a result is chosen from the dialog.
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                if (abortFullscreen) { game.FullscreenHandler.ToggleFullscreen(); }
                return LoadEdit(dlg.FileName);
            }

            if (abortFullscreen) { game.FullscreenHandler.ToggleFullscreen(); }
            return false;
        }

        /// <summary>
        /// Writes a level to a file in binary.
        /// </summary>
        /// <param name="path">The path and filename.</param>
        public void LevelSave()
        {
            // If the window is fullscreened and SaveFileDialog is used, the game hangs (white screen, unresponsive).
            bool abortFullscreen = game.FullscreenHandler.IsFullscreen;
            if (abortFullscreen) { game.FullscreenHandler.ToggleFullscreen(); }

            //Creates a stream object.
            Stream stream;

            //Creates a dialog to display.
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = "Level files (*.lvl)|*.lvl|txt files (*.txt)|*.txt";

            //If a result is chosen from the dialog.
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                //If the stream is in working order.
                if ((stream = dlg.OpenFile()) != null)
                {

                    //Opens a text writer on the string.
                    TextWriter txtWrite = new StreamWriter(stream);

                    //Writes the version.
                    txtWrite.Write("v1.0.0|");

                    //Writes all level settings.
                    txtWrite.Write("ops,");
                    txtWrite.Write(opGameDelay + ",");
                    txtWrite.Write(opLvlLink + ",");
                    txtWrite.Write(opMaxSteps + ",");
                    txtWrite.Write(opMinGoals + ",");
                    txtWrite.Write(opSyncActors + ",");
                    txtWrite.Write(opSyncDeath + "|");

                    //Writes all block items. Converts text , to tabs.
                    foreach (ImgBlock item in game.mngrEditor.items)
                    {
                        txtWrite.Write("blk,");
                        txtWrite.Write((int)item.BlockType + ",");
                        txtWrite.Write(item.X + ",");
                        txtWrite.Write(item.Y + ",");
                        txtWrite.Write(item.Layer + ",");
                        txtWrite.Write(item.SignalListenChannel + ",");
                        txtWrite.Write(item.SignalSendChannel + ",");
                        txtWrite.Write(item.ActionType + ",");
                        txtWrite.Write(item.SlotValueInt1 + ",");
                        txtWrite.Write(item.SlotValueInt2 + ",");
                        txtWrite.Write(item.BlockDir + ",");
                        txtWrite.Write(item.IsEnabled + ",");
                        txtWrite.Write((item.Properties[Utils.PropertyNameCustomString] as string).Replace(",", "\t") + "|");
                    }

                    //Closes resources.
                    txtWrite.Close();
                    stream.Close();
                }
            }

            if (abortFullscreen) { game.FullscreenHandler.ToggleFullscreen(); }
        }
    }
}