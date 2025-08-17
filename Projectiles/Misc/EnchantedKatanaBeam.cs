using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MoreKatana.Assets.ExtraTextures;
using MoreKatana.Items.Weapons.Misc;
using MoreKatana.Projectiles.PrimTrails;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace MoreKatana.Projectiles.Misc
{
    public class EnchantedKatanaBeam : ModProjectile
    {
        private int AttackType
        {
            get => (int)Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }

        public int SelectDustType => Utils.SelectRandom(Main.rand, EnchantedKatana.EnchantedDustType);

        public int collisionCount = 2;

        private TextureMapPrimTrail trail;

        public override void SetDefaults()
        {
            Projectile.width = 18;
            Projectile.height = 18;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 120;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = false;
        }

        public override void AI()
        {
            if (Projectile.localAI[0] == 0)
            {
                Projectile.localAI[0] = 1;
                trail = new TextureMapPrimTrail(Projectile, Color.DeepSkyBlue, MoreKatanaTextures.StraightlineTrailTexture.Value, 8, 20);
                MoreKatana.primitives.CreateTrail(trail);
            }

            Projectile.tileCollide = AttackType != 1;
            Projectile.spriteDirection = (Vector2.Dot(Projectile.velocity, Vector2.UnitX) >= 0f).ToDirectionInt();
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2 - MathHelper.PiOver4 * Projectile.spriteDirection;

            int fourConst = 4;
            for (int i = 0; i < 2; i++)
            {
                float shortXVel = Projectile.velocity.X / 3f * i;
                float shortYVel = Projectile.velocity.Y / 3f * i;
                int newDust = Dust.NewDust(new Vector2(Projectile.position.X + fourConst, Projectile.position.Y + fourConst), Projectile.width - (fourConst * 2), Projectile.height - (fourConst * 2), SelectDustType, 0f, 0f, 100, default, 1.2f);
                Main.dust[newDust].noGravity = true;
                Main.dust[newDust].velocity *= 0.1f;
                Main.dust[newDust].velocity += Projectile.velocity * 0.1f;
                Main.dust[newDust].position.X -= shortXVel;
                Main.dust[newDust].position.Y -= shortYVel;
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (collisionCount <= 0)
                return true;

            Projectile.velocity = -oldVelocity.RotatedByRandom(MathHelper.ToRadians(30f));
            collisionCount--;
            return false;
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 8; i++)
            {
                int newDust = Dust.NewDust(Projectile.position + Projectile.velocity, Projectile.width, Projectile.height, SelectDustType, Projectile.oldVelocity.X * 0.5f, Projectile.oldVelocity.Y * 0.5f);
                if (!Main.rand.NextBool(3))
                {
                    Main.dust[newDust].fadeIn = 1f;
                    Main.dust[newDust].noGravity = true;
                    Main.dust[newDust].velocity *= 3f;
                    Main.dust[newDust].scale *= 1.5f;
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Rectangle rectangle = new Rectangle(0, 0, texture.Width, texture.Height);
            Vector2 origin = rectangle.Size() / 2f;
            Vector2 position = Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);
            Color color = Projectile.GetAlpha(lightColor);
            SpriteEffects spriteEffects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            MoreKatanaUtil.DrawBackglow(texture, position, rectangle, color with { A = 0 } * 0.2f, Projectile.rotation, 2f * ((float)Math.Sin(Main.GameUpdateCount / 10f) + 0.3f), new Vector2(Projectile.scale), spriteEffects);
            Main.EntitySpriteDraw(texture, position, rectangle, color, Projectile.rotation, origin, Projectile.scale, spriteEffects, 0);
            return false;
        }
    }
}