using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EnduranceTheMaze
{
    /// <summary>
    /// Actors are the characters which the game is built around. Multiples
    /// are handled separately (or mimic the active one).
    /// 
    /// Dependencies: MngrLvl, MazeBlock, MazeBelt, MazeCrate, MazeLock, MazeCoinLock.
    /// 
    /// Activation types: none
    /// 
    /// Custom properties of custInt1: none
    /// Custom properties of custInt2: none
    /// Custom properties of custStr: none
    /// </summary>
    public class MazeActor : GameObj
    {
        //Relevant assets.
        public static SoundEffect sndLockOpen, sndMoveCrate;
        public static Texture2D TexActor { get; private set; }

        //Sprite information.    
        private SpriteAtlas spriteAtlas;

        //Actor-specific variables.
        public int hp; //health points.
        public List<Color> keys; //Keys listed by their color.

        //An update timer for character movement.
        private int countdownStart, countdown;
        private bool isTimerZero;

        // Color-blend animation for when hurt.
        private int isHurtCountdownStart = 8;
        private int isHurtCountdown = 0;

        // Each direction has 3 step stages for animation (0,1,0,2) and this tracks it to help simulate a sense of movement.
        // This value increments from 0 to 3, but when it's 2 it's treated the same as 0.

        // Provides a walking animation for the actor. When they move due to player input, animatedStep increments from
        // 0-3, which is added to the frame as 0,1,0,2 respectively. On last movement, stoppedMovementTimer is set to a
        // number and decrements each update tick until it resets the animatedStep at zero.
        private int animatedStep = 0;
        private int stoppedMovingTimer = 0;

        /// <summary>Sets the block location and default values.</summary>
        /// <param name="x">The column number.</param>
        /// <param name="y">The row number.</param>
        /// <param name="layer">The layer in the maze.</param>
        public MazeActor(MainLoop game, int x, int y, int layer)
            : base(game, x, y, layer)
        {
            //Sets default values.
            IsSolid = true;
            BlockType = Type.Actor;

            //Sets default values for health and key ids.
            hp = 100;
            keys = new List<Color>();

            //Sets the timer defaults.
            countdown = countdownStart = game.mngrLvl.countdownStart;
            isTimerZero = false;

            //Sets sprite information.
            BlockSprite = new Sprite(true, TexActor);
            BlockSprite.depth = 0.1f;
            BlockSprite.drawBehavior = SpriteDraw.all;
            spriteAtlas = new SpriteAtlas(BlockSprite, MainLoop.TileSize, MainLoop.TileSize, 18, 2, 9);

            //Sets the initial position.
            BlockSprite.rectDest.X = x * MainLoop.TileSize;
            BlockSprite.rectDest.Y = y * MainLoop.TileSize;

            // Actors have an associated point light.
            Lighting = new(
                new Penumbra.PointLight
                {
                    Scale = new Vector2(MainLoop.TileSize * 10),
                    ShadowType = Penumbra.ShadowType.Solid,
                    CastsShadows = true
                }, null);
        }

        /// <summary>
        /// Loads relevant graphics into memory.
        /// </summary>
        /// <param name="Content">A game content loader.</param>
        public static void LoadContent(ContentManager Content)
        {
            sndLockOpen = Content.Load<SoundEffect>("Content/Sounds/sndLockOpen");
            sndMoveCrate = Content.Load<SoundEffect>("Content/Sounds/sndMoveCrate");
            TexActor = Content.Load<Texture2D>("Content/Sprites/Game/sprActor");
        }

        /// <summary>
        /// Returns a copy of the object.
        /// </summary>
        public override GameObj Clone()
        {
            MazeActor newBlock = new(game, X, Y, Layer);
            newBlock.CopyFrom(this);

            //Sets specific variables.
            newBlock.BlockSprite = BlockSprite;
            newBlock.spriteAtlas = new SpriteAtlas(spriteAtlas, false);
            newBlock.countdown = countdown;
            newBlock.countdownStart = countdownStart;
            newBlock.isTimerZero = isTimerZero;
            newBlock.hp = hp;
            newBlock.keys = new List<Color>(keys);

            return newBlock;
        }

        /// <summary>
        /// Responds to movement requests.
        /// </summary>
        public override void Update()
        {
            #region Updates the sprite.
            if (stoppedMovingTimer > 0)
            {
                stoppedMovingTimer--;
                if (stoppedMovingTimer == 0)
                {
                    animatedStep = 0;
                }
            }
            int animStepOffset = animatedStep == 2 ? 0 : animatedStep == 3 ? 2 : animatedStep;
            int frameoffset = (this == game.mngrLvl.actor) ? animStepOffset : animStepOffset + 9; // 9 is the clone sprite.

            //Updates the actor sprite by direction.
            //Depends on the texture frames and orientation.
            if (BlockDir == Dir.Right)
            {
                spriteAtlas.frame = frameoffset;
                BlockSprite.spriteEffects = SpriteEffects.None;
            }
            else if (BlockDir == Dir.Down)
            {
                spriteAtlas.frame = frameoffset + 3;
                BlockSprite.spriteEffects = SpriteEffects.None;
            }
            else if (BlockDir == Dir.Left)
            {
                spriteAtlas.frame = frameoffset;
                BlockSprite.spriteEffects = SpriteEffects.FlipHorizontally;
            }
            else
            {
                spriteAtlas.frame = frameoffset + 6;
                BlockSprite.spriteEffects = SpriteEffects.None;
            }
            #endregion

            #region Updates timer.
            countdown--;
            if (countdown == 0)
            {
                countdown = countdownStart;
                isTimerZero = true;
            }
            else
            {
                isTimerZero = false;
            }
            #endregion

            #region Checks for actor death.
            if (hp <= 0)
            {
                if (game.mngrLvl.opSyncDeath)
                {
                    game.mngrLvl.doRevert = true;
                }
                else
                {
                    game.mngrLvl.RemoveItem(this);
                }
            }
            #endregion

            #region Player movement
            //Captures keyboard commands to move.
            if (IsEnabled && (this == game.mngrLvl.actor ||
                game.mngrLvl.opSyncActors))
            {
                //Represents intent and ability to move.
                bool doMove = false, canMove = true;

                //Contains previous direction.
                Dir dirPrev = BlockDir;

                //If dir should be reverted. Almost never set to false.
                bool doRevertDir = true;

                //If the character hasn't met the step limit.
                //Prevents keystroke processing when app is inactive.
                if ((game.mngrLvl.OpMaxSteps == 0 ||
                    game.mngrLvl.LvlSteps < game.mngrLvl.OpMaxSteps) &&
                    game.IsActive)
                {
                    if ((game.KbState.IsKeyDown(Keys.D) &&
                        game.KbStateOld.IsKeyUp(Keys.D)) ||
                        (game.KbState.IsKeyDown(Keys.Right) &&
                        game.KbStateOld.IsKeyUp(Keys.Right)))
                    {
                        countdown = countdownStart;
                        isTimerZero = true;
                    }
                    else if ((game.KbState.IsKeyDown(Keys.S) &&
                        game.KbStateOld.IsKeyUp(Keys.S)) ||
                        (game.KbState.IsKeyDown(Keys.Down) &&
                        game.KbStateOld.IsKeyUp(Keys.Down)))
                    {
                        countdown = countdownStart;
                        isTimerZero = true;
                    }
                    else if ((game.KbState.IsKeyDown(Keys.A) &&
                        game.KbStateOld.IsKeyUp(Keys.A)) ||
                        (game.KbState.IsKeyDown(Keys.Left) &&
                        game.KbStateOld.IsKeyUp(Keys.Left)))
                    {
                        countdown = countdownStart;
                        isTimerZero = true;
                    }
                    else if ((game.KbState.IsKeyDown(Keys.W) &&
                        game.KbStateOld.IsKeyUp(Keys.W)) ||
                        (game.KbState.IsKeyDown(Keys.Up) &&
                        game.KbStateOld.IsKeyUp(Keys.Up)))
                    {
                        countdown = countdownStart;
                        isTimerZero = true;
                    }

                    //Updates the player direction based on movement.
                    //Prevents keystroke processing when app is inactive.
                    if (game.IsActive && isTimerZero)
                    {
                        if (game.KbState.IsKeyDown(Keys.D) ||
                            game.KbState.IsKeyDown(Keys.Right))
                        {
                            BlockDir = Dir.Right;
                            doMove = true;
                        }
                        else if (game.KbState.IsKeyDown(Keys.S) ||
                            game.KbState.IsKeyDown(Keys.Down))
                        {
                            BlockDir = Dir.Down;
                            doMove = true;
                        }
                        else if (game.KbState.IsKeyDown(Keys.A) ||
                            game.KbState.IsKeyDown(Keys.Left))
                        {
                            BlockDir = Dir.Left;
                            doMove = true;
                        }
                        else if (game.KbState.IsKeyDown(Keys.W) ||
                            game.KbState.IsKeyDown(Keys.Up))
                        {
                            BlockDir = Dir.Up;
                            doMove = true;
                        }
                    }

                    if (doMove)
                    {
                        //Considers all scenarios where the player can't move.
                        //Gets a list of all blocks at the location to move.
                        List<GameObj> items = game.mngrLvl.items.Where(o =>
                            o.X == X + Utils.DirVector(BlockDir).X &&
                            o.Y == Y + Utils.DirVector(BlockDir).Y &&
                            o.Layer == Layer).ToList();

                        foreach (GameObj item in items)
                        {
                            #region Interaction: MazeBelt.cs
                            //Can't move into opposing belts.
                            if (item.BlockType == Type.Belt &&
                                item.IsEnabled &&
                                item.BlockDir == Utils.DirOpp(BlockDir))
                            {
                                canMove = false;
                            }
                            #endregion

                            //Can't move if space is occupied.
                            if (item.IsSolid)
                            {
                                #region Interaction: MazeLock.cs
                                if (item.BlockType == Type.Lock)
                                {
                                    if (keys.Count == 0)
                                    {
                                        canMove = false;
                                    }

                                    bool isFound = false;
                                    for (int i = keys.Count - 1; i >= 0; i--)
                                    {
                                        if (keys[i] == item.BlockSprite.color)
                                        {
                                            isFound = true;
                                            game.mngrLvl.RemoveItem(item);
                                            keys.RemoveAt(i);
                                            game.playlist.Play(sndLockOpen, X, Y);

                                            // Animation for when a lock is opened
                                            FxRing lockBreak;
                                            int spheres = 10;
                                            double angleDifference = (2 * Math.PI) / spheres;

                                            for (int j = 0; j < spheres; j++)
                                            {
                                                double radianAngle = angleDifference * j;
                                                double ySpeed = Math.Sin(radianAngle) * 2;
                                                double xSpeed = Math.Cos(radianAngle) * 2;

                                                lockBreak = new FxRing(game,
                                                    item.X * MainLoop.TileSize + MainLoop.TileSizeHalf, item.Y * MainLoop.TileSize + MainLoop.TileSizeHalf,
                                                    item.Layer, (xSpeed, ySpeed),
                                                    item.BlockSprite.color, 0.04f);

                                                game.mngrLvl.AddItem(lockBreak);
                                            }
                                            break;
                                        }
                                    }

                                    //Prevents walking through locks.
                                    if (!isFound)
                                    {
                                        canMove = false;
                                    }
                                }
                                #endregion

                                #region Interaction: MazeCoinLock.cs
                                if (item.BlockType == Type.CoinLock)
                                {
                                    bool isOpened = false;

                                    //Opens if player has enough coins.
                                    if (game.mngrLvl.ActorCoins >= item.CustInt1)
                                    {
                                        //Subtracts the coins.
                                        if (item.CustInt2 == 1)
                                        {
                                            game.mngrLvl.ActorCoins -= item.CustInt1;
                                        }

                                        isOpened = true;
                                        game.mngrLvl.RemoveItem(item);
                                        game.playlist.Play(sndLockOpen, X, Y);

                                        // Animation for when a coin lock is opened
                                        FxRing lockBreak;
                                        int spheres = 10;
                                        double angleDifference = (2 * Math.PI) / spheres;
                                        Color color = new Color(255, 255, 0);
                                        for (int j = 0; j < spheres; j++)
                                        {
                                            double radianAngle = angleDifference * j;
                                            double ySpeed = Math.Sin(radianAngle) * 2;
                                            double xSpeed = Math.Cos(radianAngle) * 2;

                                            lockBreak = new FxRing(game,
                                                item.X * MainLoop.TileSize + MainLoop.TileSizeHalf, item.Y * MainLoop.TileSize + MainLoop.TileSizeHalf,
                                                item.Layer, (xSpeed, ySpeed),
                                                color, 0.04f);

                                            game.mngrLvl.AddItem(lockBreak);
                                        }

                                        break;
                                    }

                                    //Prevents walking through locks.
                                    if (!isOpened)
                                    {
                                        canMove = false;
                                    }
                                }
                                #endregion

                                #region Interaction: MazeCrate.cs
                                else if (item.BlockType == Type.Crate)
                                {
                                    #region Interaction: MazeIce.cs
                                    //If the actor is on ice.
                                    if (game.mngrLvl.items.Any(o =>
                                        o.BlockType == Type.Ice &&
                                        o.X == X && o.Y == Y &&
                                        o.Layer == Layer))
                                    {
                                        //If on ice without solid objects in
                                        //front, moving crates is not allowed.
                                        //This prevents moving crates to the
                                        //side of the actor while they slide.
                                        if (!game.mngrLvl.items.Any(o =>
                                            o.X == X + Utils.DirVector(dirPrev).X &&
                                            o.Y == Y + Utils.DirVector(dirPrev).Y &&
                                            o.Layer == Layer && o.IsSolid))
                                        {
                                            canMove = false;
                                            continue;
                                        }
                                    }
                                    #endregion

                                    //Gets a list of all solids ahead of the
                                    //crate in the player's direction.
                                    List<GameObj> itemsFront =
                                        game.mngrLvl.items.Where(o => o.X ==
                                        item.X + Utils.DirVector(BlockDir).X &&
                                        o.Y == item.Y + Utils.DirVector(BlockDir).Y
                                        && o.Layer == Layer && o.BlockType !=
                                        Type.CrateHole).ToList();

                                    //Gets a list of all items the crate is on.
                                    List<GameObj> itemsTop =
                                        game.mngrLvl.items.Where(o => o.X ==
                                        item.X && o.Y == item.Y && o.Layer ==
                                        Layer && o.IsEnabled).ToList();

                                    //Removes crate from affecting itself.
                                    itemsTop.Remove(item);

                                    #region Interaction: MazeBelt.cs
                                    //Can't move crates into opposing belts.
                                    itemsFront = itemsFront.Where(o =>
                                        o.IsSolid || (o.BlockType == Type.Belt &&
                                        o.BlockDir == Utils.DirOpp(BlockDir) &&
                                        o.IsEnabled)).ToList();

                                    //Can't move crates from belts.
                                    itemsTop = itemsTop.Where(o =>
                                        o.IsSolid || (o.BlockType == Type.Belt &&
                                        o.IsEnabled && o.BlockDir != BlockDir)).ToList();
                                    #endregion

                                    #region Interaction: MazeMultiWay.cs
                                    itemsFront = itemsFront.Where(o =>
                                        !(o.IsEnabled &&
                                        o.BlockType == Type.MultiWay &&
                                        ((o.CustInt1 == 0 && o.BlockDir == BlockDir) ||
                                        (o.CustInt1 != 0 && (o.BlockDir == BlockDir ||
                                        o.BlockDir == Utils.DirOpp(BlockDir))))))
                                        .ToList();

                                    itemsTop = itemsTop.Where(o =>
                                        !(o.IsEnabled &&
                                        o.BlockType == Type.MultiWay &&
                                        ((o.CustInt1 == 0 && o.BlockDir == BlockDir) ||
                                        (o.CustInt1 != 0 && (o.BlockDir == BlockDir ||
                                        o.BlockDir == Utils.DirOpp(BlockDir))))))
                                        .ToList();
                                    #endregion

                                    //If nothing is in way, moves the crate.
                                    if (itemsFront.Count == 0 &&
                                        itemsTop.Count == 0)
                                    {
                                        #region Interaction: MazeTurretBullet.cs
                                        //Finds all bullets skipped over by moving {tilesize} at a time.
                                        //The +{tilesize} at the end accounts for sprite width and height.
                                        float xMin = Math.Min(item.X * MainLoop.TileSize, (item.X + Utils.DirVector(item.BlockDir).X) * MainLoop.TileSize);
                                        float xMax = Math.Max(item.X * MainLoop.TileSize, (item.X + Utils.DirVector(item.BlockDir).X) * MainLoop.TileSize) + MainLoop.TileSize;
                                        float yMin = Math.Min(item.Y * MainLoop.TileSize, (item.Y + Utils.DirVector(item.BlockDir).Y) * MainLoop.TileSize);
                                        float yMax = Math.Max(item.Y * MainLoop.TileSize, (item.Y + Utils.DirVector(item.BlockDir).Y) * MainLoop.TileSize) + MainLoop.TileSize;
                                        List<GameObj> bullets = game.mngrLvl.items.Where(o =>
                                            o.BlockType == Type.TurretBullet &&
                                            o.Layer == item.Layer &&
                                            o.X >= xMin && o.X <= xMax &&
                                            o.Y >= yMin && o.Y <= yMax)
                                            .ToList();

                                        for (int i = 0; i < bullets.Count; i++)
                                        {
                                            game.mngrLvl.RemoveItem(bullets[i]);
                                        }
                                        #endregion

                                        //Plays the crate-moving sound.
                                        game.playlist.Play
                                            (sndMoveCrate, X, Y);

                                        // Animation for crates being pushed
                                        var pushEffect = new FxCratePush(game, X, Y, Layer);
                                        pushEffect.BlockSprite.angle = (float)Utils.DirToRadians(BlockDir);
                                        game.mngrLvl.AddItem(pushEffect);

                                        item.BlockDir = BlockDir; //used for ice.
                                        item.X += (int)Utils.DirVector(BlockDir).X;
                                        item.Y += (int)Utils.DirVector(BlockDir).Y;

                                        //Recalculates sprite (no flicker).
                                        item.Update();
                                    }
                                    else
                                    {
                                        canMove = false;
                                    }
                                }
                                #endregion

                                #region Interaction: MazeMultiWay.cs
                                else if (item.BlockType == Type.MultiWay)
                                {
                                    if ((item.CustInt1 == 0 &&
                                        item.BlockDir != BlockDir) ||
                                        (item.CustInt1 != 0 &&
                                        item.BlockDir != BlockDir &&
                                        item.BlockDir != Utils.DirOpp(BlockDir)) ||
                                        (item.IsEnabled == false))
                                    {
                                        canMove = false;
                                    }
                                }
                                else
                                {
                                    canMove = false;
                                }
                                #endregion
                            }
                        }

                        //Considers all scenarios where the player can't move.
                        //Gets a list of all blocks at the current location.
                        items = game.mngrLvl.items.Where(o => o.X == X &&
                            o.Y == Y && o.Layer == Layer && o.IsEnabled)
                            .ToList();

                        foreach (GameObj item in items)
                        {
                            #region Interaction: MazeBelt.cs
                            //Cannot move while on a belt.
                            if (item.BlockType == Type.Belt && item.IsEnabled)
                            {
                                canMove = false;
                                doRevertDir = true;
                            }
                            #endregion

                            #region Interaction: MazeIce.cs
                            else if (item.BlockType == Type.Ice)
                            {
                                //Gets a list of blocks in front.
                                List<GameObj> itemsTemp;
                                itemsTemp = game.mngrLvl.items.Where(o =>
                                    o.X == X + Utils.DirVector(dirPrev).X &&
                                    o.Y == Y + Utils.DirVector(dirPrev).Y &&
                                    o.Layer == Layer).ToList();

                                #region Interaction: MazeMultiWay.cs
                                itemsTemp = itemsTemp.Where(o =>
                                    !(o.IsEnabled && o.BlockType == Type.MultiWay
                                    && ((o.CustInt1 == 0 && o.BlockDir == dirPrev) ||
                                    (o.CustInt1 != 0 && (o.BlockDir == dirPrev ||
                                    o.BlockDir == Utils.DirOpp(dirPrev)
                                    ))))).ToList();
                                #endregion
                                #region Interaction: MazeBelt.cs
                                //Blocked by solids and enabled belts.
                                itemsTemp = itemsTemp.Where(o =>
                                    o.IsSolid || (o.BlockType == Type.Belt &&
                                    o.BlockDir != Utils.DirOpp(BlockDir))).ToList();
                                #endregion

                                //Can't move unless blocked.
                                if (itemsTemp.Count == 0)
                                {
                                    canMove = false;
                                }
                            }
                            #endregion

                            #region Interaction: MazeMultiWay.cs
                            else if (item.BlockType == Type.MultiWay)
                            {
                                if ((item.CustInt1 == 0 && item.BlockDir != BlockDir) ||
                                    (item.CustInt1 != 0 && item.BlockDir != BlockDir &&
                                    item.BlockDir != Utils.DirOpp(BlockDir)))
                                {
                                    canMove = false;
                                }
                            }
                            #endregion
                        }
                    }

                    //Moves the player if capable.
                    if (doMove && canMove)
                    {
                        #region Interaction: MazeFilter.cs
                        //Gets a list of all filters the actor is standing on.
                        List<GameObj> items = game.mngrLvl.items.Where(o =>
                            o.X == X && o.Y == Y && o.Layer == Layer &&
                            o.BlockType == Type.Filter).ToList();
                        
                        //Decrements each filter's countdown.
                        foreach (GameObj item in items)
                        {
                            if (item.IsEnabled && item.CustInt1 > 0)
                            {
                                item.CustInt1--;
                            }
                        }
                        #endregion

                        #region Interaction: MazeTurretBullet.cs
                        //Finds all bullets skipped over by moving 64px at a time.
                        // The +16 and +48 effectively shrinks collision box by 16px around.
                        float xMin = Math.Min(X * MainLoop.TileSize, (X + Utils.DirVector(BlockDir).X) * MainLoop.TileSize) + 16;
                        float xMax = Math.Max(X * MainLoop.TileSize, (X + Utils.DirVector(BlockDir).X) * MainLoop.TileSize) + 48;
                        float yMin = Math.Min(Y * MainLoop.TileSize, (Y + Utils.DirVector(BlockDir).Y) * MainLoop.TileSize) + 16;
                        float yMax = Math.Max(Y * MainLoop.TileSize, (Y + Utils.DirVector(BlockDir).Y) * MainLoop.TileSize) + 48;
                        List<GameObj> bullets = game.mngrLvl.items.Where(o =>
                            o.BlockType == Type.TurretBullet &&
                            o.Layer == Layer &&
                            o.X >= xMin && o.X <= xMax &&
                            o.Y >= yMin && o.Y <= yMax)
                            .ToList();

                        List<GameObj> thisActor = new() { this };

                        // Bullets are deleted if the player moves into them here, and also in MngrLvl
                        // if the bullet moves into the player. In both cases, subtracts health.
                        hp -= 25 * bullets.Count;
                        if (bullets.Count > 0)
                        {
                            game.playlist.Play(MngrLvl.sndHit, X, Y);
                            PerformHurtAnimation();
                        }

                        for (int i = 0; i < bullets.Count; i++)
                        {
                            game.mngrLvl.RemoveItem(bullets[i]);
                        }
                        #endregion

                        animatedStep++;
                        stoppedMovingTimer = 25;
                        if (animatedStep > 3) { animatedStep = 0; }

                        X += (int)Utils.DirVector(BlockDir).X;
                        Y += (int)Utils.DirVector(BlockDir).Y;

                        //Increments the step counter if applicable.
                        if (this == game.mngrLvl.actor)
                        {
                            game.mngrLvl.LvlSteps++;
                        }
                    }
                    else if (doRevertDir)
                    {
                        BlockDir = dirPrev;
                    }
                }
            }
            #endregion

            #region player clicks to select actor
            if (!game.mngrLvl.opSyncActors && game.IsActive)
            {
                //If the player is clicked.
                if (game.MsState.LeftButton == ButtonState.Pressed &&
                    game.MsStateOld.LeftButton == ButtonState.Released &&
                    Sprite.IsIntersecting(BlockSprite, new SmoothRect
                    (game.mngrLvl.GetCoordsMouse(), 1, 1)) &&
                    Layer == game.mngrLvl.actor.Layer)
                {
                    if (this != game.mngrLvl.actor)
                    {
                        game.mngrLvl.actor = this;

                        //Interaction: TitleItemMain.cs
                        SfxPlaylist.Play(TitleItemMain.sndBttnClick);
                    }
                }
            }
            #endregion

            spriteAtlas.Update(true);
            base.Update();
        }

        /// <summary>
        /// Draws the actor. When hovered, draws health.
        /// </summary>
        public override void Draw()
        {
            if (isHurtCountdown > 0)
            {
                isHurtCountdown--;
                BlockSprite.color = (IsEnabled)
                    ? Color.Lerp(Color.White, Color.Red, (float)isHurtCountdown / isHurtCountdownStart)
                    : Color.Lerp(Color.Aqua, Color.Red, (float)isHurtCountdown / isHurtCountdownStart);
            }
            else
            {
                BlockSprite.color = (IsEnabled) ? Color.White : Color.Aqua;
            }

            base.Draw();

            //Sets the tooltip to display keys / disabled status on hover.
            if (Sprite.IsIntersecting(BlockSprite, new SmoothRect
                (game.mngrLvl.GetCoordsMouse(), 1, 1)) &&
                Layer == game.mngrLvl.actor.Layer)
            {
                game.mngrLvl.tooltip += "actor ";

                if (keys.Count > 0)
                {
                    game.mngrLvl.tooltip += $"(Has {keys.Count} keys) ";

                    #region Interaction: MazeKey.cs
                    //Draws a key for each collected key.
                    for (int i = 0; i < keys.Count; i++)
                    {
                        game.GameSpriteBatch.Draw(MazeKey.TexKey,
                            new Rectangle(
                                X*MainLoop.TileSize + i*MainLoop.TileSizeHalf,
                                Y*MainLoop.TileSize - MainLoop.TileSizeHalf,
                                MainLoop.TileSizeHalf, MainLoop.TileSizeHalf),
                            new Rectangle(0, 0, MainLoop.TileSize, MainLoop.TileSize), keys[i]);
                    }
                    #endregion
                }
                if (!IsEnabled)
                {
                    game.mngrLvl.tooltip += "(disabled) ";
                }

                game.mngrLvl.tooltip += "| ";
            }
        }

        /// <summary>
        /// Blends the actor's sprite color with red, lerping back to white over <see cref="isHurtCountdownStart"/>
        /// frames.
        /// </summary>
        public void PerformHurtAnimation()
        {
            isHurtCountdown = isHurtCountdownStart;
        }
    }
}