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

        public override void Initialize(int type)
        {
            Projectile.localNPCHitCooldown = -1;

            float x = Main.rand.NextFloat(1f, 1.3f);
            float y = Main.rand.NextFloat(0.7f, 0.9f);
            SwingEllipse = new(x, y);

            GetTextureValues();
            TrailColor = new Color(83, 5, 1);
        }

        public SwingData Down => new SwingData(OwnerItem.useAnimation, Main.rand.NextFloat(0.7f, 0.8f));
        public SwingData Up => new SwingData(OwnerItem.useAnimation, Main.rand.NextFloat(0.7f, 0.8f), backspin: true);
        public override SwingData GetSwingData(int type) => SwingData.SwingRegister(type, Down, Up);

        public CurveSegment execute = new CurveSegment(SineOutEasing, 0f, 0f, 0.95f);
        public CurveSegment unwind = new CurveSegment(LinearEasing, 0.5f, 0.95f, 0.05f);
        public override float GetProgress(int type) => PiecewiseAnimation(Progress, execute, unwind);

        public override void AdditionalAI(int type, bool onDelay)
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