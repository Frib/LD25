using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace LD25.entities
{
    public abstract class Door : Entity
    {
        public bool Locked;
        public DoorState state = DoorState.Closed;
        public bool ai;
        public float Y;
        public Cube cube;

        public Door(Vector2 position)
        {
            Position = position;
            cube = new Cube(Position.ToVector3(), RM.GetTexture("door"));
            cube.SetPosition(new Vector3(Position.X, Y, Position.Y));

            HudIcons.Add(new HudIcon() { text = "Open door", texture = RM.GetTexture("opendooricon"), Action = (() => state = DoorState.Opening) });
            HudIcons.Add(new HudIcon() { text = "Close door", texture = RM.GetTexture("closedooricon"), Action = (() => state = DoorState.Closing) });

            HudIcons.Add(new HudIcon() { text = "Lock/unlock", texture = RM.GetTexture("lockicon"), Action = (() => Locked = !Locked) });
            HudIcons.Add(new HudIcon() { text = "Toggle AI", texture = RM.GetTexture("dooraiicon"), Action = (() => ai = !ai) });
        }

        protected override void DrawExtraHudShit(int offset)
        {
            var sb = G.g.spriteBatch;

            sb.DrawString(RM.font, Locked ? "Locked" : "Unlocked", new Vector2(1056, offset), Color.Yellow);
            sb.DrawString(RM.font, state.ToString(), new Vector2(1056, offset + 48), Color.Yellow);
            sb.DrawString(RM.font, ai ? "Automatic" : "Manual", new Vector2(1056, offset + 96), Color.Yellow);
        }

        public override bool Intersects(Ray ray)
        {
            return ray.Intersects(cube.box).HasValue;
        }

        public override void Update()
        {            
            switch (state)
            {
                case DoorState.Opening:
                    {
                        Y += 0.25f;
                        if (Y >= 22)
                        {
                            Y = 22;
                            state = DoorState.Open;
                        }
                    }
                    break;
                case DoorState.Closing:
                    {
                        Y -= 0.25f;
                        if (Y <= 0)
                        {
                            state = DoorState.Closed;
                            Y = 0;
                        }
                    }
                    break;
                case DoorState.Closed: Y = 0;
                    break;
                case DoorState.Open: Y = 22;
                    break;
                default: break;
            }


            if (ai)
            {
                if ((state == DoorState.Closed || state == DoorState.Open) && !Locked)
                {
                    if (World.entities.OfType<WalkingEntity>().Any(e => e.Alive && (e.Position - Position).Length() < 32))
                    {
                        if (state == DoorState.Closed)
                        state = DoorState.Opening;
                    }
                    else
                    {
                        if (state == DoorState.Open)
                        state = DoorState.Closing;
                    }
                }
            }

            

            base.Update();

            cube.SetPosition(new Vector3(Position.X, Y, Position.Y));
        }

        internal static Entity Create(string p)
        {
            Door door;
            switch (p.First())
            {
                case 'H': door = HorizontalDoor.CreateDoor((DoorState)int.Parse(p[2].ToString()), p[4] == '1', p[6] == '1', p.Substring(8)); break;
                case 'V': door = VerticalDoor.CreateDoor((DoorState)int.Parse(p[2].ToString()), p[4] == '1', p[6] == '1', p.Substring(8)); break;
                default: throw new Exception();
            }
            return door;
        }

        public override void Draw()
        {
            cube.Draw(Selected);
            base.Draw();
        }
    }

    public enum DoorState
    {
        Open,
        Opening,
        Closed,
        Closing
    }

    public enum DoorAI
    {
        Dumb,
        ProximitryOpen
    }

    public class HorizontalDoor : Door
    {
        public HorizontalDoor(Vector2 pos) : base(pos)
        {
            cube.ScaleVector = new Vector3(15.9f, 24, 4);
        }

        public override string Export()
        {
            string result = "E:D:H:" + ((int)state).ToString() + ":" + (ai ? 1 : 0).ToString() + ":" + (Locked ? "1" : "0") + ":" + Position.ToExportString();
            return result;
        }

        internal static Door CreateDoor(DoorState doorState, bool doorAI, bool locked, string p)
        {
            var pos = p.Split(',');

            return new HorizontalDoor(new Vector2(int.Parse(pos[0]), int.Parse(pos[1]))) { ai = doorAI, state = doorState, Locked = locked };
        }
    }

    public class VerticalDoor : Door
    {
        public VerticalDoor(Vector2 pos) : base(pos)
        {
            cube.ScaleVector = new Vector3(4, 24, 15.9f);
        }

        public override string Export()
        {
            return "E:D:V:" + ((int)state).ToString() + ":" + (ai ? 1 : 0).ToString() + ":" + (Locked ? "1" : "0") + ":" + Position.ToExportString();
        }

        internal static Door CreateDoor(DoorState doorState, bool doorAI, bool locked, string p)
        {
            var pos = p.Split(',');

            return new VerticalDoor(new Vector2(int.Parse(pos[0]), int.Parse(pos[1]))) { ai = doorAI, state = doorState, Locked = locked };
        }
    }
}
