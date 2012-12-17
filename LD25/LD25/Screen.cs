using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LD25
{
    public abstract class Screen
    {
        protected Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch;
        protected Microsoft.Xna.Framework.Graphics.BasicEffect e;
        protected Microsoft.Xna.Framework.Graphics.GraphicsDevice GraphicsDevice;
        protected Microsoft.Xna.Framework.Graphics.SpriteFont font;

        public Screen()
        {
            this.g = G.g;
            this.spriteBatch = g.spriteBatch;
            this.e = g.e;
            this.GraphicsDevice = g.GraphicsDevice;
            this.font = g.font;
        }

        public abstract void Update();
        public abstract void Draw();

        public virtual void Show(){}
        public virtual void Hide(){}

        public G g { get; set; }
        
        protected static int CycleIndex(int newValue, int max, int min = 0)
        {
            return newValue < 0 ? max : newValue > max ? min : newValue;
        }
    }
}
