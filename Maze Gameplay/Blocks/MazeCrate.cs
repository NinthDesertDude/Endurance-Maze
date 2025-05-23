﻿using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Maze
{
    /// <summary>
    /// Single crates can be pushed in any direction and interact with all
    /// obstacles.
    /// 
    /// Activation types:
    /// 5: Breaks when activated.
    /// 
    /// Custom properties of custInt1:
    /// > 5: When broken, contained object is nth - 5 entry of the Type enum.
    /// Custom properties of custInt2: none
    /// Custom properties of custStr: none
    /// </summary>
    public class MazeCrate : GameObj
    {
        //Relevant assets.
        public static SoundEffect sndBreakCrate;
        public static Texture2D TexCrate { get; private set; }

        /// <summary>Sets the block location and default values.</summary>
        /// <param name="x">The column number.</param>
        /// <param name="y">The row number.</param>
        /// <param name="layer">The layer in the maze.</param>
        public MazeCrate(MainLoop game, int x, int y, int layer)
            : base(game, x, y, layer)
        {
            //Sets default values.
            IsSolid = true;
            BlockType = Type.Crate;

            //Sets sprite information.
            BlockSprite = new Sprite(true, TexCrate);
            BlockSprite.depth = 0.3f;
            BlockSprite.rectSrc = new SmoothRect(0, 0, MainLoop.TileSize, MainLoop.TileSize);
            BlockSprite.rectDest.Width = MainLoop.TileSize;
            BlockSprite.rectDest.Height = MainLoop.TileSize;
            BlockSprite.drawBehavior = SpriteDraw.basicAnimated;

            // Crates occlude.
            Lighting = new(null, new(TileHull));
        }

        /// <summary>
        /// Loads relevant graphics into memory.
        /// </summary>
        /// <param name="Content">A game content loader.</param>
        public static void LoadContent(ContentManager Content)
        {
            TexCrate = Content.Load<Texture2D>("Content/Sprites/Game/sprCrate");
            sndBreakCrate = Content.Load<SoundEffect>("Content/Sounds/sndBreakCrate");
        }

        /// <summary>
        /// Returns a copy of the object.
        /// </summary>
        public override GameObj Clone()
        {
            MazeCrate newBlock = new(game, X, Y, Layer);
            newBlock.CopyFrom(this);
            return newBlock;
        }

        /// <summary>
        /// Checks if the crate is broken open.
        /// </summary>
        public override void Update()
        {
            //If the crate breaks.
            if (IsActivated && ActionType == 5)
            {
                //Deactivates and plays the crate breaking sound.
                IsActivated = false;
                game.playlist.Play(sndBreakCrate, X, Y);

                //Removes the crate and spawns the contained item, if any.
                game.mngrLvl.RemoveItem(this);

                // Animation for crate breaking
                FxCrateBroken snap1 = new FxCrateBroken(game, X * MainLoop.TileSize, Y * MainLoop.TileSize, Layer) { BlockDir = Dir.UpLeft };
                FxCrateBroken snap2 = new FxCrateBroken(game, X * MainLoop.TileSize, Y * MainLoop.TileSize, Layer) { BlockDir = Dir.UpRight };
                FxCrateBroken snap3 = new FxCrateBroken(game, X * MainLoop.TileSize, Y * MainLoop.TileSize, Layer) { BlockDir = Dir.DownLeft };
                FxCrateBroken snap4 = new FxCrateBroken(game, X * MainLoop.TileSize, Y * MainLoop.TileSize, Layer) { BlockDir = Dir.DownRight };
                snap2.BlockSprite.spriteEffects = SpriteEffects.FlipHorizontally;
                snap3.BlockSprite.spriteEffects = SpriteEffects.FlipVertically;
                snap4.BlockSprite.spriteEffects = SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically;
                game.mngrLvl.itemsDecor.AddRange(new FxCrateBroken[] { snap1, snap2, snap3, snap4 });

                if (SlotValueInt1 != 0)
                {
                    game.mngrLvl.AddItem(Utils.BlockFromType
                            (game, (Type)(SlotValueInt1 - 1), X, Y, Layer));
                }
            }

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
                game.mngrLvl.tooltip += "Crate | ";
            }
        }
    }
}