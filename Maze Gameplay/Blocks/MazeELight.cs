using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Penumbra;
using System;

namespace Maze
{
    /// <summary>
    /// Shines while activated.
    /// 
    /// Activation types.
    /// 5: Brightens quickly and dims quickly
    /// 6: Brightens quickly and dims slower when off
    /// 7: Brightens quickly and dims slowly when off
    /// 8: Brightens slower and dims quickly when off
    /// 9: Brightens slower and dims slower when off
    /// 10: Brightens slower and dims slowly when off
    /// 11: Brightens slowly and dims quickly when off
    /// 12: Brightens slowly and dims slower when off
    /// 13: Brightens slowly and dims slowly when off
    /// Custom properties of custInt1: int index to a LightSourceColor
    /// Custom properties of custInt2: Intensity
    /// 0: 1/2 tile
    /// 1: 1 tile
    /// 2: 2 tile
    /// 3: 3 tiles
    /// 4: 4 tiles
    /// 5: 5 tiles
    /// 6: 8 tiles
    /// 7: 10 tiles
    /// 8: 20 tiles
    /// 9: 40 tiles
    /// Custom properties of custStr: none
    /// </summary>
    public class MazeELight : GameObj
    {
        //Relevant assets.
        public static Texture2D TexELight { get; private set; }

        /// <summary>
        /// Multipliers for the distances light can go.
        /// </summary>
        public static readonly float[] LightReach = new[] { 0.5f, 1, 2, 3, 4, 5, 8, 10, 20, 40 };

        //Sprite information.
        private SpriteAtlas spriteAtlas;
        private const float targetIntensity = 0.9f;
        private float brightenSpeedMult = 0.5f;
        private float dimSpeedMult = 0.5f;

        /// <summary>Sets the block location and default values.</summary>
        /// <param name="x">The column number.</param>
        /// <param name="y">The row number.</param>
        /// <param name="layer">The layer in the maze.</param>
        public MazeELight(MainLoop game, int x, int y, int layer)
            : base(game, x, y, layer)
        {
            //Sets default values.
            BlockType = Type.ELight;
            IsSolid = true;

            //Sets sprite information.
            BlockSprite = new Sprite(true, TexELight);
            BlockSprite.depth = 0.416f;
            spriteAtlas = new SpriteAtlas(BlockSprite, MainLoop.TileSize, MainLoop.TileSize, 2, 1, 2);

            // ELights cast their own light source.
            Lighting = new(
                new PointLight
                {
                    Radius = MainLoop.TileSize,
                    ShadowType = ShadowType.Occluded,
                    Intensity = 0,
                    CastsShadows = true
                }, null);
        }

        /// <summary>
        /// Loads relevant graphics into memory.
        /// </summary>
        /// <param name="Content">A game content loader.</param>
        public static void LoadContent(ContentManager Content)
        {
            TexELight = Content.Load<Texture2D>("Content/Sprites/Game/sprELight");
        }

        /// <summary>
        /// Returns a copy of the object.
        /// </summary>
        public override GameObj Clone()
        {
            MazeELight newBlock = new(game, X, Y, Layer);
            newBlock.CopyFrom(this);

            //Sets custom variables.
            newBlock.BlockSprite = BlockSprite;
            newBlock.spriteAtlas = new SpriteAtlas(spriteAtlas, false);
            return newBlock;
        }

        /// <summary>
        /// Controls sprite frames used and toggles isSolid.
        /// </summary>
        public override void Update()
        {
            #region Light intensity modulation
            if (IsEnabled)
            {
                if (Lighting.light.Intensity != 0)
                {
                    Lighting.light.Scale = new Vector2(MainLoop.TileSize * LightReach[SlotValueInt2]);
                    Lighting.light.Color = LightSourceColor.GetColorByIndex(SlotValueInt1);
                    Lighting.light.Rotation = (float)(2 * Math.PI * Utils.Rng.NextDouble());
                }

                // Updates target intensity and shifting speed.
                if (IsActivated)
                {
                    if (ActionType == 5 || ActionType == 8 || ActionType == 11) { dimSpeedMult = 0.5f; }
                    else if (ActionType == 6 || ActionType == 9 || ActionType == 12) { dimSpeedMult = 0.2f; }
                    else if (ActionType == 7 || ActionType == 10 || ActionType == 13) { dimSpeedMult = 0.02f; }
                    if (ActionType >= 5 && ActionType <= 7) { brightenSpeedMult = 0.3f; }
                    if (ActionType >= 8 && ActionType <= 10) { brightenSpeedMult = 0.1f; }
                    if (ActionType >= 11 && ActionType <= 13) { brightenSpeedMult = 0.01f; }
                }
            }

            // Dim faster
            if (IsActivated && Lighting.light.Intensity > targetIntensity)
            {
                Lighting.light.Intensity = Utils.Lerp(0.5f, Lighting.light.Intensity, targetIntensity);
                if (Math.Abs(Lighting.light.Intensity - targetIntensity) < 0.1f) { Lighting.light.Intensity = targetIntensity; }
            }
            else if ((!IsActivated || !IsEnabled) && Lighting.light.Intensity != 0)
            {
                Lighting.light.Intensity = Utils.Lerp(dimSpeedMult, Lighting.light.Intensity, 0);
                if (Lighting.light.Intensity < 0.001f) { Lighting.light.Intensity = 0; }
            }

            // Brighten slower
            if (IsEnabled && IsActivated && Lighting.light.Intensity < targetIntensity)
            {
                Lighting.light.Intensity = Utils.Lerp(brightenSpeedMult, Lighting.light.Intensity, targetIntensity);
                if (Math.Abs(Lighting.light.Intensity - targetIntensity) < 0.05f) { Lighting.light.Intensity = targetIntensity; }
            }
            #endregion

            #region Adjusts sprite.
            if (!IsEnabled || !IsActivated)
            {
                spriteAtlas.frame = 1;
            }
            else if (IsActivated)
            {
                spriteAtlas.frame = 0;
            }
            #endregion

            spriteAtlas.Update(true);
            base.Update();
        }

        /// <summary>
        /// Requires the light to also be active to display.
        /// </summary>
        public override void UpdateLighting(bool lightDefaultVis = true, bool shadowDefaultVis = true)
        {
            base.UpdateLighting(lightDefaultVis && (IsActivated || Lighting.light.Intensity != 0), shadowDefaultVis);
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
                game.mngrLvl.tooltip += "E-light | ";
            }
        }
    }
}