using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace MoreKatana.Projectiles.Misc
{
    /// <summary>
    /// パーティクル作るまでの繋ぎ
    /// てきとう
    /// </summary>
    public class KatanaSlashEffect : ModProjectile
    {

        public Vector2 Squash = new Vector2(2f, 0.5f);

        public override void SetDefaults()
        {
            Projectile.width = 200;
            Projectile.height = 24;
            Projectile.aiStyle = -1;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 15;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
            Projectile.noEnchantmentVisuals = true;
            Projectile.scale = Main.rand.NextFloat(0.05f, 0.09f);
            Projectile.stopsDealingDamageAfterPenetrateHits = true;
        }

        public override void AI()
        {
            if (Projectile.localAI[0] == 0)
            {
                Projectile.localAI[0] = 1;

                Vector2 vel = new Vector2(0.1f, 0.1f).RotatedByRandom(100);

                Projectile.rotation = vel.ToRotation();
            }


            Projectile.scale *= 0.95f;
            Squash.X *= 0.8f;
            Squash.Y *= 1.12f;
        }

        public override bool ShouldUpdatePosition() => true;

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (projHitbox.Intersects(targetHitbox))
                return true;

            float length = 200;
            float dummy = 0f;
            Vector2 offset = length / 2 * Projectile.scale * Projectile.rotation.ToRotationVector2();
            Vector2 end = Projectile.Center - offset;
            Vector2 tip = Projectile.Center + offset;

            if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), end, tip, 10f * Projectile.scale, ref dummy))
                return true;

            return false;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 scale = Squash * Projectile.scale;

            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, Color.White with { A = 0 }, Projectile.rotation, texture.Size() * 0.5f, scale * new Vector2(0.45f, 1f), 0, 0f);
            return false;
        }
    }
}