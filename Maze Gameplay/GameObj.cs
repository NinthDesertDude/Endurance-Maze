using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;

namespace EnduranceTheMaze
{
    /// <summary>
    /// Serves as a base class for all block objects.
    /// 
    /// Default activation behaviors for all derivatives:
    /// 0: Toggles visibility.
    /// 1: Toggles enabledness.
    /// 2: Switches direction clockwise.
    /// 3: Switches direction counterclockwise.
    /// 4: Deletes the object.
    /// 
    /// Custom activation behaviors are defined for most blocks. The
    /// activation index is used by blocks to discern which blocks to affect
    /// and deal with. ActuateIndex is just for actuators.
    /// </summary>
    public abstract class GameObj
    {
        //Relevant assets.
        public static SoundEffect sndActivated;

        //Refers to the game instance.
        protected MainLoop game;

        //Contains a sprite.
        private Sprite _sprite;

        //Contains a sprite.
        public Sprite BlockSprite
        {
            get
            {
                return _sprite;
            }
            protected set
            {
                _sprite = value;

                //Sets the initial position.
                if (isSynchronized)
                {
                    _sprite.rectDest.X = X * 32;
                    _sprite.rectDest.Y = Y * 32;
                }
                else
                {
                    _sprite.rectDest.X = X;
                    _sprite.rectDest.Y = Y;
                }
            }
        }

        private int _x;
        private int _y;
        private int _layer;
        private Dir _dir = Dir.Right;
        private bool _isSolid;
        private bool _isEnabled;
        private bool _isVisible;
        private bool _isDecor;
        private int _actionIndex = -1;
        private int _actionType = -1;
        private int _actionIndex2 = -1;
        private bool _isActivated = false;
        private int _custInt1 = 0;
        private int _custInt2 = 0;
        private string _custStr = "";

        /// <summary>
        /// The x-location. If <see cref="isSynchronized"/> is true, this is multiplied by the tile size and acts as
        /// the position of the tile.
        /// </summary>
        public virtual int X
        {
            get
            {
                return _x;
            }

            internal set
            {
                _x = value;
            }
        }

        /// <summary>
        /// The y-location. If <see cref="isSynchronized"/> is true, this is multiplied by the tile size and acts as
        /// the position of the tile.
        /// </summary>
        public virtual int Y
        {
            get
            {
                return _y;
            }

            internal set
            {
                _y = value;
            }
        }

        /// <summary>
        /// The layer of the maze that the object is on.
        /// </summary>
        public virtual int Layer
        {
            get
            {
                return _layer;
            }

            internal set
            {
                _layer = value;
            }
        }

        /// <summary>
        /// When true, the X,Y location provided is multiplied by the universal tile size (to support e.g. O(1) access
        /// by X,Y position). When false, the X,Y coordinates should be multiplied by the tile size manually.
        /// </summary>
        public bool isSynchronized;

        /// <summary>
        /// Block identity by type.
        /// </summary>
        public Type BlockType { get; internal set; }

        /// <summary>
        /// The direction that the block faces (if applicable).
        /// </summary>
        public virtual Dir BlockDir
        {
            get
            {
                return _dir;
            }

            internal set
            {
                _dir = value;
            }
        }

        /// <summary>
        /// Solid objects generally can't occupy the same space, and are affected by some objects and not others.
        /// </summary>
        public virtual bool IsSolid
        {
            get
            {
                return _isSolid;
            }

            internal set
            {
                _isSolid = value;
            }
        }

        /// <summary>
        /// Some blocks can be enabled or disabled and change behavior based on that.
        /// </summary>
        public virtual bool IsEnabled
        {
            get
            {
                return _isEnabled;
            }

            internal set
            {
                _isEnabled = value;
            }
        }

        /// <summary>
        /// If invisible, the block will not be drawn. It otherwise functions as normal.
        /// </summary>
        public virtual bool IsVisible
        {
            get
            {
                return _isVisible;
            }

            internal set
            {
                _isVisible = value;
            }
        }

        /// <summary>
        /// If true, all routines that add blocks should add this as a decor item, for performance. Decor items are not
        /// always drawn and do not call update().
        /// </summary>
        public virtual bool IsDecor
        {
            get
            {
                return _isDecor;
            }

            internal set
            {
                _isDecor = value;
            }
        }

        /// <summary>
        /// The activation channel: this block becomes activated only if an actuator activates on the same channel.
        /// </summary>
        public virtual int ActionIndex
        {
            get
            {
                return _actionIndex;
            }

            internal set
            {
                _actionIndex = value;
            }
        }

        /// <summary>
        /// The activation behavior: this is what happens if the block is activated.
        /// </summary>
        public virtual int ActionType
        {
            get
            {
                return _actionType;
            }

            internal set
            {
                _actionType = value;
            }
        }

        /// <summary>
        /// The actuation channel: if this block can activate other blocks, it will activate the ones on this channel.
        /// </summary>
        public virtual int ActionIndex2
        {
            get
            {
                return _actionIndex2;
            }

            internal set
            {
                _actionIndex2 = value;
            }
        }

        /// <summary>
        /// If the block is currently activated. Some blocks only activate once and turn off, others stay activated.
        /// </summary>
        public virtual bool IsActivated
        {
            get
            {
                return _isActivated;
            }

            internal set
            {
                _isActivated = value;
            }
        }

        /// <summary>
        /// Custom properties are used differently by different block types, usually to determine how they behave.
        /// </summary>
        public virtual int CustInt1
        {
            get
            {
                return _custInt1;
            }

            internal set
            {
                _custInt1 = value;
            }
        }

        /// <summary>
        /// Custom properties are used differently by different block types, usually to determine how they behave.
        /// </summary>
        public virtual int CustInt2
        {
            get
            {
                return _custInt2;
            }

            internal set
            {
                _custInt2 = value;
            }
        }

        /// <summary>
        /// A custom string property, used only by certain block types.
        /// </summary>
        public virtual string CustStr
        {
            get
            {
                return _custStr;
            }

            internal set
            {
                _custStr = value;
            }
        }

        /// <summary>
        /// Sets the block's location.
        /// </summary>
        /// <param name="x">The column number.</param>
        /// <param name="y">The row number.</param>
        /// <param name="layer">The layer in the maze.</param>
        public GameObj(MainLoop game, int x, int y, int layer, bool isDecor = false)
        {
            this.game = game;
            this.X = x;
            this.Y = y;
            this.Layer = layer;
            IsSolid = false;
            IsEnabled = true;
            IsVisible = true;
            isSynchronized = true;
            IsDecor = isDecor;
        }

        /// <summary>
        /// Loads relevant assets into memory. (Underscore in method name
        /// used to distinguish from deriving blocks' LoadContent() methods.)
        /// </summary>
        /// <param name="Content">A game content loader.</param>
        public static void _LoadContent(ContentManager Content)
        {
            sndActivated = Content.Load<SoundEffect>("Content/Sounds/sndActivated");
        }

        /// <summary>
        /// Creates a deep copy of the object by copying all members.
        /// Must be implemented in all derivatives.
        /// </summary>
        public abstract GameObj Clone();

        /// <summary>
        /// Performs basic updates. Override to add functionality. Call last.
        /// </summary>
        public virtual void Update()
        {
            //Performs activation behaviors.
            if (IsActivated)
            {
                if (ActionType == 0)
                {
                    IsActivated = false;
                    IsVisible = !IsVisible;
                }
                else if (ActionType == 1)
                {
                    IsActivated = false;
                    IsEnabled = !IsEnabled;
                }
                else if (ActionType == 2 && IsEnabled)
                {
                    IsActivated = false;
                    BlockDir = Utils.DirNext(BlockDir);
                    game.playlist.Play(sndActivated, X, Y);
                }
                else if (ActionType == 3 && IsEnabled)
                {
                    IsActivated = false;
                    BlockDir = Utils.DirPrev(BlockDir);
                    game.playlist.Play(sndActivated, X, Y);
                }
                else if (ActionType == 4 && IsEnabled)
                {
                    IsActivated = false;
                    game.mngrLvl.RemoveItem(this);
                    game.playlist.Play(sndActivated, X, Y);
                }
            }

            //Synchronizes sprite position to location.
            if (isSynchronized)
            {
                BlockSprite.rectDest.X = X * 32;
                BlockSprite.rectDest.Y = Y * 32;
            }
            else
            {
                BlockSprite.rectDest.X = X;
                BlockSprite.rectDest.Y = Y;
            }
        }

        /// <summary>
        /// Draws the sprite. Override to add functionality. Call last.
        /// </summary>
        /// <param name="spriteBatch">The sprite batch to draw with.</param>
        public virtual void Draw()
        {
            if (IsVisible)
            {
                BlockSprite.Draw(game.GameSpriteBatch);
            }
        }
    }
}