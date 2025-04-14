using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace EnduranceTheMaze
{
    /// <summary>
    /// Background decoration.
    /// 
    /// Activation types: none
    /// 
    /// Custom properties of custInt1: none
    /// Custom properties of custInt2: none
    /// Custom properties of custStr: none
    /// </summary>
    public class MazeFloor : GameObj
    {
        //Relevant assets.
        public static Texture2D TexFloor { get; private set; }

        /// <summary>Sets the block location and default values.</summary>
        /// <param name="x">The column number.</param>
        /// <param name="y">The row number.</param>
        /// <param name="layer">The layer in the maze.</param>
        public MazeFloor(MainLoop game, int x, int y, int layer)
            : base(game, x, y, layer, true)
        {
            //Sets default values.
            BlockType = Type.Floor;

            //Sets sprite information.
            BlockSprite = new Sprite(true, TexFloor);
            BlockSprite.depth = 0.6f;
        }

        /// <summary>
        /// Loads relevant graphics into memory.
        /// </summary>
        /// <param name="Content">A game content loader.</param>
        public static void LoadContent(ContentManager Content)
        {
            TexFloor = Content.Load<Texture2D>("Content/Sprites/Game/sprFloor");
        }

        /// <summary>
        /// Returns a copy of the object.
        /// </summary>
        public override GameObj Clone()
        {
            MazeFloor newBlock = new(game, X, Y, Layer);
            newBlock.CopyFrom(this);
            return newBlock;
        }
    }
}