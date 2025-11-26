using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MoreKatana.Items.Weapons.TerraKatanaTree;
using MoreKatana.Projectiles.Base;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using static MoreKatana.MoreKatanaUtil;

namespace MoreKatana.Projectiles.TerraKatanaTree
{
    public class NightKatanaSwing : CustomSword
    {
        public override void Initialize(int type)
        {
            Projectile.localNPCHitCooldown = -1;

            float x = Main.rand.NextFloat(1f, 1.3f);
            float y = Main.rand.NextFloat(0.7f, 0.9f);
            SwingEllipse = new(x, y);

            Owner.ScreenShake(2, 10);

            GetTextureValues();
        }

        public SwingData Down => new SwingData(OwnerItem.useAnimation, Main.rand.NextFloat(0.7f, 0.8f)); // 1振り目
        public SwingData Up => new SwingData(OwnerItem.useAnimation, Main.rand.NextFloat(0.7f, 0.8f), backspin: true); // 2振り目
        public override SwingData GetSwingData(int type) => SwingData.SwingRegister(type, Down, Up);

        public override float GetProgress(int type) => GeneralSwingAnimation(Progress);

        public override void AdditionalAI(int type, bool delay)
        {
            Owner.SetDummyItemTime(2);

            if (type == 1 && !Projectile.MKProj().Bool[0])
            {
                Projectile.MKProj().Bool[0] = true;

                if (Main.myPlayer == Projectile.owner)
                {
                    Vector2 spawnPos = Owner.Center;
                    const int max = 3;
                    for (int i = 1; i <= max; i++)
                    {
                        spawnPos += Vector2.Normalize(Projectile.velocity) * 200f / max;
                        int p = Projectile.NewProjectile(Projectile.GetSource_FromThis(), spawnPos, Vector2.Zero, ModContent.ProjectileType<KatanaSlashEffect>(), Projectile.damage / 2, 0f, Projectile.owner);
                        KatanaSlashEffect slash = (KatanaSlashEffect)Main.projectile[p].ModProjectile;
                        slash.slashColor = Color.Indigo;
                    }
                }
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            base.OnHitNPC(target, hit, damageDone);

            if (!Projectile.MKProj().Bool[1])
            {
                Projectile.MKProj().Bool[1] = true;
                if (Owner.MKPlayer().nightHitCount < NightKatana.MaxHitCount)
                    Owner.MKPlayer().nightHitCount++;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Item[OwnerItem.type].Value;
            Vector2 position = Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);
            Color glowColor = Color.White * Projectile.Opacity;
            Color trailColor = Color.Indigo * Projectile.Opacity;

            if (Progress != 1f)
                DrawBackglow(texture, position, null, glowColor with { A = 0 }, Projectile.rotation, 4f * (1 - Progress), new Vector2(Projectile.scale), SwingEffectsHV());

            DrawBasicSword(texture, Projectile.Center);

            // 剣先にスパークルを描画する
            Vector2 offset = Utils.DirectionTo(Owner.MountedCenter, Projectile.Center) * 40f * Projectile.scale;
            DrawPrettyStarSparkle(1f, SpriteEffects.None, position + offset, glowColor * (1 - Progress), trailColor * (1 - Progress),
                    0.5f, 0f, 0.1f, 0.9f, 1f, 0f, new Vector2(Projectile.scale, Projectile.scale * 2.5f), new Vector2(1f, 1f));

            return false;
        }
    }

    //public class NightKatanaSwing : CustomSword
    //{
    //    public override void Initialize(int type)
    //    {
    //        Projectile.localNPCHitCooldown = -1;
    //        Projectile.MKProj().Bool[0] = false;

    //        SwingEllipse = new(1.4f, 0.9f);

    //        Owner.ScreenShake(2, 10);

    //        // アイテムのスイングのコンボを管理する
    //        OwnerItem.MKItem().AttackType = SwingType;

    //        GetTextureValues();
    //    }

    //    public override SwingData GetSwingData(int type) => new SwingData(OwnerItem.useAnimation, 0.8f, backspin: type % 2 != 0);

    //    public override float GetProgress(int type) => GeneralSwingAnimation(Progress);

    //    public override void AdditionalAI(int type, bool delay)
    //    {
    //        // プレイヤーのアイテム使用時間を延長する
    //        Owner.SetDummyItemTime(2);

    //        // コンボが一定数あれば追加攻撃が可能になる
    //        if (GetProgress(type) > 0.1f && type >= NightKatana.MaxComboCount)
    //        {
    //            // クリックされた場合
    //            if (Main.mouseLeft && Main.mouseLeftRelease)
    //            {
    //                // サイクロンスラッシュ!!! ;)
    //                if (Projectile.owner == Main.myPlayer)
    //                {
    //                    int cyclone = ModContent.ProjectileType<NightCycloneSlash>();
    //                    Projectile.NewProjectile(Owner.GetSource_ItemUse(Owner.ActiveItem()), Owner.Center, new Vector2(Owner.direction, 0), cyclone, Projectile.damage, Projectile.knockBack, Projectile.owner);
    //                }

    //                // 発射体を削除
    //                Projectile.Kill();
    //            }
    //        }
    //    }

    //    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    //    {
    //        base.OnHitNPC(target, hit, damageDone);

    //        // コンボのタイマーを更新
    //        Owner.MKPlayer().NightComboTimer = NightKatana.MaxComboTime;

    //        // アイテムのスイングのコンボを増加
    //        if (SwingType < NightKatana.MaxComboCount)
    //            OwnerItem.MKItem().AttackType = SwingType + 1;

    //        // コンボテキスト
    //        if (!Projectile.MKProj().Bool[0])
    //        {
    //            Projectile.MKProj().Bool[0] = true;
    //            SoundEngine.PlaySound(SoundID.Tink, Owner.position);

    //            // テキストの位置
    //            Rectangle textPos = new Rectangle((int)Owner.position.X, (int)Owner.position.Y - 20, Owner.width, Owner.height);

    //            // テキストの色を滑らかに変化させる
    //            Color color = Color.Lerp(Color.Purple, Color.Purple with { A = 0 }, (float)SwingType / NightKatana.MaxComboCount);

    //            // テキストは最大コンボ時でなければカウントする
    //            string text = GetTextValue("Tooltips.MaxCombo");
    //            if (SwingType < NightKatana.MaxComboCount)
    //                text = SwingType + 1 + GetTextValue("Tooltips.Combo");
    //            CombatText.NewText(textPos, color, text, true, true);
    //        }
    //    }

    //    public override bool PreDraw(ref Color lightColor)
    //    {
    //        Texture2D texture = TextureAssets.Item[OwnerItem.type].Value;
    //        Vector2 position = Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);
    //        Color glowColor = Color.White * Projectile.Opacity;
    //        Color trailColor = Color.Violet * Projectile.Opacity;

    //        if (Progress != 1f)
    //            DrawBackglow(texture, position, null, glowColor with { A = 0 }, Projectile.rotation, 4f * (1 - Progress), new Vector2(Projectile.scale), SwingEffectsHV());

    //        DrawBasicSword(texture, Projectile.Center);

    //        // 剣先にスパークルを描画する
    //        Vector2 offset = Utils.DirectionTo(Owner.MountedCenter, Projectile.Center) * 40f * Projectile.scale;
    //        DrawPrettyStarSparkle(1f, SpriteEffects.None, position + offset, glowColor * (1 - Progress), trailColor * (1 - Progress),
    //                0.5f, 0f, 0.1f, 0.9f, 1f, 0f, new Vector2(Projectile.scale, Projectile.scale * 2.5f), new Vector2(1f, 1f));

    //        return false;
    //    }
    //}

    //public class NightCycloneSlash : CustomSword
    //{
    //    private const int SpinAnimationFrames = 5;

    //    public override void Initialize(int type)
    //    {
    //        Projectile.localNPCHitCooldown = -1;
    //        Projectile.MKProj().Bool[0] = false;

    //        if (type == 0) // サイクロンスラッシュ
    //        {
    //            Projectile.localNPCHitCooldown = OwnerItem.useAnimation / 4 * Projectile.MaxUpdates;

    //            SwingEllipse = new(3f, 0.3f);
    //            NoSpeedBonus = true; // 速度ボーナスを適用しない
    //            CreateSound = false; // デフォルトのサウンドを鳴らさない
    //        }
    //        else // スピンスラッシュ
    //        {
    //            SwingEllipse = new(1.1f, 1.1f);
    //            SwingDirection *= Owner.direction;
    //        }

    //        GetTextureValues();
    //    }

    //    // 全てのスイングデータを設定する
    //    public SwingData Cyclone => new SwingData(40, 3.6f, 0.2f, delay: 15); // サイクロンスラッシュ
    //    public SwingData Spin => new SwingData(OwnerItem.useAnimation * 0.8f, 1.5f, 0.2f, Owner.MKPlayer().MouseWorld.X < Owner.Center.X); // スピンスラッシュ
    //    public override SwingData GetSwingData(int type) => SwingData.SwingRegister(type, Cyclone, Spin);

    //    public override float GetProgress(int type)
    //    {
    //        if (type == 0) // サイクロンスラッシュのアニメーション
    //        {
    //            // 実行時
    //            if (Progress != 1f)
    //                return LinearEasing(Progress, 1);
    //            // ディレイ
    //            else
    //                return MathHelper.SmoothStep(1f, 1.01f, DelayProgress);
    //        }
    //        else  // スピンスラッシュのアニメーション
    //            return LinearEasing(Progress, 1);
    //    }

    //    public override void AdditionalAI(int type, bool delay)
    //    {
    //        // プレイヤーのアイテム使用時間を延長する
    //        Owner.SetDummyItemTime(2);

    //        if (type == 0)
    //        {
    //            Owner.FlipEffect(GetProgress(type) * 12f);
    //            Owner.MKPlayer().slowFallEffect = 2;

    //            if (++Projectile.frameCounter >= 4 * Projectile.MaxUpdates)
    //            {
    //                Projectile.frameCounter = 0;
    //                Projectile.frame = ++Projectile.frame % SpinAnimationFrames;
    //            }

    //            if (!delay)
    //            {
    //                Projectile.friendly = true;

    //                if (Projectile.soundDelay <= 0)
    //                {
    //                    Projectile.soundDelay = 10 * Projectile.MaxUpdates;
    //                    SoundEngine.PlaySound(MoreKatanaSounds.SwordSlash, Owner.position);

    //                    Owner.ScreenShake(2, 10);
    //                }
    //            }
    //            else
    //            {
    //                Projectile.friendly = false;

    //                if (Main.mouseLeft && Main.mouseLeftRelease)
    //                {
    //                    Owner.immune = true;
    //                    Owner.immuneTime = 2;

    //                    // 最寄りのNPCがいた場合、NPCの位置にテレポートをする
    //                    NPC target = Owner.Center.ClosestNPCAt(300f);
    //                    if (target != null)
    //                    {
    //                        Owner.Teleport(target.Top, -1);
    //                        NetMessage.SendData(MessageID.TeleportEntity, -1, -1, null, 0, Owner.whoAmI, target.Top.X, target.Top.Y, -1);
    //                    }

    //                    if (Projectile.owner == Main.myPlayer)
    //                    {
    //                        int cyclone = ModContent.ProjectileType<NightCycloneSlash>();
    //                        Projectile.NewProjectile(Owner.GetSource_ItemUse(Owner.ActiveItem()), Owner.Center, Projectile.velocity, cyclone, Projectile.damage * 2, Projectile.knockBack, Projectile.owner, 1);
    //                    }

    //                    Projectile.Kill();
    //                }
    //            }
    //        }
    //        else if (type == 1)
    //        {
    //            Owner.immune = true;
    //            Owner.immuneTime = 2;

    //            if (Owner.yoraiz0rEye < 2)
    //                Owner.yoraiz0rEye = 2;

    //            if (!Projectile.MKProj().Bool[0])
    //            {
    //                Projectile.MKProj().Bool[0] = true;
    //                SoundEngine.PlaySound(MoreKatanaSounds.SwordSlash, Owner.Center);
    //                SoundEngine.PlaySound(MoreKatanaSounds.Parry, Owner.position);

    //                int dir = SwingDirection * Owner.direction;
    //                Owner.ScreenShake(15, 20);
    //                Owner.UpdateRotation(1, dir, SwingTime);

    //                Owner.velocity = Vector2.Zero;
    //                Owner.velocity.X += 8 * dir;
    //                Owner.velocity.Y -= 8;
    //                NetMessage.SendData(MessageID.PlayerControls, number: Owner.whoAmI);

    //                ParticleHandler.SpawnParticle(new PulseCircle(Owner.Center, Vector2.UnitY, Color.Violet, new Vector2(0.8f), 40, CircOutEasing));

    //                for (int i = 0; i < 5; i++)
    //                {
    //                    Particle glowSpark = new GlowSparkParticle(Owner.Center, new Vector2(0.1f, 0.1f).RotatedByRandom(100), false, 15, Main.rand.NextFloat(0.05f, 0.1f), Color.Violet, new Vector2(2f, 0.5f), true);
    //                    ParticleHandler.SpawnParticle(glowSpark);
    //                }

    //                for (int i = 0; i < 20; ++i)
    //                {
    //                    int newDust = Dust.NewDust(Owner.Center, Owner.width, Owner.height, Utils.SelectRandom(Main.rand, DustID.Demonite, DustID.Shadowflame));
    //                    Main.dust[newDust].velocity *= 5f;
    //                    Main.dust[newDust].fadeIn = 1f;
    //                    Main.dust[newDust].scale = 1 + Main.rand.NextFloat() + Main.rand.Next(4) * 0.3f;
    //                    if (Main.rand.NextBool(3))
    //                    {
    //                        Main.dust[newDust].noGravity = true;
    //                        Main.dust[newDust].velocity *= 3f;
    //                        Main.dust[newDust].scale *= 2f;
    //                    }
    //                }
    //            }
    //        }
    //    }

    //    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    //    {
    //        base.OnHitNPC(target, hit, damageDone);

    //        // コンボのタイマーを更新
    //        Owner.MKPlayer().NightComboTimer = NightKatana.MaxComboTime;

    //        // コンボテキスト
    //        if (!Projectile.MKProj().Bool[0])
    //        {
    //            Projectile.MKProj().Bool[0] = true;
    //            SoundEngine.PlaySound(SoundID.Tink, Owner.position);

    //            Rectangle textPos = new Rectangle((int)Owner.position.X, (int)Owner.position.Y - 20, Owner.width, Owner.height);
    //            CombatText.NewText(textPos, Color.Purple with { A = 0 }, GetTextValue("Tooltips.MaxCombo"), true, true);
    //        }
    //    }

    //    public override void DrawTrail(int type)
    //    {
    //        // サイクロンスラッシュでなければトレイルを描画する
    //        if (Timer > 1f && type != 0)
    //        {
    //            if (!PrimsCreated)
    //            {
    //                PrimsCreated = true;
    //                SwordTrail = new CustomSwordPrimTrail(Projectile, TrailColor, SwordLength, (int)(SwingTime * 1.5f));
    //                MoreKatana.primitives.CreateTrail(SwordTrail);
    //            }

    //            UpdateTrail(SwordTrail);
    //        }
    //    }

    //    public override bool PreDraw(ref Color lightColor)
    //    {
    //        Texture2D texture = TextureAssets.Item[OwnerItem.type].Value;
    //        Vector2 position = Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);
    //        Color glowColor = Color.White * Projectile.Opacity;
    //        Color trailColor = Color.Violet * Projectile.Opacity;

    //        if (Progress != 1f && SwingType != 0)
    //            DrawBackglow(texture, position, null, glowColor with { A = 0 }, Projectile.rotation, 4f * (1 - Progress), new Vector2(Projectile.scale), SwingEffectsHV());

    //        DrawBasicSword(texture, Projectile.Center);

    //        if (SwingType == 0)
    //        {
    //            Texture2D spinTex = ModContent.Request<Texture2D>(this.GetTexture("NightKatanaSpin")).Value;
    //            Rectangle spinRect = spinTex.Frame(1, SpinAnimationFrames, 0, Projectile.frame);
    //            Vector2 spinOrigin = spinRect.Size() / 2f;
    //            Vector2 spinPos = Owner.MountedCenter - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);
    //            SpriteEffects spriteEffects_ = Owner.direction == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
    //            Main.spriteBatch.Draw(spinTex, spinPos, spinRect, trailColor with { A = 0 } * (1 - Progress), 0f, spinOrigin, Projectile.scale, spriteEffects_, 0f);
    //        }
    //        else
    //        {
    //            // 剣先にスパークルを描画する
    //            Vector2 offset = Utils.DirectionTo(Owner.MountedCenter, Projectile.Center) * 40f * Projectile.scale;
    //            DrawPrettyStarSparkle(1f, SpriteEffects.None, position + offset, glowColor * (1 - Progress), trailColor * (1 - Progress),
    //                    0.5f, 0f, 0.1f, 0.9f, 1f, 0f, new Vector2(Projectile.scale, Projectile.scale * 2.5f), new Vector2(1f, 1f));
    //        }

    //        return false;
    //    }
    //}
}