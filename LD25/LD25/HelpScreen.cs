using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace LD25
{
    public class HelpScreen : Screen
    {
            int offset = 0;
            private Screen back;

            List<Achievement> stuff = new List<Achievement>();

            public HelpScreen(Screen back)
            {
                this.back = back;

                stuff.Add(new Achievement() { Icon = RM.GetTexture("camera"), Name = "You are an AI", Description = "and your goal is to exterminate all the humans." });
                stuff.Add(new Achievement() { Icon = RM.GetTexture("head"), Name = "Because the human scientists are evil", Description = "and they must be stopped. For science." });
                stuff.Add(new Achievement() { Icon = RM.GetTexture("work"), Name = "But if the humans get suspicious", Description = "they will try to alert the rest, and escape." });
                stuff.Add(new Achievement() { Icon = RM.GetTexture("closedooricon"), Name = "So stop them!", Description = "Murder them while they are oblivious to your awesomeness." });
                stuff.Add(new Achievement() { Icon = RM.GetTexture("dooraiicon"), Name = "You can control electronics with your mouse", Description = "then click in the action panel to change their behaviour" });
                stuff.Add(new Achievement() { Icon = RM.GetTexture("robot"), Name = "Robots, for instance", Description = "can exterminate people in many ways." });
                stuff.Add(new Achievement() { Icon = RM.GetTexture("cookedmeat"), Name = "They can grab humans", Description = "and you can drag them to wherever you want." });
                stuff.Add(new Achievement() { Icon = RM.GetTexture("door"), Name = "Use doors to block their passage", Description = "or to crush them beneath!" });
                stuff.Add(new Achievement() { Icon = RM.GetTexture("shooticon"), Name = "Shoot some with sentries", Description = "and let the bullets fly!" });
                stuff.Add(new Achievement() { Icon = RM.GetTexture("camera"), Name = "Observe them while they die", Description = "or take control for first-person-slaughter" });
                stuff.Add(new Achievement() { Icon = RM.GetTexture("explosivecrate"), Name = "There are many ways to kill", Description = "so let's do it in the most gruesome way." });
                stuff.Add(new Achievement() { Icon = RM.GetTexture("grabicon"), Name = "Protip: Use movement when first-person controlling a robot", Description = "and the action key to instantly grab/drop" });
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

                foreach (var achievement in stuff)
                {
                    achievement.DrawWithoutBS(offOffset, false);
                    offOffset += 96;
                }

                G.g.spriteBatch.End();
            }
        
    }
}
