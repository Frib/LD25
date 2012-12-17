using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using LD25.entities;

namespace LD25
{
    public class GameScreen : Screen
    {
        private Camera3d camera;
        private World world;
        private bool paused;
        public int currentLevel;

        public GameScreen(int level)
        {
            camera = new Camera3d();
            world = new World(RM.Levels[level]);
            currentLevel = level;
        }

        public override void Update()
        {


            if (RM.IsPressed(InputAction.RestartLevel))
            {
                world = new World(RM.Levels[currentLevel]);
            }

            if (!Victorious && !world.editorEnabled && ((world.allowEditor && IM.IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.N)) || world.entities.OfType<Human>().All(x => !x.Alive)))
            {
                Victorious = true;
                world.score += (1000 - world.timer / 6);
                if (world.timer / 60 < 15)
                {
                    Achievements.Achieve("speedhunt");
                }
                if (world.DarkMode)
                {
                    Achievements.Achieve("finishdark");
                }
                if (world.killedAtLeast3BeforeAlarm)
                {
                    Achievements.Achieve("kill3withoutalarm");
                }
                if (world.killedAllAfterAlarm)
                {
                    Achievements.Achieve("killallafteralarm");
                }
            }

            if (!paused)
            {
                world.Update();
            }
            else
            {
                if (RM.IsPressed(InputAction.Accept))
                {
                    g.Showscreen(new MainMenuScreen());
                }
            }

            if (Victorious && RM.IsPressed(InputAction.Accept))
            {
                currentLevel++;
                if (currentLevel >= RM.Levels.Count)
                {
                    currentLevel = 0;
                }

                world = new World(RM.Levels[currentLevel]);
                Victorious = false;
            }

            if (RM.IsPressed(InputAction.Back))
            {
                paused = !paused;
            }

            if (RM.NextMusic.Count <= 0)
            {
                if (Victorious)
                {
                    RM.NextMusic.Enqueue("music4");
                    RM.NextMusic.Enqueue("music4");
                    RM.NextMusic.Enqueue("music4");
                    RM.NextMusic.Enqueue("music3");
                }
                else if (world.IsAlarmRinging)
                {
                    RM.NextMusic.Enqueue("music3");
                    RM.NextMusic.Enqueue("music3");
                    RM.NextMusic.Enqueue("music3");
                    RM.NextMusic.Enqueue("music4");
                }
                else
                {
                    RM.NextMusic.Enqueue("music2");
                }
            }

            // camera.position.Y = island.CheckHeightCollision(camera.position);
        }

        public override void Draw()
        {

            camera.Apply(e);

            foreach (var p in e.CurrentTechnique.Passes)
            {
                p.Apply();
            }

            world.Draw();

            spriteBatch.Begin();

            if (paused)
            {
                spriteBatch.Draw(RM.GetTexture("white"), new Rectangle((int)(G.Width * 0.25f), (int)(G.Height * 0.4f), (int)(G.Width * 0.5f), (int)(G.Height * 0.2f)), Color.Black);
                spriteBatch.DrawString(g.font, "Game paused. Press " + RM.GetButtons(InputAction.Accept).First().ToString() + " to exit", new Vector2((G.Width * 0.25f) + 64, G.Height / 2 - 32), Color.Red);
                spriteBatch.DrawString(g.font, "Press " + RM.GetButtons(InputAction.Back).First().ToString() + " to continue playing", new Vector2((G.Width * 0.25f) + 64, G.Height / 2), Color.Green);
            }
            if (Victorious)
            {
                spriteBatch.DrawString(g.font, "Victory! press " + RM.GetButtons(InputAction.Accept).First().ToString() + " to continue", new Vector2((G.Width * 0.25f) + 64, G.Height - 64), Color.White);
            }

            if (achievementToRender == null && Achievements.toShow.Any())
            {
                var newAchievement = Achievements.toShow.Dequeue();
                achievementToRender = newAchievement;
            }

            if (achievementToRender != null)
            {
                bool shouldRemove = achievementToRender.Draw();
                if (shouldRemove)
                {
                    achievementToRender = null;
                }
            }

            //spriteBatch.DrawString(g.font, camera.GetCameraDirection().ToString(), Vector2.Zero, Color.White);
            //spriteBatch.DrawString(g.font, camera.upDownRot.ToString() + ", " + camera.leftRightRot.ToString(), new Vector2(0, 64), Color.White);
            world.DrawSprites();
            spriteBatch.End();
        }

        public override void Show()
        {
            IM.SnapToCenter = false;
            g.IsMouseVisible = true;
            base.Show();
        }

        Achievement achievementToRender;

        public bool Victorious { get; set; }
    }
}
