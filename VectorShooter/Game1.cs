using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System;
using System.Reflection;
using System.IO;

using BloomPostprocess;
namespace VectorShooter
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager _graphics;
        SpriteBatch _spriteBatch;
        public static Input Input;
        public static Game1 Instance { get; private set; }
        public static Viewport Viewport { get { return Instance.GraphicsDevice.Viewport; } }
        public static Vector2 ScreenSize { get { return new Vector2(Viewport.Width, Viewport.Height); } }
        public static ParticleManager<ParticleState> ParticleManager { get; private set; }

        BloomComponent bloom;
        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            _graphics.SupportedOrientations = DisplayOrientation.LandscapeLeft;
            _graphics.IsFullScreen = true;
            _graphics.ApplyChanges();
            Content.RootDirectory = "Content";
            Instance = this;
            Input = new Input();

            bloom = new BloomComponent(this);
            Components.Add(bloom);
            bloom.Settings = new BloomSettings(null, 0.25f, 4, 3, 1, 1.5f, 1);
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            base.Initialize();
            ParticleManager = new ParticleManager<ParticleState>(1024 * 20, ParticleState.UpdateParticle);
            EntityManager.Add(PlayerShip.Instance);
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            // TODO: use this.Content to load your game content here
            Art.Load(Content);

        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here

        }
        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            Input.Update();
            EntityManager.Update();
            EnemySpawner.Update();
            ParticleManager.Update();
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            bloom.BeginDraw();
            GraphicsDevice.Clear(Color.Black);
            _spriteBatch.Begin(SpriteSortMode.Texture, BlendState.Additive);
            EntityManager.Draw(_spriteBatch);
            _spriteBatch.End();

            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive);
            ParticleManager.Draw(_spriteBatch);
            _spriteBatch.End();

            base.Draw(gameTime);

        }

        public Effect LoadShader(string path)
        {
            var assembly = this.GetType().GetTypeInfo().Assembly;
            var temp = this.GetType().GetTypeInfo().Assembly.GetManifestResourceNames();
            Stream s = assembly.GetManifestResourceStream(path);
            //Stream s = assembly.GetManifestResourceStream("SpriteEffectsSample.Content.diffusemap.mgfxo");
            BinaryReader Reader = new BinaryReader(s);
            return new Effect(_graphics.GraphicsDevice, Reader.ReadBytes((int)Reader.BaseStream.Length));
        }
    }
}
