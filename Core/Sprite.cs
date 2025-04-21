using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Maze
{
    /// <summary>
    /// Encapsulates spritebatch parameters in an object for simple reuse.
    /// </summary>
    public class Sprite
    {
        #region Members
        /// <summary>
        /// The pre-loaded 2d sprite texture.
        /// </summary>
        public Texture2D texture;

        /// <summary>
        /// The rectangle defining which portion of the source texture to use.
        /// </summary>
        public SmoothRect rectSrc;

        /// <summary>
        /// The rectangle defining the coordinates to draw the texture to.
        /// </summary>
        public SmoothRect rectDest;

        /// <summary>
        /// The position in the texture corresponding to (0, 0) for drawing.
        /// This shifts where the image is drawn and its rotation origin.
        /// </summary>
        public Vector2 origin;

        /// <summary>
        /// Whether to draw the origin translation or not. The rotation origin
        /// still follows the origin regardless.
        /// </summary>
        public bool doDrawOffset = true;

        /// <summary>
        /// The rotation angle in radians.
        /// </summary>
        public float angle = 0;

        /// <summary>
        /// The drawing order depth, so smallest are drawn first and covered.
        /// </summary>
        public float depth = 0;

        /// <summary>
        /// The color to blend the sprite with. White is zero blending.
        /// </summary>
        public Color color = Color.White;

        /// <summary>
        /// The translucency of the image. 1 is opaque. 0 is transparent.
        /// </summary>
        public float alpha = 1;

        /// <summary>
        /// Image mirroring effects.
        /// </summary>
        public SpriteEffects spriteEffects;

        /// <summary>
        /// Horizontal texture stretching multiplier. 1 is normal.
        /// </summary>
        public float scaleX = 1;

        /// <summary>
        /// Vertical texture stretching multiplier. 1 is normal.
        /// </summary>
        public float scaleY = 1;

        /// <summary>
        /// Affects how extensive the drawing is.
        /// </summary>
        public SpriteDraw drawBehavior = SpriteDraw.basic;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes an empty sprite. The texture must be set before drawing
        /// to avoid throwing an exception.
        /// </summary>
        public Sprite()
        {
            texture = null;
            rectSrc = new SmoothRect(0, 0, 0, 0);
            rectDest = new SmoothRect(0, 0, 0, 0);
            origin = new Vector2(0, 0);
            spriteEffects = SpriteEffects.None;
        }

        /// <summary>
        /// Initializes a new sprite from an existing one. The texture is
        /// referenced.
        /// </summary>
        public Sprite(Sprite sprite)
        {
            rectSrc = new SmoothRect(sprite.rectSrc);
            rectDest = new SmoothRect(sprite.rectDest);
            texture = sprite.texture;
            origin = new Vector2(sprite.origin.X, sprite.origin.Y);
            doDrawOffset = sprite.doDrawOffset;
            angle = sprite.angle;
            depth = sprite.depth;
            color = sprite.color;
            alpha = sprite.alpha;
            spriteEffects = sprite.spriteEffects;
            scaleX = sprite.scaleX;
            scaleY = sprite.scaleY;
            drawBehavior = sprite.drawBehavior;
        }

        /// <summary>Initializes a new Sprite to control drawing.</summary>
        /// <param name="doSetDimensions">Whether or not to set the width
        /// and height based on the texture.</param>
        /// <param name="tex">The 2D texture to use.</param>
        public Sprite(
            bool doSetDimensions,
            Texture2D tex)
        {
            rectSrc = new SmoothRect(0, 0, 0, 0);
            rectDest = new SmoothRect(0, 0, 0, 0);
            SetTexture(doSetDimensions, tex);
            origin = new Vector2(0, 0);
            spriteEffects = SpriteEffects.None;
        }

        /// <summary>Includes source/dest rectangles.</summary>
        /// <param name="doSetDimensions">Whether or not to set the width and height based on the texture.</param>
        /// <param name="tex">The 2D texture to use.</param>
        /// <param name="rectSrc">The source rectangle (for spritesheets).</param>
        /// <param name="rectDest">The destination rectangle (for position and stretching). Scaled by default.</param>
        public Sprite(
            Texture2D tex,
            SmoothRect rectSrc,
            SmoothRect rectDest)
        {
            this.rectSrc = rectSrc;
            this.rectDest = rectDest;
            SetTexture(false, texture);
            origin = new Vector2(0, 0);
            spriteEffects = SpriteEffects.None;
        }

        /// <summary>
        /// Sets all parameters except the SpriteEffects used and scaling.
        /// </summary>
        /// <param name="rectSrc">The source rectangle (for spritesheets).</param>
        /// <param name="rectDest">The destination rectangle (for position and stretching). Scaled by default.</param>
        /// <param name="color">The Color object to be used.</param>
        /// <param name="angle">The angle of rotation, moving clockwise with right = 0, in radians.</param>
        /// <param name="depth">The order in which sprites are drawn. 0 is drawn first.</param>
        /// <param name="origin">Where (0,0) is located on the sprite in local coordinates.</param>
        public Sprite(
            Texture2D tex,
            SmoothRect rectSrc,
            SmoothRect rectDest,
            Vector2 origin,
            float angle,
            int depth,
            Color color
            )
        {
            this.rectSrc = rectSrc;
            this.rectDest = rectDest;
            SetTexture(false, texture);
            this.origin = origin;
            this.angle = angle;
            this.depth = depth;
            this.color = color;
            spriteEffects = SpriteEffects.None;
        }

        /// <summary>
        /// Sets all parameters + scaling, except the SpriteEffects used.
        /// </summary>
        /// <param name="rectSrc">
        /// The source rectangle (for spritesheets).
        /// </param>
        /// <param name="rectDest">
        /// The destination rectangle (for position and stretching). Scaled
        /// by default.
        /// </param>
        /// <param name="color">The Color object to be used.</param>
        /// <param name="angle">
        /// The angle of rotation, moving clockwise with right = 0, in radians.
        /// </param>
        /// <param name="depth">
        /// The order in which sprites are drawn. 0 is drawn first.
        /// </param>
        /// <param name="origin">
        /// Where (0,0) is located on the sprite in local coordinates.
        /// </param>
        public Sprite(
            Texture2D tex,
            SmoothRect rectSrc,
            SmoothRect rectDest,
            Vector2 origin,
            float angle,
            int depth,
            Color color,
            float alpha,
            float scaleX,
            float scaleY
            )
        {
            this.rectSrc = rectSrc;
            this.rectDest = rectDest;
            SetTexture(false, texture);
            this.origin = origin;
            this.angle = angle;
            this.depth = depth;
            this.color = color;
            this.alpha = alpha;
            spriteEffects = SpriteEffects.None;
            this.scaleX = scaleX;
            this.scaleY = scaleY;
        }
        #endregion

        #region Methods
        /// <summary>Sets the texture and resets scaling effects.</summary>
        /// <param name="doSetDimensions">
        /// Whether or not width and height are set as well.
        /// </param>
        /// <param name="texture">The 2D texture to use.</param>
        public void SetTexture(bool doSetDimensions, Texture2D texture)
        {
            this.texture = texture;
            if (doSetDimensions)
            {
                rectSrc.Width = texture.Width;
                rectSrc.Height = texture.Height;
                rectDest.Width = (int)(texture.Width * scaleX);
                rectDest.Height = (int)(texture.Height * scaleY);
            }
        }

        /// <summary>
        /// Centers the origin of the image.
        /// </summary>
        public void CenterOrigin()
        {
            origin = new Vector2(rectSrc.Width / 2, rectSrc.Height / 2);
        }

        /// <summary>
        /// Checks for a bounding box intersection of two sprites. Takes
        /// scaling and origin into account. Ignores rotation.
        /// </summary>
        public static bool IsIntersecting(Sprite spr1, SmoothRect rect2)
        {
            //Creates smooth rects from the sprite and another rectangle,
            //which already take scaling into account.
            SmoothRect tempRect1 = new SmoothRect(spr1.rectDest);

            //Adjusts the rectangles based on the origin.
            if (!spr1.doDrawOffset)
            {
                tempRect1.X -= spr1.origin.X;
                tempRect1.Y -= spr1.origin.Y;
            }

            return SmoothRect.IsIntersecting(tempRect1, rect2);
        }

        /// <summary>
        /// Draws the sprite with a SpriteBatch.
        /// </summary>
        public void Draw(SpriteBatch spriteBatch)
        {
            float xPos = rectDest.X;
            float yPos = rectDest.Y;

            //Adjusts to remove origin while drawing if enabled.
            if (doDrawOffset)
            {
                xPos += origin.X;
                yPos += origin.Y;
            }

            switch (drawBehavior)
            {
                case (SpriteDraw.basic):
                    spriteBatch.Draw(
                        texture,
                        new Rectangle(
                            (int)xPos,
                            (int)yPos,
                            (int)(rectDest.Width * scaleX),
                            (int)(rectDest.Height * scaleY)
                            ),
                        color * alpha);
                    break;
                case (SpriteDraw.basicAnimated):
                    spriteBatch.Draw(
                        texture,
                        new Rectangle(
                            (int)xPos,
                            (int)yPos,
                            (int)(rectDest.Width * scaleX),
                            (int)(rectDest.Height * scaleY)
                            ),
                        rectSrc.ToRect(),
                        color * alpha);
                    break;
                case (SpriteDraw.all):
                    spriteBatch.Draw(
                        texture,
                        new Rectangle(
                            (int)xPos,
                            (int)yPos,
                            (int)(rectDest.Width * scaleX),
                            (int)(rectDest.Height * scaleY)
                            ),
                        rectSrc.ToRect(),
                        color * alpha,
                        angle,
                        origin,
                        spriteEffects,
                        0);
                    break;
            }
        }
        #endregion
    }
}