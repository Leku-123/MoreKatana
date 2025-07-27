using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace MoreKatana.Projectiles.TerraKatanaTree
{
    public class GrassLeaf : ModProjectile
    {
        private ref float Timer => ref Projectile.ai[0];
        public const int PrepareTime = 30;
        public float PrepareCompletion => MathHelper.Clamp(Timer / PrepareTime, 0f, 1f);

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 5;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 120;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 255;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void AI()
        {
            if (PrepareCompletion < 1)
                Projectile.velocity *= 0.92f; // 減速

            if (PrepareCompletion == 1)
            {
                if (Projectile.localAI[0] == 0)
                {
                    Projectile.localAI[0] = 1;

                    SoundEngine.PlaySound(SoundID.Grass, Projectile.Center);
                    MoreKatanaUtil.DrawRing(Projectile.Center, DustID.GrassBlades, 24, 6f);

                    // マウスの方向に加速させる
                    if (Projectile.owner == Main.myPlayer)
                    {
                        Vector2 direct = Projectile.DirectionTo(Main.MouseWorld);
                        float speed = 25f;
                        Projectile.velocity += direct * speed;
                        Projectile.netUpdate = true;
                    }
                }
            }

            // 発射体の向きと回転
            Projectile.direction = Projectile.spriteDirection = (Projectile.velocity.X > 0f) ? 1 : -1;
            Projectile.rotation = Projectile.velocity.ToRotation() + (Projectile.spriteDirection == -1 ? MathHelper.Pi : 0);

            // フェードイン
            if (Projectile.alpha > 0)
            {
                Projectile.alpha -= 25;
                if (Projectile.alpha < 0)
                    Projectile.alpha = 0;
            }

            // アニメーションフレーム
            if (++Projectile.frameCounter >= 4)
            {
                Projectile.frameCounter = 0;
                Projectile.frame = ++Projectile.frame % Main.projFrames[Projectile.type];
            }

            Timer++;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Poisoned, 60 * 7);

            for (int i = 0; i < 5; i++)
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.GrassBlades, Projectile.oldVelocity.X * 0.2f, Projectile.oldVelocity.Y * 0.2f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;

            Rectangle rectangle = texture.Frame(1, Main.projFrames[Projectile.type], 0, Projectile.frame);
            Vector2 origin = rectangle.Size() / 2f;
            Vector2 position = Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);
            SpriteEffects spriteEffects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            Color color = Projectile.GetAlpha(lightColor);

            for (int i = 0; i < ProjectileID.Sets.TrailCacheLength[Projectile.type]; i++)
            {
                float fade = (float)(ProjectileID.Sets.TrailCacheLength[Projectile.type] - i) / ProjectileID.Sets.TrailCacheLength[Projectile.type];
                Color color2 = color;
                color2 *= fade;
                float scale = Projectile.scale;
                scale *= fade;
                Vector2 pos = Projectile.oldPos[i];
                Main.EntitySpriteDraw(texture, pos + Projectile.Size / 2f - Main.screenPosition + new Vector2(0, Projectile.gfxOffY), rectangle, color2, Projectile.rotation, origin, scale, spriteEffects, 0);
            }

            float backglowAmount = 12f;
            for (int i = 0; i < backglowAmount; i++)
            {
                Vector2 backglowOffset = (MathHelper.TwoPi * i / backglowAmount).ToRotationVector2();
                backglowOffset *= PrepareCompletion;
                Color backglowColor = Color.White * (1f - (Projectile.alpha / 255f));
                backglowColor.A = 0;
                Main.EntitySpriteDraw(texture, position + backglowOffset, rectangle, backglowColor, Projectile.rotation, origin, Projectile.scale, spriteEffects, 0);
            }

            Main.EntitySpriteDraw(texture, position, rectangle, Projectile.GetAlpha(lightColor), Projectile.rotation, origin, Projectile.scale, spriteEffects, 0);
            return false;
        }
    }
}