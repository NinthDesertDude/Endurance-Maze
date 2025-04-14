using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace EnduranceTheMaze
{
    /// <summary>
    /// A solid wall object.
    /// 
    /// Activation types: none
    /// 
    /// Custom properties of custInt1: none
    /// Custom properties of custInt2: none
    /// Custom properties of custStr: none
    /// </summary>
    public class MazeWall : GameObj
    {
        //Relevant assets.
        public static Texture2D TexWall { get; private set; }

        /// <summary>Sets the block location and default values.</summary>
        /// <param name="x">The column number.</param>
        /// <param name="y">The row number.</param>
        /// <param name="layer">The layer in the maze.</param>
        public MazeWall(MainLoop game, int x, int y, int layer)
            : base(game, x, y, layer)
        {
            //Sets default values.
            IsSolid = true;
            BlockType = Type.Wall;

            //Sets sprite information.
            BlockSprite = new Sprite(true, TexWall);
            BlockSprite.depth = 0.413f;

            // Walls occlude.
            Lighting = new(null, new(TileHull));
        }

        /// <summary>
        /// Loads relevant graphics into memory.
        /// </summary>
        /// <param name="Content">A game content loader.</param>
        public static void LoadContent(ContentManager Content)
        {
            TexWall = Content.Load<Texture2D>("Content/Sprites/Game/sprWall");
        }

        /// <summary>
        /// Returns a copy of the object.
        /// </summary>
        public override GameObj Clone()
        {
            MazeWall newBlock = new(game, X, Y, Layer);
            newBlock.CopyFrom(this);

            return newBlock;
        }
    }
}