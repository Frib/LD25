using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LD25
{
    class MainMenuScreen : Screen
    {
        private List<MenuOption> options = new List<MenuOption>();
        private MenuOption selected;

        public MainMenuScreen()
        {

        }

        public override void Show()
        {
            CreateControls();
            G.g.IsMouseVisible = true;
            IM.SnapToCenter = false;
        }

        private void CreateControls()
        {
            options.Clear();
            options.Add(new MenuOption("Play!", RM.font) { Action = new Action(() => g.Showscreen(new GameScreen(0))) });
            options.Add(new MenuOption("Select level", RM.font) { Action = new Action(() => g.Showscreen(new LevelSelectScreen())) });
            options.Add(new MenuOption("Help", RM.font) { Action = new Action(() => g.Showscreen(new HelpScreen(this))) });
            options.Add(new MenuOption("View achievements", RM.font) { Action = new Action(() => g.Showscreen(new AchievementScreen(this))) });
            options.Add(new MenuOption("Configure Controls", RM.font) { Action = new Action(() => g.Showscreen(new ControlScreen(this))) });
            options.Add(new MenuOption("Other Options", RM.font) { Action = new Action(() => g.Showscreen(new OptionsScreen(this))) });
            options.Add(new MenuOption("Quit", RM.font) { Action = new Action(() => g.Exit()) });
            CalculatePositions();
        }

        private void CalculatePositions()
        {
            int offsetY = 100;
            int quarter = G.Width / 5;
            var font = RM.font;
            Vector2 minSize = new Vector2(font.MeasureString("<EMPTY>").X, 0);

            foreach (var o in options)
            {
                o.Position = new Vector2(128, offsetY);
                offsetY += 64;
            }
        }

        public override void Update()
        {
            var m = IM.MousePos;
            var mpos = new Vector2(m.X, m.Y);

            foreach (var o in options)
            {
                if (o.Intersects(mpos))
                {
                    if (RM.IsPressed(InputAction.Accept) || RM.IsPressed(InputAction.Select))
                    {
                        o.Action();
                    }
                    selected = o;
                }
            }
        }

        public override void Draw()
        {
            GraphicsDevice.Clear(new Color(48, 48, 48));
            SpriteBatch sb = G.g.spriteBatch;
            sb.Begin();
            foreach (var o in options)
            {

                o.Draw(selected == o ? Color.White : Color.Yellow);

            }

            sb.Draw(RM.GetTexture("camera"), new Rectangle(500, 192, 128, 128), Color.White);
            sb.DrawString(RM.font, "Science Will Fall!", new Vector2(464, 320), Color.Green);
            sb.DrawString(RM.font, "The evil scientists must die!", new Vector2(64, 464+128), Color.Green);
            sb.DrawString(RM.font, "They are evil!", new Vector2(64, 490+128), Color.Green);
            sb.End();
        }
    }
}
