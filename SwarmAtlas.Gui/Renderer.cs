using Assimp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SwarmAtlas.Gui
{
    public interface IRenderer
    {
        void UpdateScene(Scene scene);
        void Stop();
    }

    public class DummyRenderer : IRenderer
    {
        public void UpdateScene(Scene scene)
        {
            // does nothing
        }

        public void Stop()
        {
            // does nothing
        }
    }

    public class Renderer : IRenderer
    {
        private InnerRenderer _innerRenderer;
        private Thread _renderThread;

        public Renderer()
        {
            _innerRenderer = new InnerRenderer();
            _renderThread = new Thread(_innerRenderer.Run);
            _renderThread.IsBackground = true;
            _renderThread.Start();
            // This isn't really useful; when we break, it still hides the window
            //_innerRenderer.StartedEvent.WaitOne();
        }

        public void UpdateScene(Scene scene)
        {
            _innerRenderer.UpdateScene(scene);
        }

        public void Stop()
        {
            _innerRenderer.Stop();
            _renderThread.Join();
        }

        private class InnerRenderer : Game, IRenderer
        {
            private Vector2 _offset = new Vector2(0, 0);
            private float _scale = 1;
            private const float BaseScale = 300f; // how many units wide is the screen, at least initially
            private const float ScrollWheelScale = 1 / 120f;
            private bool _firstUpdate = true;

            private float UnscaledPixelToUnitCoeff => (float)Window.ClientBounds.Width / BaseScale;

            private Point? _mouseLastPosition = null;

            private GraphicsDeviceManager _graphics;
            private SpriteBatch _spriteBatch;

            private Scene _scene = new Scene();


            public ManualResetEvent StartedEvent { get; private set; } = new ManualResetEvent(false);
            

            public InnerRenderer()
            {
                _graphics = new GraphicsDeviceManager(this);
                Content.RootDirectory = "SwarmAtlas"; // TODO: this different?
                IsMouseVisible = true;
                Window.AllowUserResizing = true;
                Window.AllowAltF4 = false;
                Window.Title = "SwarmAtlas Graphical Debugger";
            }

            protected override void Initialize()
            {
                lock (this)
                {
                    _graphics.PreferredBackBufferWidth = 1920;
                    _graphics.PreferredBackBufferHeight = 1080;
                    _graphics.ApplyChanges();

                    base.Initialize();
                }
            }

            protected override void LoadContent()
            {
                lock (this)
                {
                    _spriteBatch = new SpriteBatch(GraphicsDevice);

                    // TODO: use this.Content to load your game content here
                }
            }

            protected override void Update(GameTime gameTime)
            {
                lock (this)
                {
                    var mouseState = Mouse.GetState();
                    CalculateScale(mouseState);
                    WindowDragging(mouseState);

                    GraphicsDevice.Clear(Color.Black);

                    // draw background
                    _spriteBatch.Begin();

                    var mapExtentDrawable = new FilledRect(Vector2.Zero, _scene.MapExtents, Color.DarkGray);
                    mapExtentDrawable.Draw(_spriteBatch, _offset, _scale, Window.ClientBounds.Height);

                    // Uncomment if you want a mouse follower
                    //var mouseFollowerVec = Scene.ScreenPointToWorldVector2(mouseState.Position, _offset, _scale, Window.ClientBounds.Height);
                    //var mouseFollower = new Square(mouseFollowerVec, 5, Color.Magenta);
                    //mouseFollower.Draw(_spriteBatch, _offset, _scale, Window.ClientBounds.Height);

                    _spriteBatch.End();

                    _spriteBatch.Begin();

                    foreach (var mapObject in _scene.MapObjects)
                    {
                        mapObject.Draw(_spriteBatch, _offset, _scale, Window.ClientBounds.Height);
                    }

                    _spriteBatch.End();

                    base.Update(gameTime);
                    _firstUpdate = false;
                    StartedEvent.Set();
                }
            }

            private void WindowDragging(MouseState mouseState)
            {
                // window dragging
                if (mouseState.LeftButton == ButtonState.Pressed)
                {
                    if (_mouseLastPosition.HasValue)
                    {
                        var diff = mouseState.Position - _mouseLastPosition.Value;
                        var diffVec = new Vector2(diff.X, -diff.Y);
                        _offset -= diffVec / _scale;
                        // logic here
                    }
                    _mouseLastPosition = mouseState.Position;
                }
                else
                {
                    _mouseLastPosition = null;
                }
            }

            private void CalculateScale(MouseState mouseState)
            {
                
                var scrollWheel = mouseState.ScrollWheelValue;
                var scrollWheelAmount = scrollWheel * ScrollWheelScale;
                var unadjustedScale = (float)Math.Pow(1.3, scrollWheelAmount); // every time scrollWheelAmount increases by 1, scale up by 30%

                var newScale = unadjustedScale * UnscaledPixelToUnitCoeff;

                // use zoomedAmount to determine what new offset should be
                // Note: this should work even if we aren't scaling
                if (_scale != newScale && !_firstUpdate)
                {
                    var mouseWorldspacePosition = Scene.ScreenPointToWorldVector2(mouseState.Position, _offset, _scale, Window.ClientBounds.Height);
                    _offset = mouseWorldspacePosition;

                    var newMouseWorldspacePosition = Scene.ScreenPointToWorldVector2(mouseState.Position, _offset, newScale, Window.ClientBounds.Height);
                    var diff = newMouseWorldspacePosition - _offset;
                    _offset -= diff;
                }

                _scale = newScale;
            }

            public void UpdateScene(Scene scene)
            {
                lock (this)
                {
                    _scene = scene;
                }
            }

            public void Stop()
            {
                lock (this)
                {
                    Exit();
                }
            }
        }
    }
}
