﻿using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Maze
{
    /// <summary>
    /// A coin lock that unlocks with a certain number of coins.
    /// 
    /// Activation types: none
    /// 
    /// Custom properties of custInt1: The number of coins required to open
    /// the gate.
    /// 
    /// Custom properties of custInt2:
    /// 0: Does not subtract coins on contact.
    /// 1: Subtracts the coins on contact.
    /// 
    /// Custom properties of custStr: none
    /// </summary>
    public class MazeCoinLock : GameObj
    {
        //Relevant assets.
        public static Texture2D TexCoinLock { get; private set; }

        /// <summary>Sets the block location and default values.</summary>
        /// <param name="x">The column number.</param>
        /// <param name="y">The row number.</param>
        /// <param name="layer">The layer in the maze.</param>
        public MazeCoinLock(MainLoop game, int x, int y, int layer)
            : base(game, x, y, layer)
        {
            //Sets default values.
            IsSolid = true;
            BlockType = Type.CoinLock;

            //Sets sprite information.
            BlockSprite = new Sprite(true, TexCoinLock);
            BlockSprite.depth = 0.410f;
            BlockSprite.drawBehavior = SpriteDraw.all;

            // Solids generally occlude.
            Lighting = new(null, new(TileHull));
        }

        /// <summary>
        /// Loads relevant graphics into memory.
        /// </summary>
        /// <param name="Content">A game content loader.</param>
        public static void LoadContent(ContentManager Content)
        {
            TexCoinLock = Content.Load<Texture2D>("Content/Sprites/Game/sprCoinLock");
        }

        /// <summary>
        /// Returns a copy of the object.
        /// </summary>
        public override GameObj Clone()
        {
            MazeCoinLock newBlock = new(game, X, Y, Layer);
            newBlock.CopyFrom(this);

            //Sets specific variables.
            newBlock.BlockSprite = BlockSprite;
            return newBlock;
        }

        /// <summary>
        /// Deleted by actors with coins on contact. Handled by MazeActor.cs.
        /// </summary>
        public override void Update()
        {
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
                game.mngrLvl.tooltip += "Coin Lock | ";
            }

            //Draws the number of coins required (in green if you have them).
            if (game.mngrLvl.ActorCoins >= SlotValueInt1)
            {
                Vector2 position = game.fntBold.MeasureString(SlotValueInt1.ToString()) / 2;

                game.GameSpriteBatch.DrawString(game.fntBold,
                    SlotValueInt1.ToString(),
                    new Vector2(X * MainLoop.TileSize + MainLoop.TileSizeHalf - position.X, Y * MainLoop.TileSize + MainLoop.TileSizeHalf - position.Y),
                    Color.Lime);
            }
            else
            {
                Vector2 position = game.fntBold.MeasureString(SlotValueInt1.ToString()) / 2;

                game.GameSpriteBatch.DrawString(game.fntBold,
                    SlotValueInt1.ToString(),
                    new Vector2(X * MainLoop.TileSize + MainLoop.TileSizeHalf - position.X, Y * MainLoop.TileSize + MainLoop.TileSizeHalf - position.Y),
                    Color.Red);
            }
        }
    }
}