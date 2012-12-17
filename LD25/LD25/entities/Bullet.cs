using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace LD25.entities
{
    public class Bullet : Crate
    {
        public Bullet(Vector2 pos, Vector2 dir) : base(pos)
        {
            cube = new Cube(new Vector3(pos.X, 0, pos.Y), RM.GetTexture("white"));

            cube.ScaleVector = new Vector3(2, 2, 2);
            cube.SetPosition(Position.ToVector3());

            this.Direction = dir;
            this.score = 50;
        }

        public override void Impact(WalkingEntity hit)
        {
            Direction = Vector2.Zero;
            thrower = null;

            if (hit != null && hit.Alive)
            {
                hit.Health -= 300;
                hit.Bleeding = true;
                if (hit.Health <= 0 && hit.GetType() == typeof(Human))
                {
                    Achievements.Achieve("bulletkill");
                    if (firedManually)
                    {
                        Achievements.Achieve("manualbulletkill");
                    }
                    if (hit.Grabbed)
                    {
                        Achievements.Achieve("killwithbulletswhileholding");
                    }
                }
            }
            

            DeleteMe = true;
        }

        public override float RunSpeed
        {
            get
            {
                return 4;
            }
        }
        public override float WalkSpeed
        {
            get
            {
                return 4;
            }
        }

        public override string Export()
        {
            return "";
        }

        public bool firedManually { get; set; }
    }
}
