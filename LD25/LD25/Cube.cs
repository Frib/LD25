using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;


namespace LD25
{
    public class Cube
    {
        private VertexPositionNormalTexture[] pointList;

        internal Vector3 position;
        internal Matrix scale;
        internal Vector3 ScaleVector;

        internal List<Texture2D> textures;

        internal BoundingBox box;

        internal Vector2 textureScale = Vector2.One;
        
        Matrix world;

        public Cube(Vector3 pos,Texture2D wall, Texture2D floor = null, Texture2D ceiling = null)
        {
            textures = new List<Texture2D>();

            textures.Add(floor ?? wall);
            textures.Add(wall);
            textures.Add(ceiling ?? floor ?? wall);

            ScaleVector = new Vector3(8, 16, 8);
            SetVertices();
            SetPosition(pos);
        }

        public void SetPosition(Vector3 pos)
        {
            position = pos + new Vector3(0, 0, 0);
            this.scale = Matrix.CreateScale(ScaleVector);

            var scaleBL = ScaleVector / 2;
            scaleBL.Y = 0;

            var scaleTR = ScaleVector / 2;
            scaleTR.Y = ScaleVector.Y;

            box = new BoundingBox(position - scaleBL , position + scaleTR);

            world = scale * GetMatrixChain();
        }

        public void SetVertices()
        {
            pointList = new VertexPositionNormalTexture[36];

            Vector3 a = new Vector3(-0.5f, 1, -0.5f);
            Vector3 b = new Vector3(0.5f, 1, -0.5f);
            Vector3 c = new Vector3(0.5f, 0, -0.5f);
            Vector3 d = new Vector3(-0.5f, 0, -0.5f);
            Vector3 e = new Vector3(-0.5f, 1, 0.5f);
            Vector3 f = new Vector3(0.5f, 1, 0.5f);
            Vector3 g = new Vector3(0.5f, 0, 0.5f);
            Vector3 h = new Vector3(-0.5f, 0, 0.5f);

            Vector2 linksonder = new Vector2(0, textureScale.Y);
            Vector2 rechtsonder = new Vector2(textureScale.X, textureScale.Y);
            Vector2 linksboven = new Vector2(0, 0);
            Vector2 rechtsboven = new Vector2(textureScale.X, 0);

            Vector3 frontNormal = new Vector3(0.0f, 0.0f, 1.0f);
            Vector3 backNormal = new Vector3(0.0f, 0.0f, -1.0f);
            Vector3 topNormal = new Vector3(0.0f, 1.0f, 0.0f);
            Vector3 bottomNormal = new Vector3(0.0f, -1.0f, 0.0f);
            Vector3 leftNormal = new Vector3(-1.0f, 0.0f, 0.0f);
            Vector3 rightNormal = new Vector3(1.0f, 0.0f, 0.0f);

            Vector2 tempScale = Vector2.One;
            //Vector2 tempScale = new Vector2(scaleVector.X / rechtsonder.X, scaleVector.Y / rechtsonder.Y);

            //Achterkant
            pointList[0] = new VertexPositionNormalTexture(d, backNormal, new Vector2(tempScale.X, tempScale.Y));
            pointList[1] = new VertexPositionNormalTexture(b, backNormal, new Vector2(0, 0));
            pointList[2] = new VertexPositionNormalTexture(a, backNormal, new Vector2(tempScale.X, 0));
            pointList[3] = new VertexPositionNormalTexture(d, backNormal, new Vector2(tempScale.X, tempScale.Y));
            pointList[4] = new VertexPositionNormalTexture(c, backNormal, new Vector2(0, tempScale.Y));
            pointList[5] = new VertexPositionNormalTexture(b, backNormal, new Vector2(0, 0));

            //Voorkant
            pointList[12] = new VertexPositionNormalTexture(h, frontNormal, new Vector2(0, tempScale.Y));
            pointList[13] = new VertexPositionNormalTexture(e, frontNormal, new Vector2(0, 0));
            pointList[14] = new VertexPositionNormalTexture(f, frontNormal, new Vector2(tempScale.X, 0));
            pointList[15] = new VertexPositionNormalTexture(h, frontNormal, new Vector2(0, tempScale.Y));
            pointList[16] = new VertexPositionNormalTexture(f, frontNormal, new Vector2(tempScale.X, 0));
            pointList[17] = new VertexPositionNormalTexture(g, frontNormal, new Vector2(tempScale.X, tempScale.Y));

            //tempScale = new Vector2(scaleVector.Z / rechtsonder.X, scaleVector.Y / rechtsonder.Y);

            //Links
            pointList[6] = new VertexPositionNormalTexture(d, leftNormal, new Vector2(0, tempScale.Y));
            pointList[7] = new VertexPositionNormalTexture(a, leftNormal, new Vector2(0, 0));
            pointList[8] = new VertexPositionNormalTexture(e, leftNormal, new Vector2(tempScale.X, 0));
            pointList[9] = new VertexPositionNormalTexture(d, leftNormal, new Vector2(0, tempScale.Y));
            pointList[10] = new VertexPositionNormalTexture(e, leftNormal, new Vector2(tempScale.X, 0));
            pointList[11] = new VertexPositionNormalTexture(h, leftNormal, new Vector2(tempScale.X, tempScale.Y));

            //Rechts
            pointList[18] = new VertexPositionNormalTexture(g, rightNormal, new Vector2(0, tempScale.Y));
            pointList[19] = new VertexPositionNormalTexture(f, rightNormal, new Vector2(0, 0));
            pointList[20] = new VertexPositionNormalTexture(b, rightNormal, new Vector2(tempScale.X, 0));
            pointList[21] = new VertexPositionNormalTexture(g, rightNormal, new Vector2(0, tempScale.Y));
            pointList[22] = new VertexPositionNormalTexture(b, rightNormal, new Vector2(tempScale.X, 0));
            pointList[23] = new VertexPositionNormalTexture(c, rightNormal, new Vector2(tempScale.X, tempScale.Y));

            //tempScale = new Vector2(scaleVector.X / rechtsonder.X, scaleVector.Z / rechtsonder.Y);

            //Bovenkant
            pointList[24] = new VertexPositionNormalTexture(e, topNormal, new Vector2(0, tempScale.Y));
            pointList[25] = new VertexPositionNormalTexture(a, topNormal, new Vector2(0, 0));
            pointList[26] = new VertexPositionNormalTexture(b, topNormal, new Vector2(tempScale.X, 0));
            pointList[27] = new VertexPositionNormalTexture(e, topNormal, new Vector2(0, tempScale.Y));
            pointList[28] = new VertexPositionNormalTexture(b, topNormal, new Vector2(tempScale.X, 0));
            pointList[29] = new VertexPositionNormalTexture(f, topNormal, new Vector2(tempScale.X, tempScale.Y));

            //Onderkant
            pointList[30] = new VertexPositionNormalTexture(h, bottomNormal, new Vector2(0, 0));
            pointList[31] = new VertexPositionNormalTexture(c, bottomNormal, new Vector2(tempScale.X, tempScale.Y));
            pointList[32] = new VertexPositionNormalTexture(d, bottomNormal, new Vector2(0, tempScale.Y));
            pointList[33] = new VertexPositionNormalTexture(h, bottomNormal, new Vector2(0, 0));
            pointList[34] = new VertexPositionNormalTexture(g, bottomNormal, new Vector2(tempScale.X, 0));
            pointList[35] = new VertexPositionNormalTexture(c, bottomNormal, new Vector2(tempScale.X, tempScale.Y));
        }


        private Matrix GetMatrixChain()
        {
            return Matrix.CreateTranslation(position);
        }

        public void Draw(bool highlight = false)
        {
            G.g.e.World = world;
            if (highlight)
            {
                G.g.e.Texture = RM.GetTexture("highlight");
            }
            else
            {
                G.g.e.Texture = textures[0];
            }
            G.g.e.CurrentTechnique.Passes[0].Apply();
            G.g.GraphicsDevice.DrawUserPrimitives<VertexPositionNormalTexture>(PrimitiveType.TriangleList, pointList, 0, 12);
        }

    }
}
