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
    public class ObsidianSwing : CustomSword
    {
        public override string Texture => this.GetTexture(Name);

        public override void Initialization(Item item, int type)
        {
            Projectile.localNPCHitCooldown = Owner.itemAnimationMax * Projectile.MaxUpdates;
            GetTextureValues(this, item);
        }

        public override bool SwingPattern(Item item, int type)
        {
            SwingEllipse = new(0.8f);
            SwingStats(Owner.itemAnimationMax, 0.8f);
            DelayTimer = 1;
            return base.SwingPattern(item, type);
        }

        public override float GetProgress(int type) => MoreKatanaUtil.CircOutEasing(Progress, 1);

        public override void AdditionalAI(Item item, int type, bool onDelay) => Owner.SetDummyItemTime(2);

        public override void SafeTileCollide(Item item, int type, Vector2 collisionPoint)
        {
            if (Progress >= 0.5f && Progress <= 0.8f)
            {
                DelayTimer = 10 * Projectile.MaxUpdates;

                if (!SwingStop)
                {
                    SwingStop = true;
                    Owner.ScreenShake(2, 2);
                    SoundEngine.PlaySound(SoundID.Tink, Owner.Center);

                    for (int i = 0; i <= 12; i++)
                        Dust.NewDustPerfect(collisionPoint, DustID.Obsidian, Vector2.UnitY.RotatedByRandom(1) * Main.rand.NextFloat(-1, 1) * 5);
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Texture2D glowTex = ModContent.Request<Texture2D>(GlowTexture).Value;
            Vector2 position = Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);
            DrawBasicSword(texture, position, Projectile.GetAlpha(lightColor));
            DrawBasicSword(glowTex, position, Color.White * Projectile.Opacity);
            return false;
        }
    }
}