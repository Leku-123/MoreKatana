using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MoreKatana.Prim;
using System;
using Terraria;

namespace MoreKatana.Projectiles.PrimTrails
{
    public class TextureMapPrimTrail : PrimTrail
    {
        private Texture2D Texture = null;

        public TextureMapPrimTrail(Projectile projectile, Color color, Texture2D texture, int width = 8, int cap = 15)
        {
            Entity = projectile;
            EntityType = projectile.type;
            DrawType = PrimTrailManager.DrawProjectile;
            Texture = texture;
            Color = color;
            Width = width;
            Cap = cap;
        }

        public override void SetDefaults() => AlphaValue = 1f;

        public override void PrimStructure(SpriteBatch spriteBatch)
        {
            if (PointCount <= 6)
                return;

            for (int i = 0; i < Points.Count; i++)
            {
                float widthVar;

                if (i == 0)
                {
                    widthVar = Width;

                    Vector2 normalAhead = CurveNormal(Points, i + 1);
                    Vector2 secondUp = Points[i + 1] - normalAhead * widthVar;
                    Vector2 secondDown = Points[i + 1] + normalAhead * widthVar;

                    AddVertex(Points[i], Color * AlphaValue, new Vector2(0, 0.5f));
                    AddVertex(secondUp, Color * AlphaValue * ((i + 1) / (float)Points.Count), new Vector2((i + 1) / (float)Points.Count, 0));
                    AddVertex(secondUp, Color * AlphaValue * ((i + 1) / (float)Points.Count), new Vector2((i + 1) / (float)Points.Count, 1));
                }
                else
                {
                    if (i == Points.Count - 1)
                        continue;

                    widthVar = Width;

                    Vector2 normal = CurveNormal(Points, i);
                    Vector2 normalAhead = CurveNormal(Points, i + 1);
                    Vector2 firstUp = Points[i] - normal * widthVar;
                    Vector2 firstDown = Points[i] + normal * widthVar;
                    Vector2 secondUp = Points[i + 1] - normalAhead * widthVar;
                    Vector2 secondDown = Points[i + 1] + normalAhead * widthVar;

                    AddVertex(firstDown, Color * AlphaValue * (i / (float)Points.Count), new Vector2(i / (float)Points.Count, 1));
                    AddVertex(firstUp, Color * AlphaValue * (i / (float)Points.Count), new Vector2(i / (float)Points.Count, 0));
                    AddVertex(secondDown, Color * AlphaValue * ((i + 1) / (float)Points.Count), new Vector2((i + 1) / (float)Points.Count, 1));

                    AddVertex(secondUp, Color * AlphaValue * ((i + 1) / (float)Points.Count), new Vector2((i + 1) / (float)Points.Count, 0));
                    AddVertex(secondDown, Color * AlphaValue * ((i + 1) / (float)Points.Count), new Vector2((i + 1) / (float)Points.Count, 1));
                    AddVertex(firstUp, Color * AlphaValue * (i / (float)Points.Count), new Vector2(i / (float)Points.Count, 0));
                }
            }
        }

        public override void SetShaders()
        {
            Effect effect = MoreKatana.PrimitiveTextureMap;
            effect.Parameters["uTexture"].SetValue(Texture);
            effect.Parameters["additive"].SetValue(true);
            effect.Parameters["repeats"].SetValue(1);
            effect.Parameters["intensify"].SetValue(true);
            effect.Parameters["scroll"].SetValue(Counter * 0.05f);
            PrepareShader(effect, "MainPS", Counter);
        }

        public override void OnUpdate()
        {
            if (Entity is not Projectile projectile)
                return;

            Counter++;
            PointCount = Points.Count * 6;

            if (Cap < PointCount / 6)
                Points.RemoveAt(0);

            if (!projectile.active || Destroyed)
                OnDestroy();
            else
                Points.Add(projectile.Center);
        }

        public override void OnDestroy()
        {
            Destroyed = true;
            Width *= 0.8f;
            Width += (float)Math.Sin(Counter * 2) * 0.3f;

            if (Width < 0.05f)
                Dispose();
        }
    }
}