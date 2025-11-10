using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MoreKatana.Assets.ExtraTextures;
using MoreKatana.Items.Weapons.TerraKatanaTree;
using MoreKatana.Particles;
using MoreKatana.Projectiles.PrimTrails;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace MoreKatana.Projectiles.TerraKatanaTree
{
    public class VolcanicEruptionRock : ModProjectile, ITrailProjectile
    {
        private ref float ProjIndex => ref Projectile.ai[0];

        public enum State
        {
            Holding,
            Firing
        }

        public State CurrentState
        {
            get => (State)Projectile.ai[1];
            set => Projectile.ai[1] = (int)value;
        }

        private ref float Timer => ref Projectile.ai[2];

        private const float Gravity = 0.8f;
        private Vector2 startingPos;
        private Vector2 InitVelocity = Vector2.Zero;

        public Player Owner => Main.player[Projectile.owner];

        public override void SetDefaults()
        {
            Projectile.width = 36;
            Projectile.height = 36;
            Projectile.aiStyle = -1;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 600;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = false;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 30;
        }

        public void DoTrailCreation(TrailManager tManager)
        {
            tManager.CreateTrail(Projectile, VolcanoKatana.FireColor(), MoreKatanaTextures.FlameTrailTexture.Value, 44, 20);
            tManager.CreateTrail(Projectile, VolcanoKatana.FireColor(), MoreKatanaTextures.StraightlineTrailTexture.Value, 36, 30);
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.WriteVector2(startingPos);
            writer.WriteVector2(InitVelocity);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            startingPos = reader.ReadVector2();
            InitVelocity = reader.ReadVector2();
        }

        public override void AI()
        {
            switch (CurrentState)
            {
                case State.Holding:
                    Behavior_Holding();
                    break;
                case State.Firing:
                    Behavior_Firing();
                    break;
            }

            Timer++;
        }

        private void Behavior_Holding()
        {
            Projectile.timeLeft = 2;
            Projectile.tileCollide = false;

            RockPositioning();

            if (Timer % 5 == 0)
            {
                Vector2 position = Main.rand.NextVector2FromRectangle(Projectile.Hitbox);
                Vector2 velocity = Vector2.UnitY.RotatedBy(MathHelper.Pi) * 10f * Main.rand.NextFloat(0.3f);
                ParticleHandler.SpawnParticle(new GlowParticle(position, velocity, Color.OrangeRed, Main.rand.NextFloat(0.3f, 0.5f), 25));
            }

            if (Projectile.owner == Main.myPlayer && !Main.mouseRight)
            {
                Projectile.timeLeft = 120;
                CurrentState = State.Firing;
                Timer = 0;
                Projectile.netUpdate = true;
            }
        }

        private void Behavior_Firing()
        {
            if (Timer % 2 == 0)
            {
                Vector2 position = Main.rand.NextVector2FromRectangle(Projectile.Hitbox);
                Vector2 velocity = Vector2.UnitY.RotatedBy(MathHelper.Pi) * 10f * Main.rand.NextFloat(0.3f);
                ParticleHandler.SpawnParticle(new GlowParticle(position, velocity, VolcanoKatana.FireColor(), Main.rand.NextFloat(0.3f, 0.5f), 25));
            }

            if (Timer > 10 * ProjIndex)
            {
                if (!Projectile.MKProj().Bool[0])
                {
                    Projectile.MKProj().Bool[0] = true;

                    Owner.ScreenShake(5, 5);
                    SoundEngine.PlaySound(SoundID.Item20, Projectile.Center);

                    Projectile.scale = 1f;

                    startingPos = Projectile.Center;
                    InitVelocity = MoreKatanaUtil.GetArcVel(startingPos, Main.MouseWorld + Main.rand.NextVector2Unit() * 50, Gravity, 50, maxXvel: 15f);

                    Projectile.velocity = InitVelocity;

                    for (int i = 0; i < 6; i++)
                    {
                        int newDust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, 0f, 0f, 100, default, 1.5f);
                        Main.dust[newDust].scale *= Main.rand.NextFloat(1, 2.5f);
                        Main.dust[newDust].noGravity = true;
                        Main.dust[newDust].velocity = Vector2.Normalize(Projectile.velocity) * 5f;
                        Main.dust[newDust].velocity = Main.dust[newDust].velocity.RotatedByRandom(MathHelper.ToRadians(30));
                        Main.dust[newDust].velocity *= Main.rand.NextFloat(1f, 3f);
                    }
                    for (int i = 0; i < 6; i++)
                    {
                        int newDust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke, 0f, 0f, 150, default, 0.5f);
                        Main.dust[newDust].fadeIn = 1.25f;
                        Main.dust[newDust].noLight = true;
                        Main.dust[newDust].velocity = Vector2.Normalize(Projectile.velocity) * 5f;
                        Main.dust[newDust].velocity = Main.dust[newDust].velocity.RotatedByRandom(MathHelper.ToRadians(15));
                        Main.dust[newDust].velocity *= Main.rand.NextFloat(0.5f, 2f);
                    }

                    Projectile.netUpdate = true;
                }

                Projectile.penetrate = 1;
                Projectile.velocity.Y += Gravity;

                if (!Projectile.tileCollide && !Collision.SolidCollision(Projectile.position, Projectile.width, Projectile.height))
                    Projectile.tileCollide = true;
            }
            else
            {
                RockPositioning();
            }
        }

        private void RockPositioning()
        {
            float f = ((float)ProjIndex / VolcanoKatanaHoldout.MaxRockCount + Owner.miscCounterNormalized * 3f) * ((float)Math.PI * 2f);
            Vector2 vector = Owner.position - Owner.oldPosition;
            Projectile.Center += vector;
            Vector2 value = f.ToRotationVector2();
            Vector2 value2 = Owner.Center + new Vector2(0, -150) + value * new Vector2(2f, 0.08f) * Projectile.width * 1.5f;
            Projectile.Center = Vector2.Lerp(Projectile.Center, value2, 0.08f);
            Projectile.localAI[0] = value.Y;
            Projectile.scale = 1f + (Projectile.localAI[0] / 4f);
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            if (Projectile.localAI[0] >= 0f)
                overPlayers.Add(index);
            else
                behindNPCs.Add(index);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire, 60 * 3);
        }

        public override void OnKill(int timeLeft)
        {
            Owner.ScreenShake(20, 25);

            ParticleHandler.SpawnParticle(new PulseCircle(Projectile.Center, Vector2.UnitY, VolcanoKatana.FireColor(), new Vector2(0.8f), 40, MoreKatanaUtil.CircOutEasing));
            ParticleHandler.SpawnParticle(new ImpactEffect(Projectile.Center, -Vector2.UnitY, VolcanoKatana.FireColor(), new Vector2(0.5f), 10));

            if (Projectile.owner == Main.myPlayer)
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center.X, Projectile.Center.Y, 0f, -1f * Owner.gravDir, ProjectileID.Volcano, Projectile.damage, Projectile.knockBack, Projectile.owner);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Texture2D bloomTex = MoreKatanaTextures.BloomTexture.Value;

            Rectangle rectangle = texture.Frame(1, Main.projFrames[Projectile.type], 0, Projectile.frame);
            Vector2 origin = rectangle.Size() / 2f;
            Vector2 position = Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);
            SpriteEffects spriteEffects = (Projectile.spriteDirection == -1) ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            // ブルームの描画
            Main.spriteBatch.Draw(bloomTex, position, null, VolcanoKatana.FireColor(0), Projectile.rotation, bloomTex.Size() / 2f, Projectile.scale * 0.45f, SpriteEffects.None, 0);

            // バックグローの描画
            MoreKatanaUtil.DrawBackglow(texture, position, rectangle, VolcanoKatana.FireColor(0), Projectile.rotation, 4f, new Vector2(Projectile.scale), spriteEffects);

            // 発射体本体の描画
            Main.EntitySpriteDraw(texture, position, rectangle, VolcanoKatana.FireColor(50), Projectile.rotation, origin, Projectile.scale, spriteEffects, 0);

            // 予測線とマークの描画
            if (InitVelocity != Vector2.Zero)
            {
                const int lineWidth = 10;
                float fade = (Timer - 10 * ProjIndex) / 15;
                fade = MathHelper.Clamp(fade, 0f, 1f);

                Vector2 startPos = startingPos;
                Vector2 endPos = Vector2.Zero;

                for (int i = 0; i < 80; i++)
                {
                    Vector2 nextVel = new Vector2(InitVelocity.X, InitVelocity.Y + Gravity * i);
                    startPos += nextVel;
                    Vector2 nextPos = startPos + nextVel * 1.5f;

                    // 描画位置がタイル接触した場合描画を止める
                    if (Collision.SolidCollision(startPos, lineWidth, lineWidth))
                    {
                        endPos = startPos;
                        break;
                    }

                    // 描画時widthの値が0になることを防ぐ
                    if (fade != 1f)
                        Utils.DrawLine(Main.spriteBatch, startPos, nextPos, VolcanoKatana.FireColor(), VolcanoKatana.FireColor(), lineWidth * (1 - fade));
                }

                // 予告線の位置がタイルに接触した場合、その位置にマークを描画する
                if (endPos != Vector2.Zero)
                {
                    Texture2D mark = ModContent.Request<Texture2D>(this.GetTexture("VolcanicEruptionMark")).Value;
                    float rot = -0.06283186f * Owner.miscCounter;
                    Main.spriteBatch.Draw(bloomTex, endPos - Main.screenPosition, null, VolcanoKatana.FireColor(0) * fade, 0f, bloomTex.Size() / 2f, Projectile.scale * 0.5f, SpriteEffects.None, 0);
                    Main.spriteBatch.Draw(mark, endPos - Main.screenPosition, null, Color.White * 0.5f * fade, rot, mark.Size() / 2f, Projectile.scale + (2f * (1 - fade)), SpriteEffects.None, 0);
                }
            }

            return false;
        }
    }
}