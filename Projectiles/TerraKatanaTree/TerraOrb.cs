using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MoreKatana.Assets.ExtraTextures;
using MoreKatana.Items.Weapons.TerraKatanaTree;
using MoreKatana.Projectiles.PrimTrails;
using System;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;

namespace MoreKatana.Projectiles.TerraKatanaTree
{
    public class TerraOrb : ModProjectile
    {
        private ref float Direction => ref Projectile.ai[0];

        private ref float Timer => ref Projectile.ai[1];

        public enum State
        {
            Holding,
            Firing,
            Hit
        }

        public State CurrentState
        {
            get => (State)Projectile.ai[2];
            set => Projectile.ai[2] = (int)value;
        }

        private Vector2 swingVel = Vector2.Zero;

        private TextureMapPrimTrail trail;

        public Player Owner => Main.player[Projectile.owner];

        public override string Texture => MoreKatana.EmptyTexture;

        public override void SetDefaults()
        {
            Projectile.width = 4;
            Projectile.height = 4;
            Projectile.aiStyle = -1;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 90;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
            Projectile.noEnchantmentVisuals = true;
        }

        public override void SendExtraAI(BinaryWriter writer) => writer.WriteVector2(swingVel);

        public override void ReceiveExtraAI(BinaryReader reader) => swingVel = reader.ReadVector2();

        public override void AI()
        {
            if (Owner.CantUseHoldout(false))
            {
                Projectile.Kill();
                return;
            }

            if (Owner.ActiveItem().type != ModContent.ItemType<TerraKatana>())
            {
                Projectile.Kill();
                return;
            }

            Direction = Direction.ToDirection();

            switch (CurrentState)
            {
                case State.Holding:
                    const float xOffset = 120f;
                    const float yOffset = 100f;

                    if (Timer > 15)
                    {
                        const float lerp = 0.09f;
                        Vector2 mouseDirection = Utils.DirectionTo(Owner.MountedCenter, Owner.MKPlayer().MouseWorld);

                        Vector2 offset = Owner.MountedCenter;
                        offset += mouseDirection * xOffset;
                        offset += mouseDirection.RotatedBy(90 * Direction * Math.Sign(mouseDirection.X)) * yOffset;

                        Vector2 target = Vector2.Lerp(Projectile.position, offset, lerp);

                        if (target != Projectile.position)
                            Projectile.netUpdate = true;

                        Projectile.position = target;
                        Projectile.velocity = Vector2.Zero;
                    }

                    Projectile.timeLeft = 90;

                    foreach (Projectile proj in Main.projectile)
                    {
                        if (proj.whoAmI == Projectile.whoAmI || !proj.active || proj.owner != Projectile.owner)
                            continue;

                        if (proj.type == ModContent.ProjectileType<TerraKatanaSwing>() && proj.ai[0] == 3)
                        {
                            Timer = 0;
                            if (swingVel == Vector2.Zero)
                            {
                                swingVel = proj.velocity;
                                Projectile.netUpdate = true;
                            }
                            else
                            {
                                float progress = ++Projectile.localAI[0] / (15 / Owner.GetTotalAttackSpeed(Projectile.DamageType));
                                Vector2 offset = Owner.MountedCenter + (Vector2.Normalize(swingVel) * xOffset);
                                Vector2 target = Vector2.SmoothStep(Projectile.position, offset, progress);

                                if (target != Projectile.position)
                                    Projectile.netUpdate = true;

                                Projectile.position = target;
                                Projectile.velocity = Vector2.Zero;
                            }
                        }

                        if (proj.type == ModContent.ProjectileType<TerraOrb>() && proj.Distance(Projectile.position) <= 10f && Direction == 1)
                        {
                            if (!Projectile.MKProj().Bool[0])
                            {
                                Projectile.MKProj().Bool[0] = true;
                                SoundEngine.PlaySound(SoundID.MaxMana, Owner.Center);
                                MoreKatanaUtil.DrawRing(Projectile.Center, [DustID.Terra], 24, 15, dustScale: 2f);
                            }
                        }
                    }
                    break;
                case State.Firing:
                    if (Timer > 5)
                    {
                        if (!Projectile.MKProj().Bool[1])
                        {
                            Projectile.MKProj().Bool[1] = true;
                            Projectile.velocity = Vector2.Normalize(swingVel) * 40f;
                            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

                            if (Direction == 1)
                            {
                                SoundEngine.PlaySound(SoundID.Item29, Owner.Center);
                                SoundEngine.PlaySound(SoundID.Item60, Owner.Center);
                                Owner.ScreenShake(3, 15);
                                Owner.CreateImpactEffect(Projectile.GetSource_FromThis(), Projectile.Center, Projectile.velocity, Projectile.owner, 1f, new Color(96, 248, 96));

                                for (int i = 0; i < 24; i++)
                                {
                                    int newDust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Terra, 0f, 0f, 100, default, 1.5f);
                                    Main.dust[newDust].scale *= Main.rand.NextFloat(1, 2.5f);
                                    Main.dust[newDust].noGravity = true;
                                    Main.dust[newDust].velocity = Vector2.Normalize(Projectile.velocity) * 15f;
                                    Main.dust[newDust].velocity = Main.dust[newDust].velocity.RotatedByRandom(MathHelper.ToRadians(60));
                                    Main.dust[newDust].velocity *= Main.rand.NextFloat(1f, 3f);
                                }

                                if (Projectile.owner == Main.myPlayer)
                                {
                                    int edge = ModContent.ProjectileType<TerraEdge>();
                                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center + Projectile.velocity * 2f, Projectile.velocity, edge, Projectile.damage * 2, Projectile.knockBack, Projectile.owner, (int)TerraEdge.AttackType.Firing);
                                }
                            }

                            Projectile.netUpdate = true;
                        }
                    }
                    break;
                case State.Hit:
                    Projectile.velocity = Projectile.velocity.RotatedBy(Math.PI / 10);
                    break;
            }

            Timer++;
        }

        public override bool? CanDamage() => (CurrentState == State.Firing || CurrentState == State.Hit) && Timer > 6;

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // テラブレードのパーティクル
            ParticleOrchestraSettings particleOrchestraSettings = default;
            particleOrchestraSettings.PositionInWorld = Main.rand.NextVector2FromRectangle(target.Hitbox);
            ParticleOrchestrator.RequestParticleSpawn(false, ParticleOrchestraType.TerraBlade, particleOrchestraSettings, Projectile.owner);

            if (CurrentState == State.Firing)
            {
                CurrentState = State.Hit;

                Projectile.velocity = Vector2.UnitY * Direction;
                Projectile.velocity *= 40f;
                Projectile.timeLeft = 22;

                if (Main.netMode != NetmodeID.Server)
                {
                    trail = new TextureMapPrimTrail(Projectile, new Color(96, 248, 96), MoreKatanaTextures.FlameTrailTexture.Value, 16);
                    MoreKatana.primitives.CreateTrail(trail);
                }

                Projectile.netUpdate = true;
            }
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (projHitbox.Intersects(targetHitbox))
                return true;

            float _ = float.NaN;
            float length = 30;
            Vector2 offset = length / 2 * Projectile.scale * Vector2.Normalize(Projectile.velocity).RotatedBy(90);
            Vector2 tip = Projectile.Center + offset;
            Vector2 end = Projectile.Center - offset;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), tip, end, Projectile.scale, ref _);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D bloomTex = MoreKatanaTextures.BloomTexture.Value;
            Texture2D starTex = MoreKatanaTextures.StarSparkleTexture.Value;
            Vector2 position = Projectile.Center - Main.screenPosition;
            Color color = new Color(96, 248, 96) with { A = 0 };
            Main.EntitySpriteDraw(bloomTex, position, null, color, 0f, bloomTex.Size() / 2f, Projectile.scale * 0.4f, SpriteEffects.None, 0);
            Main.EntitySpriteDraw(starTex, position, null, color, Main.GlobalTimeWrappedHourly * Direction, starTex.Size() / 2f, Projectile.scale * 0.7f, SpriteEffects.None, 0);
            return false;
        }
    }
}