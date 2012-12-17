using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace LD25.entities
{
    public class Camera : Entity
    {
        private Cube cube;
                
        public Vector2 LookDir = Vector2.UnitY;

        public override Vector3 CamDirection
        {
            get
            {
                return LookDir.ToVector3();
            }
        }

        public Camera(Vector2 position)
        {
            this.Position = position;
            cube = new Cube(new Vector3(position.X, 0, position.Y), RM.GetTexture("camera"));
            cube.ScaleVector = new Vector3(8, 8, 8);
            cube.SetPosition(Position.ToVector3() + new Vector3(0, 16, 0));
            supportsCamera = true;
            HudIcons.Add(new HudIcon() { text = "View", texture = RM.GetTexture("camera"), Action = (() => World.ToggleCamera(this)) });
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

                Matrix cameraRotation = Matrix.CreateRotationY(turn);
                Vector3 rotTarget = LookDir.ToVector3();
                var result = Vector3.Transform(rotTarget, cameraRotation);
                LookDir = result.ToVector2();
                LookDir.Normalize();
            }   
        }

        public override void Draw()
        {
            cube.Draw(Selected);
            base.Draw();
        }

        public override float eyeYHeight
        {
            get
            {
                return 20;
            }
        }

        public override string Export()
        {
            return "E:A:" + Position.ToExportString();
        }

        internal static Entity Create(string p)
        {
            var pos = p.Split(',');

            return new Camera(new Vector2(int.Parse(pos[0]), int.Parse(pos[1])));
        }

        public override void DrawHUDInfo()
        {
            base.DrawHUDInfo();
        }
    }
}
