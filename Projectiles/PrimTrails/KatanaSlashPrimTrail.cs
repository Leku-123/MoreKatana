using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MoreKatana.Assets.ExtraTextures;
using MoreKatana.Prim;
using System;
using Terraria;

namespace MoreKatana.Projectiles.PrimTrails
{
    public class KatanaSlashPrimTrail : PrimTrail
    {
        public KatanaSlashPrimTrail(Projectile projectile, Color color, int width = 8, int cap = 80)
        {
            Entity = projectile;
            EntityType = projectile.type;
            DrawType = PrimTrailManager.DrawProjectile;
            Color = color;
            Width = width;
            Cap = cap;
        }

        public override void SetDefaults() => AlphaValue = 0.9f;

        public override void PrimStructure(SpriteBatch spriteBatch)
        {
            if (PointCount <= 6)
                return;

            float widthVar;
            for (int i = 0; i < Points.Count; i++)
            {
                if (i == 0)
                {
                    widthVar = (float)Math.Sqrt(Points.Count) * Width;
                    Color c1 = Color;
                    Vector2 normalAhead = CurveNormal(Points, i + 1);
                    Vector2 secondUp = Points[i + 1] - normalAhead * widthVar;

                    AddVertex(Points[i], c1 * AlphaValue, new Vector2(0, 0.5f));
                    AddVertex(secondUp, c1 * AlphaValue, new Vector2((float)(i + 1) / Cap, 0));
                    AddVertex(secondUp, c1 * AlphaValue, new Vector2((float)(i + 1) / Cap, 1));
                }
                else
                {
                    if (i == Points.Count - 1)
                        continue;

                    widthVar = (float)Math.Sqrt(i) * Width;
                    Color c = Color;
                    Color CBT = Color;
                    Vector2 normal = CurveNormal(Points, i);
                    Vector2 normalAhead = CurveNormal(Points, i + 1);
                    float j = (Cap + ((float)Math.Sin(Counter / 10f) * 1) - (i * 0.1f)) / Cap;
                    widthVar *= j;
                    Vector2 firstUp = Points[i] - normal * widthVar;
                    Vector2 firstDown = Points[i] + normal * widthVar;
                    Vector2 secondUp = Points[i + 1] - normalAhead * widthVar;
                    Vector2 secondDown = Points[i + 1] + normalAhead * widthVar;

                    AddVertex(firstDown, c * AlphaValue, new Vector2(i / Cap, 1));
                    AddVertex(firstUp, c * AlphaValue, new Vector2(i / Cap, 0));
                    AddVertex(secondDown, CBT * AlphaValue, new Vector2((i + 1) / Cap, 1));

                    AddVertex(secondUp, CBT * AlphaValue, new Vector2((i + 1) / Cap, 0));
                    AddVertex(secondDown, CBT * AlphaValue, new Vector2((i + 1) / Cap, 1));
                    AddVertex(firstUp, c * AlphaValue, new Vector2(i / Cap, 0));
                }
            }
        }

        public override void SetShaders()
        {
            Effect effect = MoreKatana.PrimitiveTextureMap;
            effect.Parameters["uTexture"].SetValue(MoreKatanaTextures.StraightlineTrailTexture.Value);
            effect.Parameters["additive"].SetValue(true);
            effect.Parameters["intensify"].SetValue(true);
            PrepareShader(effect, "MainPS", Counter);
        }

        public override void OnUpdate()
        {
            if (Entity is not Projectile)
                return;

            Counter++;
            PointCount = Points.Count * 6;

            if (Cap < PointCount / 6)
                Points.RemoveAt(0);

            if ((!Entity.active && Entity != null) || Destroyed)
                OnDestroy();
            else
                Points.Add(Entity.Center);
        }

        public override void OnDestroy()
        {
            Destroyed = true;
            Width *= 0.83f;

            if (Width < 0.05f)
                Dispose();
        }
    }
}