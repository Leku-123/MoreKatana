using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace MoreKatana.Projectiles.Misc
{
    public class SkyFeather : ModProjectile
    {
        private int AttackType
        {
            get => (int)Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }

        public int TargetIndex = -1;

        public override void SetStaticDefaults()
        {
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
            Projectile.coldDamage = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            Projectile.direction = Projectile.spriteDirection = (Projectile.velocity.X > 0f) ? 1 : -1;

            if (AttackType == 0)
            {
                if (TargetIndex >= 0)
                {
                    if (!Main.npc[TargetIndex].active || !Main.npc[TargetIndex].CanBeChasedBy())
                    {
                        TargetIndex = -1;
                    }
                    else
                    {
                        Vector2 idealVelocity = Projectile.SafeDirectionTo(Main.npc[TargetIndex].Center) * (Projectile.velocity.Length() + 6.5f);
                        Projectile.velocity = Vector2.Lerp(Projectile.velocity, idealVelocity, 0.04f);
                    }
                }
                else if (TargetIndex == -1)
                {
                    NPC potentialTarget = Projectile.Center.ClosestNPCAt(200f, false);
                    if (potentialTarget != null)
                        TargetIndex = potentialTarget.whoAmI;
                }

                int fourConst = 4;
                for (int i = 0; i < 2; i++)
                {
                    float shortXVel = Projectile.velocity.X / 3f * i;
                    float shortYVel = Projectile.velocity.Y / 3f * i;
                    int newDust = Dust.NewDust(new Vector2(Projectile.position.X + fourConst, Projectile.position.Y + fourConst), Projectile.width - (fourConst * 2), Projectile.height - (fourConst * 2), DustID.Cloud, 0f, 0f, 100, default, 1.2f);
                    Main.dust[newDust].noGravity = true;
                    Main.dust[newDust].velocity *= 0.1f;
                    Main.dust[newDust].velocity += Projectile.velocity * 0.1f;
                    Main.dust[newDust].position.X -= shortXVel;
                    Main.dust[newDust].position.Y -= shortYVel;
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
            Color glowColor = Color.White * Projectile.Opacity;
            SpriteEffects spriteEffects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            // トレイル
            for (int i = 0; i < ProjectileID.Sets.TrailCacheLength[Projectile.type]; i++)
            {
                float fade = (float)(ProjectileID.Sets.TrailCacheLength[Projectile.type] - i) / ProjectileID.Sets.TrailCacheLength[Projectile.type];
                glowColor *= fade;
                float scale = Projectile.scale;
                scale *= fade;
                Vector2 pos = Projectile.oldPos[i];
                Main.EntitySpriteDraw(texture, pos + Projectile.Size / 2f - Main.screenPosition + new Vector2(0, Projectile.gfxOffY), rectangle, glowColor, Projectile.rotation, origin, scale, spriteEffects, 0);
            }

            // 本体の描画
            Main.EntitySpriteDraw(texture, position, rectangle, Projectile.GetAlpha(lightColor), Projectile.rotation, origin, Projectile.scale, spriteEffects, 0);
            return false;
        }
    }
}