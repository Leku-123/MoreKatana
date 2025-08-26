using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MoreKatana.Projectiles.Base;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static MoreKatana.MoreKatanaUtil;

namespace MoreKatana.Projectiles.Metal
{
    public class ObsidianSwing2 : CustomSword
    {
        public override string Texture => this.GetTexture(Name);

        public override void Initialization(Item item, int type)
        {
            Projectile.localNPCHitCooldown = Owner.itemAnimationMax * Projectile.MaxUpdates;
            SwordSize(58);
            TrailColor = new Color(83, 5, 1);
        }

        public override bool SwingPattern(Item item, int type)
        {
            float x = Main.rand.NextFloat(1f, 1.3f);
            float y = Main.rand.NextFloat(0.7f, 0.9f);
            SwingEllipse = new(x, y);

            float swingRange = Main.rand.NextFloat(0.7f, 0.8f);
            SwingStats(Owner.itemAnimationMax, swingRange, (0.9f - swingRange) / 2f, type % 2 != 0);

            return base.SwingPattern(item, type);
        }

        public CurveSegment execute = new CurveSegment(SineOutEasing, 0f, 0f, 0.95f);
        public CurveSegment unwind = new CurveSegment(LinearEasing, 0.5f, 0.95f, 0.05f);
        public override float GetProgress(int type) => PiecewiseAnimation(Progress, execute, unwind);

        public override void AdditionalAI(Item item, int type, bool onDelay)
        {
            Owner.SetDummyItemTime(2);

            if (type == 1 && Projectile.localAI[0] == 0)
            {
                Projectile.localAI[0] = 1;
                Owner.ScreenShake(2, 4);
                SoundEngine.PlaySound(SoundID.DD2_BetsysWrathShot, Owner.Center);

                if (Projectile.owner == Main.myPlayer)
                {
                    Vector2 vel = Vector2.Normalize(Projectile.velocity) * 10f;
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Owner.Center + vel * 2f, vel, ModContent.ProjectileType<ObsidianSlash>(), Projectile.damage / 2, Projectile.knockBack, Projectile.owner);
                }
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            base.OnHitNPC(target, hit, damageDone);
            target.AddBuff(BuffID.OnFire, 120);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Texture2D glowTex = ModContent.Request<Texture2D>(GlowTexture).Value;
            Vector2 position = Projectile.Center - Main.screenPosition;
            DrawBasicSword(texture, position, Projectile.GetAlpha(lightColor));
            DrawBasicSword(glowTex, position, Color.White * Projectile.Opacity);
            return false;
        }
    }
}