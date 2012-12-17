using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.IO;

namespace LD25
{
    public static class Achievements
    {
        public static Dictionary<string, Achievement> achievements = new Dictionary<string, Achievement>();

        public static List<Achievement> achieved = new List<Achievement>();

        public static Queue<Achievement> toShow = new Queue<Achievement>();

        public static void Achieve(string name)
        {
            var achievement = achievements[name];
            if (!achieved.Contains(achievement))
            {
                achieved.Add(achievement);
                toShow.Enqueue(achievement);
            }
            try
            {
                File.AppendAllLines("achievements", new[] { name });
            }
            catch
            {

            }
        }

        public static void AddAchievement(string id, string name, string description, Texture2D icon)
        {
            achievements.Add(id, new Achievement() { Name = name, Description = description, Icon = icon });
        }

        public static void FindAchieved()
        {            

        }

        internal static void Load()
        {
            try
            {
                if (File.Exists("achievements"))
                {
                    var lines = File.ReadAllLines("achievements");

                    foreach (var line in lines)
                    {
                        achieved.Add(achievements[line]);
                    }
                }
            }

            catch
            {

            }
        }
    }

    public class Achievement
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public Texture2D Icon { get; set; }

        int countdown = 300;
        int y = -80;
        int yToAdd = 80;
        int yToRemove = 80;

        public bool Draw()
        {
            if (yToAdd > 0)
            {
                yToAdd -= 4;
                y += 4;
            }
            if (yToAdd <= 0)
            {
                countdown--;
            }
            if (countdown <= 0)
            {
                yToRemove -= 4;
                y -= 4;
            }

            var sb = G.g.spriteBatch;
            sb.Draw(RM.GetTexture("white"), new Rectangle(0, y, 600, 80), Color.Black);
            sb.Draw(Icon, new Vector2(8, y + 8), Color.White);
            sb.DrawString(RM.font, Name, new Vector2(80, y + 8), Color.Yellow);
            sb.DrawString(RM.font, Description, new Vector2(80, y + 32), Color.White);
            
            return countdown <= 0 && yToRemove <= 0;
        }

        public void DrawWithoutBS(int offsetY, bool achieved = false)
        {
            var sb = G.g.spriteBatch;
            sb.Draw(RM.GetTexture("white"), new Rectangle(80, offsetY, 1120, 80), achieved ? Color.DarkGreen : Color.DarkRed);
            sb.Draw(Icon, new Vector2(88, offsetY + 8), Color.White);
            sb.DrawString(RM.font, Name, new Vector2(160, offsetY + 8), Color.Yellow);
            sb.DrawString(RM.font, Description, new Vector2(160, offsetY + 32), Color.White);
        }
    }

    public class AchievementScreen : Screen
    {
        int offset = 0;
        private Screen back;

        public AchievementScreen(Screen back)
        {
            this.back = back;
        }

        public override void Update()
        {
            if (RM.IsDown(InputAction.Down))
            {
                offset -= 8;
            }
            if (RM.IsDown(InputAction.Up))
            {
                offset += 8;
            }

            if (IM.ScrollDelta < 0)
            {
                offset -= 30;
            }
            if (IM.ScrollDelta > 0)
            {
                offset += 30;
            }
            if (RM.IsPressed(InputAction.Back))
            {
                G.g.Showscreen(back);
            }
        }

        public override void Draw()
        {
            GraphicsDevice.Clear(new Color(48, 48, 48));
            G.g.spriteBatch.Begin();
            int offOffset = offset;

            foreach (var achievement in Achievements.achieved)
            {
                achievement.DrawWithoutBS(offOffset, true);
                offOffset += 96;
            }

            foreach (var achievement in Achievements.achievements.Values.Except(Achievements.achieved))
            {
                achievement.DrawWithoutBS(offOffset, false);
                offOffset += 96;
            }

            G.g.spriteBatch.End();
        }
    }
}
