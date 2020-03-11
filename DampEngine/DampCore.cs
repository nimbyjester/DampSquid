using DampEngine.Drawing.Projection;
using System;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.SPIRV;
using Veldrid.StartupUtilities;

namespace DampEngine
{
    public static partial class DampCore
    {
        public static Game Game;
        public static GraphicsDevice GraphicsDevice => _graphicsDevice.Value;
        public static Sdl2Window Window => _window.Value;
        public static CommandList FrameCommands => _frameCommands.Value;
        public static Camera FrameCamera => _frameCamera.Value;
        public static void Run(Game game)
        {
            Game = game;
            game.Start();
            RunDraw().ContinueWith((a) => EndGame()).Wait();
        }

        public static void EndGame()
        {
            Game?.Dispose();
            disposer?.DisposeAll();
            GraphicsDevice?.Dispose();
        }

    }
}
