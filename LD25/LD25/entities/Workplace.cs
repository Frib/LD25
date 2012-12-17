using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace LD25.entities
{
    public abstract class Workplace : Entity
    {
        public int countdownUntilWorkNeeded = 800;
        public int workTime = 300;

        public int timer = 0;
        public bool needsWork = true;
        public int Rotation = 0;
        public bool special = false;

        public Cube cube;

        public Workplace(Vector2 pos)
        {
            Position = pos;
        }

        public override void Update()
        {
            if (timer == 0)
            {
                if (needsWork)
                {
                    Pathnode.WorkRequired = true;
                }
                else
                {
                    timer = countdownUntilWorkNeeded;
                    needsWork = true;
                }
            }
            else
            {
                timer--;
                Pathnode.WorkRequired = false;
            }

            base.Update();
        }

        public void WorkHere(Human human)
        {
            if (human != null && human.Alive)
            {
                human.WorkTime = workTime;
                Pathnode.WorkRequired = false;
                timer = workTime;
                needsWork = false;
                DoWork(human);
            }
        }

        protected virtual void DoWork(Human human)
        {
        }

        public override void Draw()
        {
            if (cube != null)
            {
                cube.Draw();
            }
            base.Draw();
        }

        internal static Entity Create(string p)
        {
            var split = p.Split(':');
            int type = int.Parse(split[0]);
            int rotation = int.Parse(split[1]);
            var pos = split[2].Split(',');

            Vector2 position = new Vector2(int.Parse(pos[0]), int.Parse(pos[1]));

            switch (type)
            {
                case 0: return new WorkTerminal(position) { Rotation = rotation };
                case 1: return new AlarmPanel(position) { Rotation = rotation };
            }
            throw new Exception();
        }
    }

    public class WorkTerminal : Workplace
    {
        public WorkTerminal(Vector2 pos) : base(pos) { }

        public override string Export()
        {
            return "E:W:0:" + Rotation + ":" + Position.ToExportString();
        }
    }

    public class AlarmPanel : Workplace
    {
        public AlarmPanel(Vector2 pos)
            : base(pos)
        {
            workTime = 0;
            cube = new Cube(new Vector3(pos.X, 0, pos.Y), RM.GetTexture("work"));
            cube.ScaleVector = new Vector3(8, 1, 8);
            cube.SetPosition(Position.ToVector3());
        }

        public override void Update()
        {

        }

        protected override void DoWork(Human human)
        {
            World.SoundTheAlarm();
        }

        public override string Export()
        {
            return "E:W:1:" + Rotation + ":" + Position.ToExportString();
        }
    }
}
