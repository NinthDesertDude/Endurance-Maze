using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Maze
{
    /// <summary>
    /// A bullet which travels in a predetermined direction.
    /// Interacts with environment. Damages actor on contact.
    /// 
    /// Dependencies: MngrLvl, MazeBlock, MazeBelt.
    /// 
    /// Activation types: none
    /// 
    /// Custom properties of custInt1: none
    /// Custom properties of custInt2: none
    /// Custom properties of custStr: none
    /// </summary>
    public class MazeTurretBullet : GameObj
    {
        private static readonly float lightMinimumIntensity = 0.025f;

        //Relevant assets.
        public static Texture2D TexTurretBullet { get; private set; }

        //Stores all mirrors bounced off of so it only bounces once.
        public List<GameObj> mirrors;

        /// <summary>Sets the block location and default values.</summary>
        /// <param name="x">The column number.</param>
        /// <param name="y">The row number.</param>
        /// <param name="layer">The layer in the maze.</param>
        public MazeTurretBullet(MainLoop game, int x, int y, int layer) :
            base(game, x, y, layer)
        {
            //Sets default values.
            BlockType = Type.TurretBullet;
            SyncToGrid = false;
            mirrors = new List<GameObj>();

            //Sets sprite information.
            BlockSprite = new Sprite(true, TexTurretBullet);
            BlockSprite.depth = 0.2f;
            BlockSprite.drawBehavior = SpriteDraw.all;
            BlockSprite.CenterOrigin();

            // Lasers cast their own light source.
            Lighting = new(
                new Penumbra.PointLight
                {
                    Radius = 0.1f,
                    Scale = new Vector2(MainLoop.TileSize * 3),
                    Intensity = lightMinimumIntensity,
                    ShadowType = Penumbra.ShadowType.Solid,
                    Color = new Color(255, 50, 50)
                }, null);
        }

        /// <summary>
        /// Loads relevant graphics into memory.
        /// </summary>
        /// <param name="Content">A game content loader.</param>
        public static void LoadContent(ContentManager Content)
        {
            TexTurretBullet = Content.Load<Texture2D>("Content/Sprites/Game/sprTurretBullet");
        }

        /// <summary>
        /// Returns a copy of the object.
        /// </summary>
        public override GameObj Clone()
        {
            MazeTurretBullet newBlock = new(game, X, Y, Layer);
            newBlock.CopyFrom(this);

            // Unique variables.
            newBlock.mirrors = mirrors;

            return newBlock;
        }

        /// <summary>
        /// Updates the atlas. Behavior handled by MngrLvl.cs.
        /// </summary>
        public override void Update()
        {
            var playerCenter = game.mngrLvl.actor.BlockSprite.rectDest.Center;
            var center = BlockSprite.rectDest.Center;

            // Light intensity attenuated by distance from player.
            if (LightingRegistered.light)
            {
                int maxDist = MainLoop.TileSize * 5;
                float xDist = playerCenter.X - center.X;
                float yDist = playerCenter.Y - center.Y;

                // Since the hypotenuse angle is always longer than its components (xdist, ydist), an expensive operation
                // can be avoided cheaply until needed.
                if (xDist < maxDist && yDist < maxDist)
                {
                    double dist = Math.Sqrt(xDist * xDist + yDist * yDist);
                    float maxIntensity = 1 - lightMinimumIntensity;

                    if (dist < maxDist)
                    {
                        Lighting.light.Intensity = maxIntensity - ((float)(dist / maxDist) * maxIntensity);
                    }
                    else
                    {
                        Lighting.light.Intensity = lightMinimumIntensity;
                    }
                }
                else
                {
                    Lighting.light.Intensity = lightMinimumIntensity;
                }
            }

            base.Update();
        }

        /// <summary>
        /// Draws the bullet. When hovered, draws enabledness/info.
        /// </summary>
        public override void Draw()
        {
            base.Draw();

            //Sets the tooltip to display disabled status and info.
            if (Sprite.IsIntersecting(BlockSprite, new SmoothRect
                (game.mngrLvl.GetCoordsMouse(), 1, 1)) &&
                Layer == game.mngrLvl.actor.Layer)
            {
                game.mngrLvl.tooltip += "Turret Bullet";

                game.mngrLvl.tooltip += " | ";
            }
        }
    }
}