using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LD25
{
    class Camera3d
    {
        public static Camera3d c;

        public Vector3 position = new Vector3(0, 96, 0);
        public float leftRightRot = 0;
        public float upDownRot = -MathHelper.PiOver2;
        public int cameraZoom = 80;

        public Camera3d()
        {
            c = this;
        }

        public void Update()
        {
            float xDifference = IM.MouseDelta.X;
            float yDifference = IM.MouseDelta.Y;

            //leftRightRot -= xDifference * IM.MouseSensitivity;
            //upDownRot -= yDifference * IM.MouseSensitivity;

            //if (upDownRot > MathHelper.TwoPi)
            //    upDownRot -= MathHelper.TwoPi;
            //else if (upDownRot < MathHelper.TwoPi * -1)
            //    upDownRot += MathHelper.TwoPi;

            //if (upDownRot > MathHelper.TwoPi)
            //    upDownRot -= MathHelper.TwoPi;
            //else if (upDownRot < MathHelper.TwoPi * -1)
            //    upDownRot += MathHelper.TwoPi;
            
            if (RM.IsDown(InputAction.ZoomIn) || IM.ScrollDelta > 0)
            {
                position.Y -= 2;
                if (position.Y < 32)
                {
                    position.Y = 32;
                }
            }
            if (RM.IsDown(InputAction.ZoomOut) || IM.ScrollDelta < 0)
            {
                position.Y += 2;
                if (position.Y > 256)
                {
                    position.Y = 256;
                }
            }

            if (RM.IsDown(InputAction.Down))
            {
                position.Z += 2;
            }
            if (RM.IsDown(InputAction.Up))
            {
                position.Z -= 2;
            }

            if (RM.IsDown(InputAction.Left))
            {
                position.X -= 2;
            }
            if (RM.IsDown(InputAction.Right))
            {
                position.X += 2;
            }
        }

        public void Apply(BasicEffect e)
        {
            e.World = Matrix.Identity;
            Matrix cameraRotation = Matrix.CreateRotationX(upDownRot) * Matrix.CreateRotationY(leftRightRot);

            Vector3 cameraOriginalTarget = new Vector3(0, 0, -1);
            Vector3 cameraRotatedTarget = Vector3.Transform(cameraOriginalTarget, cameraRotation);
            cameraFinalTarget = position + cameraRotatedTarget;

            Vector3 cameraOriginalUpVector = new Vector3(0, 1, 0);
            cameraRotatedUpVector = Vector3.Transform(cameraOriginalUpVector, cameraRotation);

            e.View = View;
            e.Projection = Projection;
        }

        public void ApplyCustom(BasicEffect e, Entity ent)
        {
            e.World = Matrix.Identity;
            var entPos = ent.Position.ToVector3();
            entPos.Y += ent.eyeYHeight;

            Vector3 cameraRotatedTarget = ent.CamDirection;
            cameraFinalTarget = entPos + cameraRotatedTarget;

            Vector3 cameraOriginalUpVector = new Vector3(0, 1, 0);

            e.View = Matrix.CreateLookAt(entPos, cameraFinalTarget, cameraOriginalUpVector);
            e.Projection = Projection;
        }

        public Vector3 GetCameraDirection()
        {
            return new Vector3(-(float)Math.Sin(leftRightRot), (float)upDownRot, -(float)Math.Cos(leftRightRot));
        }

        private Vector3 cameraFinalTarget;
        private Vector3 cameraRotatedUpVector;

        public Matrix View
        {
            get
            {
                return Matrix.CreateLookAt(position, cameraFinalTarget, cameraRotatedUpVector);
            }
        }

        public Matrix Projection
        {
            get { return Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(90), (float)G.g.Window.ClientBounds.Width / (float)G.g.Window.ClientBounds.Height, 1f, 10000f); }
        }

        public Vector3 MousePos
        {
            get
            {
                Viewport vp = G.g.GraphicsDevice.Viewport;
                //  Note the order of the parameters! Projection first.
                Vector3 pos1 = vp.Unproject(new Vector3(IM.MousePos.X, IM.MousePos.Y, 0), Projection, View, Matrix.Identity);
                Vector3 pos2 = vp.Unproject(new Vector3(IM.MousePos.X, IM.MousePos.Y, 1), Projection, View, Matrix.Identity);
                Vector3 dir = Vector3.Normalize(pos2 - pos1);

                var r = new Ray(position, dir);

                Vector3 n = new Vector3(0f, 1f, 0f);
                Plane p = new Plane(n, 0f);

                float denominator = Vector3.Dot(p.Normal, r.Direction);
                float numerator = Vector3.Dot(p.Normal, r.Position) + p.D;
                float t = -(numerator / denominator);

                // calculate the picked position on the y = 0 plane
                Vector3 pickedPosition = pos1 + dir * t;

                return pickedPosition;
            }
        }

        public Ray MouseRay
        {
            get
            {
                Viewport vp = G.g.GraphicsDevice.Viewport;
                //  Note the order of the parameters! Projection first.
                Vector3 pos1 = vp.Unproject(new Vector3(IM.MousePos.X, IM.MousePos.Y, 0), Projection, View, Matrix.Identity);
                Vector3 pos2 = vp.Unproject(new Vector3(IM.MousePos.X, IM.MousePos.Y, 1), Projection, View, Matrix.Identity);
                Vector3 dir = Vector3.Normalize(pos2 - pos1);

                var r = new Ray(position, dir);
                return r;
            }
        }

        public Ray MouseRayCustom(Entity ent)
        {
            var entPos = ent.Position.ToVector3();
            entPos.Y += ent.eyeYHeight;

            Vector3 cameraRotatedTarget = ent.CamDirection;
            cameraFinalTarget = entPos + cameraRotatedTarget;

            Vector3 cameraOriginalUpVector = new Vector3(0, 1, 0);

            var view = Matrix.CreateLookAt(entPos, cameraFinalTarget, cameraOriginalUpVector);

            Viewport vp = G.g.GraphicsDevice.Viewport;
            //  Note the order of the parameters! Projection first.
            Vector3 pos1 = vp.Unproject(new Vector3(IM.MousePos.X, IM.MousePos.Y, 0), Projection, view, Matrix.Identity);
            Vector3 pos2 = vp.Unproject(new Vector3(IM.MousePos.X, IM.MousePos.Y, 1), Projection, view, Matrix.Identity);
            Vector3 dir = Vector3.Normalize(pos2 - pos1);

            var r = new Ray(entPos, dir);
            return r;            
        }
    }
}
