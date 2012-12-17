using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace LD25
{
    public class Pathnode
    {
        public Vector2 Location;
        public int cost = 1;
        private Color color = Color.White;
        public bool EvacPoint;

        public Color Color { get { return Selected ? Color.Green : LinkedNodes.Any(n => n.Selected) ? Color.Yellow : WorkRequired ? Color.Blue : EvacPoint ? Color.Fuchsia : color; } }
        public List<Pathnode> LinkedNodes = new List<Pathnode>();

        public Pathnode(World world)
        {
            HudIcons.Add(new HudIcon() { texture = RM.GetTexture("escapeicon"), text = "Evac point", Action = () => EvacPoint = !EvacPoint });
            HudIcons.Add(new HudIcon() { texture = RM.GetTexture("deleteicon"), text = "Remove", Action = () => { world.Pathnodes.Remove(this); LinkedNodes.ForEach(l => l.LinkedNodes.Remove(this)); LinkedNodes.Clear(); world.SelectedNode = null; } });
        }

        public void Link(Pathnode other)
        {
            if (LinkedNodes.Contains(other)) return;
            
            LinkedNodes.Add(other);
            other.LinkedNodes.Add(this);            
        }

        public void EditorLink(Pathnode other)
        {
            if (LinkedNodes.Contains(other))
            {
                LinkedNodes.Remove(other);
                other.LinkedNodes.Remove(this);
            }
            else
            {
                Link(other);
            }
        }

        internal bool Intersects(Ray ray)
        {
            var box = new BoundingBox(new Vector3(Location.X - 8, 0, Location.Y - 8), new Vector3(Location.X + 8, 1, Location.Y + 8));
            return ray.Intersects(box).HasValue;
        }

        public bool Selected { get; set; }

        internal string ExportNode()
        {
            return "N:" + (EvacPoint ? "1" : "0") + ":" + Location.ToExportString();
        }

        internal string ExportLinks(List<Pathnode> others)
        {
            if (LinkedNodes.Any())
            {
                return "L:" + others.IndexOf(this) + ":" + LinkedNodes.Select(n => others.IndexOf(n).ToString()).Aggregate((x, y) => x + "," + y);
            }
            return "";
        }

        internal static Pathnode Create(World w, string p)
        {
            var split = p.Split(':');
            return new Pathnode(w) { EvacPoint = split[0] == "1",  Location = new Vector2(int.Parse(split[1].Split(',')[0]), int.Parse(split[1].Split(',')[1])) };
        }

        internal static void ImportLinks(List<Pathnode> Pathnodes, string p)
        {
            var split = p.Split(':');
            var me = Pathnodes[int.Parse(split[0])];

            foreach (var other in split[1].Split(','))
            {
                me.Link(Pathnodes[int.Parse(other)]);
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
        }

        public virtual void UpdateHudShit()
        {
            var pos = IM.MousePos;
            if (RM.IsPressed(InputAction.Select) && pos.X > 1056 && pos.X < 1248)
            {
                int start = 128;
                for (int i = 0; i < HudIcons.Count; i++)
                {
                    if (pos.Y - start < 32 && pos.Y - start > 0)
                    {
                        HudIcons[i].Action();
                    }
                    start += 48;
                }
            }
        }

        public List<HudIcon> HudIcons = new List<HudIcon>();

        public bool WorkRequired { get; set; }
    }
}
