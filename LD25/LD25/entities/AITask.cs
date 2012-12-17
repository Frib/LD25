using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace LD25.entities
{
    public abstract class AITask
    {
        protected Human human;
        protected World world;

        public List<Pathnode> path;

        public AITask(World world, Human human)
        {
            this.world = world;
            this.human = human;
        }

        internal virtual void Update()
        {
            if (path != null)
            {
                var target = path.FirstOrDefault();
                if (target == null)
                {
                    human.CurrentTask = null;
                    human.Direction = Vector2.Zero;
                    return;
                }
                human.Direction = (target.Location - human.Position);
                human.Direction.Normalize();
                if ((target.Location - human.Position).Length() < 4)
                {
                    human.Direction = Vector2.Zero;
                    path = path.Skip(1).ToList();

                }
            }
        }

        protected void CreatePath(Pathnode target)
        {
            float shortest = float.MaxValue;
            if (target == null) return;
            if (path == null || !path.Any())
            {
                path = new List<Pathnode>();

            }
            else
            {
                shortest = CalcLength(path, shortest);
            }

            Queue<Pathnode> queueThing = new Queue<Pathnode>();
            List<Pathnode> done = new List<Pathnode>();

            done.AddRange(human.restrictedPaths);

            var closestNode = world.Pathnodes.OrderBy(n => (n.Location - human.Position).Length()).Where(n => !human.restrictedPaths.Contains(n)).FirstOrDefault();
            if (closestNode != null)
            {
                done.Add(closestNode);
            }
            if (closestNode == target)
            {
                path = new List<Pathnode>() { target };
                return;
            }

            bool found = false;
            foreach (var link in closestNode.LinkedNodes)
            {
                if (human.restrictedPaths.Contains(link)) continue;

                var route = new List<Pathnode>() { closestNode, link };
                var result = new List<Pathnode>();
                if (FindPath(link, target, route, done.ToList(), result))
                {
                    found = true;
                    float length = CalcLength(result, shortest);
                    if (length < shortest)
                    {
                        path = result;
                        shortest = length;
                    }
                }
            }

            if (!found)
            {
                shortest = float.MaxValue;
                
                foreach (var link in closestNode.LinkedNodes)
                {
                    var route = new List<Pathnode>() { closestNode, link };
                    var result = new List<Pathnode>();
                    done = new List<Pathnode>() { closestNode };
                    if (FindPath(link, target, route, done.ToList(), result))
                    {
                        found = true;
                        float length = CalcLength(result, shortest);
                        if (length < shortest)
                        {
                            path = result;
                            shortest = length;
                        }
                    }
                }
            }

        }

        private bool FindPath(Pathnode link, Pathnode target, List<Pathnode> path, List<Pathnode> avoid, List<Pathnode> result)
        {
            bool found = false;
            float shortest = float.MaxValue;
            avoid.Add(link);
            if (link == target)
            {
                result.AddRange(path);
                return true;
            }
            foreach (var l in link.LinkedNodes)
            {
                if (!avoid.Contains(l))
                {
                    var potential = new List<Pathnode>();
                    if (FindPath(l, target, path.Concat(new[] { l }).ToList(), avoid.ToList(), potential))
                    {
                        found = true;
                        var newLength = CalcLength(potential, shortest);
                        if (newLength < shortest)
                        {
                            result.Clear();
                            result.AddRange(potential);
                            shortest = newLength;
                        }
                    }
                }
            }
            return found;
        }

        public float CalcLength(List<Pathnode> nodes, float cap)
        {
            var result = 0f;

            for (int i = 1; i < nodes.Count; i++)
            {
                result += (nodes[i].Location - nodes[i - 1].Location).Length();
                if (result > cap)
                {
                    return result + 1;
                }
            }

            return result;
        }
    }

    public class ActivateAlarmTask : AITask
    {
        public ActivateAlarmTask(World world, Human human)
            : base(world, human)
        {

        }

        internal override void Update()
        {
            if (path == null || !path.Any() || !path.Last().WorkRequired)
            {
                if (path != null)
                {
                    path = path.Take(1).ToList();
                }
                human.Direction = Vector2.Zero;
                if (human.aitimer <= 0)
                {
                    foreach (var target in world.entities.OfType<AlarmPanel>().Select(p => p.Pathnode))
                    {
                        CreatePath(target);
                    }
                    human.aitimer = 60;
                }
            }

            base.Update();

            var ws = world.entities.OfType<AlarmPanel>().OrderBy(x => (x.Position - human.Position).Length()).FirstOrDefault();
            if (ws != null && (ws.Position - human.Position).Length() < 4)
            {
                ws.WorkHere(human);
                human.CurrentTask = new EscapeTask(world, human);
            }

        }
        // find path to alarm panel
        // follow path
        // when arrived, work at alarm panel
    }

    public class WorkTask : AITask
    {
        public WorkTask(World world, Human human)
            : base(world, human)
        {
            
        }

        internal override void Update()
        {
            if (path == null || !path.Any() || !path.Last().WorkRequired)
            {
                if (path != null)
                {
                    path = path.Take(1).ToList();
                }
                human.Direction = Vector2.Zero;
                if (human.aitimer <= 0)
                {
                    CreatePath(world.Pathnodes.Where(x => x.WorkRequired).OrderBy(x => (x.Location - human.Position).Length()).Random());                    
                    human.aitimer = 60;
                }
            }

            base.Update();

            var ws = world.entities.OfType<Workplace>().OrderBy(x => (x.Position - human.Position).Length()).FirstOrDefault();
            if (ws != null && ws.Pathnode.WorkRequired && ws.needsWork && (ws.Position - human.Position).Length() < 4)
            {
                ws.WorkHere(human);
            }

        }
    }

    public class EscapeTask : AITask
    {
        public EscapeTask(World world, Human human)
            : base(world, human)
        {

        }

        internal override void Update()
        {
            if (path == null || !path.Any())
            {
                if (human.aitimer <= 0)
                {
                    foreach (var target in world.Pathnodes.Where(x => x.EvacPoint))
                    {
                        CreatePath(target);
                    }
                    human.aitimer = 60;
                }
            }

            base.Update();

            if (world.Pathnodes.Where(x => x.EvacPoint && (x.Location - human.Position).Length() < 4).Any())
            {
                world.SaveHuman(human);                
            }
        }
    }

    public class GoToNearestPathNodeTask : AITask
    {
        public GoToNearestPathNodeTask(World world, Human human)
            : base(world, human)
        {

        }

        internal override void Update()
        {
            var closestNode = world.Pathnodes.OrderBy(n => (n.Location - human.Position).Length()).FirstOrDefault();
            if (closestNode != null)
            {
                human.Direction = (closestNode.Location - human.Position);
                human.Direction.Normalize();

                if ((closestNode.Location - human.Position).Length() < 1)
                {
                    human.Direction = Vector2.Zero;
                    human.CurrentTask = null;
                }
            }

        }
    }
}
