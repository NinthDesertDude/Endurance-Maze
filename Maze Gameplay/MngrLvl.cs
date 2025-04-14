using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Penumbra;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Keys = Microsoft.Xna.Framework.Input.Keys;

namespace EnduranceTheMaze
{
    /// <summary>
    /// The level manager. Handles loading/saving levels and all gameplay
    /// logic.
    /// 
    /// Dependencies: MainLoop.cs, maze block textures.
    /// </summary>
    public class MngrLvl
    {
        //Refers to the game instance.
        private MainLoop game;

        //Relevant assets.
        public static SoundEffect sndHit, sndFinish, sndWin, sndCheckpoint;

        public static Texture2D TexInGameLevelEditorBg { get; private set; }
        public static Texture2D TexPixel { get; private set; }
        public static Texture2D TexMenuHud { get; private set; }
        public static Texture2D TexFx { get; private set; }

        public static PenumbraComponent LightingEngine { get; private set; }

        //HUD assets (sprites and text).
        private Sprite sprInGameLevelEditorBg, sprHudOverlay, sprMenuHud;
        public string tooltip = "";

        /* Descriptions:
         * 1. If true, any death counts as player death.
         * 2. If true, all actors copy player movements.
         * 3. Max number of steps the player can take (0 = no effect).
         * 4. The minimum amount of goals to be eligible to win.
         * 5. The level to link to and load upon victory.
         */
        public bool opSyncDeath, opSyncActors;
        private int _lvlMaxSteps, _lvlSteps, _lvlStepsChkpt;
        private int _opReqGoals;
        public int OpMaxSteps
        {
            set
            {
                if (value >= 0)
                {
                    _lvlMaxSteps = value;
                }
            }

            get
            {
                return _lvlMaxSteps;
            }
        }
        public int LvlSteps
        {
            set
            {
                if (value >= 0)
                {
                    _lvlSteps = value;
                }
            }

            get
            {
                return _lvlSteps;
            }
        }
        public int OpReqGoals
        {
            set
            {
                if (value >= 0)
                {
                    _opReqGoals = value;
                }
            }

            get
            {
                return _opReqGoals;
            }
        }
        internal string opLvlLink;

        //Whether the level must be restarted/reverted or not.
        public bool doRestart, doRevert, doCheckpoint, doWin;

        //Whether a message is being shown or not.
        public bool isMessageShown = false;
        private bool isPaused = false;

        //When shown, this message will cover the screen until dismissed.
        public string message = "";

        //Contains all maze blocks in the level, organized by original, last
        //checkpoint, and current.
        private List<GameObj> ItemsOrig { get; set; }
        private List<GameObj> ItemsDecorOrig { get; set; }

        /// <summary>
        /// A replica list of the items as they were at last checkpoint.
        /// </summary>
        public List<GameObj> itemsChkpt;

        /// <summary>
        /// A list of all non-decor items, unorganized.
        /// </summary>
        public List<GameObj> items;

        /// <summary>
        /// A list of only the actor objects from the non-decor items list, for performance.
        /// </summary>
        public List<MazeActor> itemsJustActors;

        /// <summary>
        /// A list of all decor-only items, unorganized. These are only drawn on the same layer, with no update.
        /// </summary>
        public List<GameObj> itemsDecor;

        public MazeActor actor; //active player.
        private int _actorCoins, _actorGoals; //total coins and goals.
        private int actorCoinsChkpt, actorGoalsChkpt;

        /// <summary>
        /// The current count of coins collectively picked up by any actors.
        /// </summary>
        public int ActorCoins
        {
            set
            {
                if (value >= 0)
                {
                    _actorCoins = value;
                }
            }

            get
            {
                return _actorCoins;
            }
        }

        /// <summary>
        /// The current count of goals collectively picked up by any actors.
        /// </summary>
        public int ActorGoals
        {
            set
            {
                if (value >= 0)
                {
                    _actorGoals = value;
                }
            }

            get
            {
                return _actorGoals;
            }
        }

        //Controls the position of the screen.
        public Matrix Camera { get; private set; }
        private float camZoom;

        //An update timer for objects to utilize as needed.
        internal int countdownStart, _countdown;
        public bool IsTimerZero { get; private set; }

        /// <summary>
        /// Sets the game instance and default level options.
        /// </summary>
        /// <param name="game">The game instance to use.</param>
        public MngrLvl(MainLoop game)
        {
            this.game = game;

            //Sets default level option values.
            opSyncDeath = false;
            opSyncActors = false;
            OpMaxSteps = 0;
            LvlSteps = _lvlStepsChkpt = 0;
            OpReqGoals = 0;
            opLvlLink = "";

            //Sets default level variables.
            ActorCoins = ActorGoals = 0;
            actorCoinsChkpt = actorGoalsChkpt = 0;

            //Sets triggers to false.
            doRestart = doRevert = doCheckpoint = doWin = false;

            //Sets the timer defaults.
            _countdown = countdownStart = 8;
            IsTimerZero = false;

            //Controls the position of the screen (zoom).
            camZoom = 1;

            //Initializes the item lists.
            ItemsOrig = new();
            itemsChkpt = new();
            items = new();
            itemsJustActors = new();
            ItemsDecorOrig = new();
            itemsDecor = new();

            //Miscellaneous.
            LightingEngine?.Dispose();
            LightingEngine = new(game)
            {
                AmbientColor = new Color(20, 20, 40)
            };
        }

        private void Window_ClientSizeChanged(object sender, EventArgs e)
        {
            var screenSize = game.GetScreenSize();

            sprInGameLevelEditorBg.rectDest.X = screenSize.X / 2 - sprInGameLevelEditorBg.texture.Width / 2;
            sprInGameLevelEditorBg.rectDest.Y = screenSize.Y / 2 - sprInGameLevelEditorBg.texture.Height / 2;

            sprHudOverlay.rectDest = new SmoothRect
                (0, screenSize.Y - MainLoop.TileSize, screenSize.X, MainLoop.TileSize);
            sprMenuHud.rectDest = new SmoothRect
                (0, screenSize.Y - MainLoop.TileSize, MainLoop.TileSize * 2, MainLoop.TileSize);
        }

        ///<summary>
        ///Loads relevant graphics into memory.
        ///
        /// Dependencies: MainLoop.cs, maze block textures.
        /// </summary>
        public void LoadContent(ContentManager Content)
        {
            //Loads relevant assets.
            sndCheckpoint = Content.Load<SoundEffect>("Content/Sounds/sndCheckpoint");
            sndFinish = Content.Load<SoundEffect>("Content/Sounds/sndFinish");
            sndHit = Content.Load<SoundEffect>("Content/Sounds/sndHit");
            sndWin = Content.Load<SoundEffect>("Content/Sounds/sndWin");
            TexPixel = new Texture2D(game.GraphicsDevice, 1, 1);
            TexPixel.SetData(new Color[] { Color.White });
            TexMenuHud = game.Content.Load<Texture2D>("Content/Sprites/Gui/sprMenuHud");
            TexFx = game.Content.Load<Texture2D>("Content/Sprites/Game/sprFx");

            //Sets up hud sprites.
            TexInGameLevelEditorBg = Content.Load<Texture2D>("Content/Sprites/Gui/sprInGameLevelEditorBg");
            sprInGameLevelEditorBg = new Sprite(true, TexInGameLevelEditorBg);

            sprHudOverlay = new Sprite(true, TexPixel) { color = Color.Gray, alpha = 0.5f };
            sprMenuHud = new Sprite(true, TexMenuHud);

            //Loads all maze block textures.
            GameObj.LoadContent(game.Content); //base class.
            MazeActor.LoadContent(game.Content);
            MazeBelt.LoadContent(game.Content);
            MazeCoin.LoadContent(game.Content);
            MazeCoinLock.LoadContent(game.Content);
            MazeCrate.LoadContent(game.Content);
            MazeCrateHole.LoadContent(game.Content);
            FxCratePush.LoadContent(game.Content);
            MazeEnemy.LoadContent(game.Content);
            MazeFilter.LoadContent(game.Content);
            MazeFloor.LoadContent(game.Content);
            MazeFreeze.LoadContent(game.Content);
            MazeGate.LoadContent(game.Content);
            MazeHealth.LoadContent(game.Content);
            MazeIce.LoadContent(game.Content);
            MazeKey.LoadContent(game.Content);
            MazeLaserActuator.LoadContent(game.Content);
            MazeLock.LoadContent(game.Content);
            MazeMultiWay.LoadContent(game.Content);
            MazePanel.LoadContent(game.Content);
            MazeSpawner.LoadContent(game.Content);
            MazeSpike.LoadContent(game.Content);
            MazeStairs.LoadContent(game.Content);
            MazeTeleporter.LoadContent(game.Content);
            MazeThaw.LoadContent(game.Content);
            MazeWall.LoadContent(game.Content);
            MazeCheckpoint.LoadContent(game.Content);
            MazeEPusher.LoadContent(game.Content);
            MazeELight.LoadContent(game.Content);
            MazeEAuto.LoadContent(game.Content);
            MazeGoal.LoadContent(game.Content);
            MazeFinish.LoadContent(game.Content);
            MazeMessage.LoadContent(game.Content);
            MazeClick.LoadContent(game.Content);
            MazeRotate.LoadContent(game.Content);
            MazeTurret.LoadContent(game.Content);
            MazeTurretBullet.LoadContent(game.Content);
            MazeMirror.LoadContent(game.Content);

            game.Window.ClientSizeChanged += Window_ClientSizeChanged;
            Window_ClientSizeChanged(null, null);
        }

        /// <summary>
        /// Loads a level from a list of blocks and clears old info.
        /// </summary>
        public void LevelStart(List<GameObj> blocks, List<GameObj> decorBlocks)
        {
            //Sets the new item list.
            items.Clear();
            itemsJustActors.Clear();

            //Reset lighting engine.
            LightingEngine.Lights.Clear();
            LightingEngine.Hulls.Clear();

            foreach (GameObj block in blocks)
            {
                items.Add(block.Clone());

                if (block.BlockType == Type.Actor)
                    { itemsJustActors.Add(items[^1] as MazeActor); }
            }

            itemsDecor.Clear();
            foreach (GameObj block in decorBlocks)
            {
                itemsDecor.Add(block.Clone());
            }

            //Resets the item lists.
            ItemsOrig.Clear();
            itemsChkpt.Clear();
            ItemsDecorOrig.Clear();
            ActorCoins = 0;
            ActorGoals = 0;
            LvlSteps = 0;
            actorCoinsChkpt = 0;
            actorGoalsChkpt = 0;
            _lvlStepsChkpt = 0;

            //Sets the active actor.
            actor = itemsJustActors.FirstOrDefault() ?? actor;

            //Sets up the original and checkpoint lists.
            foreach (GameObj item in items)
            {
                ItemsOrig.Add(item.Clone());
                itemsChkpt.Add(item.Clone());
            }

            foreach (GameObj item in itemsDecor)
            {
                ItemsDecorOrig.Add(item.Clone());
            }

            //Clears any paused status.
            isPaused = false;
        }

        /// <summary>
        /// Reverts a level to clear old info.
        /// </summary>
        public void LevelRevert()
        {
            //Duplicates each item.
            items.Clear();
            itemsJustActors.Clear();

            //Reset lighting engine.
            LightingEngine.Lights.Clear();
            LightingEngine.Hulls.Clear();

            foreach (GameObj block in itemsChkpt)
            {
                items.Add(block.Clone());
                if (block.BlockType == Type.Actor) { itemsJustActors.Add(items[^1] as MazeActor); }
            }

            //Resets coins/goals/steps.
            ActorCoins = actorCoinsChkpt;
            ActorGoals = actorGoalsChkpt;
            LvlSteps = _lvlStepsChkpt;

            //Sets the active actor.
            actor = itemsJustActors.LastOrDefault();
        }

        /// <summary>
        /// Loads a level from an embedded resource to play.
        /// </summary>
        /// <param name="path">The level path and name.</param>
        public void LoadResource(string path)
        {
            Stream stream = GetType().Assembly
                .GetManifestResourceStream(path);

            LoadPlay(stream);
        }

        /// <summary>
        /// Starts playing a level directly from a URL.
        /// </summary>
        /// <param name="path"></param>
        public void LoadPlay(string path)
        {
            if (File.Exists(path))
            {
                //Creates a stream object to the file.
                Stream stream = File.OpenRead(path);

                //Loads and plays the level from the stream.
                LoadPlay(stream);
            }
        }

        /// <summary>
        /// Loads a level from a file with a given path.
        /// Works like MngrEditor.LoadEdit, but does not make itself
        /// available for editing.
        /// </summary>
        /// <param name="path">The path and filename.</param>
        public void LoadPlay(Stream stream)
        {
            //Opens a text reader on the file.
            TextReader txtRead = new StreamReader(stream);

            //Gets all block items with each entry as a block.
            //Includes level settings.
            List<string> strItems =
                txtRead.ReadToEnd().Split('|').ToList();

            //Closes the text reader.
            txtRead.Close();

            //Creates a temporary item list.
            List<GameObj> itemsTemp = new List<GameObj>();
            List<GameObj> decorTemp = new List<GameObj>();

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

                        Int32.TryParse(strBlock[1], out countdownStart);
                        opLvlLink = strBlock[2];
                        Int32.TryParse(strBlock[3], out _lvlMaxSteps);
                        Int32.TryParse(strBlock[4], out _opReqGoals);
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
                        GameObj tempBlock;
                        Type tempType;
                        int tempX, tempY, tempLayer;
                        int tempAInd, tempAInd2, tempAType;
                        int tempInt1, tempInt2;
                        bool tempEnabled;

                        //Gets all values.
                        Enum.TryParse(strBlock[1], out tempType);
                        Int32.TryParse(strBlock[2], out tempX);
                        Int32.TryParse(strBlock[3], out tempY);
                        Int32.TryParse(strBlock[4], out tempLayer);
                        Int32.TryParse(strBlock[5], out tempAInd);
                        Int32.TryParse(strBlock[6], out tempAInd2);
                        Int32.TryParse(strBlock[7], out tempAType);
                        Int32.TryParse(strBlock[8], out tempInt1);
                        Int32.TryParse(strBlock[9], out tempInt2);
                        Boolean.TryParse(strBlock[11],
                            out tempEnabled);

                        //Creates and adds the block with the values.
                        tempBlock = Utils.BlockFromType(game, tempType,
                            tempX, tempY, tempLayer);
                        tempBlock.ActionIndex = tempAInd;
                        tempBlock.ActionIndex2 = tempAInd2;
                        tempBlock.ActionType = tempAType;
                        tempBlock.CustInt1 = tempInt1;
                        tempBlock.CustInt2 = tempInt2;
                        tempBlock.BlockDir = (Dir)Enum.Parse(typeof(Dir),
                            strBlock[10]);
                        tempBlock.IsEnabled = tempEnabled;
                        tempBlock.CustStr =
                            strBlock[12].Replace("\t", ",");

                        if (tempBlock.IsDecor)
                        {
                            decorTemp.Add(tempBlock);
                        }
                        else
                        {
                            itemsTemp.Add(tempBlock);
                        }
                    }
                }
            }

            //Preps the level for gameplay.
            LevelStart(itemsTemp, decorTemp);

            //Closes resources.
            stream.Close();
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
        /// Equivalent of items.Add(), but also works in loops.
        /// </summary>
        /// <param name="item">The item to add.</param>
        public void AddItem(GameObj item)
        {
            if (item.IsDecor)
            {
                List<GameObj> newItems = new(itemsDecor) { item };
                itemsDecor = newItems;
            }
            else
            {
                List<GameObj> newItems = new(items) { item };
                items = newItems;
            }

            if (item.BlockType == Type.Actor) { itemsJustActors.Add(item as MazeActor); }
        }

        /// <summary>
        /// Equivalent of items.Remove(), but also works in loops.
        /// </summary>
        /// <param name="item">The item to remove.</param>
        public void RemoveItem(GameObj item)
        {
            List<GameObj> newItems = new List<GameObj>();

            if (item.IsDecor)
            {
                //Creates a shallow copy of the original list by copying all
                //objects except for the specified one.
                foreach (GameObj currentItem in itemsDecor)
                {
                    if (item != currentItem)
                    {
                        newItems.Add(currentItem);
                    }
                }

                itemsDecor = newItems;
            }
            else
            {
                //Creates a shallow copy of the original list by copying all
                //objects except for the specified one.
                foreach (GameObj currentItem in items)
                {
                    if (item != currentItem)
                    {
                        newItems.Add(currentItem);
                    }
                }

                items = newItems;

                if (item.BlockType == Type.Actor)
                {
                    itemsJustActors.Remove(item as MazeActor);
                }
            }

            item.UpdateLighting(false, false);
        }

        /// <summary>
        /// Updates all block logic.
        /// </summary>
        public void Update()
        {
            bool pausedDuringThisFrame = false;

            //Enables pausing the game.
            if (!isPaused && !isMessageShown &&
                game.KbState.IsKeyUp(Keys.P) &&
                game.KbStateOld.IsKeyDown(Keys.P))
            {
                isPaused = true;
                pausedDuringThisFrame = true;
            }

            //Enables basic zooming.
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
                    camZoom = ((int)Math.Round(10 * (camZoom + 0.1))) / 10f;
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
                    camZoom = ((int)Math.Round(10 * (camZoom - 0.1))) / 10f;
                }
            }

            //Resets the drawn tooltip.
            if (OpMaxSteps == 0)
            {
                tooltip = "";
            }
            else
            {
                tooltip = "steps: " + LvlSteps + " / " +
                    OpMaxSteps + " | ";
            }
            if (ActorGoals > 0 || OpReqGoals > 0)
            {
                tooltip += "goals: " + ActorGoals + " / " +
                    OpReqGoals + " | ";
            }

            //Does not update the game while a message is displayed.
            if (isPaused || isMessageShown)
            {
                if (!pausedDuringThisFrame &&
                    game.KbState.IsKeyUp(Keys.P) &&
                    game.KbStateOld.IsKeyDown(Keys.P))
                {
                    isPaused = false;
                    isMessageShown = false;
                }

                return;
            }

            //Tabs through the active actors.
            if (game.KbState.IsKeyDown(Keys.Tab) &&
                game.KbStateOld.IsKeyUp(Keys.Tab) &&
                !opSyncActors &&
                itemsJustActors.Count != 0)
            {
                actor = itemsJustActors[(itemsJustActors.IndexOf(actor) + 1) % itemsJustActors.Count];
            }

            //Updates the timer.
            _countdown--;
            if (_countdown == 0)
            {
                _countdown = countdownStart;
                IsTimerZero = true;
            }
            else
            {
                IsTimerZero = false;
            }

            #region Handles MazeTurretBullet triggering
            //Handles all bullets.
            items.ForEach(item =>
            {
                if (item.BlockType != Type.TurretBullet) { return; }

                //Moves the bullet.
                item.X += (int)Utils.DirVector(item.BlockDir).X * item.CustInt2;
                item.Y += (int)Utils.DirVector(item.BlockDir).Y * item.CustInt2;

                //Gets a list of all solids in front of the bullet.
                List<GameObj> itemsFront = items.Where(obj =>
                    Math.Abs(obj.X * MainLoop.TileSize + MainLoop.TileSizeHalf - item.X + item.CustInt2 / MazeTurret.bulletSpeedTileSizeMult) < 15 &&
                    Math.Abs(obj.Y * MainLoop.TileSize + MainLoop.TileSizeHalf - item.Y + item.CustInt2 / MazeTurret.bulletSpeedTileSizeMult) < 15 &&
                    obj.Layer == item.Layer && obj.IsSolid).ToList();

                foreach (GameObj item2 in itemsFront)
                {
                    //Damages all actors it hits.
                    if (item2.BlockType == Type.Actor)
                    {
                        (item2 as MazeActor).hp -= 25;
                        (item2 as MazeActor).PerformHurtAnimation();
                        game.playlist.Play(sndHit, item.X / MainLoop.TileSize, item.Y / MainLoop.TileSize);
                    }

                    #region Interaction: MazeMultiWay
                    //If the multiway is in the direction of the bullet.
                    if (item2.BlockType == Type.MultiWay &&
                        (item.BlockDir == item2.BlockDir ||
                        item2.IsEnabled == false ||
                        (item2.CustInt1 == 1 &&
                        item.BlockDir == Utils.DirOpp(item2.BlockDir))))
                    {
                        continue;
                    }
                    #endregion

                    #region Interaction: MazeMirror
                    //The mirrors bend or absorb the bullets.
                    else if (item2.BlockType == Type.Mirror)
                    {
                        //Bullet is coming in the opposite direction of the mirror.
                        if (item.BlockDir == Utils.DirOpp(item2.BlockDir))
                        {
                            if (!(item as MazeTurretBullet).mirrors.Contains(item2))
                            {
                                (item as MazeTurretBullet).mirrors.Add(item2);
                                item.BlockDir = Utils.DirPrev(item2.BlockDir);
                            }

                            continue;
                        }
                        //Bullet is coming in opposite to the other direction.
                        else if (item.BlockDir == Utils.DirNext(item2.BlockDir))
                        {
                            if (!(item as MazeTurretBullet).mirrors.Contains(item2))
                            {
                                (item as MazeTurretBullet).mirrors.Add(item2);
                                item.BlockDir = item2.BlockDir;
                            }

                            continue;
                        }
                        else if ((item as MazeTurretBullet).mirrors.Contains(item2))
                        {
                            continue;
                        }
                    }
                    #endregion

                    #region Interaction: MazeLaserActuator
                    //The laser actuator absorbs the bullets.
                    else if (item2 is MazeLaserActuator item2AsLaserActuator)
                    {
                        item2AsLaserActuator.ReceivedBullet();
                    }
                    #endregion

                    RemoveItem(item);
                }
            });
            #endregion

            //Handles the behavior of blocks when the timer is zero.
            if (IsTimerZero)
            {
                #region Handles MazeBelt triggering
                List<GameObj> itemsTemp, itemsTop, itemsFront;
                //Gets a list of all belt blocks.
                itemsTemp = items.Where(o => o.BlockType == Type.Belt && o.IsEnabled).ToList();

                //Tracks blocks that get moved and direction.
                //Moves all blocks in sync to avoid getting moved
                //multiple times in one update.
                List<GameObj> queueItems = new();
                List<Vector2> queuePos = new();

                foreach (GameObj belt in itemsTemp)
                {
                    //Gets a list of all objects on the belt.
                    itemsTop = items.Where(o =>
                        o.X == belt.X && o.Y == belt.Y && o.Layer == belt.Layer &&
                        o.BlockSprite.depth < belt.BlockSprite.depth &&
                        o != belt).ToList();

                    //If there are blocks on the belt.
                    if (itemsTop.Any())
                    {
                        //Gets a list of all solids in front of the belt.
                        itemsFront = items.Where(o =>
                            o.X == belt.X + Utils.DirVector(belt.BlockDir).X &&
                            o.Y == belt.Y + Utils.DirVector(belt.BlockDir).Y &&
                            o.Layer == belt.Layer && o.IsSolid).ToList();

                        #region Interaction: MazeMultiWay.cs
                        itemsFront = itemsFront.Where(o =>
                            !(o.BlockType == Type.MultiWay && o.IsEnabled &&
                            ((o.CustInt1 == 0 && o.BlockDir == belt.BlockDir) ||
                            (o.CustInt1 != 0 && (o.BlockDir == belt.BlockDir ||
                            o.BlockDir == Utils.DirOpp(belt.BlockDir)))))).ToList();
                        #endregion

                        //If nothing is blocking the belt.
                        if (itemsFront.Count == 0)
                        {
                            //Moves the items on the belt over and changes their direction.
                            foreach (GameObj itemTop in itemsTop)
                            {
                                itemTop.BlockDir = belt.BlockDir;

                                //Adds to queues to update in sync.
                                queueItems.Add(itemTop);
                                queuePos.Add(new Vector2(
                                    Utils.DirVector(belt.BlockDir).X,
                                    Utils.DirVector(belt.BlockDir).Y));
                            }
                        }
                    }
                    MazeLaserActuator.LoadContent(game.Content);
                }
                #endregion
                #region Handles MazeEnemy triggering
                //Gets a list of all enabled enemies.
                itemsTemp = items.Where(o => o.IsEnabled && o.BlockType == Type.Enemy).ToList();

                foreach (GameObj item in itemsTemp)
                {
                    //Gets a list of all solids in front of the enemy.
                    itemsFront = items.Where(o =>
                        o.X == item.X + Utils.DirVector(item.BlockDir).X &&
                        o.Y == item.Y + Utils.DirVector(item.BlockDir).Y &&
                        o.Layer == item.Layer && o.IsSolid).ToList();

                    #region Interaction: MazeMultiWay.cs
                    itemsFront = itemsFront.Where(o =>
                        !(o.BlockType == Type.MultiWay && o.IsEnabled &&
                        ((o.CustInt1 == 0 && o.BlockDir == item.BlockDir) ||
                        (o.CustInt1 != 0 && (o.BlockDir == item.BlockDir ||
                        o.BlockDir == Utils.DirOpp(item.BlockDir)))))).ToList();
                    #endregion

                    //Moves the enemy if there are no solids, otherwise
                    //bounces off the solid (damaging it if it's an actor).
                    if (itemsFront.Count == 0)
                    {
                        item.X += (int)Utils.DirVector(item.BlockDir).X;
                        item.Y += (int)Utils.DirVector(item.BlockDir).Y;
                    }
                    else
                    {
                        //Damages all actors it bounces off of.
                        foreach (GameObj item2 in itemsFront)
                        {
                            if (item2.BlockType == Type.Actor)
                            {
                                (item2 as MazeActor).hp -= 25;
                                (item2 as MazeActor).PerformHurtAnimation();
                                game.playlist.Play(sndHit, item.X, item.Y);
                            }
                        }

                        // Animation for bouncing off.
                        int xOffset = -4;
                        int yOffset = -4;
                        Vector2 itemDirVector = Utils.DirVector(item.BlockDir);
                        if (itemDirVector.Y < 0) { xOffset += MainLoop.TileSizeHalf; }
                        else if (itemDirVector.Y > 0) { xOffset += MainLoop.TileSizeHalf; yOffset += MainLoop.TileSize; }
                        if (itemDirVector.X < 0) { yOffset += MainLoop.TileSizeHalf; }
                        else if (itemDirVector.X > 0) { xOffset += MainLoop.TileSize; yOffset += MainLoop.TileSizeHalf; }
                        if (!Utils.DirCardinal(item.BlockDir))
                        {
                            xOffset -= MainLoop.TileSizeHalf;
                            yOffset -= MainLoop.TileSizeHalf;
                        }

                        Vector2 ring1Speed = Utils.DirVector(Utils.DirNext(item.BlockDir));

                        FxRing ring1 = new FxRing(
                            game,
                            item.X * MainLoop.TileSize + xOffset,
                            item.Y * MainLoop.TileSize + yOffset,
                            item.Layer, (ring1Speed.X, ring1Speed.Y), Color.Gray, 0.06f);
                        AddItem(ring1);
                        Vector2 ring2Speed = Utils.DirVector(Utils.DirPrev(item.BlockDir));
                        FxRing ring2 = new FxRing(
                            game,
                            item.X * MainLoop.TileSize + xOffset,
                            item.Y * MainLoop.TileSize + yOffset,
                            item.Layer, (ring2Speed.X, ring2Speed.Y), Color.Gray, 0.06f);
                        AddItem(ring2);

                        // Updates enemy direction.
                        item.BlockDir = Utils.DirOpp(item.BlockDir);
                    }
                }
                #endregion
                #region Handles MazeIce triggering

                //Gets a list of all ice blocks.
                itemsTemp = items.Where(o => o.BlockType == Type.Ice).ToList();

                List<GameObj> iceItemsTop = items.Where(
                    o => o.BlockType == Type.Actor || o.BlockType == Type.Enemy ||
                    o.BlockType == Type.Crate || o.BlockType == Type.Belt)
                    .ToList();

                foreach (GameObj ice in itemsTemp)
                {
                    //Gets a list of actors/enemies/crates/belts in location.
                    itemsTop = iceItemsTop.Where(o => o.X == ice.X &&
                        o.Y == ice.Y && o.Layer == ice.Layer)
                        .ToList();

                    //if there are no belts on the ice.
                    if (!itemsTop.Any(o => o.BlockType == Type.Belt
                        && o.IsEnabled))
                    {
                        foreach (GameObj block in itemsTop)
                        {
                            //Gets a list of blocks in front of the block.
                            itemsFront = items.Where(o =>
                                o.X == (int)block.X +
                                    Utils.DirVector(block.BlockDir).X &&
                                o.Y == (int)block.Y +
                                    Utils.DirVector(block.BlockDir).Y &&
                                o.Layer == block.Layer).ToList();

                            #region Interaction: MazeCrate.cs
                            if (block.BlockType == Type.Crate)
                            {
                                itemsFront = itemsFront.Where(o =>
                                    o.BlockType != Type.CrateHole).ToList();
                            }
                            #endregion
                            #region Interaction: MazeMultiWay.cs
                            itemsFront = itemsFront.Where(o =>
                                !(o.BlockType == Type.MultiWay && o.IsEnabled &&
                                ((o.CustInt1 == 0 && o.BlockDir == block.BlockDir) ||
                                (o.CustInt1 != 0 && (o.BlockDir == block.BlockDir ||
                                o.BlockDir == Utils.DirOpp(block.BlockDir))))))
                                .ToList();
                            #endregion
                            #region Interaction: MazeBelt.cs
                            //Makes it so nothing stops on belts unless they
                            //are in the total opposite direction.
                            itemsFront = itemsFront.Where(o =>
                                !(o.BlockType == Type.Belt && (!o.IsEnabled ||
                                o.BlockDir != Utils.DirOpp(block.BlockDir)))).ToList();

                            //Can slide past all non-solid, non-belt objects.
                            itemsFront = itemsFront.Where(o =>
                                o.IsSolid || (o.BlockType == Type.Belt &&
                                o.IsEnabled)).ToList();

                            //Removes disabled belts from the list so they
                            //don't slide across the ice.
                            if (block.BlockType == Type.Belt)
                            {
                                continue;
                            }
                            #endregion

                            //If no solids block the path.
                            if (itemsFront.Count == 0)
                            {
                                {
                                    queueItems.Add(block);
                                    queuePos.Add(new Vector2(
                                        Utils.DirVector(block.BlockDir).X,
                                        Utils.DirVector(block.BlockDir).Y));
                                }
                            }
                        }
                    }
                }
                #endregion

                //Updates all moved blocks in sync.
                for (int i = 0; i < queueItems.Count; i++)
                {
                    // Animation for sliding on ice.
                    FxRing dust;
                    var dirVector = Utils.DirVector(Utils.DirOpp(queueItems[i].BlockDir));

                    for (int j = 0; j < 2 + Utils.Rng.Next(2); j++)
                    {
                        dust = new FxRing(game,
                            queueItems[i].X * MainLoop.TileSize + Utils.Rng.Next(MainLoop.TileSizeHalf),
                            queueItems[i].Y * MainLoop.TileSize + MainLoop.TileSizeHalf + Utils.Rng.Next(MainLoop.TileSizeHalf),
                            queueItems[i].Layer, (0, 0), Color.White, 0.06f + Utils.Rng.Next(3) / 100f);
                        dust.BlockSprite.scaleY = 0.05f + Utils.Rng.Next(10) / 100f;
                        dust.BlockSprite.scaleX = 0.05f + Utils.Rng.Next(10) / 100f;
                        game.mngrLvl.AddItem(dust);
                    }

                    queueItems[i].X += (int)queuePos[i].X;
                    queueItems[i].Y += (int)queuePos[i].Y;
                }
            }

            //Updates each item.
            foreach (GameObj item in items)
            {
                item.Update();

                //Selects a new & valid actor when needed.
                if (actor.hp <= 0 || !actor.IsEnabled)
                {
                    if (item.BlockType == Type.Actor)
                    {
                        if ((item as MazeActor).hp > 0 &&
                            (item as MazeActor).IsEnabled)
                        {
                            actor = (MazeActor)item;
                        }
                    }
                }

                //Processes win conditions.
                if (doWin)
                {
                    break;
                }
            }

            // Updates decor (if you didn't just win).
            if (!doWin)
            {
                foreach (GameObj item in itemsDecor)
                {
                    item.Update();
                }
            }

            //Handles winning.
            if (doWin)
            {
                doWin = false;
                if (game.GmState != GameState.stateGameplay)
                {
                    if (opLvlLink == "")
                    {
                        SfxPlaylist.Play(sndWin);

                        if (game.GmState == GameState.stateGameplayEditor)
                        {
                            game.GmState = GameState.stateMenuEditor;
                        }
                        else
                        {
                            game.GmState = GameState.stateCampaignModes;
                        }
                    }
                    else
                    {
                        SfxPlaylist.Play(sndFinish);
                        game.mngrLvl.LoadPlay(opLvlLink);
                    }
                }
                else
                {
                    SfxPlaylist.Play(sndFinish);
                    game.mngrCampaign.CurrentSeries().LevelNum++;
                    if (game.mngrCampaign.CurrentSeries().LevelExists())
                    {
                        game.mngrCampaign.CurrentSeries().LoadCampaign();
                    }
                    else
                    {
                        SfxPlaylist.Play(sndWin);

                        if (game.GmState == GameState.stateGameplayEditor)
                        {
                            game.GmState = GameState.stateMenuEditor;
                        }
                        else
                        {
                            game.GmState = GameState.stateGameplaySeriesComplete;
                        }
                    }
                }
            }

            //If there are no valid actors, reverts.
            //If the max steps has been reached, reverts.
            if ((actor.hp <= 0 || !actor.IsEnabled) ||
                (OpMaxSteps != 0 && LvlSteps >= OpMaxSteps))
            {
                doRevert = true;

                // Checks to see if any actors satisfy win condition for finish line, because we don't want to fail
                // them if they reach in the exact no. steps. This also tracks how many goals are collected in the same
                // step to see if it meets the required count.
                bool onFinish = false;
                int goalsAdded = 0;

                foreach (var actor in itemsJustActors)
                {
                    onFinish = onFinish || game.mngrLvl.items.Any(o => o.BlockType == Type.Finish &&
                        o.X == actor.X && o.Y == actor.Y && o.Layer == actor.Layer);

                    goalsAdded += game.mngrLvl.items.Count(o => o.BlockType == Type.Goal &&
                    o.X == actor.X && o.Y == actor.Y && o.Layer == actor.Layer);

                    if (onFinish &&
                        game.mngrLvl.ActorGoals + goalsAdded >= game.mngrLvl.OpReqGoals)
                    {
                        doWin = true;
                        doRevert = false;
                        break;
                    }
                }
            }

            #region Player reverts/restarts level
            //If R is pressed, revert to the last checkpoint.
            if (game.KbState.IsKeyDown(Keys.R) &&
                game.KbStateOld.IsKeyUp(Keys.R))
            {
                //If control is held, restarts the whole level.
                if (game.KbState.IsKeyDown(Keys.LeftControl) ||
                    game.KbState.IsKeyDown(Keys.RightControl))
                {
                    doRestart = true;
                }
                else //otherwise, reverts to last checkpoint.
                {
                    doRevert = true;
                }
            }
            #endregion

            #region Handles checkpoints and restarting.

            //Reverts to last checkpoint if desired.
            if (doRevert)
            {
                doRevert = false;
                LevelRevert();
            }

            //Restarts the level if desired.
            else if (doRestart)
            {
                doRestart = false;
                LevelStart(new List<GameObj>(ItemsOrig), new List<GameObj>(ItemsDecorOrig));
            }

            //Saves a checkpoint if initiated.
            if (doCheckpoint)
            {
                SfxPlaylist.Play(sndCheckpoint);
                doCheckpoint = false;
                actorCoinsChkpt = ActorCoins;
                actorGoalsChkpt = ActorGoals;
                _lvlStepsChkpt = LvlSteps;

                //Checkpoints the level by overwriting the chkpt list.
                itemsChkpt = new List<GameObj>();
                foreach (GameObj item in items)
                {
                    itemsChkpt.Add(item.Clone());
                }
            }
            #endregion

            //Updates the camera position.
            Camera = Matrix.CreateTranslation(
                new Vector3(-actor.BlockSprite.rectDest.X,
                            -actor.BlockSprite.rectDest.Y, 0)) *
                Matrix.CreateScale(new Vector3(camZoom, camZoom, 1)) *
                Matrix.CreateTranslation(
                new Vector3(game.GetScreenSize().X * 0.5f,
                            game.GetScreenSize().Y * 0.5f, 0));
        }

        /// <summary>
        /// Draws blocks, including adjacent layers at 25% alpha.
        /// </summary>
        public void Draw()
        {
            //Organizes all items by sprite depth.
            List<GameObj> combinedItems = new(items.Count + itemsDecor.Count);
            combinedItems.AddRange(items);
            combinedItems.AddRange(itemsDecor);
            combinedItems = combinedItems.OrderByDescending(o => o.BlockSprite.depth).ToList();

            //Draws each item.
            SmoothRect scrnBounds = game.GetVisibleBounds(Camera, camZoom);

            //Draws a message when the screen is paused.
            if (isPaused)
            {
                //Updates the camera position.
                Camera = Matrix.CreateScale(new Vector3(camZoom, camZoom, 1));

                string pausedText = "Paused: Press P to unpause.";
                game.GameSpriteBatch.DrawString(game.fntBoldBig,
                    pausedText,
                    game.GetScreenSize() / 2 - (game.fntBoldBig.MeasureString(pausedText) / 2),
                    Color.Black);

                return;
            }

            //Draws any custom messages to the screen.
            else if (isMessageShown)
            {
                //Updates the camera position.
                Camera = Matrix.CreateScale(new Vector3(camZoom, camZoom, 1));
                Vector2 scrnCenter = scrnBounds.Center;

                game.GameSpriteBatch.DrawString(game.fntBoldBig,
                    message, scrnCenter, Color.Black, 0,
                    game.fntBoldBig.MeasureString(message) / 2,
                    1, SpriteEffects.None, 0);

                string pauseText = "Press P to continue.";
                game.GameSpriteBatch.DrawString(game.fntBoldBig,
                    pauseText, scrnCenter + new Vector2(0, 24),
                    Color.Black, 0, game.fntDefault.MeasureString(pauseText) / 2,
                    1, SpriteEffects.None, 0);

                return;
            }

            foreach (GameObj item in combinedItems)
            {
                int layerDiff = Math.Abs(actor.Layer - item.Layer);
                bool onScreen = SmoothRect.IsIntersecting(scrnBounds, item.BlockSprite.rectDest);

                // Turns lights or shadows on/off based on distance.
                bool lightsVisible = layerDiff == 0;
                bool shadowsVisible = layerDiff == 0 && onScreen;
                if (lightsVisible && !onScreen && item.Lighting.light != null)
                {
                    // Check if lights are actually off-screen.
                    var halfLightRadius = item.Lighting.light.Scale / 2;
                    var destWithLight = new SmoothRect(
                        item.BlockSprite.rectDest.Position - halfLightRadius,
                        item.BlockSprite.rectDest.Width + halfLightRadius.X,
                        item.BlockSprite.rectDest.Height + halfLightRadius.Y);

                    lightsVisible = SmoothRect.IsIntersecting(scrnBounds, destWithLight);
                }
                item.UpdateLighting(lightsVisible, shadowsVisible);

                // Don't draw items on far-away layers or when they're off-screen.
                if (layerDiff > 1 || !onScreen)
                {
                    continue;
                }

                // Draw decor only for same layer.
                // Draws non-decor for same/above/below layer.
                if (item.IsDecor)
                {
                    if (layerDiff == 0) { item.Draw(); }
                }
                else
                {
                    item.BlockSprite.alpha = layerDiff == 1 ? 0.25f : 1;
                    item.Draw();
                }
            }
        }

        /// <summary>
        /// Draws all sprites which do not shift with the camera.
        /// </summary>
        public void DrawHud()
        {
            //Sets up health and coins text.
            SpriteText hudHp = new(game.fntBold, actor.hp.ToString());
            hudHp.CenterOriginHor();
            hudHp.color = Color.LightPink;
            hudHp.drawBehavior = SpriteDraw.all;
            hudHp.position = new Vector2(MainLoop.TileSizeHalf,
                (int)game.GetScreenSize().Y - MainLoop.TileSize + 18); // 18 is ad-hoc visual balancing.

            SpriteText hudCoins = new(game.fntBold, ActorCoins.ToString());
            hudCoins.CenterOriginHor();
            hudCoins.color = Color.Yellow;
            hudCoins.drawBehavior = SpriteDraw.all;
            hudCoins.position = new Vector2(MainLoop.TileSize + MainLoop.TileSizeHalf,
                (int)game.GetScreenSize().Y - MainLoop.TileSize + 18); // 18 is ad-hoc visual balancing.

            //Draws the bottom info bar with health and coins.
            sprHudOverlay.Draw(game.GameSpriteBatch);
            sprMenuHud.Draw(game.GameSpriteBatch);
            hudHp.Draw(game.GameSpriteBatch);
            hudCoins.Draw(game.GameSpriteBatch);

            //Removes the ending item separator on the tooltip.
            if (tooltip.EndsWith("| "))
            {
                tooltip = tooltip[..^2];
            }

            //Draws the tooltip.
            game.GameSpriteBatch.DrawString(game.fntBoldBig, tooltip,
                new Vector2(MainLoop.TileSize * 2 + 4, (int)game.GetScreenSize().Y - MainLoop.TileSize + 16),
                Color.Black);
        }
    }
}