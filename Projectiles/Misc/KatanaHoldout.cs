using Microsoft.Xna.Framework;
using MoreKatana.Projectiles.Base;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using static MoreKatana.MoreKatanaUtil;

namespace MoreKatana.Projectiles.Misc
{
    public class KatanaHoldout : ModProjectile
    {
        private float charge;
        private const float chargeMax = 60;
        private bool fullyCharged;

        private Player Owner => Main.player[Projectile.owner];

        public override string Texture => MoreKatana.EmptyTexture;

        public override void SetDefaults()
        {
            Projectile.width = 2;
            Projectile.height = 2;
            Projectile.aiStyle = -1;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 9999;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.noEnchantmentVisuals = true;
            Projectile.hide = true;
        }

        public override void SendExtraAI(BinaryWriter writer) => writer.Write(fullyCharged);
        public override void ReceiveExtraAI(BinaryReader reader) => fullyCharged = reader.ReadBoolean();

        public override bool? CanDamage() => false;

        public override void AI()
        {
            if (Owner.CantUseHoldout(false))
            {
                Projectile.Kill();
                return;
            }

            if (Projectile.owner == Main.myPlayer && !Main.mouseLeft)
            {
                Projectile.Kill();
                return;
            }

            Owner.heldProj = Projectile.whoAmI;
            Owner.SetDummyItemTime(2);
            Owner.ChangeDir(Math.Sign(Owner.DirectionTo(Owner.MKPlayer().MouseWorld).X));

            Projectile.timeLeft = 2;
            Projectile.Center = Owner.Center;
            Projectile.velocity = Vector2.Zero;

            if (charge < chargeMax)
                charge++;
           
            if (charge == chargeMax)
            {
                if (!fullyCharged)
                {
                    fullyCharged = true;
                    SoundEngine.PlaySound(SoundID.MaxMana, Owner.Center);
                    DrawRing(Owner.Center, [DustID.GemDiamond], 24, 10f);
                }

                if (Owner.yoraiz0rEye < 2)
                    Owner.yoraiz0rEye = 2;
            }
        }

        public override void OnKill(int timeLeft)
        {
            if (Projectile.owner == Main.myPlayer)
            {
                int ai0 = 0;
                Owner.ScreenShake(2, 2);
                if (fullyCharged)
                {
                    ai0 = 1;
                    Owner.ScreenShake(10, 2);
                }

                int swing = ModContent.ProjectileType<KatanaSwing>();
                float damageMultiplier = 1 + 2 * (charge / chargeMax);
                Projectile.NewProjectile(Owner.GetSource_ItemUse(Owner.ActiveItem()), Owner.Center, Owner.SafeDirectionTo(Owner.MKPlayer().MouseWorld), swing, (int)(Projectile.damage * damageMultiplier), Projectile.knockBack, Owner.whoAmI, ai0);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // 別途UIを作りたい
            if (Projectile.owner == Main.myPlayer)
            {
                Vector2 gaugePos = Owner.Center + new Vector2(0, 50);

                Color color = Color.Gold;
                if (charge >= chargeMax - 3 && charge < chargeMax)
                    color = Color.White;

                DrawGauge(gaugePos, charge / chargeMax, color, size: new Vector2(60, 10));
            }

            return false;
        }
    }

    public class KatanaSwing : CustomSword
    {
        private Vector2 defVelocity;

        public override void SendExtraAI(BinaryWriter writer) => writer.WriteVector2(defVelocity);
        public override void ReceiveExtraAI(BinaryReader reader) => defVelocity = reader.ReadVector2();

        public override void Initialization(Item item, int type)
        {
            Projectile.localNPCHitCooldown = -1; // 1振りで同じターゲットに2回ヒットしないようにする
            GetTextureValues();
        }

        public override bool SwingPattern(Item item, int type)
        {
            SwingEllipse = new(1f, 0.7f);
            SwingStats(item.useAnimation, 0.7f);
            if (type == 1)
                ImpactCharge = 4;
            return base.SwingPattern(item, type);
        }

        public CurveSegment execute = new CurveSegment(SineOutEasing, 0f, 0f, 0.95f); // 振りのアニメーション
        public CurveSegment unwind = new CurveSegment(LinearEasing, 0.25f, 0.95f, 0.05f); // 振りの減衰のアニメーション
        public override float GetProgress(int type) => PiecewiseAnimation(Progress, execute, unwind);

        public override void AdditionalAI(Item item, int type, bool delay)
        {
            // プレイヤーのアイテム使用時間を延長する
            Owner.SetDummyItemTime(2);

            if (GetProgress(type) <= 0.95f)
            {
                if (type == 1)
                {
                    Main.projectile.Where(p => p.active && p.hostile && p.damage > 0
                    && Vector2.Distance(p.Center, Projectile.Center) <= SwordLength + Math.Min(p.width, p.height) / 2
                    && ProjectileLoader.CanDamage(p) != false && ProjectileLoader.CanHitPlayer(p, Owner)
                    ).ToList().ForEach(p =>
                    {
                        if (p.aiStyle == ProjAIStyleID.FallingTile && p.velocity.X == 0)
                            return;

                        if (Projectile.localAI[0] == 0)
                        {
                            Projectile.localAI[0] = 1;
                            defVelocity = p.velocity;
                            p.velocity *= 0.01f;
                            p.netUpdate = true;
                            ImpactChargeLaunch();
                            Projectile.netUpdate = true;
                        }
                        
                        if (OnImpact)
                        {
                            // 発射体のオーナーを設定する
                            p.hostile = false;
                            p.friendly = true;
                            p.owner = Owner.whoAmI;

                            // 速度を逆向きに
                            p.velocity = -defVelocity;

                            // ダメージ
                            p.damage = Projectile.damage;

                            // スプライト(テクスチャ)を反転させる
                            if (p.Center.X > Owner.Center.X)
                            {
                                p.direction = 1;
                                p.spriteDirection = 1;
                            }
                            else
                            {
                                p.direction = -1;
                                p.spriteDirection = -1;
                            }

                            p.netUpdate = true;
                            Projectile.netUpdate = true;

                            // プレイヤーに反動を付ける
                            Owner.velocity = Vector2.Normalize(p.Center - Owner.Center);
                            Owner.velocity *= -5f;
                            NetMessage.SendData(MessageID.PlayerControls, number: Owner.whoAmI);

                            // サウンド
                            SoundEngine.PlaySound(SoundID.NPCHit4 with { Pitch = +0.3f }, Owner.position);

                            // ダスト
                            for (int i = 0; i < 5; i++)
                            {
                                int newDust = Dust.NewDust(new Vector2(p.position.X, p.position.Y + 2f), p.width, p.height + 5, DustID.GemDiamond, p.velocity.X * 0.2f, p.velocity.Y * 0.2f, 100, default, 3f);
                                Main.dust[newDust].noGravity = true;
                            }

                            if (Main.myPlayer == Projectile.owner)
                            {
                                for (int i = 0; i < 2; i++)
                                {
                                    float maxOffset = p.width * 0.4f;
                                    if (maxOffset > 300f)
                                        maxOffset = 300f;

                                    Vector2 spawnOffset = (MathHelper.Pi + Main.rand.NextFloatDirection() * 0.2f).ToRotationVector2() * Main.rand.NextFloatDirection() * maxOffset;
                                    Vector2 sliceVelocity = spawnOffset.SafeNormalize(Vector2.UnitY) * 0.1f;
                                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), p.Center + spawnOffset, sliceVelocity, ModContent.ProjectileType<KatanaSlashEffect>(), Projectile.damage, 0f, Projectile.owner);
                                }
                            }
                        }
                    });
                }
            }
        }
    }
}