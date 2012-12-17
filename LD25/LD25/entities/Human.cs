using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LD25.entities
{
    public class Human : WalkingEntity
    {
        public Cube cube;
        public List<Pathnode> restrictedPaths = new List<Pathnode>();
        public List<Human> witnessedHumans = new List<Human>();

        public Human(World world, Vector2 pos)
        {
            cube = new Cube(new Vector3(pos.X, 0, pos.Y), RM.GetTexture("head"));
            Position = pos;
            Direction = new Vector2(0, 0);
            PanicLevel = 0;
            CurrentTask = new GoToNearestPathNodeTask(world, this);
            this.World = world;
            CanRun = true;
        }

        public override void LastMoveCheck()
        {
            var max = cube.box.Max;
            var min = cube.box.Min;
            max += Velocity.ToVector3();
            min += Velocity.ToVector3();

            if (World.entities.OfType<Human>().Any(h => h != this && h.Alive && h.Velocity.Length() > 0 && h.cube.box.Intersects(new BoundingBox(min, max))))
            {
                Velocity = Vector2.Zero;
            }
            base.LastMoveCheck();
        }

        protected override void Die()
        {
            World.score += 100;
            if (PanicLevel >= 6 && World.Pathnodes.Any(x => (x.Location - Position).Length() < 32 && x.EvacPoint))
            {
                Achievements.Achieve("killwithin32ofexit");
            }
            if (PanicLevel >= 10 && !World.IsAlarmRinging && World.entities.OfType<AlarmPanel>().Any(x => (x.Position - Position).Length() < 32))
            {
                Achievements.Achieve("killpanicwithin32ofalarm");
            }
            World.Deaths++;
        }

        protected override void Score(int p)
        {
            World.score += p;    
        }

        public override void Update()
        {
            if (Grabbed && Alive)
            {
                Health -= 1;
                if (Health % 300 == 0)
                {
                    PanicLevel += 1;
                }
                if (Health == 0)
                {
                    Achievements.Achieve("chokekill");
                }
            }

            var doors = World.entities.Where(e => (e.Position - Position).Length() < 12).OfType<Door>();

            foreach (var d in doors.Where(x => x.Locked && x.state != DoorState.Open && x.state != DoorState.Opening))
            {
                if (!restrictedPaths.Contains(d.Pathnode))
                {
                    restrictedPaths.Add(d.Pathnode);
                }
                if (CurrentTask != null && CurrentTask.path != null)
                {
                    CurrentTask.path.Clear();
                }
            }


            aitimer--;
            ProcessAI();
            
            if (World.entities.OfType<Incinerator>().Any(i => i.cube.box.Intersects(cube.box)))
            {
                Direction = Vector2.Zero;
            }

            base.Update();
            cube.SetPosition(new Vector3(Position.X, 0, Position.Y));

            cube.ScaleVector.Y = Stancee == entities.Stance.Upright ? 18 : Stancee == entities.Stance.Crouched ? 12 : 8;

            if (Alive)
            {
                foreach (var human in World.entities.OfType<Human>())
                {
                    if (!human.Alive && (human.Position - Position).Length() < 36)
                    {
                        PanicLevel += 10;
                    }
                    else if (human != this && human.Alive && human.Grabbed && (human.Position - Position).Length() < 36)
                    {
                        if (!witnessedHumans.Contains(human))
                        {
                            witnessedHumans.Add(human);
                            PanicLevel += 6;
                        }
                    }
                }
            }
        }

        internal override void BurnToDeath()
        {
            cube.textures[0] = RM.GetTexture("cookedmeat");
            if (Alive)
            {
                World.score += 100;
                Achievements.Achieve("incineratekill");
                RM.PlaySoundEueue("incinerate");
            } 
            base.BurnToDeath();           
        }

        private void ProcessAI()
        {
            if (PanicLevel >= 10)
            {
                if (!World.IsAlarmRinging && (CurrentTask == null || CurrentTask.GetType() != typeof(ActivateAlarmTask)))
                {
                    CurrentTask = new ActivateAlarmTask(World, this);
                }
                else if (World.IsAlarmRinging && (CurrentTask == null || CurrentTask.GetType() != typeof(EscapeTask)))
                {
                    CurrentTask = new EscapeTask(World, this);
                }
                WantsToRun = true;
            }
            else if (PanicLevel >= 6)
            {
                if (CurrentTask == null || CurrentTask.GetType() != typeof(EscapeTask))
                {
                    CurrentTask = new EscapeTask(World, this);
                }
            }
            else if (CurrentTask == null)
            {
                CurrentTask = new WorkTask(World, this);
            }

            if (WorkTime <= 0)
            {
                CurrentTask.Update();
            }
            else
            {
                Direction = Vector2.Zero;
            }
            WorkTime--;


        }

        public override string Export()
        {
            return "E:H:" + Position.ToExportString();
        }

        public override bool Intersects(Ray ray)
        {
            return ray.Intersects(cube.box).HasValue;
        }

        public override void Draw()
        {
            cube.Draw(Selected);

            //if (Selected && CurrentTask != null && CurrentTask.path != null)
            //{
            //    var e = G.g.e;
            //    e.TextureEnabled = false;
            //    e.LightingEnabled = false;
            //    e.VertexColorEnabled = true;
            //    e.World = Matrix.Identity;
            //    e.CurrentTechnique.Passes[0].Apply();
            //    foreach (var node in CurrentTask.path)
            //    {
            //        var p = node.Location;
            //        G.g.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, new VertexPositionColor[4]{ 
            //            new VertexPositionColor(new Vector3(p.X-8f, 1,  p.Y-8f), Color.Red),
            //            new VertexPositionColor(new Vector3(p.X+8f, 1,  p.Y-8f), Color.Red),
            //            new VertexPositionColor(new Vector3(p.X-8f, 1,  p.Y+8f), Color.Red),
            //            new VertexPositionColor(new Vector3(p.X+8f, 1,  p.Y+8f), Color.Red),
            //            }, 0, 2);

            //        foreach (var linked in node.LinkedNodes)
            //        {
            //            G.g.GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, new VertexPositionColor[2] { new VertexPositionColor(new Vector3(p.X - 8f, 0, p.Y - 8f), node.Color),
            //                    new VertexPositionColor(new Vector3(linked.Location.X + 8f, 0, linked.Location.Y + 8f), node.Color),
            //                    }, 0, 1);
            //        }
            //    } 
            //    e.LightingEnabled = true;
            //    e.TextureEnabled = true;
            //    e.VertexColorEnabled = false;
            //    e.CurrentTechnique.Passes[0].Apply();
            //}

            base.Draw();
        }

        internal static Entity Create(World w, string p)
        {
            var pos = p.Split(',');

            return new Human(w, new Vector2(int.Parse(pos[0]), int.Parse(pos[1])));
        }

        protected override void DrawExtraHudShit(int offset)
        {
            var sb = G.g.spriteBatch;

            sb.DrawString(RM.font, "Vital signs: " + Health.ToString(), new Vector2(1056, offset + 32), Color.Yellow);
        }

        public int WorkTime { get; set; }

        public AITask CurrentTask { get; set; }

        public override bool CanMoveHere(Vector2 target)
        {
            cube.SetPosition(Position.ToVector3());
            var corners = cube.box.GetCorners().Select(x => (x.ToVector2() + target).ToVector3()).ToList();
            return World.IsOnFloor(corners) && !World.entities.OfType<Door>().Where(x => x.state != DoorState.Open).Any(x => (x.Position - (Position + target)).Length() < 12);
        }

        public int aitimer { get; set; }
    }
}
