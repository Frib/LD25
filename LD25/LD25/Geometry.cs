using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LD25
{
    public class Geometry
    {
        private List<Vector3> Points = new List<Vector3>();

        public VertexBuffer vb;
        private GeomType geomType;

        public void IsAboveFloor(List<Vector3> points)
        {
            if (geomType != GeomType.Floor)
                return;

            var corners = points.ToList();
            foreach (var c in corners)
            {
                if (IsOnFloor(c.ToVector2()))
                {
                    points.Remove(c);
                }
            }
        }

        private bool IsOnFloor(Vector2 c)
        {
            bool b1, b2, b3;

            var v1 = Points[0].ToVector2();
            var v2 = Points[1].ToVector2();
            var v3 = Points[2].ToVector2();

            b1 = sign(c, v1, v2) < 0.0f;
            b2 = sign(c, v2, v3) < 0.0f;
            b3 = sign(c, v3, v1) < 0.0f;

            return ((b1 == b2) && (b2 == b3));
        }

        float sign(Vector2 p1, Vector2 p2, Vector2 p3)
        {
            return (p1.X - p3.X) * (p2.Y - p3.Y) - (p2.X - p3.X) * (p1.Y - p3.Y);
        }

        public Geometry(IEnumerable<Vector3> points, GeomType gType)
        {
            this.Points = points.ToList();
            this.geomType = gType;

            switch (gType)
            {
                case GeomType.Floor:
                    {
                        vb = new VertexBuffer(G.g.GraphicsDevice, typeof(VertexPositionNormalTexture), 6, BufferUsage.WriteOnly);

                        vb.SetData<VertexPositionNormalTexture>(points.Select(x => new VertexPositionNormalTexture(x, Vector3.Up, new Vector2(x.X / 4, x.Z / 4))).Concat(
                        points.Reverse().Select(x => new VertexPositionNormalTexture(new Vector3(x.X, 24, x.Z), Vector3.Down, new Vector2(x.X / 4, x.Z / 4)))
                        ).ToArray());
                    }
                    break;
                case GeomType.Wall:
                    {
                        vb = new VertexBuffer(G.g.GraphicsDevice, typeof(VertexPositionNormalTexture), 6, BufferUsage.WriteOnly);
                        var result = new List<VertexPositionNormalTexture>();
                        var a = points.First();
                        var b = points.Skip(1).First();

                        var scale = 4;
                        var length = (b - a).Length() / scale;
                        result.Add(new VertexPositionNormalTexture(a, Vector3.Cross(a, b), new Vector2(0, 0)));
                        result.Add(new VertexPositionNormalTexture(b + new Vector3(0, 24, 0), Vector3.Cross(a, b), new Vector2(8, length)));
                        result.Add(new VertexPositionNormalTexture(b, Vector3.Cross(a, b), new Vector2(0, length)));

                        result.Add(new VertexPositionNormalTexture(a + new Vector3(0, 24, 0), Vector3.Cross(a, b), new Vector2(8, 0)));
                        result.Add(new VertexPositionNormalTexture(b + new Vector3(0, 24, 0), Vector3.Cross(a, b), new Vector2(8, length)));
                        result.Add(new VertexPositionNormalTexture(a, Vector3.Cross(a, b), new Vector2(0, 0)));


                        vb.SetData<VertexPositionNormalTexture>(result.ToArray());
                    }
                    break;
                default:
                    throw new Exception();
            }
        }

        public string Export()
        {
            return "G:" + Points.Select(p => p.ToExportString()).Aggregate((x, y) => x + ":" + y);
        }

        internal static Geometry Parse(string input)
        {
            List<Vector3> result = new List<Vector3>();

            var points = input.Split(':');
            foreach (var point in points)
            {
                var coords = point.Split(',');
                Vector3 p = new Vector3(int.Parse(coords[0]), 0, int.Parse(coords[1]));
                result.Add(p);
            }

            return new Geometry(result, result.Count == 3 ? GeomType.Floor : GeomType.Wall);
        }
    }

    public enum GeomType
    {
        Floor, Wall
    }
}
