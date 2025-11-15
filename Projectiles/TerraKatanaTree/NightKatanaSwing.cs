using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MoreKatana.Items.Weapons.TerraKatanaTree;
using MoreKatana.Particles;
using MoreKatana.Projectiles.Base;
using MoreKatana.Projectiles.PrimTrails;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;
using static MoreKatana.MoreKatanaUtil;

namespace MoreKatana.Projectiles.TerraKatanaTree
{
    public class NightKatanaSwing : CustomSword
    {
        public override void Initialize(int type)
        {
            Main.projFrames[Projectile.type] = 5;

            Projectile.localNPCHitCooldown = -1;
            Projectile.MKProj().DashProjectile = false;
            Projectile.MKProj().Bool[0] = false;

            if (type == 6)
            {
                Projectile.localNPCHitCooldown = OwnerItem.useAnimation / 4 * Projectile.MaxUpdates;
                SwingEllipse = new(3f, 0.3f);
                FixedDirection = true;
                NoSpeedBonus = true;
                CreateSound = false;
                OwnerItem.MKItem().AttackType = 0;
            }
            else if (type == 7)
            {
                SwingEllipse = new(1.1f, 1.1f);
                SwingDirection *= Owner.direction;
                OwnerItem.MKItem().AttackType = 0;
            }
            else
            {
                SwingEllipse = new(1.4f, 0.9f);
                Owner.ScreenShake(2, 10);
                OwnerItem.MKItem().AttackType = SwingType;
            }

            GetTextureValues();
        }

        // 全てのスイングデータを設定する
        public SwingData StandardDown => new SwingData(OwnerItem.useAnimation, 0.8f); // 通常の切り下げ
        public SwingData StandardUp => new SwingData(OwnerItem.useAnimation, 0.8f, backspin: true); // 通常の切り上げ
        public SwingData LargeDown => new SwingData(OwnerItem.useAnimation * 2f, 0.65f, 0.2f); // 大振りの切り下げ
        public SwingData Spin => new SwingData(OwnerItem.useAnimation * 2f, 3.6f, 0.2f, delay: OwnerItem.useAnimation / 2f); // スピン
        public SwingData Retreat => new SwingData(OwnerItem.useAnimation * 0.8f, 1.5f, 0.2f, Owner.MKPlayer().MouseWorld.X < Owner.Center.X);
        // なんか分かんないけどなんかバカ
        public override SwingData GetSwingData(int type) => SwingData.SwingRegister(type,
            StandardDown, StandardUp,
            StandardDown, StandardUp,
            StandardDown, StandardUp,
            Spin, Retreat);

        public override float GetProgress(int type)
        {
            if (type == 6)
            {
                if (Progress != 1f)
                    return LinearEasing(Progress, 1);
                else
                    return MathHelper.SmoothStep(1f, 1.01f, DelayProgress);
            }
            else if (type == 7)
                return LinearEasing(Progress, 1);
            else // それ以外はテンプレートのものを適用する
                return GeneralSwingAnimation(Progress);
        }

        public override void AdditionalAI(int type, bool delay)
        {
            Owner.SetDummyItemTime(2);

            if (type == 6)
            {
                if (Projectile.owner == Main.myPlayer && !Main.mouseLeft)
                    Projectile.Kill();

                if (++Projectile.frameCounter >= 4 * Projectile.MaxUpdates)
                {
                    Projectile.frameCounter = 0;
                    Projectile.frame = ++Projectile.frame % Main.projFrames[Projectile.type];
                }

                Owner.armorEffectDrawShadow = true;
                Owner.FlipEffect(GetProgress(type) * 12f);
                Owner.MKPlayer().slowFallEffect = 2;

                if (!delay)
                {
                    Projectile.friendly = true;

                    if (Projectile.soundDelay <= 0)
                    {
                        Projectile.soundDelay = 10 * Projectile.MaxUpdates;
                        SoundEngine.PlaySound(MoreKatanaSounds.SwordSlash, Owner.position);

                        Owner.ScreenShake(2, 10);
                    }
                }
                else
                {
                    Projectile.friendly = false;
                }
            }
            if (type == 7)
            {
                Owner.immune = true;
                Owner.immuneTime = 60;
                Owner.immuneAlpha = 255;

                if (Owner.yoraiz0rEye < 2)
                    Owner.yoraiz0rEye = 2;

                if (!Projectile.MKProj().Bool[0])
                {
                    Projectile.MKProj().Bool[0] = true;
                    SoundEngine.PlaySound(MoreKatanaSounds.SwordSlash, Owner.Center);

                    Owner.UpdateRotation(1, SwingDirection * Owner.direction, SwingTime);
                    Owner.velocity = Vector2.Zero;
                    Owner.velocity.X += 8 * SwingDirection * Owner.direction;
                    Owner.velocity.Y -= 8;

                    NetMessage.SendData(MessageID.PlayerControls, number: Owner.whoAmI);
                }
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            base.OnHitNPC(target, hit, damageDone);

            ParticleOrchestraSettings particleOrchestraSettings = default;
            particleOrchestraSettings.PositionInWorld = Main.rand.NextVector2FromRectangle(target.Hitbox);
            ParticleOrchestrator.RequestParticleSpawn(false, ParticleOrchestraType.NightsEdge, particleOrchestraSettings, Projectile.owner);

            Owner.MKPlayer().NightComboTimer = NightKatana.MaxComboTime;

            if (SwingType == 6)
            {
                OwnerItem.MKItem().AttackType = SwingType;

                if (!Projectile.MKProj().Bool[0])
                {
                    Projectile.MKProj().Bool[0] = true;
                    SoundEngine.PlaySound(SoundID.Tink, Owner.position);
                    Rectangle textPos = new Rectangle((int)Owner.position.X, (int)Owner.position.Y - 20, Owner.width, Owner.height);
                    CombatText.NewText(textPos, Color.Violet, "Max Combo!", true, true);
                }
            }
            else if (SwingType != 7)
            {
                OwnerItem.MKItem().AttackType = SwingType + 1;

                if (!Projectile.MKProj().Bool[0])
                {
                    Projectile.MKProj().Bool[0] = true;
                    SoundEngine.PlaySound(SoundID.Tink, Owner.position);
                    Rectangle textPos = new Rectangle((int)Owner.position.X, (int)Owner.position.Y - 20, Owner.width, Owner.height);
                    CombatText.NewText(textPos, Color.Violet, SwingType + 1 + "Combo!", true, true);
                }
            }
        }

        public override void OnKill(int timeLeft)
        {
            if (SwingType == 6 && !Main.mouseLeft)
            {
                NPC target = Owner.Center.ClosestNPCAt(300f);
                if (target != null)
                {
                    Owner.Teleport(target.Top, -1);
                    NetMessage.SendData(MessageID.TeleportEntity, -1, -1, null, 0, Owner.whoAmI, target.Top.X, target.Top.Y, -1);
                }

                Owner.ScreenShake(15, 20);
                SoundEngine.PlaySound(MoreKatanaSounds.Parry, Owner.position);
                ParticleHandler.SpawnParticle(new PulseCircle(Owner.Center, Vector2.UnitY, Color.Violet, new Vector2(0.8f), 40, CircOutEasing));

                for (int i = 0; i < 5; i++)
                {
                    Particle glowSpark = new GlowSparkParticle(Owner.Center, new Vector2(0.1f, 0.1f).RotatedByRandom(100), false, 15, Main.rand.NextFloat(0.05f, 0.1f), Color.Violet, new Vector2(2f, 0.5f), true);
                    ParticleHandler.SpawnParticle(glowSpark);
                }

                for (int i = 0; i < 20; ++i)
                {
                    int newDust = Dust.NewDust(Owner.Center, Owner.width, Owner.height, Utils.SelectRandom(Main.rand, DustID.Demonite, DustID.Shadowflame));
                    Main.dust[newDust].velocity *= 5f;
                    Main.dust[newDust].fadeIn = 1f;
                    Main.dust[newDust].scale = 1 + Main.rand.NextFloat() + Main.rand.Next(4) * 0.3f;
                    if (Main.rand.NextBool(3))
                    {
                        Main.dust[newDust].noGravity = true;
                        Main.dust[newDust].velocity *= 3f;
                        Main.dust[newDust].scale *= 2f;
                    }
                }

                if (Projectile.owner == Main.myPlayer)
                {
                    int swing = ModContent.ProjectileType<NightKatanaSwing>();
                    Projectile.NewProjectile(Owner.GetSource_ItemUse(Owner.ActiveItem()), Owner.Center, Projectile.velocity, swing, Projectile.damage * 5, Projectile.knockBack, Owner.whoAmI, 7);
                }
            }
        }

        public override void DrawTrail(int type)
        {
            // スピン以外はトレイルを描画する
            if (GetProgress(type) >= 0f && Timer > 1f && type != 6)
            {
                if (!PrimsCreated)
                {
                    PrimsCreated = true;
                    SwordTrail = new CustomSwordPrimTrail(Projectile, TrailColor, SwordLength, (int)(SwingTime * 1.5f));
                    MoreKatana.primitives.CreateTrail(SwordTrail);
                }

                UpdateTrail(SwordTrail);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Item[OwnerItem.type].Value;
            Vector2 position = Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);
            Color glowColor = Color.White * Projectile.Opacity;
            Color trailColor = Color.Violet * Projectile.Opacity;

            if (Progress != 1f && SwingType != 6)
                DrawBackglow(texture, position, null, glowColor with { A = 0 }, Projectile.rotation, 4f * (1 - Progress), new Vector2(Projectile.scale), SwingEffectsHV());

            DrawBasicSword(texture, Projectile.Center);

            if (SwingType == 6)
            {
                Texture2D spinTex = ModContent.Request<Texture2D>(this.GetTexture("NightKatanaSpin")).Value;
                Rectangle spinRect = spinTex.Frame(1, Main.projFrames[Projectile.type], 0, Projectile.frame);
                Vector2 spinOrigin = spinRect.Size() / 2f;
                Vector2 spinPos = Owner.MountedCenter - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);
                SpriteEffects spriteEffects_ = Owner.direction == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
                Main.spriteBatch.Draw(spinTex, spinPos, spinRect, trailColor with { A = 0 } * (1 - Progress), 0f, spinOrigin, Projectile.scale, spriteEffects_, 0f);
            }
            else
            {
                // 剣先にスパークルを描画する
                Vector2 offset = Utils.DirectionTo(Owner.MountedCenter, Projectile.Center) * 40f * Projectile.scale;
                DrawPrettyStarSparkle(1f, SpriteEffects.None, position + offset, glowColor * (1 - Progress), trailColor * (1 - Progress),
                        0.5f, 0f, 0.1f, 0.9f, 1f, 0f, new Vector2(Projectile.scale, Projectile.scale * 2.5f), new Vector2(1f, 1f));
            }

            return false;
        }
    }
}