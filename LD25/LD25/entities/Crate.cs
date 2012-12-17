using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace LD25.entities
{
    public class ExplosiveCrate : Crate
    {
        public ExplosiveCrate(Vector2 position) : base(position)
        {
            cube = new Cube(new Vector3(position.X, 0, position.Y), RM.GetTexture("explosivecrate"));

            cube.ScaleVector = new Vector3(8, 8, 8);
            cube.SetPosition(Position.ToVector3());
        }

        public override string Export()
        {
            return "E:E:" + Position.ToExportString();
        }
        internal static Entity Createueue(string p)
        {
            var pos = p.Split(',');

            return new ExplosiveCrate(new Vector2(int.Parse(pos[0]), int.Parse(pos[1])));
        }

        public override void Impact(WalkingEntity hit)
        {
            Direction = Vector2.Zero;
            thrower = null;

            int kills = 0;

            foreach (var ent in World.entities.OfType<WalkingEntity>().Where(e => (e.Position - Position).Length() < 32))
            {
                if (ent.Alive)
                {
                    ent.Health -= 2500;
                    if (!ent.Alive && ent.GetType() == typeof(Human))
                    {
                        kills++;
                    }
                }
            }

            int add = 50 * kills + Math.Max(0, (50 * (kills - 1)));
            World.score += add;
            if (kills >= 1)
            {
                Achievements.Achieve("explosivekill");
            }
            if (kills >= 3)
            {
                Achievements.Achieve("multikill");
            }
            RM.PlaySound("explosion");
            DeleteMe = true;
        }
    }

    public class Crate : WalkingEntity
    {
        public int score = 50;
        public bool meat = false;

        public Crate(Vector2 position)
        {
            this.Position = position;
            cube = new Cube(new Vector3(position.X, 0, position.Y), RM.GetTexture("crate"));

            cube.ScaleVector = new Vector3(8, 8, 8);
            cube.SetPosition(Position.ToVector3());
            CanRun = true;
            WantsToRun = true;
        }

        public void TurnIntoMeat()
        {
            meat = true;
            score = 400;
            cube.textures[0] = RM.GetTexture("cookedmeat");
        }

        public override float RunSpeed
        {
            get
            {
                return 2.0f;
            }
        }

        public override float WalkSpeed
        {
            get
            {
                return 2.0f;
            }
        }

        public override bool Grabable
        {
            get
            {
                return !this.Grabbed;
            }
        }

        internal override void ReleaseGrab(Entity holder)
        {
            var dir = this.Position - holder.Position;
            Direction = dir;
            thrower = holder;
            Health = 3000;
        }

        public Cube cube { get; set; }

        public override string Export()
        {
            return "E:C:" + Position.ToExportString();
        }

        internal static Entity Create(string p)
        {
            var pos = p.Split(',');

            return new Crate(new Vector2(int.Parse(pos[0]), int.Parse(pos[1])));
        }

        public override void Update()
        {
            var pos = Position.ToVector3();
            if (Grabbed || Direction.Length() > 0)
            {
                pos.Y += 5;
            }
            cube.SetPosition(pos);

            if (Direction.Length() > 0)
            {
                var hit = World.entities.OfType<WalkingEntity>().Where(e => e != thrower && e != this && (e.Position - Position).Length() < 8).FirstOrDefault();
                if (hit != null)
                {
                    Impact(hit);
                }


                if (!Grabbed && (!World.IsOnFloor(cube.box) || World.entities.OfType<Door>().Where(e => e.state != DoorState.Open && (e.Position - Position).Length() < 12).Any()))
                {
                    Impact(null);
                }
            }
            base.Update();
        }

        public virtual void Impact(WalkingEntity hit)
        {
            Direction = Vector2.Zero;
            thrower = null;
            if (hit != null && hit.Alive)
            {
                hit.Health -= 1000;
                if (hit.GetType() == typeof(Human))
                {
                    if (!hit.Alive)
                    {
                        World.score += score;
                        if (meat)
                        {
                            Achievements.Achieve("killwithmeatcube");
                        }
                        else
                        {
                            Achievements.Achieve("cratekill");
                        }
                    }
                }
            }
        }

        public override void Draw()
        {
            cube.Draw();
            base.Draw();
        }

        public Entity thrower { get; set; }
    }
}
