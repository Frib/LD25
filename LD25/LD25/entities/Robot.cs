using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace LD25.entities
{
    public class Robot : WalkingEntity
    {
        private Cube cube;

        public Vector2 LookDir = new Vector2(1, 0);
        bool moving = false;
        public Robot(World w, Vector2 pos)
        {
            cube = new Cube(new Vector3(pos.X, 0, pos.Y), RM.GetTexture("robot"));
            Position = pos;
            Direction = new Vector2(0, 0);
            CanRun = false;
            supportsCamera = true;

            HudIcons.Add(new HudIcon() { text = "Robocam", texture = RM.GetTexture("camera"), Action = (() => w.ToggleCamera(this)) });
            HudIcons.Add(new HudIcon() { text = "Move", texture = RM.GetTexture("arrowupicon"), Action = (() => moving = !moving) });
            HudIcons.Add(new HudIcon() { text = "Turn left", texture = RM.GetTexture("arrowlefticon"), Action = (() => {
                Matrix cameraRotation = Matrix.CreateRotationY(0.05f);
                Vector3 rotTarget = LookDir.ToVector3();
                var result = Vector3.Transform(rotTarget, cameraRotation);
                LookDir = result.ToVector2();
                lastCamDir = result;
            }), OnDown = true });
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
                    lastCamDir = result;
                }),
                OnDown = true
            });
            HudIcons.Add(new HudIcon() { text = "Grab/drop", texture = RM.GetTexture("grabicon"), Action = ToggleGrab });
        }

        public override void WantToOpenDoor(Door door)
        {
                
        }

        public void ToggleGrab()
        {
            if (GrabbedEntity != null)
            {
                GrabbedEntity.ReleaseGrab(this);
                GrabbedEntity.Grabbed = false;
                GrabbedEntity = null;
            }
            else
            {
                var closest = World.entities.Where(e => e.Grabable && e != this).OrderBy(e => (e.Position - this.Position).Length()).FirstOrDefault();
                if (closest != null)
                {
                    var dist = (closest.Position - this.Position).Length();
                    if (dist < 12)
                    {
                        GrabbedEntity = closest;
                        GrabbedEntity.Grabbed = true;
                        if (typeof(Human).IsInstanceOfType(GrabbedEntity))
                        {
                            (GrabbedEntity as Human).PanicLevel += 4;
                        }
                    }
                }
            }
        }

        public Entity GrabbedEntity;
        
        public override void Update()
        {
            if (!Alive && GrabbedEntity != null)
            {
                ToggleGrab();
            }

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
                if (RM.IsPressed(InputAction.Action))
                {
                    ToggleGrab();
                }

                Matrix cameraRotation = Matrix.CreateRotationY(turn);
                Vector3 rotTarget = LookDir.ToVector3();
                var result = Vector3.Transform(rotTarget, cameraRotation);
                LookDir = result.ToVector2();
                lastCamDir = result;
            }

            if (moving || (World.entityCam == this && RM.IsDown(InputAction.Up)))
            {
                Direction = LookDir;
            }

            if (Direction.Length() != 0)
            {
                Direction.Normalize();
                var TempVel = Direction * (running ? RunSpeed : WalkSpeed);
                switch (Stancee)
                {
                    case Stance.Crouched: TempVel *= CrouchModifier; break;
                    case Stance.Prone: TempVel *= ProneModifier; break;
                    default: break;
                }

                var expectedPos = Position + TempVel;

                cube.SetPosition(expectedPos.ToVector3());
                if (!World.IsOnFloor(cube.box))
                {
                    Direction = Vector2.Zero;
                }

                if (GrabbedEntity != null && !GrabbedEntity.CanMoveHere((LookDir * 2)))
                {
                    Direction = Vector2.Zero;
                }
            }

            if (World.entities.OfType<Incinerator>().Any(i => i.cube.box.Intersects(cube.box)))
            {
                Direction = Vector2.Zero;
                if (GrabbedEntity != null)
                {
                    if (typeof(WalkingEntity).IsInstanceOfType(GrabbedEntity))
                    {                        
                        (GrabbedEntity as WalkingEntity).BurnToDeath();
                        if (GrabbedEntity.GetType() == typeof(Human))
                        {
                            GrabbedEntity.DeleteMe = true;
                            var crate = new Crate(GrabbedEntity.Position);
                            World.AddEntity(crate);
                            crate.TurnIntoMeat();
                            GrabbedEntity = crate;
                            crate.Grabbed = true;

                        }
                    }
                }
            }

            base.Update();

            if (GrabbedEntity != null)
            {
                GrabbedEntity.Position = this.Position + (LookDir * 8);
            }

            cube.SetPosition(new Vector3(Position.X, 0, Position.Y));

            cube.ScaleVector.Y = Stancee == entities.Stance.Upright ? 18 : Stancee == entities.Stance.Crouched ? 12 : 8;

            Direction = Vector2.Zero;
        }

        public override string Export()
        {
            return "E:R:" + Position.ToExportString();
        }

        public override bool Intersects(Ray ray)
        {
            return ray.Intersects(cube.box).HasValue;
        }

        internal static Entity Create(World w, string p)
        {
            var pos = p.Split(',');

            return new Robot(w, new Vector2(int.Parse(pos[0]), int.Parse(pos[1])));
        }

        public override void Draw()
        {
            cube.Draw(Selected);
            base.Draw();
        }

        protected override void DrawExtraHudShit(int offset)
        {
            if (GrabbedEntity != null)
            {
                G.g.spriteBatch.DrawString(RM.font, "Holding " + GrabbedEntity.GetType().Name, new Vector2(1056, offset), Color.Yellow);
            }
            offset += 32;
        }
    }
}
