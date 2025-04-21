using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Maze
{
    /// <summary>
    /// Displays a message when hovering the mouse.
    /// 
    /// Activation types: none
    /// 
    /// Custom properties of custInt1: none
    /// Custom properties of custInt2: none
    /// Custom properties of custStr: The desired message.
    /// </summary>
    public class MazeMessage : GameObj
    {
        //Relevant assets.
        public static Texture2D TexMessage { get; private set; }

        /// <summary>
        /// The transparency of the text.
        /// </summary>
        private float TextAlpha;

        /// <summary>Sets the block location and default values.</summary>
        /// <param name="x">The column number.</param>
        /// <param name="y">The row number.</param>
        /// <param name="layer">The layer in the maze.</param>
        public MazeMessage(MainLoop game, int x, int y, int layer)
            : base(game, x, y, layer)
        {
            //Sets default values.
            BlockType = Type.Message;

            //Sets sprite information.
            BlockSprite = new Sprite(true, TexMessage);

            TextAlpha = IsEnabled ? 1 : 0;
        }

        /// <summary>
        /// Loads relevant graphics into memory.
        /// </summary>
        /// <param name="Content">A game content loader.</param>
        public static void LoadContent(ContentManager Content)
        {
            TexMessage = Content.Load<Texture2D>("Content/Sprites/Game/sprMessage");
        }

        /// <summary>
        /// Returns a copy of the object.
        /// </summary>
        public override GameObj Clone()
        {
            MazeMessage newBlock = new(game, X, Y, Layer);
            newBlock.CopyFrom(this);

            //Sets specific variables.
            newBlock.BlockSprite = BlockSprite;
            return newBlock;
        }

        public override void Update()
        {
            // Fade in/out animation
            const float speed = 0.05f;
            if (!IsEnabled && TextAlpha != 0)
            {
                TextAlpha = Utils.Lerp(speed, TextAlpha, 0);
                if (TextAlpha < 0.1f) { TextAlpha = 0; }
            }
            if (IsEnabled && TextAlpha < 1)
            {
                TextAlpha = Utils.Lerp(speed, TextAlpha, 1);
                if (Math.Abs(TextAlpha - 1) < 0.1f) { TextAlpha = 1; }
            }

            base.Update();
        }

        /// <summary>
        /// Draws the sprite. Sets a custom tooltip.
        /// </summary>
        public override void Draw()
        {
            if ((IsEnabled || TextAlpha > 0) && !string.IsNullOrWhiteSpace(SlotValueString))
            {
                SpriteText txt = new(game.fntBoldBig, Properties[Utils.PropertyNameCustomString].ToString())
                {
                    color = Microsoft.Xna.Framework.Color.White,
                    depth = 0.009f,
                    drawBehavior = SpriteDraw.all,
                    position = BlockSprite.rectDest.Center,
                    alpha = TextAlpha
                };

                txt.CenterOriginVert();

                // Update the width/height so that text appears/disappears correctly when off-screen.
                var txtSize = game.fntBoldBig.MeasureString(txt.text);
                BlockSprite.rectDest.Width = txtSize.X;
                BlockSprite.rectDest.Height = txtSize.Y;
                txt.Draw(game.GameSpriteBatch);
            }
        }
    }
}