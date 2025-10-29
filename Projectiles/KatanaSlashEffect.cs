using Microsoft.Xna.Framework;
using MoreKatana.Particles;
using Terraria;
using Terraria.ModLoader;

namespace MoreKatana.Projectiles
{
    public class KatanaSlashEffect : ModProjectile
    {
        public Color slashColor = Main.rand.NextBool() ? Color.Silver : Color.DimGray;

        public override string Texture => MoreKatana.EmptyTexture;

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
            Projectile.scale = 0.75f;
            Projectile.hide = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
            Projectile.noEnchantmentVisuals = true;
        }

        public override void AI()
        {
            Vector2 vel = new Vector2(0.1f, 0.1f).RotatedByRandom(100);

            if (!Projectile.MKProj().Bool[0])
            {
                Projectile.MKProj().Bool[0] = true;

                Particle glowSpark = new GlowSparkParticle(Projectile.Center, vel, false, 15, Main.rand.NextFloat(0.05f, 0.09f), slashColor, new Vector2(2f, 0.5f), true);
                ParticleHandler.SpawnParticle(glowSpark);
            }

            Projectile.rotation = vel.ToRotation();
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
    }
}