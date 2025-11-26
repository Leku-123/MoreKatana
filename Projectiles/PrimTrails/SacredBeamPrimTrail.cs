using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MoreKatana.Assets.ExtraTextures;
using MoreKatana.Prim;
using System;
using Terraria;

namespace MoreKatana.Projectiles.PrimTrails
{
    public class SacredBeamPrimTrail : PrimTrail
    {
        public float Scale = 1;

        public SacredBeamPrimTrail(Projectile projectile, Color color, int width = 200)
        {
            Entity = projectile;
            EntityType = projectile.type;
            DrawType = PrimTrailManager.DrawProjectile;
            Color = color;
            Width = width;
        }

        public override void SetDefaults()
        {
            Cap = 100;
            AlphaValue = 0.9f;
        }

        public override void PrimStructure(SpriteBatch spriteBatch)
        {
            if (PointCount <= 10)
                return;

            DrawBasicTrail(Color, Width * Scale);
        }

        public override void SetShaders()
        {
            Effect effect = MoreKatana.PrimitiveTextureMap;
            effect.Parameters["uTexture"].SetValue(MoreKatanaTextures.BeamTrailTexture.Value);
            effect.Parameters["additive"].SetValue(true);
            effect.Parameters["repeats"].SetValue(1);
            effect.Parameters["intensify"].SetValue(true);
            effect.Parameters["scroll"].SetValue(Counter * -0.05f);
            PrepareShader(effect, "MainPS", Counter);
        }

        public override void OnUpdate()
        {
            if (Entity is not Projectile projectile)
                return;

            Counter++;
            PointCount = Points.Count * 10;

            if (!projectile.active || Destroyed)
                OnDestroy();
        }

        public override void OnDestroy()
        {
            Destroyed = true;
            Width *= 0.85f;
            Width += (float)Math.Sin(Counter * 2) * 0.3f;
            AlphaValue *= 0.85f;

            if (Width < 0.05f)
                Dispose();
        }
    }
}