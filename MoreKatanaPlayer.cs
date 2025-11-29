using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MoreKatana.Assets.ExtraTextures;
using MoreKatana.Dusts;
using MoreKatana.Items.Accessories;
using MoreKatana.Items.Weapons.Misc;
using MoreKatana.Items.Weapons.TerraKatanaTree;
using MoreKatana.Projectiles;
using MoreKatana.Projectiles.Misc;
using MoreKatana.Projectiles.TerraKatanaTree;
using System;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;
using static MoreKatana.MoreKatana;

namespace MoreKatana
{
    public partial class MoreKatanaPlayer : ModPlayer
    {
        // -------- Timer --------
        public int ExtraJumpTimer;

        // -------- Cooldown --------
        public int ActiveSkillCD;
        public int ActiveSkillCDMax = 1;
        public int CounterattackCD;
        public int ShieldCD;

        // -------- Player Effect --------
        public const int Down = 0;
        public const int Up = 1;
        public const int Right = 2;
        public const int Left = 3;
        public bool[] DoubleTap = new bool[4];
        public int[] DoubleTapDelay = new int[4];
        public int[] DoubleTapDrawTimer = new int[4] { -1, -1, -1, -1 };

        public bool FlashAttackDoubleTap;
        public bool BackflipSlashDoubleTap;
        public bool NightDoubleTap;

        public void IsDoubleTap()
        {
            if (Player.controlDown && Player.releaseDown && Player.doubleTapCardinalTimer[Down] < 15)
            {
                DoubleTap[Down] = true;
            }
            else if (Player.controlUp && Player.releaseUp && Player.doubleTapCardinalTimer[Up] < 15)
            {
                DoubleTap[Up] = true;
            }
            else if (Player.controlRight && Player.releaseRight && Player.doubleTapCardinalTimer[Right] < 15 && Player.doubleTapCardinalTimer[Left] == 0)
            {
                DoubleTap[Right] = true;
            }
            else if (Player.controlLeft && Player.releaseLeft && Player.doubleTapCardinalTimer[Left] < 15 && Player.doubleTapCardinalTimer[Right] == 0)
            {
                DoubleTap[Left] = true;
            }
            else
            {
                for (int i = 0; i < DoubleTap.Length; i++)
                    DoubleTap[i] = false;
            }

            FlashAttackDoubleTap = false;
            BackflipSlashDoubleTap = false;
            NightDoubleTap = false;
        }

        public bool DashState;
        public bool GeneralDash;
        public bool SuddenStop;
        public int DashDistance;
        public float DashTimer;
        public float DashTimerMax;
        public Vector2 DashDirection, DashStartPos, DashEndPos;

        public bool Rolling;
        public int RollingCount;
        public int RollingDirection;
        public float RollingTimer;
        public float RollingTimerMax;

        public int slowFallEffect;

        // -------- Screen Shake --------
        public int ScreenShakeTimer;
        public int ScreenShakeStrength;
        public bool ScreenShakeX;
        public bool ScreenShakeY;

        // -------- Player Draw --------
        public float Flipping;
        public float R, G, B, A;
        public bool FullBright;

        // -------- World Effect --------
        public int ForgottenAltarEffect;
        public int ForgottenAltarMusicOverride;

        // -------- Buff --------
        public int TimePotionSick;

        // -------- Item --------
        public int NoUsingItems;

        public bool muramasaCounterattack;
        public bool enchantedHurtEffect;
        public bool skyJumpEffect;
        public bool nightAuraEffect;
        public int nightHitCount;
        public bool holyShield;
        public int HolyShieldDurability;
        public bool trueHolyShield;
        public int TrueHolyShieldDurability;
        public bool terraShield;
        public int TerraShieldDurability;

        // -------- Sync --------
        public Vector2 MouseWorld;

        // -------- Misc --------
        public const int ShieldRechargeTime = 30 * 60;

        public override void OnEnterWorld()
        {
            Flipping = 0f;
        }

        public override void ResetEffects()
        {
            IsDoubleTap();
            DashState = false;
            if (!GeneralDash)
                DashTimer = 0f;
            if (!Rolling)
                RollingTimer = 0f;
            if (ScreenShakeTimer > 0)
                ScreenShakeTimer--;
            if (NoUsingItems > 0)
                NoUsingItems--;
            Flipping = 0f;
            R = G = B = A = 1f;
            FullBright = false;
            muramasaCounterattack = false;
            enchantedHurtEffect = false;
            skyJumpEffect = false;
            if (!nightAuraEffect)
                nightHitCount = 0;
            nightAuraEffect = false;
            holyShield = false;
            trueHolyShield = false;
            terraShield = false;
        }

        public override void UpdateDead()
        {
            ResetEffects();
            ExtraJumpTimer = 0;
            GeneralDash = false;
            Rolling = false;
            slowFallEffect = 0;
            NoUsingItems = 0;
            ShieldCD = 0;
            nightHitCount = 0;
            HolyShieldDurability = 0;
            TrueHolyShieldDurability = 0;
            TerraShieldDurability = 0;
        }

        public override void ModifyScreenPosition()
        {
            // スクリーンを揺らす
            if (ScreenShakeTimer > 0)
            {
                if (ScreenShakeX)
                    Main.screenPosition.X += Main.rand.Next(-ScreenShakeStrength, ScreenShakeStrength) * MoreKatanaConfig.Instance.ScreenShakePower;
                if (ScreenShakeY)
                    Main.screenPosition.Y += Main.rand.Next(-ScreenShakeStrength, ScreenShakeStrength) * MoreKatanaConfig.Instance.ScreenShakePower;
            }
        }

        public override void PreUpdate()
        {
            // 汎用のダッシュのステータス
            if (DashState)
            {
                Player.immune = true;
                Player.immuneTime = 60;
                Player.immuneNoBlink = true;
                Player.noFallDmg = true;
                Player.controlJump = false;
                Player.maxFallSpeed = 2000f;

                // フックとマウントを解除
                Player.RemoveAllGrapplingHooks();
                if (Player.mount.Active)
                    Player.mount.Dismount(Player);
            }

            // プレイヤーの宙返り
            if (Rolling && RollingTimerMax != 0)
            {
                float rollingProgress = RollingTimer / RollingTimerMax;
                if (rollingProgress < 1f)
                {
                    float baseRotation = MathHelper.WrapAngle(rollingProgress * MathHelper.TwoPi * RollingDirection);
                    Player.fullRotation = baseRotation * RollingCount;
                    Player.fullRotationOrigin = Player.Center - Player.position;
                }
                else
                {
                    Player.fullRotation = 0f;
                    Rolling = false;
                    RollingTimer = 0f;
                }

                RollingTimer++;
            }

            // マウス位置の同期
            if (Main.myPlayer == Player.whoAmI)
            {
                MouseWorld = Main.MouseWorld;

                if (Main.netMode == NetmodeID.MultiplayerClient)
                    SyncData(MessageType.MouseWorld, Player.whoAmI, -1, Player.whoAmI);
            }
        }

        public override void PostUpdateMiscEffects()
        {
            // クールダウン
            if (ActiveSkillCD > 0)
                ActiveSkillCD--;
            if (CounterattackCD > 0)
                CounterattackCD--;
            if (ShieldCD > 0)
                ShieldCD--;

            for (int i = 0; i < DoubleTap.Length; i++)
            {
                if (DoubleTap[i] && DoubleTapDelay[i] <= 0)
                    DoubleTapEffects(i);
            }

            for (int i = 0; i < DoubleTapDelay.Length; i++)
            {
                if (DoubleTapDelay[i] == 1)
                {
                    SoundEngine.PlaySound(SoundID.MaxMana, Player.Center);
                    DoubleTapDrawTimer[i] = 0;
                }

                if (DoubleTapDelay[i] > 0)
                    DoubleTapDelay[i]--;
            }

            if (slowFallEffect > 0)
                Player.slowFall = true;
            if (slowFallEffect > 0)
                slowFallEffect--;

            // ForgottenAltarのシーン効果
            if (ForgottenAltarEffect > 0)
            {
                // TO-DO
                // できればシェーダーを自作する
                // スクリーンパーティクルを追加

                Player.dontStarveShader = true;
                Vector2 screenCenter = Main.screenPosition + new Vector2(Main.screenWidth / 2, Main.screenHeight / 2);
                Vector2 startingPosition = new Vector2(Main.rand.NextFloat(screenCenter.X - Main.screenWidth / 2, screenCenter.X + Main.screenWidth / 2), screenCenter.Y - Main.screenHeight / 2);

                // 桜の花びらを画面全体に舞わせる
                if (Main.netMode != NetmodeID.Server && Main.rand.NextBool(20))
                    Gore.NewGore(Player.GetSource_FromThis(), startingPosition, Vector2.Zero, GoreID.TreeLeaf_VanityTreeSakura, 1f);

                ForgottenAltarEffect--;
            }
            if (ForgottenAltarMusicOverride > 0)
                ForgottenAltarMusicOverride--;

            if (enchantedHurtEffect)
            {
                if (Player.manaRegenDelay > 5)
                {
                    int mana = Dust.NewDust(Player.Center, 0, 0, DustID.ManaRegeneration, 0, 0, 100, default, 0.8f);
                    Main.dust[mana].noGravity = true;
                    Main.dust[mana].fadeIn = 1f;
                    Vector2 offsetVector = Utils.NextVector2CircularEdge(Main.rand, 80f, 80f);
                    Main.dust[mana].position = Player.Center - offsetVector;
                    Vector2 newVelocity = Player.Center - Main.dust[mana].position;
                    Main.dust[mana].velocity = newVelocity * 0.1f;
                    Main.dust[mana].velocity += Player.velocity;
                }
                if (Player.manaRegenDelay == 1)
                {
                    SoundEngine.PlaySound(SoundID.Item4, Player.position);
                    MoreKatanaUtil.DrawRing(Player.Center, DustID.ManaRegeneration, 36, 10);
                }
            }
        }

        public void DoubleTapEffects(int trigger)
        {
            Item item = Player.ActiveItem();
            int damage = Player.GetWeaponDamage(item);
            float knockBack = Player.GetWeaponKnockback(item, item.knockBack);
            float scale = Player.GetAdjustedItemScale(item);

            if (trigger == Down)
            {

            }
            if (trigger == Up)
            {
                if (BackflipSlashDoubleTap)
                {
                    if (item.IsAir)
                        return;

                    if (item.MKItem().Katana && !Player.ItemAnimationActive && !Player.mount.Active)
                    {
                        int direction = MouseWorld.X > Player.Center.X ? 1 : -1;
                        float speed = 8f;
                        Vector2 newVelocity = Player.velocity;
                        newVelocity.X = speed * direction;
                        newVelocity.Y = -speed;
                        Player.velocity = newVelocity;
                        NetMessage.SendData(MessageID.PlayerControls, number: Player.whoAmI);

                        float spinTime = 20f;
                        Player.UpdateRotation(2, direction, spinTime);
                        Player.immune = true;
                        Player.immuneTime = 30;
                        Player.immuneNoBlink = true;

                        SoundEngine.PlaySound(MoreKatanaSounds.Spinning, Player.Center);

                        if (Player.whoAmI == Main.myPlayer)
                            Projectile.NewProjectile(Player.GetSource_FromThis(), Player.MountedCenter, new Vector2(direction, 0f), ModContent.ProjectileType<BackflipSlash>(), damage, knockBack, Player.whoAmI, direction * Player.gravDir, spinTime, scale);

                        DoubleTapDelay[Up] = 2 * 60;
                    }
                }
            }
            if (trigger == Right || trigger == Left)
            {
                int direction = trigger == Right ? 1 : -1;

                if (FlashAttackDoubleTap)
                {
                    if (item.IsAir)
                        return;

                    if (item.MKItem().Katana && SecretBook_FlashAttack.ValidItem(item) && !Player.ItemAnimationActive && !Player.mount.Active)
                    {
                        // プレイヤーの向きを変える
                        Player.ChangeDir(direction);

                        // サウンド
                        SoundEngine.PlaySound(MoreKatanaSounds.SwordSlash_2, Player.Center);

                        // ダッシュ切りの発射体
                        if (Player.whoAmI == Main.myPlayer)
                        {
                            float dashDistance = 400;
                            float dashTime = 15;
                            Projectile.NewProjectile(Player.GetSource_ItemUse(item), Player.MountedCenter, new Vector2(direction, 0), ModContent.ProjectileType<GeneralDashSlash>(), damage, knockBack, Player.whoAmI, dashDistance, dashTime);
                        }

                        DoubleTapDelay[Right] = DoubleTapDelay[Left] = 2 * 60;
                    }
                }
                else if (NightDoubleTap)
                {
                    if (!Player.mount.Active)
                    {
                        Player.immune = true;
                        Player.immuneTime = 30;
                        Player.UpdateRotation(1, direction, 15);

                        if (Player.whoAmI == Main.myPlayer)
                        {
                            float dashVelocity = 10f;
                            Vector2 newVelocity = Player.velocity;
                            newVelocity.X = dashVelocity * direction;
                            Player.velocity = newVelocity;
                            NetMessage.SendData(MessageID.PlayerControls, number: Player.whoAmI);
                        }

                        DoubleTapDelay[Right] = DoubleTapDelay[Left] = 2 * 60;
                    }
                }
            }
        }

        public override void PreUpdateMovement()
        {
            // 汎用の簡単なダッシュ
            if (GeneralDash && DashTimerMax != 0)
            {
                // 最初のフレームでダッシュの情報を取得
                if (DashTimer == 0)
                {
                    DashStartPos = Player.MountedCenter;
                    DashDirection.Normalize();
                    DashDirection *= DashDistance;
                    DashEndPos = DashStartPos + DashDirection;
                }

                // 現在の進行度と次のフレームの進行度
                float currentProgress = DashTimer / DashTimerMax;
                float nextProgress = (DashTimer + 1) / DashTimerMax;

                if (currentProgress < 1f)
                {
                    // それぞれのフレームごとの始点から終点までのLerpを取得
                    Vector2 currentPoint = Vector2.Lerp(DashStartPos, DashEndPos, currentProgress);
                    Vector2 nextPoint = Vector2.Lerp(DashStartPos, DashEndPos, nextProgress);

                    // フレームの差でベロシティを計算する
                    Player.velocity = nextPoint - currentPoint;
                }
                else
                {
                    // 変数の初期化
                    GeneralDash = false;
                    DashTimer = 0f;
                    DashTimerMax = 0f;

                    // ダッシュ後に止めるならベロシティを修正する
                    if (SuddenStop)
                        Player.velocity = Vector2.Zero;
                }

                // タイマーを増加
                DashTimer++;

                // マルチプレイヤーでの動きを同期する
                //NetMessage.SendData(MessageID.PlayerControls, number: Player.whoAmI);
            }
        }

        public override void PostUpdate()
        {
            if (ActiveSkillCD == 1)
            {
                SoundEngine.PlaySound(MoreKatanaSounds.DrawSword, Player.position);
                Rectangle textPos = new Rectangle((int)Player.position.X, (int)Player.position.Y - 20, Player.width, Player.height);
                CombatText.NewText(textPos, Color.OrangeRed, MoreKatanaUtil.GetTextValue("Tooltips.CooldownOver"));
            }

            if (ShieldCD == 1)
                SoundEngine.PlaySound(SoundID.MaxMana, Player.position);
            if (ShieldCD <= 0)
            {
                // それぞれのシールドの耐久値を適用する

                if (HolyShieldDurability == 0)
                    HolyShieldDurability = SacredNaginata.ShieldDurabilityMax;

                if (TrueHolyShieldDurability == 0)
                    TrueHolyShieldDurability = TrueSacredNaginata.ShieldDurabilityMax;

                if (TerraShieldDurability == 0)
                    TerraShieldDurability = TerraKatana.ShieldDurabilityMax;
            }

            if (TimePotionSick == 1)
            {
                ShieldCD = 0;
                HolyShieldDurability = SacredNaginata.ShieldDurabilityMax;
                TrueHolyShieldDurability = TrueSacredNaginata.ShieldDurabilityMax;
                TerraShieldDurability = TerraKatana.ShieldDurabilityMax;
            }
        }

        public override void PostUpdateBuffs()
        {
            if (Player.whoAmI == Main.myPlayer)
            {
                if (Player.potionDelay == 0)
                    TimePotionSick = 0;
                else
                    TimePotionSick++;
            }
        }

        public override void ExtraJumpVisuals(ExtraJump jump)
        {
            if (skyJumpEffect)
            {
                if (jump is FlipperJump)
                    return;

                if (Player.mount.Active)
                    return;

                if (Player.whoAmI == Main.myPlayer)
                {
                    Vector2 velocity = Vector2.UnitY * 10f;
                    int feather = ModContent.ProjectileType<SkyFeather>();
                    int damage = 10;

                    if (jump is SandstormInABottleJump)
                    {
                        const int SandstormJumpTime = 60;
                        if (ExtraJumpTimer > SandstormJumpTime / SkyKatana.FeatherCountInExtraJump)
                            ExtraJumpTimer = 0;

                        if (ExtraJumpTimer == 0)
                        {
                            SoundEngine.PlaySound(SoundID.Item32, Player.position);
                            Projectile.NewProjectile(Player.GetSource_FromThis(), Player.Bottom, velocity, feather, damage, 0f, Player.whoAmI, -1);
                        }
                    }
                    else
                    {
                        if (ExtraJumpTimer != -1)
                        {
                            SoundEngine.PlaySound(SoundID.Item32, Player.position);

                            ExtraJumpTimer = -1;
                            for (int i = 0; i < SkyKatana.FeatherCountInExtraJump; i++)
                            {
                                Vector2 newVelocity = velocity.RotatedByRandom(MathHelper.ToRadians(60));
                                newVelocity *= 1f - Main.rand.NextFloat(0.3f);
                                Projectile.NewProjectile(Player.GetSource_FromThis(), Player.Bottom, newVelocity, feather, damage, 0f, Player.whoAmI, -1);
                            }
                        }
                    }
                }
            }

            if (ExtraJumpTimer >= 0)
                ExtraJumpTimer++;
        }

        public override void OnExtraJumpEnded(ExtraJump jump)
        {
            ExtraJumpTimer = 0;
        }

        public override void ModifyHurt(ref Player.HurtModifiers modifiers)
        {
            // シールドに対応するにはこれしかなかった
            modifiers.ModifyHurtInfo += ModifyHurtInfo;

            // シールドの効果
            void ShieldHurtEffect(ref Player.HurtModifiers modifiers, ParticleOrchestraType type)
            {
                // ヒットした際に音を鳴らす。デフォルトのヒット音は消す
                SoundEngine.PlaySound(SoundID.NPCHit42 with { Pitch = +0.3f }, Player.position);
                SoundEngine.PlaySound(SoundID.NPCHit4, Player.position);
                modifiers.DisableSound();

                // パーティクル
                ParticleOrchestraSettings particleOrchestraSettings = default;
                particleOrchestraSettings.PositionInWorld = Main.rand.NextVector2FromRectangle(Player.Hitbox);
                ParticleOrchestrator.RequestParticleSpawn(false, type, particleOrchestraSettings, Player.whoAmI);
            }

            if (holyShield && HolyShieldDurability > 0)
                ShieldHurtEffect(ref modifiers, ParticleOrchestraType.Excalibur);

            if (trueHolyShield && TrueHolyShieldDurability > 0)
                ShieldHurtEffect(ref modifiers, ParticleOrchestraType.TrueExcalibur);

            if (terraShield && TerraShieldDurability > 0)
                ShieldHurtEffect(ref modifiers, ParticleOrchestraType.TerraBlade);
        }

        private void ModifyHurtInfo(ref Player.HurtInfo info)
        {
            if (enchantedHurtEffect)
            {
                if (Player.statMana == 0 || Player.manaRegenDelay > 0)
                    return;

                // マナでどれだけダメージを防いだか計算する
                // 最大で被ダメの半分まで防ぐことができる
                int manaDamageBlocked = Math.Min(info.Damage / 2, Player.statMana);

                // 防いだダメージを表示する
                Rectangle location = new Rectangle((int)Player.position.X, (int)Player.position.Y - 16, Player.width, Player.height);
                CombatText.NewText(location, EnchantedKatana.EnchantedDamageColor, -manaDamageBlocked);

                // マナを消費する
                Player.CheckMana(manaDamageBlocked, true, true);
                Player.manaRegenDelay = EnchantedKatana.EnchantedHurtCD;

                // 実際の被弾のダメージを軽減する
                info.Damage -= manaDamageBlocked;
            }

            if (ShieldCD <= 0)
            {
                void ShieldEffect(ref Player.HurtInfo info, int durabilityMax, int durability, Color dustColor, Color dustColor2)
                {
                    // すべてのシールドにダメージを与える
                    HolyShieldDurability -= (int)(info.Damage * ((float)SacredNaginata.ShieldDurabilityMax / durabilityMax));
                    TrueHolyShieldDurability -= (int)(info.Damage * ((float)TrueSacredNaginata.ShieldDurabilityMax / durabilityMax));
                    TerraShieldDurability -= (int)(info.Damage * ((float)TerraKatana.ShieldDurabilityMax / durabilityMax));

                    // シールドでどれだけダメージを防いだか計算する
                    int shieldDamageBlocked = Math.Min(durability, info.Damage);

                    // 防いだダメージを表示する
                    Rectangle location = new Rectangle((int)Player.position.X, (int)Player.position.Y - 16, Player.width, Player.height);
                    CombatText.NewText(location, Color.LightYellow, -shieldDamageBlocked);

                    // 実際の被弾のダメージを軽減する
                    info.Damage -= shieldDamageBlocked;
                }

                if (holyShield && HolyShieldDurability > 0)
                    ShieldEffect(ref info, SacredNaginata.ShieldDurabilityMax, HolyShieldDurability, Color.Gold, Color.Gold);

                if (trueHolyShield && TrueHolyShieldDurability > 0)
                    ShieldEffect(ref info, TrueSacredNaginata.ShieldDurabilityMax, TrueHolyShieldDurability, Color.Gold, Color.Crimson);

                if (terraShield && TerraShieldDurability > 0)
                    ShieldEffect(ref info, TerraKatana.ShieldDurabilityMax, TerraShieldDurability, TerraKatana.TerraColor[0], TerraKatana.TerraColor[1]);
            }
        }

        public override void OnHurt(Player.HurtInfo info)
        {
            if (nightAuraEffect)
                nightHitCount -= Math.Min(2, nightHitCount);

            if (ShieldCD <= 0)
            {
                void CrashEffect(Color dustColor, Color dustColor2)
                {
                    // クールダウンを設ける
                    ShieldCD = ShieldRechargeTime;

                    // シールドの耐久値を0にする
                    HolyShieldDurability = 0;
                    TrueHolyShieldDurability = 0;
                    TerraShieldDurability = 0;

                    // 音とスクリーンシェイク
                    SoundEngine.PlaySound(SoundID.DD2_WitherBeastDeath, Player.position);
                    SoundEngine.PlaySound(SoundID.Item27, Player.position);
                    Player.ScreenShake(5, 10);

                    // ダスト
                    double spread = 2 * Math.PI / 12;
                    for (int i = 0; i < 12; i++)
                    {
                        Vector2 velocity = new Vector2(2, 2).RotatedBy(spread * i);
                        int newDust = Dust.NewDust(Player.Center, 0, 0, ModContent.DustType<PixelDust>(), velocity.X, velocity.Y, 0, Main.rand.NextBool() ? dustColor : dustColor2, 1f);
                        Main.dust[newDust].scale *= 6f * Main.rand.Next(1, 3);
                    }
                }

                if (holyShield && HolyShieldDurability <= 0)
                    CrashEffect(Color.Gold, Color.Gold);

                if (trueHolyShield && TrueHolyShieldDurability <= 0)
                    CrashEffect(Color.Gold, Color.Crimson);

                if (terraShield && TerraShieldDurability <= 0)
                    CrashEffect(TerraKatana.TerraColor[0], TerraKatana.TerraColor[1]);
            }
        }

        public override void OnHitByNPC(NPC npc, Player.HurtInfo hurtInfo)
        {
            OnHitByEither(npc, null, hurtInfo);
        }

        public override void OnHitByProjectile(Projectile proj, Player.HurtInfo hurtInfo)
        {
            OnHitByEither(null, proj, hurtInfo);
        }

        public void OnHitByEither(NPC npc, Projectile proj, Player.HurtInfo hurtInfo)
        {
            if (muramasaCounterattack)
            {
                if (CounterattackCD <= 0)
                {
                    CounterattackCD = 120;

                    if (npc != null)
                    {
                        if (Player.whoAmI == Main.myPlayer)
                        {
                            Vector2 vector = Vector2.Normalize(npc.Center - Player.Bottom) * 10f;
                            int damage = Player.GetWeaponDamage(Player.HeldItem);
                            float knockBack = Player.GetWeaponKnockback(Player.HeldItem, Player.HeldItem.knockBack);
                            Projectile.NewProjectile(Player.GetSource_FromThis(), Player.Center, vector, ModContent.ProjectileType<MuramasaCounterattack>(), damage, knockBack, Main.myPlayer);

                            int dir = -1;
                            if (npc.position.X + (npc.width / 2) < Player.position.X + (Player.width / 2))
                                dir = 1;
                            Player.ApplyDamageToNPC(npc, damage, knockBack, -dir, false);
                        }
                    }
                }
            }
        }

        public override void DrawEffects(PlayerDrawSet drawInfo, ref float r, ref float g, ref float b, ref float a, ref bool fullBright)
        {
            r = R; g = G; b = B; a = A;

            if (FullBright)
                fullBright = true;

            for (int i = 0; i < DoubleTapDrawTimer.Length; i++)
            {
                float progress = DoubleTapDrawTimer[i] / 15f;

                if (DoubleTapDrawTimer[i] != -1 && progress != 1)
                {
                    Texture2D texture = MoreKatanaTextures.DirectionalTexture[i].Value;

                    Vector2 position = drawInfo.Center - Main.screenPosition;
                    position = new Vector2((int)position.X, (int)position.Y);

                    Color color = Color.White with { A = 0 };
                    color *= 1 - progress;
                 
                    if (drawInfo.shadow == 0f)
                        drawInfo.DrawDataCache.Add(new DrawData(texture, position, null, color, 0f, texture.Size() / 2, 1f + (1f * progress), SpriteEffects.None, 0));

                    DoubleTapDrawTimer[i]++;
                }
            }
        }

        public static void SyncMouseWorld(Mod mod, BinaryReader reader, int whoAmI)
        {
            byte player = reader.ReadByte();
            MoreKatanaPlayer mk = Main.player[player].MKPlayer();
            Vector2 vector = reader.ReadVector2();
            bool rightClick = reader.ReadBoolean();
            mk.MouseWorld = vector;
            Main.player[player].controlUseTile = rightClick;
            if (Main.netMode == NetmodeID.Server)
                SyncData(MessageType.MouseWorld, player, -1, player);
        }

        // ここらへんはもうちょっと良い方法を考える
        public static void AddRenderUI(SpriteBatch spriteBatch, Player player)
        {
            //ForgottenAltarDisplay.Draw(spriteBatch, player);
            //ForgottenAltarCursor.Draw(spriteBatch, player);
        }

        public static DrawData ManipulateDrawInfo(DrawData input, Player player)
        {
            float rotation = player.MKPlayer().Flipping;
            if (rotation != 0)
            {
                float sin = (float)Math.Sin(rotation - MathHelper.PiOver2);
                int off = Math.Abs((int)((input.useDestinationRectangle ? input.destinationRectangle.Width : input.sourceRect?.Width ?? input.texture.Width) * sin));

                SpriteEffects effect = sin > 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
                if (input.effect == SpriteEffects.FlipHorizontally)
                    effect = effect == SpriteEffects.FlipHorizontally ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

                var newRect = new Rectangle((int)input.position.X, (int)input.position.Y, off, input.useDestinationRectangle ? input.destinationRectangle.Height : input.sourceRect?.Height ?? input.texture.Height);
                var newData = new DrawData(input.texture, newRect, input.sourceRect, input.color, input.rotation, input.origin, effect, 0)
                {
                    shader = input.shader
                };

                return newData;
            }
            else
            {
                return input;
            }
        }
    }
}