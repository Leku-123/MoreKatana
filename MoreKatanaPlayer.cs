using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MoreKatana.Dusts;
using MoreKatana.Items.Weapons.Misc;
using MoreKatana.Items.Weapons.TerraKatanaTree;
using MoreKatana.Projectiles.TerraKatanaTree;
using MoreKatana.UI;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace MoreKatana
{
    public class MoreKatanaPlayer : ModPlayer
    {
        // -------- Dash --------
        public bool DashState;
        public bool GeneralDash;
        public bool SuddenStop;
        public int DashDistance;
        public float DashTimer;
        public float DashTimerMax;
        public Vector2 DashDirection, DashStartPos, DashEndPos;

        // -------- Cooldown --------
        public int CounterattackCD;
        public int ShieldCD;

        public int ScreenShakeTimer;
        public int ScreenShakeStrength;
        public float Flipping;
        public float R, G, B, A;
        public int TimePotionSick;
        public int EnchantedKatanaShrineEffect;
        public int EnchantedKatanaShrineMusicOverride;

        public bool muramasaCounterattack;
        public bool enchantedHurtEffect;
        public bool holyShield;
        public int HolyShieldDurability;
        public bool trueHolyShield;
        public int TrueHolyShieldDurability;
        public bool terraShield;
        public int TerraShieldDurability;

        public override void OnEnterWorld()
        {
            Flipping = 0f;
        }

        public override void ResetEffects()
        {
            DashState = false;
            if (!GeneralDash)
                DashTimer = 0f;
            if (ScreenShakeTimer > 0)
                ScreenShakeTimer--;
            Flipping = 0f;
            R = G = B = A = 1f;
            muramasaCounterattack = false;
            enchantedHurtEffect = false;
            holyShield = false;
            trueHolyShield = false;
        }

        public override void UpdateDead()
        {
            ResetEffects();
            GeneralDash = false;
            ShieldCD = 0;
            HolyShieldDurability = 0;
            TrueHolyShieldDurability = 0;
        }

        public override void ModifyScreenPosition()
        {
            // スクリーンを揺らす
            if (ScreenShakeTimer > 0)
            {
                Main.screenPosition.Y += Main.rand.Next(-ScreenShakeStrength, ScreenShakeStrength) * MoreKatanaConfig.Instance.ScreenShakePower;
                Main.screenPosition.X += Main.rand.Next(-ScreenShakeStrength, ScreenShakeStrength) * MoreKatanaConfig.Instance.ScreenShakePower;
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

            /*if (Gold)
            {
                long coin = Utils.CoinsCount(out bool over, Player.inventory);
                int bonus = 0;
                if (over || coin >= 10000)
                    bonus = 10;
                else if (coin > 999)
                    bonus = (int)(coin /= 1000);
                Player.statDefense += 3 + bonus;
            }*/
        }

        public override void PostUpdateMiscEffects()
        {
            if (CounterattackCD > 0)
                CounterattackCD--;
            if (ShieldCD > 0)
                ShieldCD--;

            if (EnchantedKatanaShrineEffect > 0)
            {
                Player.dontStarveShader = true;
                Vector2 screenCenter = Main.screenPosition + new Vector2(Main.screenWidth / 2, Main.screenHeight / 2);
                Vector2 startingPosition = new Vector2(Main.rand.NextFloat(screenCenter.X - Main.screenWidth / 2, screenCenter.X + Main.screenWidth / 2), screenCenter.Y - Main.screenHeight / 2);

                if (Main.rand.NextBool(20))
                {
                    if (Main.netMode != NetmodeID.Server)
                        Gore.NewGore(Player.GetSource_FromThis(), startingPosition, Vector2.Zero, GoreID.TreeLeaf_VanityTreeSakura, 1f);
                }

                EnchantedKatanaShrineEffect--;
            }
            if (EnchantedKatanaShrineMusicOverride > 0)
                EnchantedKatanaShrineMusicOverride--;
        }

        public override void PostUpdateRunSpeeds()
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
            }
        }

        public override void PostUpdate()
        {
            if (ShieldCD == 1)
                SoundEngine.PlaySound(SoundID.MaxMana, Player.position);
            if (ShieldCD <= 0)
            {
                // それぞれのシールドの耐久値を適用する

                if (HolyShieldDurability == 0)
                    HolyShieldDurability = SacredNaginata.ShieldDurabilityMax;

                if (TrueHolyShieldDurability == 0)
                    TrueHolyShieldDurability = TrueSacredNaginata.ShieldDurabilityMax;
            }

            if (TimePotionSick == 1)
            {
                ShieldCD = 0;
                HolyShieldDurability = SacredNaginata.ShieldDurabilityMax;
                TrueHolyShieldDurability = TrueSacredNaginata.ShieldDurabilityMax;
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

        public override void OnExtraJumpStarted(ExtraJump jump, ref bool playSound)
        {

        }

        public override void ModifyHurt(ref Player.HurtModifiers modifiers)
        {
            if (holyShield && HolyShieldDurability > 0)
            {
                // ヒットした際に音を鳴らす。デフォルトのヒット音は消す
                modifiers.DisableSound();
                SoundEngine.PlaySound(SoundID.NPCHit42 with { Pitch = +0.3f }, Player.position);
                SoundEngine.PlaySound(SoundID.NPCHit4, Player.position);

                // パーティクル
                ParticleOrchestraSettings particleOrchestraSettings = default;
                particleOrchestraSettings.PositionInWorld = Main.rand.NextVector2FromRectangle(Player.Hitbox);
                ParticleOrchestrator.RequestParticleSpawn(false, ParticleOrchestraType.Excalibur, particleOrchestraSettings, Player.whoAmI);
            }
            if (trueHolyShield && TrueHolyShieldDurability > 0)
            {
                // ヒットした際に音を鳴らす。デフォルトのヒット音は消す
                modifiers.DisableSound();
                SoundEngine.PlaySound(SoundID.NPCHit42 with { Pitch = +0.3f }, Player.position);
                SoundEngine.PlaySound(SoundID.NPCHit4, Player.position);

                // パーティクル
                ParticleOrchestraSettings particleOrchestraSettings = default;
                particleOrchestraSettings.PositionInWorld = Main.rand.NextVector2FromRectangle(Player.Hitbox);
                ParticleOrchestrator.RequestParticleSpawn(false, ParticleOrchestraType.TrueExcalibur, particleOrchestraSettings, Player.whoAmI);
            }
        }

        public override void OnHurt(Player.HurtInfo info)
        {
            if (ShieldCD <= 0)
            {
                if (holyShield && HolyShieldDurability > 0)
                {
                    // すべてのシールドにダメージを与える。
                    int shieldDurability = SacredNaginata.ShieldDurabilityMax;
                    HolyShieldDurability -= info.Damage;
                    TrueHolyShieldDurability -= (int)(info.Damage * ((float)TrueSacredNaginata.ShieldDurabilityMax / shieldDurability));

                    // シールドが破壊された時の処理
                    if (HolyShieldDurability <= 0)
                    {
                        CrashEffect(SacredNaginata.ShieldRechargeTime);

                        double spread = 2 * Math.PI / 12;
                        for (int i = 0; i < 12; i++)
                        {
                            Vector2 velocity = new Vector2(2, 2).RotatedBy(spread * i);
                            int newDust = Dust.NewDust(Player.Center, 0, 0, ModContent.DustType<PixelDust>(), velocity.X, velocity.Y, 0, Color.Gold, 1f);
                            Main.dust[newDust].scale *= 6f * Main.rand.Next(1, 3);
                        }
                    }

                    // ダメージの処理
                    OnDamage(ref info, HolyShieldDurability);
                }
                if (trueHolyShield && TrueHolyShieldDurability > 0)
                {
                    // すべてのシールドにダメージを与える。
                    int shieldDurability = TrueSacredNaginata.ShieldDurabilityMax;
                    TrueHolyShieldDurability -= info.Damage;
                    HolyShieldDurability -= (int)(info.Damage * ((float)SacredNaginata.ShieldDurabilityMax / shieldDurability));

                    // シールドが破壊された時の処理
                    if (TrueHolyShieldDurability <= 0)
                    {
                        CrashEffect(TrueSacredNaginata.ShieldRechargeTime);

                        double spread = 2 * Math.PI / 12;
                        for (int i = 0; i < 24; i++)
                        {
                            Vector2 velocity = new Vector2(2, 2).RotatedBy(spread * i);
                            int newDust = Dust.NewDust(Player.Center, 0, 0, ModContent.DustType<PixelDust>(), velocity.X, velocity.Y, 0, Main.rand.NextBool() ? Color.Gold : Color.Crimson, 1f);
                            Main.dust[newDust].scale *= 6f * Main.rand.Next(1, 3);
                        }
                    }

                    // ダメージの処理
                    OnDamage(ref info, TrueHolyShieldDurability);
                }

                void CrashEffect(int cd)
                {
                    // クールダウンを設ける
                    ShieldCD = cd;

                    // シールドの耐久値を0にする
                    HolyShieldDurability = 0;
                    TrueHolyShieldDurability = 0;

                    // 音とスクリーンシェイク
                    SoundEngine.PlaySound(SoundID.DD2_WitherBeastDeath, Player.position);
                    SoundEngine.PlaySound(SoundID.Item27, Player.position);
                    Player.ScreenShake(5, 10);
                }

                void OnDamage(ref Player.HurtInfo info, int durability)
                {
                    // シールドでどれだけダメージを防いだか計算する
                    int shieldDamageBlocked = Math.Min(durability, info.Damage);

                    // 防いだダメージを表示する
                    string trueHolyShieldDamageText = (-shieldDamageBlocked).ToString();
                    Rectangle location = new Rectangle((int)Player.position.X, (int)Player.position.Y - 16, Player.width, Player.height);
                    CombatText.NewText(location, Color.LightYellow, Language.GetTextValue(trueHolyShieldDamageText));

                    // 実際に被弾のダメージを除去し、後のシールドの被弾を少なくする
                    info.Damage -= shieldDamageBlocked;
                }
            }

            if (enchantedHurtEffect)
            {
                // マナが0なら発動前に中止する
                if (Player.statMana == 0)
                    return;

                // マナでどれだけダメージを肩代わりしたか計算する
                int manaDamageBlocked = int.Max(1, info.Damage / 10);

                // マナが不足している場合はマナの値まで軽減値を減らす
                manaDamageBlocked = int.Min(manaDamageBlocked, Player.statMana);

                // 消費したマナを画面に表示する
                Rectangle location = new Rectangle((int)Player.position.X, (int)Player.position.Y - 16, Player.width, Player.height);
                CombatText.NewText(location, EnchantedKatana.EnchantedDamageColor, -manaDamageBlocked);

                // 肩代わりした分、マナを除去する（ダメージの量にかかわらず、最低1マナを消費する）
                Player.statMana -= manaDamageBlocked;

                // 肩代わりした分をダメージから減算（１ダメージの場合は軽減できない）
                info.Damage -= manaDamageBlocked;
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

        public override void DrawEffects(PlayerDrawSet drawInfo, ref float r, ref float g, ref float b, ref float a, ref bool fullBright)
        {
            r = R; g = G; b = B; a = A;
        }

        public static void AddRenderDrawLayers(ref PlayerDrawSet drawinfo)
        {
            SacredNaginata.DrawHolyShield(ref drawinfo);
            TrueSacredNaginata.DrawTrueHolyShield(ref drawinfo);
            EnchantedShrineCursor.DrawEnchantedShrineCursor(ref drawinfo);
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