using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Reflection;
using System.IO;
using System.Globalization;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;

namespace LD25
{
    /// <summary>
    /// ResourceManager, handling dictionaries for fonts, inputs, textures, etc
    /// </summary>
    public static class RM
    {
        public static SoundEffect music;

        public static List<string[]> Levels = new List<string[]>();
        public static List<string> LevelNames = new List<string>();

        private static Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();
        public static void AddTexture(string name, Texture2D tex)
        {
            textures.Add(name, tex);
        }

        public static Texture2D GetTexture(string name)
        {
            return textures[name];
        }

        private static Dictionary<InputAction, List<Button>> input = new Dictionary<InputAction, List<Button>>();

        /// <summary>
        /// Initialize the input dictionary lists.
        /// </summary>
        static RM()
        {
            foreach (InputAction ia in GetValidInputActions())
            {
                input.Add(ia, new List<Button>());
            }
        }

        /// <summary>
        /// Get an array with all the inputs that need to be bound
        /// </summary>
        /// <returns></returns>
        public static InputAction[] GetValidInputActions()
        {
            List<InputAction> result = new List<InputAction>();

            foreach (string s in Enum.GetNames(typeof(InputAction)))
            {
                result.Add((InputAction)Enum.Parse(typeof(InputAction), s));
            }

            return result.ToArray();
        }

        /// <summary>
        /// Add a button to a specified input
        /// </summary>
        /// <param name="keyName"></param>
        /// <param name="linkedButton"></param>
        public static void AddKey(InputAction ia, Button linkedButton)
        {
            input[ia].Add(linkedButton);
        }

        /// <summary>
        /// Insert a button to a specified input into the input list, potentially replacing an old one.
        /// </summary>
        /// <param name="keyName"></param>
        /// <param name="linkedButton"></param>
        /// <param name="position"></param>
        public static void InsertKey(InputAction ia, Button linkedButton, int position)
        {
            if (linkedButton == null || linkedButton.Key == Keys.Enter || linkedButton.Key == Keys.Escape)
            {
                return;
            }
            if (input[ia].Count <= position)
            {
                AddKey(ia, linkedButton);
            }
            else
            {
                if (input[ia][position].Key != Keys.Escape && input[ia][position].Key != Keys.Enter)
                {
                    input[ia][position] = linkedButton;
                }
            }
        }

        /// <summary>
        /// Check if the specified input button is held down
        /// </summary>
        /// <param name="ia"></param>
        /// <returns></returns>
        internal static bool IsDown(InputAction ia)
        {
            foreach (Button b in input[ia])
            {
                if (b.IsDown())
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Check if the specified input button is not held down
        /// </summary>
        /// <param name="ia"></param>
        /// <returns></returns>
        internal static bool IsUp(InputAction ia)
        {
            foreach (Button b in input[ia])
            {
                if (b.IsUp())
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Check if the specified input button has been pressed this update
        /// </summary>
        /// <param name="ia"></param>
        /// <returns></returns>
        internal static bool IsPressed(InputAction ia)
        {
            foreach (Button b in input[ia])
            {
                if (b.IsPressed())
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Check if the specified input button has been released this update
        /// </summary>
        /// <param name="ia"></param>
        /// <returns></returns>
        internal static bool IsReleased(InputAction ia)
        {
            foreach (Button b in input[ia])
            {
                if (b.IsReleased())
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Configure the keys from config text files.
        /// </summary>
        public static void ConfigureKeys()
        {
            ValidateConfigFiles();

            string[] lines = File.ReadAllLines("config.txt");
            foreach (string line in lines.Where((s) => !s.StartsWith("//", StringComparison.OrdinalIgnoreCase) && s.Contains(':')))
            {
                string key = new string(line.Split(':')[0].Where((char c) => { return !char.IsWhiteSpace(c); }).ToArray()).ToUpperInvariant();
                string values = new string(line.Split(':')[1].Where((char c) => { return !char.IsWhiteSpace(c); }).ToArray()).ToUpperInvariant();

                foreach (string val in values.Split(','))
                {
                    AddButton(key, val);
                }
            }
        }

        private static void AddButton(string inputaction, string button)
        {
            Button b = CreateButtonFromText(button);
            if (b != null && b.IsBound)
            {
                InputAction ia;
                if (Enum.TryParse<InputAction>(inputaction, true, out ia))
                {
                    AddKey(ia, b);
                }
            }
        }

        private static Button CreateButtonFromText(string val)
        {
            switch (val)
            {
                case ("LEFTMOUSE"): return new Button(MouseButton.Left);
                case ("MIDDLEMOUSE"): return new Button(MouseButton.Middle);
                case ("RIGHTMOUSE"): return new Button(MouseButton.Right);
                case ("SIDE1MOUSE"): return new Button(MouseButton.Side1);
                case ("SIDE2MOUSE"): return new Button(MouseButton.Side2);
                default: break;
            }
            try
            {
                Keys k = (Keys)Enum.Parse(typeof(Keys), val, true);
                return new Button(k);
            }
            catch (ArgumentException)
            {
                return new Button(Keys.None);
            }
        }

        private static void ValidateConfigFiles()
        {
            if (!File.Exists("config.txt"))
            {
                if (File.Exists("defaultconfig.txt"))
                {
                    File.Copy("defaultconfig.txt", "config.txt", true);
                }
                else
                {
                    CreateDefaultConfigFiles();
                }
            }
        }

        private static void CreateDefaultConfigFiles()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("//Autogenerated, might be missing some things");
            sb.AppendLine("up:Up,W");
            sb.AppendLine("down:Down,S");
            sb.AppendLine("left:Left,A");
            sb.AppendLine("right:Right,D");
            sb.AppendLine("back:escape");
            sb.AppendLine("accept:enter");
            sb.AppendLine("select:LeftMouse");
            sb.AppendLine("altfire:RightMouse");
            sb.AppendLine("action:Space");
            sb.AppendLine("zoomin:E");
            sb.AppendLine("zoomout:Q");
            sb.AppendLine("nextcamera:D2");
            sb.AppendLine("previouscamera:D1");
            sb.AppendLine("restartlevel:R");
            sb.AppendLine("changesound:M");

            File.WriteAllText("defaultconfig.txt", sb.ToString());
            File.Copy("defaultconfig.txt", "config.txt", true);
        }

        /// <summary>
        /// Get all the Buttons linked to the specified name
        /// </summary>
        /// <param name="name">The name for the buttons</param>
        /// <returns>The list with all the buttons</returns>
        internal static List<Button> GetButtons(InputAction ia)
        {
            return input[ia];
        }

        internal static string GetFirstMappedButton(InputAction ia)
        {
            if (input[ia].Count > 0)
            {
                return input[ia][0].ToString();
            }
            return "<EMPTY>";
        }

        /// <summary>
        /// Save the current key configuration to the config text file
        /// </summary>
        internal static void SaveConfig()
        {
            StringBuilder sb = new StringBuilder();
            if (File.Exists("config.txt"))
            {
                AppendCommentsFromConfigFile(sb);
            }

            foreach (InputAction ia in input.Keys)
            {
                sb.Append(ia + ":\t");
                bool added = false;
                foreach (Button b in input[ia])
                {
                    if (b.IsBound)
                    {
                        if (added)
                        {
                            sb.Append(",\t");
                        }
                        added = true;
                        sb.Append(b.ToString());
                    }
                }
                sb.AppendLine("");
            }

            File.WriteAllText("config.txt", sb.ToString());
        }

        private static void AppendCommentsFromConfigFile(StringBuilder sb)
        {
            string[] lines = File.ReadAllLines("config.txt");
            foreach (string line in lines)
            {
                if (line.StartsWith("//", StringComparison.OrdinalIgnoreCase))
                {
                    sb.AppendLine(line);
                }
                else
                {
                    break;
                }
            }
        }

        internal static HashSet<InputAction> GetCurrentInputActions()
        {
            HashSet<InputAction> result = new HashSet<InputAction>();

            foreach (InputAction ia in input.Keys)
            {
                foreach (Button b in input[ia])
                {
                    if (b.IsBound && b.IsDown())
                    {
                        result.Add(ia);
                        break;
                    }
                }
            }

            return result;
        }

        public static SpriteFont font { get; set; }

        public static SpriteFont bigFont { get; set; }

        internal static bool ContainsKey(Keys key)
        {
            return IM.GetPressedKeys().Contains(key);
        }

        private static float fov = MathHelper.ToRadians(60);
        public static float FOV { get { return fov; } set { fov = MathHelper.ToRadians((float)Math.Round(MathHelper.ToDegrees(Math.Min(Math.Max(value, 0.01f), MathHelper.Pi - 0.01f)))); } }
        public static string FOVAsText { get { return MathHelper.ToDegrees(FOV).ToString(); } }

        internal static void AddSound(string p, SoundEffect soundEffect)
        {
            sounds.Add(p, soundEffect);
        }

        private static Dictionary<string, SoundEffect> sounds = new Dictionary<string, SoundEffect>();

        public static void PlaySound(string name)
        {
            if (Volume > 0)
            {
                var se = sounds[name].CreateInstance();
                if (Volume == 1)
                {
                    se.Volume = 0.3f;
                }
                se.Play();
                playingSounds.Add(se);
            }            
        }

        public static List<SoundEffectInstance> playingSounds = new List<SoundEffectInstance>();
        public static bool MusicEnabled = true;

        public static void CleanupSounds()
        {
            var toDispose = playingSounds.Where(x => x.State != SoundState.Playing).ToArray();
            foreach (var s in toDispose)
            {
                s.Dispose();
                playingSounds.Remove(s);
            }
        }
        public static int Volume { get; set; }

        internal static void LoadLevels()
        {
            var files = Directory.GetFiles("levels").OrderBy(x => x);
            foreach (var f in files)
            {
                LevelNames.Add(f);
                Levels.Add(File.ReadAllLines(f));
            }
        }

        static Dictionary<string, SoundEffectInstance> eueue = new Dictionary<string, SoundEffectInstance>();

        internal static void PlaySoundEueue(string name)
        {
            if (Volume > 0)
            {
                var se = sounds[name].CreateInstance();
                if (Volume == 1 && name != "alarm")
                {                    
                    se.Volume = 0.3f;
                }

                if (eueue.ContainsKey(name))
                {
                    if (eueue[name].State != SoundState.Playing)
                    {
                        eueue[name].Dispose();
                        eueue[name] = se;
                        se.Play();
                    }
                }
                else
                {
                    eueue.Add(name, se);
                    se.Play();
                }
            }        
        }

        internal static void UpdateMusic()
        {
            if (RM.MusicEnabled)
            {
                if (musicplaying == null || musicplaying.State != SoundState.Playing)
                {
                    if (musicplaying != null)
                    {
                        musicplaying.Dispose();
                        musicplaying = null;
                    }
                    var next = "music1";
                    if (NextMusic.Any())
                    {
                        next = NextMusic.Dequeue();
                    }

                    var se = sounds[next].CreateInstance();
                    
                    musicplaying = se;
                    se.Play();
                    
                }
            }
        }
        private static SoundEffectInstance musicplaying;
        public static Queue<string> NextMusic = new Queue<string>();
    }
}
