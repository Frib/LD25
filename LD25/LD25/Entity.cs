using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using LD25.entities;
using Microsoft.Xna.Framework.Graphics;

namespace LD25
{
    public abstract class Entity
    {
        public bool DeleteMe = false;
        public World World;
        public bool supportsCamera = false;
        public virtual float eyeYHeight
        {
            get
            {
                return 16f;
            }
        }

        public bool Selected;

        public virtual void Update()
        {
            var expectedPos = Position + Velocity;
            Position += Velocity;
        }

        public virtual void ProcessCollision(Entity e)
        {

        }

        public virtual void Draw() 
        { 

        }

        public virtual bool Intersects(Ray ray)
        {
            return false;
        }

        public abstract string Export();

        public Vector2 Position { get; set; }

        public Vector2 Velocity { get; set; }

        internal static Entity Parse(World w, string p)
        {
            switch (p.First())
            {
                case 'H': return Human.Create(w, p.Substring(2));
                case 'D': return Door.Create(p.Substring(2));
                case 'W': return Workplace.Create(p.Substring(2));
                case 'R': return Robot.Create(w, p.Substring(2));
                case 'C': return Crate.Create(p.Substring(2));
                case 'E': return ExplosiveCrate.Createueue(p.Substring(2));
                case 'S': return SentryGun.Create(p.Substring(2));
                case 'A': return Camera.Create(p.Substring(2));
                case 'I': return Incinerator.Create(p.Substring(2));
                default: throw new Exception();
            }
        }

        public virtual void DrawHUDInfo()
        {
            var sb = G.g.spriteBatch;

            sb.DrawString(RM.font, this.ToString(), new Vector2(1056, 32), Color.Yellow);
            
            int offset = 128;
            foreach (var hi in HudIcons)
            {
                sb.Draw(hi.texture, new Vector2(1056, offset), Color.White);
                sb.DrawString(RM.font, hi.text, new Vector2(1120, offset + 4), Color.White);

                offset += 48;
            }

            DrawExtraHudShit(offset + 24);
        }

        public virtual void UpdateHudShit()
        {
            var pos = IM.MousePos;
            if (RM.IsDown(InputAction.Select) && pos.X > 1056 && pos.X < 1248)
            {
                int start = 128;
                for (int i = 0; i < HudIcons.Count; i++)
                {
                    if (HudIcons[i].OnDown || RM.IsPressed(InputAction.Select))
                    {
                        if (pos.Y - start < 32 && pos.Y - start > 0)
                        {
                            HudIcons[i].Action();
                        }
                    }
                    start += 48;
                }
            }
        }

        protected virtual void DrawExtraHudShit(int offset)
        {

        }

        public List<HudIcon> HudIcons = new List<HudIcon>();

        public override string ToString()
        {
            return this.GetType().Name;    
        }


        public Pathnode Pathnode { get; set; }

        internal string PathExport()
        {
            if (Pathnode != null)
            {
                return "Q:" + World.entities.IndexOf(this) + ":" + World.Pathnodes.IndexOf(Pathnode);
            }
            else
            {
                return "";
            }
        }

        internal static void LinkToNode(World w, string p)
        {
            var split = p.Split(':');
            w.entities[int.Parse(split[0])].Pathnode = w.Pathnodes[int.Parse(split[1])];
        }

        public virtual Vector3 CamDirection { get { return Vector3.Forward; } }

        public bool Grabbed { get; set; }

        public virtual bool Grabable { get { return false; } }

        public virtual bool CanMoveHere(Vector2 target)
        {
            return true;
        }

        internal virtual void ReleaseGrab(Entity holder)
        {
          
        }
    }

    public class HudIcon
    {
        public Texture2D texture { get; set; }
        public string text { get; set; }
        public Action Action { get; set; }
        public bool OnDown { get; set; }
    }
}
