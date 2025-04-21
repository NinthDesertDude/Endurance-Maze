using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Maze
{
    /// <summary>
    /// Laser actuators can activate after receiving an amount of bullets.
    /// 
    /// Activation types:
    /// 5: Activates linked items every custInt1 bullets.
    /// 6: Activates/deactivates linked items every custInt1 bullets.
    /// 7: Activates linked items after custInt1 bullets, then deactivates after custInt2 frames without a bullet.
    /// 8: Activates after custInt1 bullets and doesn't deactivate.
    /// 
    /// Custom properties of custInt1:
    /// > -1: The number of bullets required to activate.
    /// Custom properties of custInt2:
    /// > -1: The number of frames to wait. At 0, deactivation is guaranteed next frame.
    /// Custom properties of custStr: none
    /// </summary>
    public class MazeLaserActuator : GameObj
    {
        //Relevant assets.
        public static Texture2D TexLaserActuator { get; private set; }

        //Sprite information.    
        private SpriteAtlas spriteAtlas;

        private bool receivedBullet = false;
        private bool isActivating = false;
        private int framesWithoutBullets = 0;
        private int bulletsReceived = 0;

        /// <summary>Sets the block location and default values.</summary>
        /// <param name="x">The column number.</param>
        /// <param name="y">The row number.</param>
        /// <param name="layer">The layer in the maze.</param>
        public MazeLaserActuator(MainLoop game, int x, int y, int layer)
            : base(game, x, y, layer)
        {
            //Sets default values.
            IsSolid = true;
            BlockType = Type.LaserActuator;

            //Sets sprite information.
            BlockSprite = new Sprite(true, TexLaserActuator);
            BlockSprite.depth = 0.421f;
            spriteAtlas = new SpriteAtlas(BlockSprite, MainLoop.TileSize, MainLoop.TileSize, 5, 1, 5);

            // Solids generally occlude.
            Lighting = new(null, new(TileHull));
        }

        /// <summary>
        /// Loads relevant graphics into memory.
        /// </summary>
        /// <param name="Content">A game content loader.</param>
        public static void LoadContent(ContentManager Content)
        {
            TexLaserActuator = Content.Load<Texture2D>("Content/Sprites/Game/sprLaserActuator");
        }

        /// <summary>
        /// Returns a copy of the object.
        /// </summary>
        public override GameObj Clone()
        {
            MazeLaserActuator newBlock = new(game, X, Y, Layer);
            newBlock.CopyFrom(this);
            return newBlock;
        }

        /// <summary>
        /// Updates the sprite.
        /// </summary>
        public override void Update()
        {
            if (!IsEnabled)
            {
                spriteAtlas.frame = 0;
            }
            else if (receivedBullet)
            {
                spriteAtlas.frame = isActivating ? 3 : 4;
            }
            else
            {
                spriteAtlas.frame = isActivating ? 2 : 1;
            }

            if (!receivedBullet)
            {
                framesWithoutBullets++;

                if (ActionType == 7 && isActivating && (framesWithoutBullets > SlotValueInt2 || SlotValueInt2 == 0))
                {
                    framesWithoutBullets = 0;
                    bulletsReceived = 0;
                    ActivateLinkedItems((item) => false);
                    isActivating = false;
                }
            }

            spriteAtlas.Update(true);
            base.Update();
        }

        /// <summary>
        /// Toggles the expected events on linked items.
        /// </summary>
        /// <param name="setActivated"></param>
        private void ActivateLinkedItems(Func<GameObj, bool> setActivated)
        {
            isActivating = true;

            //Gets all items matching the index to affect.
            List<GameObj> items = game.mngrLvl.items.Where(o =>
                o.SignalListenChannel == SignalSendChannel).ToList();

            //If there are linked items to activate, plays sound.
            if (items.Count != 0)
            {
                game.playlist.Play(SndActivated, X, Y);

                // Animation for laser actuator triggering linked items
                FxPoof poofEffect;
                for (int i = 0; i < 4; i++)
                {
                    poofEffect = new FxPoof(
                        game, X * MainLoop.TileSize, Y * MainLoop.TileSize, Layer,
                        new Microsoft.Xna.Framework.Color(255, 255, 255),
                        (-1 + 2 * Utils.Rng.NextDouble(), -1 + 2 * Utils.Rng.NextDouble()));
                    poofEffect.X += -MainLoop.TileSizeHalf;
                    game.mngrLvl.AddItem(poofEffect);
                }

                foreach (GameObj item in items)
                {
                    item.IsActivated = setActivated(item);
                }
            }
        }

        /// <summary>
        /// Handles receiving a bullet.
        /// </summary>
        public void ReceivedBullet()
        {
            framesWithoutBullets = 0;
            receivedBullet = true;
            bulletsReceived++;

            if ((ActionType == 5 || ActionType == 6) && bulletsReceived >= SlotValueInt1)
            {
                bulletsReceived = 0;
                ActivateLinkedItems((item) => ActionType == 5 || !item.IsActivated);
            }
            else if ((ActionType == 7 || ActionType == 8) && !isActivating && bulletsReceived >= SlotValueInt1)
            {
                bulletsReceived = 0;
                ActivateLinkedItems((item) => true);
            }
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
                game.mngrLvl.tooltip += "Laser Actuator | ";
            }

            receivedBullet = false;

            if (ActionType != 7 && ActionType != 8)
            {
                isActivating = false;
            }
        }
    }
}