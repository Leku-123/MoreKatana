using Microsoft.Xna.Framework;
using MoreKatana.Particles;
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
        private ref float Charge => ref Projectile.ai[0];
        private const float ChargeMax = 60;
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

            if (Charge < ChargeMax)
                Charge++;

            if (Charge == ChargeMax)
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
                float damageMultiplier = 1 + 2 * (Charge / ChargeMax);
                int damage = (int)(Projectile.damage * damageMultiplier);
                Projectile.NewProjectile(Owner.GetSource_ItemUse(Owner.ActiveItem()), Owner.Center, Owner.SafeDirectionTo(Owner.MKPlayer().MouseWorld), swing, damage, Projectile.knockBack, Owner.whoAmI, ai0);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // 別途UIを作りたい...気もする
            if (Projectile.owner == Main.myPlayer)
            {
                Vector2 gaugePos = Owner.Center + new Vector2(0, 50);

                Color color = Color.Gold;
                if (Charge >= ChargeMax - 5 && Charge < ChargeMax)
                    color = Color.White with { A = 0 };

                DrawGauge(gaugePos, Charge / ChargeMax, color, size: new Vector2(60, 10));
            }

            return false;
        }
    }

    public class KatanaSwing : CustomSword
    {
        public bool Reflected;
        public bool ExecuteReflection;
        public int ReflectedIndex = -1;

        public bool ReflectionCheck(Projectile p)
            => p.active && p.hostile && p.damage > 0 && p.velocity.Length() > 1
            && Projectile.Colliding(Projectile.Hitbox, p.Hitbox)
            && !Reflected;

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(Reflected);
            writer.Write(ExecuteReflection);
            writer.Write(ReflectedIndex);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            Reflected = reader.ReadBoolean();
            ExecuteReflection = reader.ReadBoolean();
            ReflectedIndex = reader.ReadInt32();
        }

        public override void Initialize(int type)
        {
            Projectile.localNPCHitCooldown = -1; // 1振りで同じターゲットに2回ヒットしないようにする
            SwingEllipse = new(1f, 0.7f);
            GetTextureValues();
            if (type == 1)
                ImpactCharge = 5;
        }

        public override float GetProgress(int type) => GeneralSwingAnimation(Progress);

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
                        Reflected = true; // 反射のフラグを立てる
                        ReflectedIndex = p.whoAmI; // 反射する発射体のインデックスを保存
                        Projectile.netUpdate = true;

                        // 一応これらをやっておく
                        p.position = p.oldPosition;
                        p.netUpdate = true;

                        // ImpactChargeを処理
                        ImpactChargeLaunch();

                        // スクリーンシェイクを止める
                        Owner.ScreenShake(0, 0);
                    }

                    if (ReflectedIndex != -1)
                    {
                        // ImpactChargeの終了する時に反射実行のフラグを立てる
                        if (HitTimer <= 1)
                        {
                            ExecuteReflection = true;
                            Projectile.netUpdate = true;
                        }

                        Projectile reflected = Main.projectile[ReflectedIndex];

                        if (!ExecuteReflection) // 反射の実行前
                        {
                            // 反射する対象発射体を固定する
                            reflected.position = reflected.oldPosition;

                            // プレイヤーの位置を固定する
                            Owner.position = Owner.oldPosition;
                            NetMessage.SendData(MessageID.PlayerControls, number: Owner.whoAmI);
                        }
                        else // 反射を実行
                        {
                            // 発射体のオーナーを設定する
                            reflected.hostile = false;
                            reflected.friendly = true;
                            reflected.owner = Projectile.owner;

                            // 発射体のベロシティを設定する
                            reflected.velocity = Projectile.velocity.Normalized() * reflected.velocity.Length();

                            // ダメージ
                            reflected.damage = Projectile.damage;

                            // 一応これもやっとく
                            reflected.netUpdate = true;

                            // 変数を初期化する
                            ExecuteReflection = false;
                            ReflectedIndex = -1;
                            Projectile.netUpdate = true;

                            // プレイヤーに反動を付ける
                            Owner.velocity = Projectile.velocity.Normalized();
                            Owner.velocity *= -5f;
                            NetMessage.SendData(MessageID.PlayerControls, number: Owner.whoAmI);

                            // スクリーンシェイク
                            Owner.ScreenShake(5, 20);

                            // サウンド
                            SoundEngine.PlaySound(MoreKatanaSounds.Parry, Owner.position);

                            // ダスト
                            for (int i = 0; i < 5; i++)
                            {
                                int newDust = Dust.NewDust(new Vector2(reflected.position.X, reflected.position.Y + 2f), reflected.width, reflected.height, DustID.GemDiamond, reflected.velocity.X * 0.2f, reflected.velocity.Y * 0.2f, 100, default, 3f);
                                Main.dust[newDust].noGravity = true;
                            }

                            // パーティクル
                            for (int i = 0; i < 2; i++)
                            {
                                Particle glowSpark = new GlowSparkParticle(Projectile.Center, new Vector2(0.1f, 0.1f).RotatedByRandom(100), false, 15, Main.rand.NextFloat(0.05f, 0.09f), Main.rand.NextBool() ? Color.Silver : Color.DimGray, new Vector2(2f, 0.5f), true);
                                ParticleHandler.SpawnParticle(glowSpark);
                            }
                        }
                    }
                }
            }
        }
    }
}