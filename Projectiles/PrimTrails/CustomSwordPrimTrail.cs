using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MoreKatana.Assets.ExtraTextures;
using MoreKatana.Prim;
using System;
using System.Linq;
using Terraria;

namespace MoreKatana.Projectiles.PrimTrails
{
    public class CustomSwordPrimTrail : PrimTrail
    {
        public CustomSwordPrimTrail(Projectile projectile, Color color, int width = 50, int cap = 50)
        {
            Entity = projectile;
            EntityType = projectile.type;
            DrawType = PrimTrailManager.DrawProjectile;
            Color = color;
            Width = width;
            Cap = cap;
        }

        public override void SetDefaults() => AlphaValue = 0.9f;

        public int TextureType;
        public int Direction = 1;
        public int ModifiedWidth = 0;
        public Vector2 PrimCenter;

        public override void PrimStructure(SpriteBatch spriteBatch)
        {
            if (PointCount <= 6)
                return;

            //float widthVar;
            for (int i = 0; i < Points.Count; i++)
            {
                if (i == 0)
                {
                    /*
                    widthVar = (float)Math.Sqrt(Points.Count) * Width;
                    Vector2 normalAhead = CurveNormal(Points, i + 1);
                    Vector2 secondUp = Points[i + 1] - normalAhead * widthVar;
                    Vector2 secondDown = Points[i + 1] + normalAhead * widthVar;
                    */
                }
                else
                {
                    if (i != Points.Count - 1)
                    {
                        float dist = Math.Abs(Points.Count - i - Counter * 3);
                        //widthVar = (float)Math.Sin(i * (Math.PI / Points.Count)) * Width * i / 300f;
                        //float widthVar2 = (float)Math.Sin((i + 1) * (Math.PI / Points.Count)) * Width * (i + 1) / 300f;
                        Color c = Color * ((100 - dist) * 0.01f) * (Counter / 10f);
                        Color CBT = Color * ((100 - dist) * 0.01f) * (Counter / 10f);
                        Vector2 normal = CurveNormal(Points, i);
                        Vector2 normalAhead = CurveNormal(Points, i + 1);
                        Vector2 firstUp = Points[i] - normal * Width;
                        Vector2 firstDown = Points[i] + normal * Width;
                        Vector2 secondUp = Points[i + 1] - normalAhead * Width;
                        Vector2 secondDown = Points[i + 1] + normalAhead * Width;
                        if (Direction == 1)
                        {
                            AddVertex(firstDown + PrimCenter, c * AlphaValue, new Vector2(i / (float)Cap, 1));
                            AddVertex(firstUp + PrimCenter, c * AlphaValue, new Vector2(i / (float)Cap, 0));
                            AddVertex(secondDown + PrimCenter, CBT * AlphaValue, new Vector2((i + 1) / (float)Cap, 1));

                            AddVertex(secondUp + PrimCenter, CBT * AlphaValue, new Vector2((i + 1) / (float)Cap, 0));
                            AddVertex(secondDown + PrimCenter, CBT * AlphaValue, new Vector2((i + 1) / (float)Cap, 1));
                            AddVertex(firstUp + PrimCenter, c * AlphaValue, new Vector2(i / (float)Cap, 0));
                        }
                        else
                        {
                            AddVertex(firstDown + PrimCenter, c * AlphaValue, new Vector2(i / (float)Cap, 0));
                            AddVertex(firstUp + PrimCenter, c * AlphaValue, new Vector2(i / (float)Cap, 1));
                            AddVertex(secondDown + PrimCenter, CBT * AlphaValue, new Vector2((i + 1) / (float)Cap, 0));

                            AddVertex(secondUp + PrimCenter, CBT * AlphaValue, new Vector2((i + 1) / (float)Cap, 1));
                            AddVertex(secondDown + PrimCenter, CBT * AlphaValue, new Vector2((i + 1) / (float)Cap, 0));
                            AddVertex(firstUp + PrimCenter, c * AlphaValue, new Vector2(i / (float)Cap, 1));
                        }
                    }
                }
            }
        }

        public override void SetShaders()
        {
            Effect effect = MoreKatana.PrimitiveTextureMap;
            effect.Parameters["uTexture"].SetValue(MoreKatanaTextures.SwordTrailTexture[TextureType].Value);
            effect.Parameters["additive"].SetValue(true);
            effect.Parameters["repeats"].SetValue(1);
            effect.Parameters["intensify"].SetValue(true);
            effect.Parameters["scroll"].SetValue(Counter);
            PrepareShader(effect, "MainPS", Counter);
        }

        public override void OnUpdate()
        {
            Counter = 10;
            PointCount = Points.Count() * 6;

            if (ModifiedWidth != 0)
                Width = ModifiedWidth;

            while (Points.Count() > Cap)
                Points.RemoveAt(0);

            if (Destroyed || !Entity.active)
                OnDestroy();
        }

        public override void OnDestroy()
        {
            Destroyed = true;
            if (Points.Count() < 10)
            {
                Dispose();
            }
            else
            {
                Points.RemoveAt(0);
                Points.RemoveAt(0);
                Points.RemoveAt(0);
                Points.RemoveAt(0);
                Points.RemoveAt(0);
                Points.RemoveAt(0);
            }
        }
    }
}