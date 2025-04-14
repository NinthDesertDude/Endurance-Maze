using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;

namespace EnduranceTheMaze
{
    /// <summary>
    /// A stationary hazard that kills actors on contact.
    /// 
    /// Dependencies: MngrLvl, MazeBlock.
    /// 
    /// Activation types: none
    /// 
    /// Custom properties of custInt1: none
    /// Custom properties of custInt2: none
    /// Custom properties of custStr: none
    /// </summary>
    public class MazeSpike : GameObj
    {
        //Relevant assets.
        public static Texture2D TexSpike { get; private set; }

        //Sprite information.    
        private SpriteAtlas spriteAtlas;     

        /// <summary>Sets the block location and default values.</summary>
        /// <param name="x">The column number.</param>
        /// <param name="y">The row number.</param>
        /// <param name="layer">The layer in the maze.</param>
        public MazeSpike(MainLoop game, int x, int y, int layer) :
            base(game, x, y, layer)
        {
            //Sets default values.
            BlockType = Type.Spike;

            //Sets sprite information.
            BlockSprite = new Sprite(true, TexSpike);
            BlockSprite.depth = 0.409f;
            BlockSprite.drawBehavior = SpriteDraw.all;
            BlockSprite.doDrawOffset = true;
            spriteAtlas = new SpriteAtlas(BlockSprite, MainLoop.TileSize, MainLoop.TileSize, 19, 2, 10);
            spriteAtlas.frameSpeed = 0.2f;
            spriteAtlas.CenterOrigin();
        }

        /// <summary>
        /// Loads relevant graphics into memory.
        /// </summary>
        /// <param name="Content">A game content loader.</param>
        public static void LoadContent(ContentManager Content)
        {
            TexSpike = Content.Load<Texture2D>("Content/Sprites/Game/sprSpike");
        }

        /// <summary>
        /// Returns a copy of the object.
        /// </summary>
        public override GameObj Clone()
        {
            MazeSpike newBlock = new(game, X, Y, Layer);
            newBlock.CopyFrom(this);

            //Sets specific variables.
            newBlock.BlockSprite = BlockSprite;
            newBlock.spriteAtlas = new SpriteAtlas(spriteAtlas, false);
            return newBlock;
        }

        /// <summary>
        /// Updates the atlas and damages actor on contact.
        /// </summary>
        public override void Update()
        {
            //Slowly rotates the sprite.
            BlockSprite.angle += 0.02f;

            //Destroys all actors touching the spike.
            var items = game.mngrLvl.itemsJustActors.Where(o => o.X == X && o.Y == Y && o.Layer == Layer);
            foreach (MazeActor item in items)
            {
                item.hp = 0;
                game.playlist.Play(MngrLvl.sndHit, X, Y); //Depends: MngrLvl.
            }

            spriteAtlas.Update(true);
            base.Update();
        }

        /// <summary>
        /// Draws the spike. When hovered, draws enabledness/info.
        /// </summary>
        public override void Draw()
        {
            base.Draw();

            //Sets the tooltip to display disabled status and info.
            if (Sprite.IsIntersecting(BlockSprite, new SmoothRect
                (game.mngrLvl.GetCoordsMouse(), 1, 1)) &&
                Layer == game.mngrLvl.actor.Layer)
            {
                game.mngrLvl.tooltip += "Spike";
                if (!IsEnabled)
                {
                    game.mngrLvl.tooltip += "(disabled)";
                }

                game.mngrLvl.tooltip += " | ";
            }
        }
    }
}