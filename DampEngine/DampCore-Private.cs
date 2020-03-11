using DampEngine.Drawing.Projection;
using DampEngine.Extensions;
using DampEngine.Input;
using System;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.SPIRV;
using Veldrid.StartupUtilities;
using Veldrid.Utilities;

namespace DampEngine
{
    public static partial class DampCore
    {
        private static Boolean _windowResize = false;
        private static Boolean _windowClosing = false;
        private static Lazy<GraphicsDevice> _graphicsDevice = new Lazy<GraphicsDevice>(() => CreateGraphicsDevice());
        private static WindowCreateInfo _windowCI;
        private static Lazy<Sdl2Window> _window = new Lazy<Sdl2Window>(() => CreateWindow());
        private static Lazy<CommandList> _frameCommands = new Lazy<CommandList>(() => CreateFrameCommandList());
        private static Lazy<Camera> _frameCamera = new Lazy<Camera>(() => CreateFramCamera());        
        private static DisposeCollectorResourceFactory factory;
        private static DisposeCollector disposer => factory.DisposeCollector;
        private static Boolean IsRunning => !_windowClosing && Window.Exists;

        private static GraphicsDevice CreateGraphicsDevice()
        {
            var output = VeldridStartup.CreateGraphicsDevice(Window, GraphicsBackend.Vulkan);
            factory = new DisposeCollectorResourceFactory(output.ResourceFactory);
            return output;
        }

        private static Camera CreateFramCamera()
        {
            var output = new Camera();
            output.CreateDeviceObjects(factory);
            return output;
        }

        private static Sdl2Window CreateWindow()
        {
            _windowCI = new WindowCreateInfo()
            {
                X = 100,
                Y = 100,
                WindowWidth = 960,
                WindowHeight = 540,
                WindowTitle = "Damp Engine"
            };
            return CreateWindow(ref _windowCI);

        }

        private static Sdl2Window CreateWindow(ref WindowCreateInfo windowCI)
        {
            SDL_WindowFlags flags = SDL_WindowFlags.OpenGL | SDL_WindowFlags.Resizable
                                 | GetWindowFlags(windowCI.WindowInitialState);
            if (windowCI.WindowInitialState != WindowState.Hidden)
            {
                flags |= SDL_WindowFlags.Shown;
            }
            Sdl2Window window = new Sdl2Window(
                windowCI.WindowTitle,
                windowCI.X,
                windowCI.Y,
                windowCI.WindowWidth,
                windowCI.WindowHeight,
                flags,
                true);
            window.Closing += () => { _windowClosing = true; };
            window.Resized += () => _windowResize = true;
            return window;
        }

        private static SDL_WindowFlags GetWindowFlags(WindowState state)
        {
            switch (state)
            {
                case WindowState.Normal:
                    return 0;
                case WindowState.FullScreen:
                    return SDL_WindowFlags.Fullscreen;
                case WindowState.Maximized:
                    return SDL_WindowFlags.Maximized;
                case WindowState.Minimized:
                    return SDL_WindowFlags.Minimized;
                case WindowState.BorderlessFullScreen:
                    return SDL_WindowFlags.FullScreenDesktop;
                case WindowState.Hidden:
                    return SDL_WindowFlags.Hidden;
                default:
                    throw new VeldridException("Invalid WindowState: " + state);
            }
        }

        private static CommandList CreateFrameCommandList()
        {
            var output = GraphicsDevice.ResourceFactory.CreateCommandList();
            output.Name = "Frame Commands List";
            disposer.Add(output);
            return output;
        }

        private static Task RunDraw()
        {
            return Task.Run(() =>
            {                
                while (IsRunning)
                {
                    Sdl2Events.ProcessEvents();
                    var snapshot = Window.PumpEvents();
                    InputTracker.UpdateFrameInput(snapshot, Window);
                    Game.Update();
                    PreDrawEvents();
                    Game.Draw();
                    GraphicsDevice.SwapBuffers();
                }
            });
        }

        private static void PreDrawEvents()
        {
            ResizeGraphicsDevice();
            FrameCommands.Begin();
            FrameCamera.Update();

            FrameCommands.Submit(GraphicsDevice);
        }

        private static void ResizeGraphicsDevice()
        {
            if (!_windowResize) return;

            _windowResize = false;
            FrameCamera.UpdatePerspectiveMatrix();
            GraphicsDevice.ResizeMainWindow((uint)Window.Width, (uint)Window.Height);
        }
    }
}
