﻿using ImpossiMaze;
using Microsoft.Xna.Framework;

namespace Maze
{
    /// <summary>
    /// Visual effect for when a receiver is used.
    /// Dependencies: MngrLvl for the sprFx texture.
    /// </summary>
    public class FxReceived : GameObj
    {
        private SpriteAtlas spriteAtlas;

        /// <summary>Sets the block location and default values.</summary>
        /// <param name="x">The column number.</param>
        /// <param name="y">The row number.</param>
        /// <param name="layer">The layer in the maze.</param>
        public FxReceived(MainLoop game, int x, int y, int layer)
            : base(game, x, y, layer, true)
        {
            //Sets default values.
            SyncToGrid = true;

            //Sets sprite information.
            BlockSprite = new Sprite(true, MngrLvl.TexFx);
            BlockSprite.depth = 0.001f;
            BlockSprite.rectSrc = new SmoothRect(0, 0, MainLoop.TileSize, MainLoop.TileSize);
            BlockSprite.rectDest.Width = MainLoop.TileSize;
            BlockSprite.rectDest.Height = MainLoop.TileSize;
            BlockSprite.drawBehavior = SpriteDraw.all;

            spriteAtlas = new SpriteAtlas(BlockSprite, MainLoop.TileSize, MainLoop.TileSize, 2, 1, 2);

            BlockSprite.scaleX = 0.1f;
            BlockSprite.scaleY = 0.1f;
            BlockSprite.alpha = 0.5f;
            spriteAtlas.CenterOrigin();
        }

        /// <summary>
        /// Returns a copy of the object.
        /// </summary>
        public override GameObj Clone()
        {
            FxReceived newBlock = new(game, X, Y, Layer);
            newBlock.CopyFrom(this);
            return newBlock;
        }

        public override void Update()
        {
            base.Update();

            BlockSprite.scaleX *= 1.5f;
            BlockSprite.scaleY *= 1.5f;

            if (BlockSprite.scaleX >= 0.8f)
            {
                game.mngrLvl.RemoveItem(this);
                return;
            }

            spriteAtlas.Update(false);
        }

        public override void Draw()
        {
            base.Draw();
        }
    }
}