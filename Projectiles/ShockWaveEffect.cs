using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;

namespace MoreKatana.Projectiles
{
    public class ShockWaveEffect : ModProjectile
    {
        public int RippleCount;

        public int RippleSize;

        public float RippleSpeed;

        bool boom = false;

        private readonly float distortStrength = 300f;

        public override string Texture => MoreKatana.EmptyTexture;

        public override void SetDefaults()
        {
            Projectile.width = 0;
            Projectile.height = 0;
            Projectile.timeLeft = 35;
            Projectile.tileCollide = false;
        }

        public override bool? CanDamage() => false;

        public override bool PreAI()
        {
            if (!boom)
            {
                if (Main.netMode != NetmodeID.Server && !Filters.Scene["Shockwave"].IsActive())
                {
                    Filters.Scene.Activate("Shockwave", Projectile.Center).GetShader().UseColor(RippleCount, RippleSize, RippleSpeed).UseTargetPosition(Projectile.Center);
                }
                boom = true;
            }
            if (Main.netMode != NetmodeID.Server && Filters.Scene["Shockwave"].IsActive())
            {
                float progress = (35f - Projectile.timeLeft) / 30f;
                Filters.Scene["Shockwave"].GetShader().UseProgress(progress).UseOpacity(distortStrength * (1 - progress / 5f));
            }
            return false;
        }

        public override void OnKill(int timeLeft)
        {
            if (Main.netMode != NetmodeID.Server && Filters.Scene["Shockwave"].IsActive())
            {
                Filters.Scene["Shockwave"].Deactivate();
            }
        }
    }
}