using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Maze
{
    /// <summary>
    /// Handles fullscreening logic.
    /// </summary>
    public class ScreenResizeUtils
    {
        public static readonly int DefaultWidth = 1280;
        public static readonly int DefaultHeight = 640;

        public bool IsFullscreen { get; private set; } = false;

        private int prevWidth = DefaultWidth;
        private int prevHeight = DefaultHeight;
        private GraphicsDeviceManager gDevice;

        public ScreenResizeUtils(GraphicsDeviceManager gDevice)
        {
            IsFullscreen = false;
            prevWidth = DefaultWidth;
            prevHeight = DefaultHeight;
            this.gDevice = gDevice;
        }

        /// <summary>
        /// This returns 0,0 if not fullscreened, else the X,Y coordinates of where the initial window's dimensions
        /// would be if it were centered on the monitor.
        /// </summary>
        public (int, int) GetCurrentOffset()
        {
            if (!IsFullscreen)
            {
                return (0, 0);
            }

            return (
                (gDevice.PreferredBackBufferWidth - DefaultWidth) / 2,
                (gDevice.PreferredBackBufferHeight - DefaultHeight) / 2
            );
        }

        /// <summary>
        /// Toggles fullscreen on and off.
        /// </summary>
        public void ToggleFullscreen()
        {
            if (IsFullscreen)
            {
                gDevice.PreferredBackBufferWidth = prevWidth;
                gDevice.PreferredBackBufferHeight = prevHeight;
                prevWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
                prevHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            }
            else
            {
                prevWidth = gDevice.PreferredBackBufferWidth;
                prevHeight = gDevice.PreferredBackBufferHeight;
                gDevice.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
                gDevice.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            }

            IsFullscreen = !IsFullscreen;
            gDevice.IsFullScreen = IsFullscreen;
            gDevice.ApplyChanges();
        }
    }
}