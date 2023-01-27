using ImpossiMaze;

namespace EnduranceTheMaze
{
    /// <summary>
    /// Visual effect for when a crate breaks.
    /// Dependencies: MazeCrate and its texture.
    /// </summary>
    public class FxCrateBroken : GameObj
    {
        private SpriteAtlas spriteAtlas;

        /// <summary>Sets the block location and default values.</summary>
        /// <param name="x">The column number.</param>
        /// <param name="y">The row number.</param>
        /// <param name="layer">The layer in the maze.</param>
        public FxCrateBroken(MainLoop game, int x, int y, int layer)
            : base(game, x, y, layer)
        {
            //Sets default values.
            isSynchronized = false;

            //Sets sprite information.
            BlockSprite = new Sprite(true, MazeCrate.TexCrate);
            BlockSprite.depth = 0.001f;
            BlockSprite.rectSrc = new SmoothRect(0, 0, 32, 32);
            BlockSprite.rectDest.Width = 32;
            BlockSprite.rectDest.Height = 32;
            BlockSprite.drawBehavior = SpriteDraw.all;
            spriteAtlas = new SpriteAtlas(BlockSprite, 32, 32, 4, 1, 4);
            spriteAtlas.frame = 1;
            spriteAtlas.frameSpeed = 0.5f;
            spriteAtlas.frameEndBehavior = FrameEnd.End;
            spriteAtlas.OnAnimationEnd += RemoveAfterPlayback;
        }

        private void RemoveAfterPlayback()
        {
            game.mngrLvl.RemoveDecor(this);
        }

        /// <summary>
        /// Returns an exact copy of the object.
        /// </summary>
        public override GameObj Clone()
        {
            //Sets common variables.
            FxCrateBroken newBlock = new FxCrateBroken(game, X, Y, Layer);
            newBlock.ActionIndex = ActionIndex;
            newBlock.ActionIndex2 = ActionIndex2;
            newBlock.ActionType = ActionType;
            newBlock.CustInt1 = CustInt1;
            newBlock.CustInt2 = CustInt2;
            newBlock.CustStr = CustStr;
            newBlock.BlockDir = BlockDir;
            newBlock.IsActivated = IsActivated;
            newBlock.IsEnabled = IsEnabled;
            newBlock.IsVisible = IsVisible;
            return newBlock;
        }

        public override void Update()
        {
            base.Update();
            spriteAtlas.Update(false);

            // Moves 4px per frame in the set direction.
            X += (int)Utils.DirVector(BlockDir).X * 4;
            Y += (int)Utils.DirVector(BlockDir).Y * 4;
        }

        public override void Draw()
        {
            base.Draw();
        }
    }
}