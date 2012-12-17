using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LD25.entities
{
    public class SentryGun : Entity
    {
        private Cube cube;

        int cooldown = 20;

        private bool fire;
        public bool ai;

        public Vector2 LookDir = Vector2.UnitY;

        public override Vector3 CamDirection
        {
            get
            {
                return LookDir.ToVector3();
            }
        }

        public SentryGun(Vector2 position)
        {
            this.Position = position;
            cube = new Cube(new Vector3(position.X, 0, position.Y), RM.GetTexture("sentry"));
            cube.ScaleVector = new Vector3(8, 12, 8);
            cube.SetPosition(Position.ToVector3());
            supportsCamera = true;
            HudIcons.Add(new HudIcon() { text = "Sentrycam", texture = RM.GetTexture("camera"), Action = (() => World.ToggleCamera(this)) });
            HudIcons.Add(new HudIcon()
            {
                text = "Turn left",
                texture = RM.GetTexture("arrowlefticon"),
                Action = (() =>
                {
                    Matrix cameraRotation = Matrix.CreateRotationY(0.05f);
                    Vector3 rotTarget = LookDir.ToVector3();
                    var result = Vector3.Transform(rotTarget, cameraRotation);
                    LookDir = result.ToVector2();
                    LookDir.Normalize();
                }),
                OnDown = true
            });
            HudIcons.Add(new HudIcon()
            {
                text = "Turn right",
                texture = RM.GetTexture("arrowrighticon"),
                Action = (() =>
                {
                    Matrix cameraRotation = Matrix.CreateRotationY(-0.05f);
                    Vector3 rotTarget = LookDir.ToVector3();
                    var result = Vector3.Transform(rotTarget, cameraRotation);
                    LookDir = result.ToVector2();
                    LookDir.Normalize();
                }),
                OnDown = true
            });
            HudIcons.Add(new HudIcon() { texture = RM.GetTexture("shooticon"), text = "Shoot", Action = () => fire = true, OnDown = true });
            HudIcons.Add(new HudIcon() { texture = RM.GetTexture("sentryaiicon"), text = "Toggle AI", Action = () => ai = !ai });
        }

        public override bool Intersects(Ray ray)
        {
            return ray.Intersects(cube.box).HasValue;
        }

        public override void Update()
        {
            base.Update();

            if (World.entityCam == this)
            {
                float turn = 0f;
                if (RM.IsDown(InputAction.Left))
                {
                    turn = 0.05f;
                }
                else if (RM.IsDown(InputAction.Right))
                {
                    turn = -0.05f;
                }

                if (RM.IsDown(InputAction.Action))
                {
                    fire = true;
                }
                Matrix cameraRotation = Matrix.CreateRotationY(turn);
                Vector3 rotTarget = LookDir.ToVector3();
                var result = Vector3.Transform(rotTarget, cameraRotation);
                LookDir = result.ToVector2();
                LookDir.Normalize();
            }

            if (ai)
            {
                var newDir = Vector2.Zero;

                var e = World.entities.OfType<Human>().Where(h => h.Alive).OrderBy(h => (h.Position - Position).Length()).FirstOrDefault();

                if (e != null)
                {
                    if ((e.Position - Position).Length() < 100)
                    {
                        List<Vector3> chain = new List<Vector3>();


                        newDir = e.Position - Position;
                        newDir.Normalize();

                        for (int i = 1; i < 25; i++)
                        {
                            chain.Add((Position + (newDir * i)).ToVector3());
                        }
                        if (World.IsOnFloor(chain))
                        {
                            fire = true;
                        }
                        else
                        {
                            newDir = Vector2.Zero;
                        }
                    }

                    if (newDir.Length() != 0)
                    {
                        LookDir += newDir / 14;
                        LookDir.Normalize();
                    }
                }
            }

            if (fire)
            {
                cooldown--;
                if (cooldown <= 0)
                {
                    RM.PlaySound("shoot");
                    cooldown = 20;
                    Bullet b = new Bullet(this.Position, LookDir);
                    b.thrower = this;
                    World.AddEntity(b);
                    if (!ai)
                    {
                        b.firedManually = true;
                    }
                }
            }

            fire = false;
        }

        public override void Draw()
        {
            cube.Draw(Selected);

            if (Selected)
            {
                G.g.e.TextureEnabled = false;
                G.g.e.VertexColorEnabled = true;
                G.g.e.LightingEnabled = false;
                G.g.e.World = Matrix.Identity;
                G.g.e.CurrentTechnique.Passes[0].Apply();

                var pos = Position.ToVector3() + new Vector3(0, 6, 0);
                var posPlus = pos + LookDir.ToVector3() * 100;
                G.g.GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, new VertexPositionColor[2]{
                    new VertexPositionColor(pos, Color.Red),
                    new VertexPositionColor(posPlus, Color.Red)
                }, 0, 1);

                
                G.g.e.TextureEnabled = true;
                G.g.e.VertexColorEnabled = false;
                G.g.e.LightingEnabled = true;
                G.g.e.CurrentTechnique.Passes[0].Apply();
            }
            base.Draw();
        }

        public override float eyeYHeight
        {
            get
            {
                return 8;
            }
        }

        public override string Export()
        {
            return "E:S:" + Position.ToExportString();
        }

        internal static Entity Create(string p)
        {
            var pos = p.Split(',');

            return new SentryGun(new Vector2(int.Parse(pos[0]), int.Parse(pos[1])));
        }

        public override void DrawHUDInfo()
        {
            base.DrawHUDInfo();
        }
    }
}
