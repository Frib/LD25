using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace LD25.entities
{
    public class Incinerator : Entity
    {
        public Cube cube;

        public Incinerator(Vector2 position)
        {
            this.Position = position;
            cube = new Cube(new Vector3(position.X, 0, position.Y), RM.GetTexture("incinerator"));

            cube.ScaleVector = new Vector3(24, 64, 24);
            cube.SetPosition(position.ToVector3() + new Vector3(0, -56, 0));
        }

        public override void Draw()
        {
            cube.Draw();
            base.Draw();
        }

        public override string Export()
        {
            return "E:I:" + Position.ToExportString();
        }

        internal static Entity Create(string p)
        {
            var pos = p.Split(',');

            return new Incinerator(new Vector2(int.Parse(pos[0]), int.Parse(pos[1])));
        }
    }
}
