using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MoreKatana.Projectiles.Base;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace MoreKatana.Projectiles.Metal
{
    public class ObsidianSwing2 : CustomSword
    {
        public override string Texture => this.GetTexture();

        public override void Initialize(int type)
        {
            Projectile.localNPCHitCooldown = -1;

            float x = Main.rand.NextFloat(1f, 1.3f);
            float y = Main.rand.NextFloat(0.7f, 0.9f);
            SwingEllipse = new(x, y);

            GetTextureValues();

            Owner.ScreenShake(4, 8);
        }

        public override SwingData GetSwingData(int type) => new SwingData(OwnerItem.useAnimation, Main.rand.NextFloat(0.7f, 0.8f), backspin: type == 1);

        public override float GetProgress(int type) => GeneralSwingAnimation(Progress);

        public override void AdditionalAI(int type, bool onDelay)
        {
            Owner.SetDummyItemTime(2);

            if (type == 1 && !Projectile.MKProj().Bool[0])
            {
                Projectile.MKProj().Bool[0] = true;
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
            MoreKatanaUtil.DrawBackglow(texture, Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY), null, Projectile.GetAlpha(lightColor) with { A = 0 }, Projectile.rotation, 4f * (1 - Progress), new Vector2(Projectile.scale), SwingEffectsHV());
            DrawBasicSword(TextureAssets.Projectile[Projectile.type].Value, Projectile.Center, Projectile.GetAlpha(lightColor));
            DrawBasicSword(ModContent.Request<Texture2D>(GlowTexture).Value, Projectile.Center);
            return false;
        }
    }
}