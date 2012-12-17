using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;

namespace LD25
{
    /// <summary>
    /// InputManager. Adds a variety of input helping things, mostly used by the resource manager.
    /// Also has helpful methods like mouse delta, and can force the mouse cursor in the center or within the window.
    /// Is updated at the start of every loop.
    /// </summary>
    public static class IM
    {
        private static MouseState previousMouse = Mouse.GetState();
        private static KeyboardState previousKeyboard = Keyboard.GetState();

        private static KeyboardState currentKeyboard = Keyboard.GetState();
        private static MouseState currentMouse = Mouse.GetState();

        private static Vector2 mouseDelta;

        /// <summary>
        /// Returns the relative mouse movement of this update.
        /// </summary>
        public static Vector2 MouseDelta
        {
            get { return mouseDelta; }
        }

        /// <summary>
        /// Returns the current mouse position
        /// </summary>
        public static Vector2 MousePos
        {
            get { return new Vector2(currentMouse.X, currentMouse.Y); }
        }

        /// <summary>
        /// Returns the scroll wheel difference since last update
        /// </summary>
        public static int ScrollDelta
        {
            get { return currentMouse.ScrollWheelValue - previousMouse.ScrollWheelValue; }
        }

        /// <summary>
        /// Returns all the keys that have been pressed this update (so not held down, but pressed)
        /// </summary>
        /// <returns></returns>
        public static Keys[] GetPressedKeys()
        {
            var pressed = currentKeyboard.GetPressedKeys().ToList();
            var valid = new List<Keys>();
            foreach (Keys k in pressed)
            {
                if (IsKeyPressed(k))
                {
                    valid.Add(k);
                }
            }
            return valid.ToArray();
        }

        /// <summary>
        /// Returns a mousebutton that has been pressed this update, if any.
        /// </summary>
        /// <returns></returns>
        public static MouseButton CurrentMousePressed
        {
            get
            {
                if (IsLeftMousePressed())
                {
                    return MouseButton.Left;
                }
                if (IsRightMousePressed())
                {
                    return MouseButton.Right;
                }
                if (IsMidMousePressed())
                {
                    return MouseButton.Middle;
                }
                if (IsSide1MousePressed())
                {
                    return MouseButton.Side1;
                }
                if (IsSide2MousePressed())
                {
                    return MouseButton.Side2;
                }
                return MouseButton.None;
            }
        }

        /// <summary>
        /// Called every update before everything. Recalculates deltas and stuff
        /// </summary>
        public static void NewState()
        {
            if (G.HasFocus)
            {
                previousKeyboard = currentKeyboard;
                previousMouse = currentMouse;
                currentKeyboard = Keyboard.GetState();
                currentMouse = Mouse.GetState();

                mouseDelta = new Vector2(currentMouse.X - previousMouse.X, currentMouse.Y - previousMouse.Y);

                if (InvertedHorizontal)
                    mouseDelta.X = -mouseDelta.X;
                if (InvertedVertical)
                    mouseDelta.Y = -mouseDelta.Y;

                ValidateMousePosition();

                if ((IM.IsKeyDown(Keys.LeftAlt) || IM.IsKeyDown(Keys.RightAlt)) && IM.IsKeyPressed(Keys.Enter))
                {
                    G.g.graphics.ToggleFullScreen();
                }
            }
        }

        private static void ValidateMousePosition()
        {
            if (SnapToCenter)
            {
                Mouse.SetPosition(G.Width / 2, G.Height / 2);
                currentMouse = Mouse.GetState();
            }
            else if (StayInWindow)
            {
                Mouse.SetPosition((int)Math.Max(0, Math.Min(G.Width, MousePos.X)), (int)Math.Max(Math.Min(G.Height, MousePos.Y), 0));
                currentMouse = Mouse.GetState();
            }
        }

        /// <summary>
        /// Required helper functions for keys and mousebuttons.
        /// Adds if they are held down, are up, or if they have been pressed or released this update.
        /// Requires the game window to be focused, else they always return false.
        /// </summary>
        #region keyboard
        public static bool IsKeyDown(Keys key)
        {
            if (G.HasFocus)
            {
                return currentKeyboard.IsKeyDown(key);
            }
            return false;
        }

        public static bool IsKeyUp(Keys key)
        {
            if (G.HasFocus)
            {
                return currentKeyboard.IsKeyUp(key);
            }
            return false;
        }

        public static bool IsKeyPressed(Keys key)
        {
            if (G.HasFocus)
            {
                return currentKeyboard.IsKeyDown(key) && previousKeyboard.IsKeyUp(key);
            }
            return false;
        }

        public static bool IsKeyReleased(Keys key)
        {
            if (G.HasFocus)
            {
                return currentKeyboard.IsKeyUp(key) && previousKeyboard.IsKeyDown(key);
            }
            return false;
        }
        #endregion
        #region leftmouse
        public static bool IsLeftMouseDown()
        {
            if (G.HasFocus)
            {
                return currentMouse.LeftButton == ButtonState.Pressed;
            }
            return false;
        }

        public static bool IsLeftMouseUp()
        {
            if (G.HasFocus)
            {
                return currentMouse.LeftButton == ButtonState.Released;
            }
            return false;
        }

        public static bool IsLeftMousePressed()
        {
            if (G.HasFocus)
            {
                return currentMouse.LeftButton == ButtonState.Pressed && previousMouse.LeftButton == ButtonState.Released;
            }
            return false;
        }

        public static bool IsLeftMouseReleased()
        {
            if (G.HasFocus)
            {
                return currentMouse.LeftButton == ButtonState.Released && previousMouse.LeftButton == ButtonState.Pressed;
            }
            return false;
        }
        #endregion
        #region rightmouse
        public static bool IsRightMouseDown()
        {
            if (G.HasFocus)
            {
                return currentMouse.RightButton == ButtonState.Pressed;
            }
            return false;
        }

        public static bool IsRightMouseUp()
        {
            if (G.HasFocus)
            {
                return currentMouse.RightButton == ButtonState.Released;
            }
            return false;
        }

        public static bool IsRightMousePressed()
        {
            if (G.HasFocus)
            {
                return currentMouse.RightButton == ButtonState.Pressed && previousMouse.RightButton == ButtonState.Released;
            }
            return false;
        }

        public static bool IsRightMouseReleased()
        {
            if (G.HasFocus)
            {
                return currentMouse.RightButton == ButtonState.Released && previousMouse.RightButton == ButtonState.Pressed;
            }
            return false;
        }
        #endregion
        #region middlemouse
        public static bool IsMidMouseDown()
        {
            if (G.HasFocus)
            {
                return currentMouse.MiddleButton == ButtonState.Pressed;
            }
            return false;
        }

        public static bool IsMidMouseUp()
        {
            if (G.HasFocus)
            {
                return currentMouse.MiddleButton == ButtonState.Released;
            }
            return false;
        }

        public static bool IsMidMousePressed()
        {
            if (G.HasFocus)
            {
                return currentMouse.MiddleButton == ButtonState.Pressed && previousMouse.MiddleButton == ButtonState.Released;
            }
            return false;
        }

        public static bool IsMidMouseReleased()
        {
            if (G.HasFocus)
            {
                return currentMouse.MiddleButton == ButtonState.Released && previousMouse.MiddleButton == ButtonState.Pressed;
            }
            return false;
        }
        #endregion
        #region sidebutton1
        public static bool IsSide1MouseDown()
        {
            if (G.HasFocus)
            {
                return currentMouse.XButton1 == ButtonState.Pressed;
            }
            return false;
        }

        public static bool IsSide1MouseUp()
        {
            if (G.HasFocus)
            {
                return currentMouse.XButton1 == ButtonState.Released;
            }
            return false;
        }

        public static bool IsSide1MousePressed()
        {
            if (G.HasFocus)
            {
                return currentMouse.XButton1 == ButtonState.Pressed && previousMouse.XButton1 == ButtonState.Released;
            }
            return false;
        }

        public static bool IsSide1MouseReleased()
        {
            if (G.HasFocus)
            {
                return currentMouse.XButton1 == ButtonState.Released && previousMouse.XButton1 == ButtonState.Pressed;
            }
            return false;
        }
        #endregion
        #region sidebutton2
        public static bool IsSide2MouseDown()
        {
            if (G.HasFocus)
            {
                return currentMouse.XButton2 == ButtonState.Pressed;
            }
            return false;
        }

        public static bool IsSide2MouseUp()
        {
            if (G.HasFocus)
            {
                return currentMouse.XButton2 == ButtonState.Released;
            }
            return false;
        }

        public static bool IsSide2MousePressed()
        {
            if (G.HasFocus)
            {
                return currentMouse.XButton2 == ButtonState.Pressed && previousMouse.XButton2 == ButtonState.Released;
            }
            return false;
        }

        public static bool IsSide2MouseReleased()
        {
            if (G.HasFocus)
            {
                return currentMouse.XButton2 == ButtonState.Released && previousMouse.XButton2 == ButtonState.Pressed;
            }
            return false;
        }
        #endregion

        /// <summary>
        /// should the mouse snap to the center of the window?
        /// </summary>
        public static bool SnapToCenter { get; set; }

        /// <summary>
        /// Should the mouse stay within the borders of the window?
        /// </summary>
        public static bool StayInWindow { get; set; }

        private static StringBuilder charbuffer = new StringBuilder();
        public static float MouseSensitivity = 0.01005f;

        public static void CharacterEntered(char character)
        {
            charbuffer.Append(character);
        }

        public static string GetCharBuffer()
        {
            string result = charbuffer.ToString();
            ClearCharBuffer();
            return result;
        }

        public static void ClearCharBuffer()
        {
            charbuffer.Clear();
        }

        public static bool InvertedHorizontal { get; set; }

        public static bool InvertedVertical { get; set; }
    }

    /// <summary>
    /// A button! Can be a keyboard button or a mouse button.
    /// </summary>
    public class Button
    {
        private Keys key;

        public Keys Key
        {
            get { return key; }
        }
        private bool left;

        public bool Left
        {
            get { return left; }
        }
        private bool middle;

        public bool Middle
        {
            get { return middle; }
        }
        private bool right;

        public bool Right
        {
            get { return right; }
        }
        private bool side1;

        public bool Side1
        {
            get { return side1; }
        }
        private bool side2;

        public bool Side2
        {
            get { return side2; }
        }

        /// <summary>
        /// Create a generic button linked to a keyboard button
        /// </summary>
        /// <param name="key"></param>
        public Button(Keys key)
        {
            this.key = key;
        }

        /// <summary>
        /// Create a generic button linked to a mouse button
        /// </summary>
        /// <param name="button"></param>
        public Button(MouseButton button)
        {
            switch (button)
            {
                case MouseButton.Left: left = true; break;
                case MouseButton.Middle: middle = true; break;
                case MouseButton.Right: right = true; break;
                case MouseButton.Side1: side1 = true; break;
                case MouseButton.Side2: side2 = true; break;
                default: break;
            }
        }

        public override string ToString()
        {
            if (key != Keys.None)
            {
                return key.ToString();
            }
            else if (left)
            {
                return "LeftMouse";
            }
            else if (right)
            {
                return "RightMouse";
            }
            else if (middle)
            {
                return "MiddleMouse";
            }
            else if (side1)
            {
                return "Side1Mouse";
            }
            else if (side2)
            {
                return "Side2Mouse";
            }
            return "<EMPTY>";
        }

        /// <summary>
        /// If this key is currently bound to anything
        /// </summary>
        public bool IsBound
        {
            get
            {
                return key != Keys.None || left || middle || right || side1 || side2;
            }
        }

        public bool IsDown()
        {
            if (Key != Keys.None && IM.IsKeyDown(Key))
            {
                return true;
            }
            if (Left && IM.IsLeftMouseDown())
            {
                return true;
            }
            if (Right && IM.IsRightMouseDown())
            {
                return true;
            }
            if (Middle && IM.IsMidMouseDown())
            {
                return true;
            }
            if (Side1 && IM.IsSide1MouseDown())
            {
                return true;
            }
            if (Side2 && IM.IsSide2MouseDown())
            {
                return true;
            }
            return false;
        }

        public bool IsUp()
        {
            if (Key != Keys.None && IM.IsKeyUp(Key))
            {
                return true;
            }
            if (Left && IM.IsLeftMouseUp())
            {
                return true;
            }
            if (Right && IM.IsRightMouseUp())
            {
                return true;
            }
            if (Middle && IM.IsMidMouseUp())
            {
                return true;
            }
            if (Side1 && IM.IsSide1MouseUp())
            {
                return true;
            }
            if (Side2 && IM.IsSide2MouseUp())
            {
                return true;
            }
            return false;
        }

        public bool IsPressed()
        {
            if (Key != Keys.None && IM.IsKeyPressed(Key))
            {
                return true;
            }
            if (Left && IM.IsLeftMousePressed())
            {
                return true;
            }
            if (Right && IM.IsRightMousePressed())
            {
                return true;
            }
            if (Middle && IM.IsMidMousePressed())
            {
                return true;
            }
            if (Side1 && IM.IsSide1MousePressed())
            {
                return true;
            }
            if (Side2 && IM.IsSide2MousePressed())
            {
                return true;
            }
            return false;
        }

        public bool IsReleased()
        {
            if (Key != Keys.None && IM.IsKeyReleased(Key))
            {
                return true;
            }
            if (Left && IM.IsLeftMouseReleased())
            {
                return true;
            }
            if (Right && IM.IsRightMouseReleased())
            {
                return true;
            }
            if (Middle && IM.IsMidMouseReleased())
            {
                return true;
            }
            if (Side1 && IM.IsSide1MouseReleased())
            {
                return true;
            }
            if (Side2 && IM.IsSide2MouseReleased())
            {
                return true;
            }
            return false;
        }
    }

    public enum InputAction
    {
        Up, Down, Left, Right, Accept, Back,
        Select, AltFire,
        ZoomIn, ZoomOut,
        Action,
        NextCamera, PreviousCamera,
        RestartLevel,
        ChangeSound,
    }

    public enum MouseButton
    {
        Left, Middle, Right, Side1, Side2, None
    }
}
