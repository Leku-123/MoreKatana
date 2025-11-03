using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MoreKatana.Dusts;
using MoreKatana.Items.Weapons.Misc;
using MoreKatana.Items.Weapons.TerraKatanaTree;
using MoreKatana.Projectiles.Misc;
using MoreKatana.Projectiles.TerraKatanaTree;
using System;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static MoreKatana.MoreKatana;

namespace MoreKatana
{
    public class MoreKatanaPlayer : ModPlayer
    {
        // -------- Timer --------
        public int ExtraJumpTimer;

        // -------- Cooldown --------
        public int ActiveSkillCD;
        public int ActiveSkillCDMax;
        public int CounterattackCD;
        public int ShieldCD;

        // -------- Dash --------
        public bool DashState;
        public bool GeneralDash;
        public bool SuddenStop;
        public int DashDistance;
        public float DashTimer;
        public float DashTimerMax;
        public Vector2 DashDirection, DashStartPos, DashEndPos;

        // -------- Backflip --------
        public bool Rolling;
        public int RollingCount;
        public int RollingDirection;
        public float RollingTimer;
        public float RollingTimerMax;

        // -------- Screen Shake --------
        public int ScreenShakeTimer;
        public int ScreenShakeStrength;

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
        public bool isExtraJumping;
        public bool holyShield;
        public int HolyShieldDurability;
        public bool trueHolyShield;
        public int TrueHolyShieldDurability;
        public bool terraShield;
        public int TerraShieldDurability;

        // -------- Sync --------
        public Vector2 MouseWorld;

        public override void OnEnterWorld()
        {
            Flipping = 0f;
        }

        public override void ResetEffects()
        {
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
            holyShield = false;
            trueHolyShield = false;
            terraShield = false;
            if (Player.velocity.Y == 0)
                isExtraJumping = false;
        }

        public override void UpdateDead()
        {
            ResetEffects();
            ExtraJumpTimer = 0;
            GeneralDash = false;
            Rolling = false;
            NoUsingItems = 0;
            ShieldCD = 0;
            HolyShieldDurability = 0;
            TrueHolyShieldDurability = 0;
            TerraShieldDurability = 0;
            isExtraJumping = false;
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

            // ForgottenAltarのシーン効果
            if (ForgottenAltarEffect > 0)
            {
                Player.dontStarveShader = true; // TO-DO できればシェーダーを自作する
                Vector2 screenCenter = Main.screenPosition + new Vector2(Main.screenWidth / 2, Main.screenHeight / 2);
                Vector2 startingPosition = new Vector2(Main.rand.NextFloat(screenCenter.X - Main.screenWidth / 2, screenCenter.X + Main.screenWidth / 2), screenCenter.Y - Main.screenHeight / 2);

                // 桜の花びらを画面全体に舞わせる
                if (Main.netMode != NetmodeID.Server && Main.rand.NextBool(20))
                    Gore.NewGore(Player.GetSource_FromThis(), startingPosition, Vector2.Zero, GoreID.TreeLeaf_VanityTreeSakura, 1f);

                ForgottenAltarEffect--;
            }
            if (ForgottenAltarMusicOverride > 0)
                ForgottenAltarMusicOverride--;
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

                // マルチプレイヤーでの動きを同期する
                NetMessage.SendData(MessageID.PlayerControls, number: Player.whoAmI);
            }
        }

        public override void PostUpdate()
        {
            if (ActiveSkillCD == 1)
            {
                SoundEngine.PlaySound(MoreKatanaSounds.DrawSword, Player.position);
                Rectangle textPos = new Rectangle((int)Player.position.X, (int)Player.position.Y - 20, Player.width, Player.height);
                CombatText.NewText(textPos, Color.OrangeRed, "Cooldown over!");
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
            // シールドの効果
            void ShieldHurtEffect(ref Player.HurtModifiers modifiers, ParticleOrchestraType type)
            {
                // ヒットした際に音を鳴らす。デフォルトのヒット音は消す
                modifiers.DisableSound();
                SoundEngine.PlaySound(SoundID.NPCHit42 with { Pitch = +0.3f }, Player.position);
                SoundEngine.PlaySound(SoundID.NPCHit4, Player.position);

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

        public override void OnHurt(Player.HurtInfo info)
        {
            if (ShieldCD <= 0)
            {
                if (holyShield && HolyShieldDurability > 0)
                {
                    // すべてのシールドにダメージを与える。
                    ShieldDamage(SacredNaginata.ShieldDurabilityMax);

                    // シールドが破壊された時の処理
                    if (HolyShieldDurability <= 0)
                        CrashEffect(SacredNaginata.ShieldRechargeTime, Color.Gold, Color.Gold);

                    // ダメージの処理
                    OnDamage(ref info, HolyShieldDurability);
                }
                if (trueHolyShield && TrueHolyShieldDurability > 0)
                {
                    // すべてのシールドにダメージを与える。
                    ShieldDamage(TrueSacredNaginata.ShieldDurabilityMax);

                    // シールドが破壊された時の処理
                    if (TrueHolyShieldDurability <= 0)
                        CrashEffect(TrueSacredNaginata.ShieldRechargeTime, Color.Gold, Color.Crimson);

                    // ダメージの処理
                    OnDamage(ref info, TrueHolyShieldDurability);
                }
                if (terraShield && TerraShieldDurability > 0)
                {
                    // すべてのシールドにダメージを与える。
                    ShieldDamage(TerraKatana.ShieldDurabilityMax);

                    // シールドが破壊された時の処理
                    if (TerraShieldDurability <= 0)
                        CrashEffect(TerraKatana.ShieldRechargeTime, TerraKatana.TerraColor[0], TerraKatana.TerraColor[1]);

                    // ダメージの処理
                    OnDamage(ref info, TerraShieldDurability);
                }

                void ShieldDamage(int shieldDurability)
                {
                    // すべてのシールドにダメージを与える。
                    HolyShieldDurability -= (int)(info.Damage * ((float)SacredNaginata.ShieldDurabilityMax / shieldDurability));
                    TrueHolyShieldDurability -= (int)(info.Damage * ((float)TrueSacredNaginata.ShieldDurabilityMax / shieldDurability));
                    TerraShieldDurability -= (int)(info.Damage * ((float)TerraKatana.ShieldDurabilityMax / shieldDurability));
                }

                void CrashEffect(int cd, Color dustColor, Color dustColor2)
                {
                    // クールダウンを設ける
                    ShieldCD = cd;

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

            if (FullBright)
                fullBright = true;
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
        public static void AddRenderDrawLayers(ref PlayerDrawSet drawinfo)
        {
            SacredNaginata.DrawHolyShield(ref drawinfo);
            TrueSacredNaginata.DrawTrueHolyShield(ref drawinfo);
            TerraKatana.DrawTerraShield(ref drawinfo);
        }
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