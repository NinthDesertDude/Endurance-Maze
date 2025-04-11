using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace EnduranceTheMaze
{
    /// <summary>
    /// Visually represents an item, but doesn't simulate behavior.
    /// </summary>
    public class ImgType : ImgBlock
    {

        /// <summary>
        /// Sets the block's location.
        /// </summary>
        /// <param name="x">The column number.</param>
        /// <param name="y">The row number.</param>
        /// <param name="layer">The layer in the maze.</param>
        public ImgType(MainLoop game, Type type)
            : base (game, type, 0, 0, 0)
        {
            //Executes base constructor.
        }

        /// <summary>
        /// Sets as active type if needed; updates sprite atlas.
        /// </summary>
        public override void Update()
        {
            //Sets this block's type as the active type if clicked.
            if (game.MsState.LeftButton == ButtonState.Pressed &&
                game.MsState.X >= 0 && game.MsState.X <= MainLoop.TileSize &&
                game.MsState.Y >= BlockSprite.rectDest.Y &&
                game.MsState.Y <= BlockSprite.rectDest.Y + MainLoop.TileSize)
            {
                game.mngrEditor.activeType = BlockType;
            }

            base.Update();

            //Synchronizes sprite position to location. (Req MngrEditor.cs).
            BlockSprite.rectDest.X = 0;
            BlockSprite.rectDest.Y = Y * MainLoop.TileSize + game.mngrEditor.sidebarScroll;
        }

        public override void Draw()
        {
            base.Draw();

            //Draws a rectangle over the sprite if it's active.
            if (BlockType == game.mngrEditor.activeType)
            {
                game.GameSpriteBatch.Draw(MngrLvl.TexPixel, new Rectangle(
                    0, (int)BlockSprite.rectDest.Y, MainLoop.TileSize, MainLoop.TileSize), Color.Yellow * 0.5f);
            }
        }
    }
}