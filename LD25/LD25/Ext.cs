using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace LD25
{
    public static class Ext
    {
        public static string ToExportString(this Vector3 p)
        {
            return (int)Math.Round(p.X) + "," + (int)Math.Round(p.Z);
        }

        public static string ToExportString(this Vector2 p)
        {
            return (int)Math.Round(p.X) + "," + (int)Math.Round(p.Y);
        }

        public static Vector2 ToVector2Rounded(this Vector3 p)
        {
            return new Vector2((float)Math.Round(p.X), (float)Math.Round(p.Z));
        }

        public static Vector2 ToVector2(this Vector3 p)
        {
            return new Vector2(p.X, p.Z);
        }

        public static Vector3 ToVector3(this Vector2 p)
        {
            return new Vector3(p.X, 0, p.Y);
        }
                
        public static T Random<T>(this IEnumerable<T> collection)
        {

            var list = collection.ToList();
            if (list.Count == 0) return default(T);

            return list[G.r.Next(list.Count)];
        }
                
        public static string GetName(this InputAction ia)
        {
            switch (ia)
            {
                case InputAction.Up: return "Move up";
                case InputAction.Down: return "Move down";
                case InputAction.Left: return "Move left";
                case InputAction.Right: return "Move right";
                case InputAction.Accept: return "Confirm";
                case InputAction.Back: return "Back";
                case InputAction.Action: return "Perform action";
                case InputAction.Select: return "Click";
                case InputAction.AltFire: return "Ignore this one";
                case InputAction.ZoomIn: return "Zoom in";
                case InputAction.ZoomOut: return "Zoom out";
                case InputAction.ChangeSound: return "Change volume";
                case InputAction.RestartLevel: return "Restart level";
                case InputAction.NextCamera: return "Next camera";
                case InputAction.PreviousCamera: return "Previous camera";
                default: return ia.ToString();
            }
        }
    }
}
