using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MoreKatana.Buffs;
using MoreKatana.Items.Weapons.Metal;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace MoreKatana.Projectiles.Metal
{
    public class ObsidianKatanaHoldUp : ModProjectile
    {
        private ref float Timer => ref Projectile.ai[0];

        private const float Lifespan = 90f;

        private float fadeInVal = 0.05f;
        private float rotX;

        public override string Texture => MoreKatana.EmptyTexture;

        private Player Owner => Main.player[Projectile.owner];

        public override void SetDefaults()
        {
            Projectile.width = 2;
            Projectile.height = 2;
            Projectile.aiStyle = -1;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = (int)Lifespan;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.noEnchantmentVisuals = true;
            Projectile.MKProj().SourceIsItemUse = true;
        }

        public override bool? CanDamage() => false;

        public override bool? CanCutTiles() => false;

        public override void AI()
        {
            if (Owner.CantUseHoldout(false))
            {
                Projectile.Kill();
                return;
            }

            if (Timer > 0)
                fadeInVal = Math.Clamp(MathHelper.Lerp(fadeInVal, 1.25f, 0.08f), 0f, 1f);

            if (Timer <= Lifespan - 60f)
            {
                if (Timer % 8 == 0)
                {
                    SoundEngine.PlaySound(SoundID.Tink, Owner.position);

                    for (int i = 0; i < 12; i++)
                    {
                        float f = Main.rand.NextFloat() * MathHelper.TwoPi;
                        float num = Main.rand.NextFloat();
                        Dust dust = Dust.NewDustPerfect(Owner.Center + f.ToRotationVector2() * (110 + 200 * num), DustID.Torch, (f - MathHelper.Pi).ToRotationVector2() * (14 + 8 * num));
                        dust.scale = 0.9f;
                        dust.fadeIn = 1.15f + num * 0.3f;
                        dust.noGravity = true;
                        dust.customData = Owner;
                    }
                }
            }
            else if (Timer >= Lifespan - 30f)
            {
                Owner.AddBuff(ModContent.BuffType<ObsidianBurnBuff>(), ObsidianKatana.FireBuffTime);

                if (Projectile.ai[1] == 0f)
                {
                    Projectile.ai[1] = 1f;
                    Owner.ScreenShake(10, 7);
                    SoundEngine.PlaySound(SoundID.DD2_BetsysWrathShot, Owner.Center);
                    MoreKatanaUtil.DrawRing(Owner.Center, DustID.Torch, 30, 5f);
                    MoreKatanaUtil.DrawRing(Owner.Center, DustID.Torch, 24, 10f, dustScale: 3f);

                    for (int i = 0; i <= 12; i++)
                        Dust.NewDustPerfect(Projectile.Center, DustID.Obsidian, -Vector2.UnitY.RotatedByRandom(1) * Main.rand.NextFloat(0.8f, 1f) * 5f);

                    if (Main.netMode != NetmodeID.Server)
                    {
                        for (int i = 0; i < 6; i++)
                            Gore.NewGore(Projectile.GetSource_FromThis(), Projectile.Center, -Vector2.UnitY.RotatedByRandom(1) * Main.rand.NextFloat(0.8f, 1f) * 5f, ModContent.Find<ModGore>("MoreKatana/ObsidianGore").Type, 1);
                    }
                }
            }

            SetSwordPosition();

            Timer++;
        }

        private void SetSwordPosition()
        {
            Owner.SetDummyItemTime(2);
            Owner.heldProj = Projectile.whoAmI;
            Owner.itemRotation = (Projectile.velocity * Projectile.direction).ToRotation();
            Owner.itemRotation = MathHelper.WrapAngle(Owner.itemRotation);
            Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation);

            float xOffset = Owner.velocity.X * 0.04f * Owner.direction;
            float yOffset = Owner.velocity.Y * 0.02f * -Owner.direction;
            float desiredArmAngle = (0.3f - MathHelper.PiOver2 + xOffset + yOffset) * Projectile.direction;
            Projectile.rotation = Projectile.rotation.AngleLerp(desiredArmAngle, fadeInVal < 1 ? 0.2f : 0.1f);
            Projectile.Center = Owner.Center + new Vector2(20 * Projectile.direction, -20);
            rotX = MathHelper.Lerp(rotX, Owner.velocity.X * 0.015f, 0.1f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            string textureKey = Projectile.ai[1] == 0f ? ModContent.GetInstance<ObsidianSwing>().Texture : ModContent.GetInstance<ObsidianSwing2>().Texture;
            Texture2D texture = ModContent.Request<Texture2D>(textureKey).Value;
            Texture2D glowTex = ModContent.Request<Texture2D>(textureKey + "_Glow").Value;

            Vector2 armPosition = Owner.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, Projectile.rotation);
            armPosition -= Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);
            if (Projectile.ai[1] == 0f)
                armPosition += Main.rand.NextVector2Unit() * 2f;

            Vector2 origin = Owner.direction == 1 ? new Vector2(4, texture.Height - 4) : new Vector2(4, 4);
            SpriteEffects effects = Owner.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipVertically;

            lightColor = Color.Lerp(lightColor, Color.Black * 0.2f, 1f - fadeInVal);

            float rot = Owner.direction == 1 ? 3f + (float)Math.PI : 1f + (float)Math.PI - (MathF.PI / 4f);
            float scale = Projectile.scale + (1f - fadeInVal) * 0.15f;

            Main.EntitySpriteDraw(texture, armPosition, null, lightColor, rot + rotX, origin, scale, effects, 0f);

            if (fadeInVal == 1)
                Main.EntitySpriteDraw(glowTex, armPosition, null, Color.White, rot + rotX, origin, scale, effects, 0f);

            return false;
        }
    }
}