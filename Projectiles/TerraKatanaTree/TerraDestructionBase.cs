using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MoreKatana.Assets.ExtraTextures;
using MoreKatana.Items.Weapons.TerraKatanaTree;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace MoreKatana.Projectiles.TerraKatanaTree
{
    public class TerraDestructionBase : ModProjectile
    {
        private NPC Target => Main.npc[(int)Projectile.ai[0]];

        public ref float Timer => ref Projectile.ai[1];

        public ref float SlashCount => ref Projectile.ai[2];

        private const int MaxSlashCount = 5;
        private const int SlashTime = 9;
        private const int AttackRange = TerraDestructionHoldout.AttackRange;

        public Player Owner => Main.player[Projectile.owner];

        public override string Texture => MoreKatana.EmptyTexture;

        public override void SetDefaults()
        {
            Projectile.width = 0;
            Projectile.height = 0;
            Projectile.aiStyle = -1;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 9999;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.hide = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 5;
            Projectile.noEnchantmentVisuals = true;
            Projectile.MKProj().SourceIsItemUse = true;
            Projectile.MKProj().ActivateCD = true;
        }

        public override void AI()
        {
            // 発射体のベロシティを0にする
            Projectile.velocity = Vector2.Zero;

            // ターゲットがいる場合といない場合で発射体の位置を調節する
            if (Projectile.ai[0] != -1 && Target.active)
            {
                // ターゲットの位置にタイルがない場合、発射体の位置をターゲットの中心にする
                if (!Collision.SolidCollision(Target.Center, Target.width / 2, Target.height / 2))
                    Projectile.position = Target.position;

                // そうでなければ発射体の位置をプレイヤーのの位置にする
                // 完全な位置のズレを無くすためoldPositionにする
                else
                    Projectile.position = Owner.oldPosition;
            }
            else
            {
                // 発射体の位置をプレイヤーのの位置にする
                Projectile.position = Owner.oldPosition;
            }

            // プレイヤーの保持する発射体のIDを更新して、プレイヤーの使用時間を延長する
            Owner.heldProj = Projectile.whoAmI;
            Owner.SetDummyItemTime(2);

            // プレイヤーの位置を発射体の位置にしてベロシティをゼロにする
            Owner.position = Projectile.position;
            Owner.velocity = Owner.oldVelocity;

            // プレイヤーの免疫フレームを設定してダメージを受けないようにする
            Owner.immune = true;
            Owner.immuneTime = 120;
            Owner.immuneAlpha = 255; // プレイヤーが透明になる
            Owner.gills = true; // 水中呼吸可能
            Owner.lavaImmune = true; // 溶岩耐性

            // プレイヤーの基本的な動作を制限する
            Owner.controlLeft = false;
            Owner.controlRight = false;
            Owner.controlUp = false;
            Owner.controlDown = false;
            Owner.controlJump = false;
            Owner.controlUseTile = false;
            Owner.controlHook = false;
            Owner.controlMount = false;
            Owner.canRocket = false; // ロケットブーツなどでの飛行をさせない

            // フックとマウントの解除
            Owner.RemoveAllGrapplingHooks();
            if (Owner.mount.Active)
                Owner.mount.Dismount(Owner);

            // デバフを解除
            for (int i = 0; i < Player.MaxBuffs; i++)
            {
                int hasBuff = Owner.buffType[i];
                if (Main.debuff[hasBuff] && !BuffID.Sets.NurseCannotRemoveDebuff[hasBuff])
                    Owner.ClearBuff(hasBuff);
            }

            // 攻撃範囲にダストをスポーンさせる
            for (int i = 0; i < 40; i++)
            {
                Vector2 offset = new Vector2();
                double angle = Main.rand.NextDouble() * 2d * Math.PI;
                offset.X += (float)(Math.Sin(angle) * AttackRange);
                offset.Y += (float)(Math.Cos(angle) * AttackRange);
                int newDust = Dust.NewDust(Owner.Center + offset, 0, 0, TerraKatana.DustType, 0, 0, 100, default, 0.5f);
                Main.dust[newDust].noGravity = true;
                if (Main.rand.NextBool(3))
                    Main.dust[newDust].velocity += Vector2.Normalize(offset) * 5f;
            }

            // SlashTime毎にダメージを与える
            Projectile.friendly = Timer % SlashTime == 0 && SlashCount < MaxSlashCount;
            if (Projectile.friendly)
            {
                Owner.ScreenShake(2, 5);
                SoundEngine.PlaySound(MoreKatanaSounds.SwordSlash_3 with { Pitch = +Main.rand.NextFloat(0.3f) }, Owner.Center);

                Vector2 slashPosition = Projectile.Center; // 斬撃の位置
                Vector2 slashDirection = -Vector2.UnitY; // 斬撃の向き

                // 斬撃ごとに位置と向きを微調整する
                switch (SlashCount)
                {
                    case 0:
                        slashDirection = slashDirection.RotatedBy(MathHelper.ToRadians(45));
                        slashPosition += slashDirection.TurnRight() * 70;
                        break;
                    case 1:
                        slashDirection = slashDirection.RotatedBy(MathHelper.ToRadians(-100));
                        slashPosition += slashDirection.TurnRight() * 90;
                        break;
                    case 2:
                        slashDirection = slashDirection.RotatedBy(MathHelper.ToRadians(120));
                        slashPosition += slashDirection.TurnRight() * 100;
                        break;
                    case 3:
                        slashDirection = slashDirection.RotatedBy(MathHelper.ToRadians(-85));
                        slashPosition += slashDirection.TurnLeft() * 140;
                        break;
                    case 4:
                        slashDirection = slashDirection.RotatedBy(MathHelper.ToRadians(30));
                        slashPosition += slashDirection.TurnLeft() * 150;

                        // 衝撃波のエフェクト
                        MoreKatanaUtil.CreateShockwave(Projectile.GetSource_FromThis(), Owner.Center, Projectile.owner);

                        // サウンドの入りのズレがあるためここで鳴らし始める
                        SoundEngine.PlaySound(MoreKatanaSounds.Thunder, Owner.Center);
                        break;
                    default:
                        break;
                }

                if (Projectile.owner == Main.myPlayer)
                {
                    // 斬撃の発射体を発射する
                    int slash = ModContent.ProjectileType<TerraSlash>();
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), slashPosition, slashDirection, slash, Projectile.damage, Projectile.knockBack, Projectile.owner);

                    // 攻撃範囲内のNPCの免疫フレームを0にして確実にダメージを与えられるようにする
                    for (int i = 0; i < Main.maxNPCs; i++)
                    {
                        if (Main.npc[i].active && Projectile.Distance(Main.npc[i].Center) < AttackRange)
                            Main.npc[i].immune[Projectile.owner] = 0;
                    }

                    // ダメージを与える
                    Projectile.Damage();
                }

                // カウントを増加
                SlashCount++;

                Projectile.netUpdate = true;
            }

            // 斬撃がすべて終わったら発射体を削除する
            if (Timer > SlashTime * MaxSlashCount)
            {
                Projectile.Kill();
                return;
            }

            // タイマーを増加
            Timer++;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            int clampedX = projHitbox.Center.X - targetHitbox.Center.X;
            int clampedY = projHitbox.Center.Y - targetHitbox.Center.Y;

            if (Math.Abs(clampedX) > targetHitbox.Width / 2)
                clampedX = targetHitbox.Width / 2 * Math.Sign(clampedX);
            if (Math.Abs(clampedY) > targetHitbox.Height / 2)
                clampedY = targetHitbox.Height / 2 * Math.Sign(clampedY);

            int dX = projHitbox.Center.X - targetHitbox.Center.X - clampedX;
            int dY = projHitbox.Center.Y - targetHitbox.Center.Y - clampedY;

            return Math.Sqrt(dX * dX + dY * dY) <= AttackRange;
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item60 with { Volume = 0.7f }, Owner.Center);
            SoundEngine.PlaySound(SoundID.DD2_DefenseTowerSpawn with { Volume = 0.7f }, Owner.Center);
            SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode with { Volume = 0.7f }, Owner.Center);

            Owner.ScreenShake(20, 25);
            Owner.CreateImpactEffect(Projectile.GetSource_FromThis(), Owner.Center + new Vector2(0, 100), -Vector2.UnitY, Projectile.owner, 2f, TerraKatana.TerraColor[0]);

            // ダスト盛り盛り
            ProduceDust(TerraKatana.DustType);

            if (Projectile.owner == Main.myPlayer)
            {
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    if (Main.npc[i].active && Projectile.Distance(Main.npc[i].Center) < AttackRange)
                        Main.npc[i].immune[Projectile.owner] = 0;
                }

                Projectile.friendly = true;
                Projectile.damage *= 10;
                Projectile.Damage();

                int lightning = ModContent.ProjectileType<TerraLightning>();
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Owner.Center - new Vector2(0, 800), new Vector2(0, 10), lightning, Projectile.damage, 3, Projectile.owner);
            }
        }

        private void ProduceDust(int tyoe)
        {
            for (int i = 0; i < 40; ++i)
            {
                int newDust = Dust.NewDust(Owner.Center, Owner.width, Owner.height, tyoe);
                Main.dust[newDust].velocity *= 10f;
                Main.dust[newDust].fadeIn = 1f;
                Main.dust[newDust].scale = 1 + Main.rand.NextFloat() + Main.rand.Next(4) * 0.3f;
                if (Main.rand.NextBool(3))
                {
                    Main.dust[newDust].noGravity = true;
                    Main.dust[newDust].velocity *= 3f;
                    Main.dust[newDust].scale *= 2f;
                }
            }
            for (int i = 0; i < 30; i++)
            {
                int newDust = Dust.NewDust(Owner.Center, Owner.width, Owner.height, tyoe);
                Main.dust[newDust].scale = Main.rand.NextFloat(1f, 4f);
                Main.dust[newDust].noGravity = true;
                Main.dust[newDust].velocity.Y = -10f;
                Main.dust[newDust].velocity = Main.dust[newDust].velocity.RotatedByRandom(MathHelper.ToRadians(30));
                Main.dust[newDust].velocity *= Main.rand.NextFloat(1f, 3f);
            }
            for (int i = 0; i < 12; i++)
            {
                int newDust = Dust.NewDust(Owner.Center, Owner.width, Owner.height, DustID.Smoke, 0f, 0f, 150, default, 1f);
                Main.dust[newDust].scale = Main.rand.NextFloat(1f, 4f);
                Main.dust[newDust].fadeIn = 1.25f;
                Main.dust[newDust].noLight = true;
                Main.dust[newDust].velocity = new Vector2(0f, Main.rand.Next(-5, -2));
                Main.dust[newDust].velocity = Main.dust[newDust].velocity.RotatedByRandom(MathHelper.ToRadians(90));
                Main.dust[newDust].velocity *= Main.rand.NextFloat(0.5f, 2f);
            }
            for (int i = 1; i <= 3; i++)
            {
                for (int j = 0; j < 30; j++)
                {
                    Vector2 vector2 = Vector2.UnitX * -Projectile.width / 2f;
                    vector2 += Utils.RotatedBy(Vector2.UnitY, j * Math.PI / 15f) * new Vector2(50f * i, 10f);
                    vector2 = Utils.RotatedBy(vector2, Vector2.UnitY.ToRotation() - Math.PI / 2f) * 1.3f;
                    int newDust = Dust.NewDust(Owner.Bottom + vector2 - (Vector2.UnitY * 50 * i), 0, 0, tyoe, 0f, 0f, 160, default, 2f);
                    Main.dust[newDust].noGravity = true;
                    Main.dust[newDust].velocity = Projectile.velocity * 0.5f;
                    Main.dust[newDust].velocity = Vector2.Normalize(Owner.Center - Projectile.velocity * 3f - Main.dust[newDust].position) * 1.5f;
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D bloomTex = MoreKatanaTextures.BloomTexture.Value;
            Vector2 position = Projectile.Center - Main.screenPosition;
            Color color = TerraKatana.TerraColor[0] with { A = 0 } * 0.2f;
            Main.spriteBatch.Draw(bloomTex, position, null, color, 0f, bloomTex.Size() / 2f, 6f, SpriteEffects.None, 0);
            return false;
        }
    }
}