using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MoreKatana.Assets.ExtraTextures;
using MoreKatana.Particles;
using MoreKatana.Projectiles.PrimTrails;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace MoreKatana.Projectiles
{
    public class GeneralDashSlash : ModProjectile, ITrailProjectile
    {
        public ref float DashDistance => ref Projectile.ai[0];
        public ref float DashTime => ref Projectile.ai[1];
        public ref float Timer => ref Projectile.ai[2];

        /// <summary>
        /// この発射体を呼び出したときに設定することで追加挙動をさせることができます
        /// 例えばダッシュ中に別の発射体を飛ばしたりとか
        /// </summary>
        public delegate void UpdateAction(Projectile projectile);
        public UpdateAction action;

        public const int FadeoutTime = 30;
        private Color TrailColor;

        public Player Owner => Main.player[Projectile.owner];
        private Item ActiveItem => Owner.ActiveItem();

        public override string Texture => MoreKatana.EmptyTexture;

        public override void SetDefaults()
        {
            Projectile.width = Player.defaultWidth;
            Projectile.height = Player.defaultHeight;
            Projectile.aiStyle = -1;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.timeLeft = (int)DashTime + FadeoutTime;
            Projectile.penetrate = -1;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.ownerHitCheck = true;
            Projectile.alpha = 255;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.noEnchantmentVisuals = true;
            Projectile.MKProj().SourceIsItemUse = true;
            Projectile.MKProj().DashProjectile = true;
        }

        public void DoTrailCreation(TrailManager tManager)
        {
            TrailColor = GetTrailColor();
            tManager.CreateTrail(Projectile, TrailColor, MoreKatanaTextures.CutlineTrailTexture.Value, 42, 100, 0);
            tManager.CreateTrail(Projectile, TrailColor, MoreKatanaTextures.CutlineTrailTexture.Value, 10, 100, 0);
        }

        public bool DoTrailDeletion() => Projectile.timeLeft <= (int)DashTime;

        private Color GetTrailColor()
        {
            Color[] colors = MoreKatanaUtil.GetColors(TextureAssets.Item[ActiveItem.type].Value);
            int a = 0;
            Vector4 vector4 = new Vector4(0, 0, 0, 0);
            for (int i = 0; i < colors.Length; i++)
            {
                if (colors[i] != new Color(0, 0, 0, 0))
                {
                    a++;
                    vector4 += colors[i].ToVector4();
                }
            }
            vector4 /= a * 2;
            return new Color(vector4.X, vector4.Y, vector4.Z, 255);
        }

        public override void AI()
        {
            if (Timer == 1)
            {
                Owner.GeneralDashEffect(Projectile.velocity, (int)DashDistance, DashTime, true);

                for (int i = 0; i < 12; i++)
                {
                    int newDust = Dust.NewDust(Owner.MountedCenter, 32, 32, DustID.Smoke, 0f, 0f, 100, default, 2f);
                    Main.dust[newDust].velocity -= Projectile.velocity.Normalized() * 2f;
                    Main.dust[newDust].velocity = Main.dust[newDust].velocity.RotatedByRandom(MathHelper.ToRadians(10));
                    Main.dust[newDust].velocity *= Main.rand.NextFloat(1f, 3f);
                }

                ParticleHandler.SpawnParticle(new ImpactEffect(Owner.MountedCenter, -Projectile.velocity.Normalized(), TrailColor, new Vector2(1f), 10));
            }

            Projectile.Center = Owner.MountedCenter;
            Projectile.spriteDirection = Owner.direction;
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2 - MathHelper.PiOver4 * Projectile.spriteDirection;

            Owner.heldProj = Projectile.whoAmI;
            Owner.SetDummyItemTime(2);
            Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.velocity.ToRotation() - (float)Math.PI / 2f);
            Owner.armorEffectDrawShadow = true;
            Owner.canRocket = false; // ロケットブーツなどでの飛行をさせない

            if (Projectile.timeLeft > (int)DashTime)
            {
                Projectile.alpha = 0;
                Owner.legFrame.Y = 56 * 5;

                if (Owner.velocity.Length() > 2f)
                {
                    for (int i = 0; i < (int)(Owner.velocity.Length() / 10f); i++)
                    {
                        Vector2 pos = Projectile.Center + Main.rand.NextVector2Circular(10, 10) - Owner.velocity.Normalized() * 50f;
                        Vector2 vel = Owner.velocity.Normalized() * 0.5f;
                        Color color = TrailColor;
                        Vector2 scale = new Vector2(0.25f, Main.rand.NextFloat(0.5f, 1.5f)) * 2;
                        Particle line = new ImpactLine(pos, vel, color, scale, 60);
                        line.TimeActive = 30;
                        ParticleHandler.SpawnParticle(line);
                    }
                }
            }
            else
            {
                Projectile.alpha += 255 / FadeoutTime;
            }

            action?.Invoke(Projectile);

            Timer++;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Item[ActiveItem.type].Value;

            DrawAnimation animation = Main.itemAnimations[ActiveItem.type];

            Vector2 offset = Projectile.velocity * (texture.Width - 6);
            Vector2 position = Projectile.Center + offset - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);
            Rectangle? rectangle = new Rectangle?(animation == null ? texture.Frame(1, 1, 0, 0, 0, 0) : animation.GetFrame(texture, -1));

            float frame = animation == null ? 1 : animation.FrameCount;
            Vector2 origin = new Vector2(texture.Width / 2, texture.Height / frame / 2);

            SpriteEffects spriteEffects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            MoreKatanaUtil.DrawBackglow(texture, position, (Rectangle)rectangle, Color.White with { A = 0 } * Projectile.Opacity, Projectile.rotation, 2f, new Vector2(Projectile.scale), spriteEffects);

            Main.EntitySpriteDraw(texture, position, rectangle, Projectile.GetAlpha(lightColor) * Projectile.Opacity, Projectile.rotation, origin, Projectile.scale, spriteEffects, 0f);

            return false;
        }
    }
}