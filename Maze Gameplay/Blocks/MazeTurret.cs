using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Maze
{
    /// <summary>
    /// A turret which launches a bullet at some intervals.
    /// 
    /// Dependencies: MngrLvl, MazeBlock, MazeBelt.
    /// 
    /// Activation types:
    /// 5: Fires a single bullet.
    /// 
    /// Custom properties of custInt1: milliseconds between each bullet.
    /// 
    /// Custom properties of custInt2: none
    /// Custom properties of custStr: none
    /// </summary>
    public class MazeTurret : GameObj
    {
        //Relevant assets.
        public static Texture2D TexTurret { get; private set; }

        //Sprite information.
        private SpriteAtlas spriteAtlas;
        public int msDelay;

        /// <summary>
        /// Changing the tile size broke turret collisions once. This now keeps speed proportional, and levels retain
        /// the same gameplay experience.
        /// </summary>
        public static readonly double bulletSpeedTileSizeMult = MainLoop.TileSize / 32.0;

        /// <summary>Sets the block location and default values.</summary>
        /// <param name="x">The column number.</param>
        /// <param name="y">The row number.</param>
        /// <param name="layer">The layer in the maze.</param>
        public MazeTurret(MainLoop game, int x, int y, int layer) :
            base(game, x, y, layer)
        {
            //Sets default values.
            IsSolid = true;
            BlockType = Type.Turret;
            msDelay = SlotValueInt1;

            //Sets sprite information.
            BlockSprite = new Sprite(true, TexTurret);
            BlockSprite.depth = 0.419f;
            BlockSprite.drawBehavior = SpriteDraw.all;
            spriteAtlas = new SpriteAtlas(BlockSprite, MainLoop.TileSize, MainLoop.TileSize, 8, 2, 4);

            // Turrets occlude.
            Lighting = new(null, new(TileHull));
        }

        /// <summary>
        /// Loads relevant graphics into memory.
        /// </summary>
        /// <param name="Content">A game content loader.</param>
        public static void LoadContent(ContentManager Content)
        {
            TexTurret = Content.Load<Texture2D>("Content/Sprites/Game/sprTurret");
        }

        /// <summary>
        /// Returns a copy of the object.
        /// </summary>
        public override GameObj Clone()
        {
            MazeTurret newBlock = new(game, X, Y, Layer);
            newBlock.CopyFrom(this);

            //Sets specific variables.
            newBlock.BlockSprite = BlockSprite;
            newBlock.msDelay = msDelay;
            newBlock.spriteAtlas = new SpriteAtlas(spriteAtlas, false);
            return newBlock;
        }

        /// <summary>
        /// Updates the atlas. Behavior handled by MngrLvl.cs.
        /// </summary>
        public override void Update()
        {
            #region Determines sprite by dir and isEnabled.
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
            else if (BlockDir == Dir.Up)
            {
                spriteAtlas.frame = 3;
            }
            if (!IsEnabled)
            {
                spriteAtlas.frame += 4;
            }
            #endregion

            //Manages turret bullet spawning.
            if (IsEnabled)
            {
                msDelay -= 1;
                if (msDelay <= 0)
                {
                    msDelay = SlotValueInt1;

                    MazeTurretBullet bullet = new MazeTurretBullet(game,
                            X * MainLoop.TileSize + MainLoop.TileSizeHalf, Y * MainLoop.TileSize + MainLoop.TileSizeHalf, Layer);

                    //Updates the bullet position after adjusting it.                    
                    bullet.X += (int)(Utils.DirVector(BlockDir).X * MainLoop.TileSizeHalf - 8);
                    bullet.Y += (int)(Utils.DirVector(BlockDir).Y * MainLoop.TileSizeHalf - 8);

                    bullet.BlockSprite.rectDest.X = bullet.X;
                    bullet.BlockSprite.rectDest.Y = bullet.Y;

                    bullet.BlockDir = BlockDir;

                    // Provides the bullet speed.
                    bullet.SlotValueInt2 = SlotValueInt2 * (int)bulletSpeedTileSizeMult;
                    game.mngrLvl.AddItem(bullet);
                }

                //Fires a bullet when activated.
                if (IsActivated && ActionType == 5)
                {
                    IsActivated = false;

                    MazeTurretBullet bullet = new MazeTurretBullet(game,
                        X * MainLoop.TileSize + MainLoop.TileSizeHalf,
                        Y * MainLoop.TileSize + MainLoop.TileSizeHalf,
                        Layer);

                    //Updates the bullet position after adjusting it.                    
                    bullet.X += (int)(Utils.DirVector(BlockDir).X * MainLoop.TileSizeHalf - 8);
                    bullet.Y += (int)(Utils.DirVector(BlockDir).Y * MainLoop.TileSizeHalf - 8);

                    bullet.BlockSprite.rectDest.X = bullet.X;
                    bullet.BlockSprite.rectDest.Y = bullet.Y;

                    bullet.BlockDir = BlockDir;
                    game.mngrLvl.AddItem(bullet);
                }
            }

            spriteAtlas.Update(true);
            base.Update();
        }

        /// <summary>
        /// Draws the turret. When hovered, draws enabledness/info.
        /// </summary>
        public override void Draw()
        {
            base.Draw();

            //Sets the tooltip to display disabled status and info.
            if (Sprite.IsIntersecting(BlockSprite, new SmoothRect
                (game.mngrLvl.GetCoordsMouse(), 1, 1)) &&
                Layer == game.mngrLvl.actor.Layer)
            {
                game.mngrLvl.tooltip += "Turret";

                if (!IsEnabled)
                {
                    game.mngrLvl.tooltip += "(disabled)";
                }

                game.mngrLvl.tooltip += " | ";
            }
        }
    }
}