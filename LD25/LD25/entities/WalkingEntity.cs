using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace LD25.entities
{
    public abstract class WalkingEntity : Entity
    {

        private int health = 3000;

        public int Health { get { return health; } set { if (health > 0 && value <= 0) { Die(); } health = value; if (PanicLevel < 20 - (health / 150)) { PanicLevel = 20 - (health / 150); } } }

        protected virtual void Die()
        {
        }

        public virtual float WalkSpeed { get { return 0.25f; } }
        public virtual float RunSpeed { get { return 0.6f; } }

        public int PanicLevel { get; set; }
        public Stance Stancee { get; set; }
        public Stance PossibleStance { get { return Health > 2000 ? Stance.Upright : Health > 1000 ? Stance.Crouched : Stance.Prone; } }
        public bool running { get { return CanRun && WantsToRun; } }
        public bool Alive { get { return Health > 0; } }

        public Vector2 Direction;
        List<Door> evildoors = new List<Door>();

        public virtual float CrouchModifier { get { return 0.6f; } }
        public virtual float ProneModifier { get { return 0.25f; } }

        int timer = 0;

        public override bool Grabable
        {
            get
            {
                return !Grabbed;
            }
        }

        public override void Update()
        {
            timer++;
            var thisStance = PossibleStance;

            if (Alive)
            {
                if (Bleeding && timer % 4 == 0)
                {
                    health -= 1;
                    if (health == 0 && this.GetType() == typeof(Human))
                    {
                        Achievements.Achieve("bleedkill");
                    }
                }

                if (Grabbed)
                {
                    thisStance = Stance.Crouched;
                }
                if (Direction.Length() != 0)
                {
                    Direction.Normalize();
                    Velocity = Direction * (running ? RunSpeed : WalkSpeed);
                    switch (Stancee)
                    {
                        case Stance.Crouched: Velocity *= CrouchModifier; break;
                        case Stance.Prone: Velocity *= ProneModifier; break;
                        default: break;
                    }
                }
                else
                {
                    Velocity = Vector2.Zero;
                }

                var doors = World.entities.Where(e => (e.Position - Position).Length() < 32).OfType<Door>();

                foreach (var door in doors)
                {
                    var distance = (door.Position - Position).Length();
                    bool goingTowards = (door.Position - (Position + Velocity)).Length() < distance;

                    if (door.state == DoorState.Opening || door.state == DoorState.Closed)
                    {
                        if (goingTowards)
                        {
                            WantsToRun = false;
                            if (distance < 12)
                            {
                                Velocity = Vector2.Zero;
                                WantToOpenDoor(door);
                            }
                        }
                    }
                    else if (door.state == DoorState.Closing)
                    {
                        if (goingTowards)
                        {
                            WantsToRun = true;
                            if (distance < 8)
                            {
                                if (!evildoors.Contains(door))
                                {
                                    PanicLevel += 2;
                                    evildoors.Add(door);
                                }
                                if (thisStance != entities.Stance.Prone)
                                    thisStance = entities.Stance.Crouched;
                            }
                            else
                            {
                                Velocity = Vector2.Zero;
                            }

                            
                        }
                        if (distance < 4)
                        {
                            if (!evildoors.Contains(door))
                            {
                                PanicLevel += 2;
                                evildoors.Add(door);
                            }
                            if (door.Y < 12)
                            {
                                thisStance = entities.Stance.Prone;
                            }
                            if (door.Y < 7 && Alive)
                            {
                                Velocity = Vector2.Zero;
                                Health = 0;
                                door.ai = false;
                                if (this.GetType() == typeof(Human))
                                {
                                    Score(200);
                                    Achievements.Achieve("doorkill");
                                    RM.PlaySoundEueue("doorcrush");
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                Velocity = Vector2.Zero;
            }

            if (Grabbed)
            {
                Velocity = Vector2.Zero;
            }

            Stancee = thisStance;
            LastMoveCheck();
            base.Update();
        }

        protected virtual void Score(int p)
        {
        }

        public virtual void LastMoveCheck()
        {

        }
        public virtual void WantToOpenDoor(Door door)
        {
            if (!door.Locked)
            {
                door.state = DoorState.Opening;
            }
        }

        public override float eyeYHeight
        {
            get
            {
                return !Alive ? 3 : Stancee == entities.Stance.Upright ? 14 : Stancee == entities.Stance.Crouched ? 9 : 6;
            }
        }

        public bool CanRun { get; set; }

        protected override void DrawExtraHudShit(int offset)
        {

            var sb = G.g.spriteBatch;

            sb.DrawString(RM.font, Alive ? "Alive" : "Dead", new Vector2(1056, offset), Color.Yellow);
        }

        protected Vector3 lastCamDir = Vector3.Right;
        public override Vector3 CamDirection
        {
            get
            {
                if (Direction.Length() != 0)
                {
                    lastCamDir += Direction.ToVector3() / 10;
                    lastCamDir.Normalize();
                   
                }
                return lastCamDir;
            }
        }

        public bool WantsToRun { get; set; }

        public bool Bleeding { get; set; }

        internal virtual void BurnToDeath()
        {
            health = 0;
        }
    }

    public enum Stance
    {
        Upright, Crouched, Prone
    }
}
