using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace LD25
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class G : Microsoft.Xna.Framework.Game
    {
        public GraphicsDeviceManager graphics;
        public SpriteBatch spriteBatch;
        public static G g;
        public static Random r = new Random();
        public BasicEffect e;
        public SpriteFont font;
        Screen currentScreen;

        public G()
        {
            g = this;
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            Window.Title = "Exterminate!";
            graphics.PreferredBackBufferHeight = 720;
            graphics.PreferredBackBufferWidth = 1280;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            RM.ConfigureKeys();
            currentScreen = new MainMenuScreen();
            currentScreen.Show();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            e = new BasicEffect(GraphicsDevice);
            spriteBatch = new SpriteBatch(GraphicsDevice);
            font = Content.Load<SpriteFont>("font");
            RM.font = font;

            LoadTexture("white");
            LoadTexture("floor");
            LoadTexture("head");
            LoadTexture("door");
            LoadTexture("highlight");
            LoadTexture("hudbar");
            LoadTexture("work");
            LoadTexture("robot");
            LoadTexture("crate");
            LoadTexture("camera");
            LoadTexture("sentry");
            LoadTexture("cookedmeat");
            LoadTexture("incinerator");
            LoadTexture("explosivecrate");

            LoadTexture("closedooricon");
            LoadTexture("opendooricon");
            LoadTexture("lockicon");
            LoadTexture("dooraiicon");
            LoadTexture("escapeicon");
            LoadTexture("deleteicon");
            LoadTexture("arrowlefticon");
            LoadTexture("arrowrighticon");
            LoadTexture("arrowupicon");
            LoadTexture("grabicon");
            LoadTexture("shooticon");
            LoadTexture("sentryaiicon");
            LoadTexture("unknownachievement");

            RM.AddSound("alarm", Content.Load<SoundEffect>("salarm"));
            RM.AddSound("doorcrush", Content.Load<SoundEffect>("doorcrush"));
            RM.AddSound("explosion", Content.Load<SoundEffect>("explosion"));
            RM.AddSound("incinerate", Content.Load<SoundEffect>("incinerate"));
            RM.AddSound("shoot", Content.Load<SoundEffect>("shoot"));


            RM.AddSound("music1", Content.Load<SoundEffect>("musicloopbase"));
            RM.AddSound("music2", Content.Load<SoundEffect>("musicloopdefault"));
            RM.AddSound("music3", Content.Load<SoundEffect>("musicloopfull"));
            RM.AddSound("music4", Content.Load<SoundEffect>("musicloopfullpositive"));
            RM.LoadLevels();

            RM.Volume = 1;

            Achievements.AddAchievement("killwithin32ofexit", "Last second kill", "Killed someone that almost escaped", RM.GetTexture("unknownachievement"));
            Achievements.AddAchievement("killpanicwithin32ofalarm", "Oh no you don't!", "Killed someone that almost pushed the alarm", RM.GetTexture("unknownachievement"));
            Achievements.AddAchievement("killwithmeatcube", "Eat this!", "Killed someone with what used to be their co-worker", RM.GetTexture("unknownachievement"));
            Achievements.AddAchievement("multikill", "Multikill", "Killed at least 3 people at once", RM.GetTexture("unknownachievement"));
            Achievements.AddAchievement("doorkill", "Squish!", "Killed someone with a door", RM.GetTexture("unknownachievement"));
            Achievements.AddAchievement("chokekill", "Bear hug", "Killed someone by choking them to death", RM.GetTexture("unknownachievement"));
            Achievements.AddAchievement("bleedkill", "What a mess", "Someone bled to death", RM.GetTexture("unknownachievement"));
            Achievements.AddAchievement("bulletkill", "Swiss cheese", "Shot someone to death", RM.GetTexture("unknownachievement"));
            Achievements.AddAchievement("manualbulletkill", "Sharpshooter", "YOU shot someone to death!", RM.GetTexture("unknownachievement"));
            Achievements.AddAchievement("killcam", "Say cheese!", "Watched someone die through a camera", RM.GetTexture("unknownachievement"));
            Achievements.AddAchievement("speedhunt", "Speedhunt", "Finished a level within 15 seconds", RM.GetTexture("unknownachievement"));
            Achievements.AddAchievement("incineratekill", "Extra crispy", "Incinerated someone", RM.GetTexture("unknownachievement"));
            Achievements.AddAchievement("cratekill", "JC would be proud", "Crated someone to death", RM.GetTexture("unknownachievement"));
            Achievements.AddAchievement("explosivekill", "Kaboom!", "Killed someone with an explosion", RM.GetTexture("unknownachievement"));
            Achievements.AddAchievement("kill3withoutalarm", "Silent hunter", "Killed at least 3 people without triggering the alarm", RM.GetTexture("unknownachievement"));
            Achievements.AddAchievement("killallafteralarm", "I love a good chase", "Killed every human (at least 3) after the alarm was triggered", RM.GetTexture("unknownachievement"));
            Achievements.AddAchievement("killwithbulletswhileholding", "Let me give you a body", "Shot a human to death while he was being held", RM.GetTexture("unknownachievement"));
            Achievements.AddAchievement("finishdark", "The Hunt", "Finished a dark map", RM.GetTexture("unknownachievement"));

            Achievements.Load();

        }

        private void LoadTexture(string name)
        {
            RM.AddTexture(name, Content.Load<Texture2D>(name));
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            IM.NewState();

            if (RM.IsPressed(InputAction.ChangeSound))
            {
                RM.Volume += 1;
                if (RM.Volume >= 3)
                {
                    RM.Volume = 0;
                }
            }

            RM.UpdateMusic();

            currentScreen.Update();

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            GraphicsDevice.BlendState = BlendState.NonPremultiplied;
            GraphicsDevice.DepthStencilState = new DepthStencilState() { DepthBufferEnable = true, DepthBufferWriteEnable = true };
            GraphicsDevice.SamplerStates[0] = SamplerState.AnisotropicWrap;
            currentScreen.Draw();

            base.Draw(gameTime);
        }

        public static bool HasFocus { get { return g.IsActive; } }
        public static int Width { get { return g.Window.ClientBounds.Width; } }
        public static int Height { get { return g.Window.ClientBounds.Height; } }

        internal void Showscreen(Screen newScreen)
        {
            currentScreen.Hide();
            currentScreen = newScreen;
            currentScreen.Show();
        }
    }
}
