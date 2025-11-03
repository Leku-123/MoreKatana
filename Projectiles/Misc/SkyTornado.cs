using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace MoreKatana.Projectiles.Misc
{
    public class SkyTornado : ModProjectile
    {
        private ref float Timer => ref Projectile.ai[1];

        private const float FadeInTime = 15f;
        private const float FadeOutTime = 60f;

        private Player Owner => Main.player[Projectile.owner];

        public override void SetDefaults()
        {
            Projectile.width = 56;
            Projectile.height = 224;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 9999;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override bool? CanDamage() => Timer > FadeInTime;

        public override void AI()
        {
            Projectile proj = MoreKatanaUtil.ProjectileExists(Projectile.ai[0], ModContent.ProjectileType<ChargingSkyKatana>());
            if (proj == null)
            {
                if (Timer >= FadeInTime)
                {
                    if (!Projectile.MKProj().Bool[1])
                    {
                        Projectile.MKProj().Bool[1] = true;
                        Projectile.velocity = new Vector2(Owner.DirectionTo(Owner.MKPlayer().MouseWorld).X, 0);
                        Projectile.velocity *= 12f;
                        Projectile.netUpdate = true;
                        Owner.ScreenShake(2, 5);
                    }

                    if (Timer > FadeInTime + FadeOutTime)
                        Projectile.Kill();

                    Timer++;
                }
                else
                {
                    Projectile.Kill();
                    return;
                }
            }
            else
            {
                if (proj.ai[0] >= 5)
                {
                    if (Timer < FadeInTime)
                    {
                        Timer++;
                    }
                    else
                    {
                        if (!Projectile.MKProj().Bool[0])
                        {
                            Projectile.MKProj().Bool[0] = true;
                            Owner.ScreenShake(2, 5);
                            SoundEngine.PlaySound(SoundID.MaxMana, Owner.Center);
                            MoreKatanaUtil.DrawRing(Owner.Center, DustID.Cloud, 48, 12f, dustScale: 1.5f);
                        }
                    }
                }

                Projectile.Bottom = Owner.Bottom;
            }

            Projectile.timeLeft = 2;

            for (int i = 0; i < 1; i++)
            {
                float lerpRandomizer = Main.rand.NextFloat();
                Vector2 dustVelocity = new Vector2(MathHelper.Lerp(0.1f, 1f, Main.rand.NextFloat()), MathHelper.Lerp(-0.5f, 0.9f, lerpRandomizer));
                dustVelocity.X *= MathHelper.Lerp(2.2f, 0.6f, lerpRandomizer);
                dustVelocity.X *= -1f;
                Vector2 dustCustomData = new Vector2(6f, 10f);
                Vector2 dustPosition = Projectile.Center + Projectile.Size * dustVelocity * 0.5f + dustCustomData;
                Dust cloudDust = Main.dust[Dust.NewDust(dustPosition, 0, 0, DustID.Cloud, 0f, 0f, 0, default, 1.5f)];
                cloudDust.position = dustPosition;
                cloudDust.customData = Projectile.Center + dustCustomData;
                cloudDust.fadeIn = 1f;
                cloudDust.scale = 0.3f;
                if (dustVelocity.X > -1.2f)
                {
                    cloudDust.velocity.X = 1f + Main.rand.NextFloat();
                }
                cloudDust.velocity.Y = Main.rand.NextFloat() * -0.5f - 1f;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            float trackerClamp = MathHelper.Clamp(Timer / FadeInTime, 0f, 1f);
            if (Timer > FadeInTime)
                trackerClamp = MathHelper.Lerp(1f, 0f, (Timer - FadeInTime) / FadeOutTime);

            float vectorMult = 0.2f;
            Vector2 TopVector = Projectile.Top;
            Vector2 BottomVector = Projectile.Bottom;

            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Rectangle drawRectangle = texture.Frame(1, 1, 0, 0);
            Vector2 smallRect = drawRectangle.Size() / 2f;
            float aiTrackMult = -0.06283186f * Owner.miscCounter;
            Vector2 spinningpoint2 = Vector2.UnitY.RotatedBy((double)(Owner.miscCounter * 0.1f), default);
            float incrementStorage = 0f;
            float increment = 5.1f;
            Color cloudColor = new Color(225, 225, 225);

            for (float i = (int)BottomVector.Y; i > (int)TopVector.Y; i -= increment)
            {
                incrementStorage += increment;
                float colorChanger = incrementStorage / Projectile.Size.Y;
                float incStorageMult = incrementStorage * 6.28318548f / -20f;
                float lowerColorChanger = colorChanger - 0.15f;
                Vector2 spinArea = spinningpoint2.RotatedBy((double)incStorageMult, default);
                Vector2 colorChangeVector = new Vector2(0f, colorChanger + 1f);
                colorChangeVector.X = colorChangeVector.Y * vectorMult;
                Color newCloudColor = Color.Lerp(Color.Transparent, cloudColor, colorChanger * 2f);
                if (colorChanger > 0.5f)
                {
                    newCloudColor = Color.Lerp(Color.Transparent, cloudColor, 2f - colorChanger * 2f);
                }
                newCloudColor.A = (byte)(newCloudColor.A * 0.5f);
                newCloudColor *= trackerClamp;
                spinArea *= colorChangeVector * 100f;
                spinArea.Y = 0f;
                spinArea.X = 0f;
                spinArea += new Vector2(BottomVector.X, i) - Main.screenPosition;
                Main.spriteBatch.Draw(texture, spinArea, drawRectangle, newCloudColor, aiTrackMult + incStorageMult, smallRect, 1f + lowerColorChanger, SpriteEffects.None, 0);
            }
            return false;
        }
    }
}