using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LD25
{
    public class LevelSelectScreen : Screen
    {
        private List<MenuOption> options = new List<MenuOption>();
        private MenuOption selected;

        public LevelSelectScreen()
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

            foreach (var level in RM.LevelNames)
            {
                string derp = level;
                options.Add(new MenuOption(level, RM.font) { Action = () => G.g.Showscreen(new GameScreen(RM.LevelNames.IndexOf(derp))) });
            }

            CalculatePositions();
        }

        private void CalculatePositions()
        {
            int offsetY = 32;
            int quarter = G.Width / 5;
            var font = RM.font;
            Vector2 minSize = new Vector2(font.MeasureString("<EMPTY>").X, 0);

            foreach (var o in options)
            {
                o.Position = new Vector2(128, offsetY);
                offsetY += 32;
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
            if (RM.IsPressed(InputAction.Back))
            {
                G.g.Showscreen(new MainMenuScreen());
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
            sb.End();
        }
    }
}
