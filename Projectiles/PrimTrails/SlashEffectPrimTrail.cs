using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MoreKatana.Assets.ExtraTextures;
using MoreKatana.Prim;
using System;
using Terraria;

namespace MoreKatana.Projectiles.PrimTrails
{
    public class SlashEffectPrimTrail : PrimTrail
    {
        public SlashEffectPrimTrail(Projectile projectile, Color color, int width = 20, int cap = 50)
        {
            Entity = projectile;
            EntityType = projectile.type;
            DrawType = PrimTrailManager.DrawProjectile;
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
                float widthVar = Width - Math.Abs(i - Points.Count / 2f) / Points.Count * Width * 2;
                if (i == 0)
                {
                    Vector2 normalAhead = CurveNormal(Points, i + 1);
                    Vector2 secondUp = Points[i + 1] - normalAhead * widthVar;
                    Vector2 secondDown = Points[i + 1] + normalAhead * widthVar;

                    AddVertex(Points[i], Color * AlphaValue, new Vector2((float)Math.Sin(Counter / 20f), (float)Math.Sin(Counter / 20f)));
                    AddVertex(secondUp, Color * AlphaValue, new Vector2((float)Math.Sin(Counter / 20f), (float)Math.Sin(Counter / 20f)));
                    AddVertex(secondDown, Color * AlphaValue, new Vector2((float)Math.Sin(Counter / 20f), (float)Math.Sin(Counter / 20f)));
                }
                else
                {
                    if (i != Points.Count - 1)
                    {
                        Vector2 normal = CurveNormal(Points, i);
                        Vector2 normalAhead = CurveNormal(Points, i + 1);
                        Vector2 firstUp = Points[i] - normal * widthVar;
                        Vector2 firstDown = Points[i] + normal * widthVar;
                        Vector2 secondUp = Points[i + 1] - normalAhead * widthVar;
                        Vector2 secondDown = Points[i + 1] + normalAhead * widthVar;

                        AddVertex(firstDown, Color * AlphaValue, new Vector2(i / Cap, 1));
                        AddVertex(firstUp, Color * AlphaValue, new Vector2(i / Cap, 0));
                        AddVertex(secondDown, Color * AlphaValue, new Vector2((i + 1) / Cap, 1));

                        AddVertex(secondUp, Color * AlphaValue, new Vector2((i + 1) / Cap, 0));
                        AddVertex(secondDown, Color * AlphaValue, new Vector2((i + 1) / Cap, 1));
                        AddVertex(firstUp, Color * AlphaValue, new Vector2(i / Cap, 0));
                    }
                }
            }
        }

        public override void SetShaders()
        {
            Effect effect = MoreKatana.PrimitiveTextureMap;
            effect.Parameters["uTexture"].SetValue(MoreKatanaTextures.StraightlineTrailTexture.Value);
            effect.Parameters["additive"].SetValue(true);
            effect.Parameters["repeats"].SetValue(1);
            effect.Parameters["intensify"].SetValue(true);
            effect.Parameters["scroll"].SetValue(Counter);
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

            if (!Entity.active && Entity != null || Destroyed)
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