using Microsoft.Xna.Framework;
using MoreKatana.Projectiles.Base;
using System;
using System.IO;
using System.Linq;
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
                    Owner.ScreenShake(2, 10);
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
        public bool Reflected;
        public bool ExecuteReflection;
        public int ReflectedIndex = -1;
        private Vector2 defVelocity;

        public bool ReflectionCheck(Projectile p)
            => p.active && p.hostile && p.damage > 0 && p.velocity.Length() > 0
            && Projectile.Colliding(Projectile.Hitbox, p.Hitbox)
            && !Reflected;

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(Reflected);
            writer.Write(ExecuteReflection);
            writer.Write(ReflectedIndex);
            writer.WriteVector2(defVelocity);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            Reflected = reader.ReadBoolean();
            ExecuteReflection = reader.ReadBoolean();
            ReflectedIndex = reader.ReadInt32();
            defVelocity = reader.ReadVector2();
        }

        public override void Initialize(int type)
        {
            Projectile.localNPCHitCooldown = -1; // 1振りで同じターゲットに2回ヒットしないようにする
            SwingEllipse = new(1f, 0.7f);
            GetTextureValues();
            if (type == 1)
                ImpactCharge = 5;
        }

        public CurveSegment execute = new CurveSegment(SineOutEasing, 0f, 0f, 0.95f); // 振りのアニメーション
        public CurveSegment unwind = new CurveSegment(LinearEasing, 0.25f, 0.95f, 0.05f); // 振りの減衰のアニメーション
        public override float GetProgress(int type) => PiecewiseAnimation(Progress, execute, unwind);

        public override void AdditionalAI(int type, bool delay)
        {
            // プレイヤーのアイテム使用時間を延長する
            Owner.SetDummyItemTime(2);

            if (GetProgress(type) < 0.95f)
            {
                if (type == 1)
                {
                    foreach (Projectile p in Main.projectile.Where(ReflectionCheck))
                    {
                        ReflectedIndex = p.whoAmI; // 反射する発射体のインデックスを保存
                        Reflected = true;
                        defVelocity = p.velocity;
                        p.velocity = Vector2.Zero;
                        p.netUpdate = true;

                        ImpactChargeLaunch();

                        // スクリーンシェイクを止める
                        Owner.ScreenShake(0, 0);

                        // プレイヤーのベロシティを0にする
                        Owner.velocity = Vector2.Zero;
                        NetMessage.SendData(MessageID.PlayerControls, number: Owner.whoAmI);
                    }

                    //if (OnImpact && ReflectedIndex != -1)
                    //{
                    //    ExecuteReflection = true;
                    //    Projectile.netUpdate = true;
                    //}

                    if (ExecuteReflection)
                    {
                        Projectile reflected = Main.projectile[ReflectedIndex];

                        // 発射体のオーナーを設定する
                        reflected.hostile = false;
                        reflected.friendly = true;
                        reflected.owner = Projectile.owner;

                        // 発射体の速度を設定する
                        reflected.velocity = Vector2.Normalize(Projectile.velocity);
                        reflected.velocity *= defVelocity.Length();

                        // ダメージ
                        reflected.damage = Projectile.damage;

                        // 一応これもやっとく
                        reflected.netUpdate = true;

                        ExecuteReflection = false;
                        ReflectedIndex = -1;
                        Projectile.netUpdate = true;

                        // プレイヤーに反動を付ける
                        Owner.velocity = Vector2.Normalize(Projectile.velocity);
                        Owner.velocity *= -5f;
                        NetMessage.SendData(MessageID.PlayerControls, number: Owner.whoAmI);

                        Owner.ScreenShake(5, 30);
                        // サウンド
                        SoundEngine.PlaySound(SoundID.NPCHit4 with { Pitch = +0.3f }, Owner.position);

                        // ダスト
                        for (int i = 0; i < 5; i++)
                        {
                            int newDust = Dust.NewDust(new Vector2(reflected.position.X, reflected.position.Y + 2f), reflected.width, reflected.height, DustID.GemDiamond, reflected.velocity.X * 0.2f, reflected.velocity.Y * 0.2f, 100, default, 3f);
                            Main.dust[newDust].noGravity = true;
                        }

                        if (Main.myPlayer == Projectile.owner)
                        {
                            for (int i = 0; i < 2; i++)
                            {
                                float maxOffset = reflected.width * 0.4f;
                                if (maxOffset > 300f)
                                    maxOffset = 300f;

                                Vector2 spawnOffset = (MathHelper.Pi + Main.rand.NextFloatDirection() * 0.2f).ToRotationVector2() * Main.rand.NextFloatDirection() * maxOffset;
                                Vector2 sliceVelocity = spawnOffset.SafeNormalize(Vector2.UnitY) * 0.1f;
                                Projectile.NewProjectile(Projectile.GetSource_FromThis(), reflected.Center + spawnOffset, sliceVelocity, ModContent.ProjectileType<KatanaSlashEffect>(), Projectile.damage, 0f, Projectile.owner);
                            }
                        }
                    }
                }
            }
        }
    }
}