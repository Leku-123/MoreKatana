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
        private float animationStoppedPoint;

        public override string Texture => this.GetTexture();

        public override void Initialize(int type)
        {
            Projectile.localNPCHitCooldown = -1;
            SwingEllipse = new(0.8f);
            GetTextureValues();
        }

        public override SwingData GetSwingData(int type) => new SwingData(Owner.itemAnimationMax, 0.8f);

        public override float GetProgress(int type)
        {
            if (!SwingStop) // 振り下げ
                return GeneralSwingAnimation(Progress);

            else // タイルに衝突した場合の反動
                return MathHelper.SmoothStep(animationStoppedPoint, animationStoppedPoint - 0.05f, DelayProgress);
        }

        public override void AdditionalAI(int type, bool onDelay) => Owner.SetDummyItemTime(2);

        public override void SafeTileCollide(int type, Vector2 collisionPoint, float oldProgress)
        {
            if (GetProgress(type) >= 0.5f && GetProgress(type) <= 0.8f)
            {
                SwingDelay = Owner.itemAnimationMax / 2f;
                animationStoppedPoint = oldProgress;

                if (!SwingStop)
                {
                    SwingStop = true;
                    KillPrims = true;
                    Owner.ScreenShake(3, 4);
                    SoundEngine.PlaySound(SoundID.Tink, Owner.Center);

                    for (int i = 0; i <= 12; i++)
                        Dust.NewDustPerfect(collisionPoint, DustID.Obsidian, Vector2.UnitY.RotatedByRandom(1) * Main.rand.NextFloat(-1, 1) * 5);
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            DrawBasicSword(TextureAssets.Projectile[Projectile.type].Value, Projectile.Center, Projectile.GetAlpha(lightColor));
            DrawBasicSword(ModContent.Request<Texture2D>(GlowTexture).Value, Projectile.Center);
            return false;
        }
    }
}