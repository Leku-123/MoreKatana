using Microsoft.Xna.Framework;
using MoreKatana.Projectiles.Base;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace MoreKatana.Projectiles.TerraKatanaTree
{
    public class MuramasaCounterattack : ModProjectile
    {
        private ref float Timer => ref Projectile.ai[0];

        private const float LifeTime = 30;

        public Player clone;

        public Player Owner => Main.player[Projectile.owner];

        private Item ActiveItem => Owner.ActiveItem();

        public override string Texture => MoreKatana.EmptyTexture;

        public override void SetDefaults()
        {
            Projectile.width = Player.defaultWidth;
            Projectile.height = Player.defaultHeight;
            Projectile.aiStyle = -1;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.timeLeft = (int)LifeTime;
            Projectile.penetrate = -1;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.noEnchantmentVisuals = true;
        }

        public override void AI()
        {
            Projectile.direction = Projectile.spriteDirection = (Projectile.velocity.X > 0f) ? 1 : -1;
            Projectile.velocity *= 0.85f;
            Projectile.Opacity = Utils.GetLerpValue(LifeTime, 0f, Timer, true);
            if (Projectile.Opacity == 0f)
                Projectile.Kill();

            // プレイヤーのクローンを作成する
            // プレイヤーの見た目を引き継ぐ
            clone ??= new Player();
            clone.CopyVisuals(Owner);

            // クローンのデータを更新する
            clone.ResetEffects();
            clone.ResetVisibleAccessories();
            clone.DisplayDollUpdate();
            clone.UpdateSocialShadow();
            clone.UpdateDyes();
            clone.PlayerFrame();

            // 目を青くする
            clone.eyeColor = Color.DeepSkyBlue;
            // 脚は直立
            clone.legFrame.Y = 0;
            // 発射体の向きに合わせる
            clone.direction = Projectile.direction;

            if (Timer == 5)
            {
                SoundEngine.PlaySound(SoundID.Item1, Owner.Center);

                if (Projectile.owner == Main.myPlayer)
                {
                    int p = Projectile.NewProjectile(Owner.GetSource_ItemUse(ActiveItem), Projectile.Center, Vector2.Normalize(Projectile.velocity), ModContent.ProjectileType<MuramasaCounterattackSwing>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
                    MuramasaCounterattackSwing swing = (MuramasaCounterattackSwing)Main.projectile[p].ModProjectile;
                    swing.hostIndex = Projectile.whoAmI;
                    swing.clone = clone;
                }
            }

            // ダストのスポーン
            for (int i = 0; i < 3; i++)
            {
                int newDust = Dust.NewDust(new Vector2(Projectile.Center.X - Projectile.width, Projectile.Center.Y + Projectile.height / 2), Projectile.width * 2 - 3, 0, DustID.DungeonWater, 0, Main.rand.Next(-5, -2), 150, default, 0.5f);
                Main.dust[newDust].fadeIn = 0.3f;
                Main.dust[newDust].noGravity = true;
            }

            // 青色の光
            Lighting.AddLight(Projectile.position, Color.Blue.ToVector3());

            Timer++;
        }

        public override bool? CanDamage() => false;

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.NPCDeath52, Projectile.Center);

            for (int i = 0; i < 12; i++)
            {
                int newDust = Dust.NewDust(Projectile.Center, Projectile.width, Projectile.height, DustID.DungeonWater);
                Main.dust[newDust].scale = Main.rand.NextFloat(0.9f, 1.75f);
                Main.dust[newDust].noGravity = true;
                Main.dust[newDust].velocity.Y = -1f;
                Main.dust[newDust].velocity = Main.dust[newDust].velocity.RotatedByRandom(MathHelper.ToRadians(10));
                Main.dust[newDust].velocity *= Main.rand.NextFloat(1f, 3f);
            }
            for (int i = 0; i < 9; i++)
            {
                int newDust = Dust.NewDust(Projectile.Center, Projectile.width, Projectile.height, DustID.Smoke, 0.0f, 0f, 150, default, 0.5f);
                Main.dust[newDust].fadeIn = 1.25f;
                Main.dust[newDust].noLight = true;
                Main.dust[newDust].velocity = new Vector2(0f, Main.rand.Next(-2, -1));
                Main.dust[newDust].velocity = Main.dust[newDust].velocity.RotatedByRandom(MathHelper.ToRadians(15));
                Main.dust[newDust].velocity *= Main.rand.NextFloat(0.5f, 2f);
            }
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) => overPlayers.Add(index);

        public override bool PreDraw(ref Color lightColor)
        {
            // プレイヤーのテクスチャを描画
            clone.DrawColorEffect(Color.SkyBlue.ToVector3(), 0.5f * Projectile.Opacity);
            Main.PlayerRenderer.DrawPlayer(Main.Camera, clone, Projectile.position, 0f, clone.fullRotationOrigin, 0f, 1f);
            return false;
        }

        public class MuramasaCounterattackSwing : CustomSword
        {
            public const float SwingUseTime = 10f;
            public const float SwingDelayTime = 15f;

            public Player clone;

            public int hostIndex;

            private Projectile HostProj => Main.projectile[hostIndex];

            public override void SetSwordPosition(Vector2 v)
            {
                // 発射体の位置と向き
                Projectile.Center = HostProj.Center + (v * Projectile.scale);
                clone.direction = Projectile.direction;
                Projectile.spriteDirection = Projectile.direction;

                // 発射体の回転を調節する
                Projectile.rotation = (Projectile.Center - HostProj.Center).ToRotation()
                    + (MathHelper.PiOver2 - MathHelper.PiOver4 * Projectile.spriteDirection);

                // クローンの保持する発射体のIDを更新する
                clone.heldProj = Projectile.whoAmI;

                // クローンの腕の回転の設定をする
                float armRot = (HostProj.Center - Projectile.Center).ToRotation() + (float)Math.PI / 2f;
                clone.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, armRot);
                clone.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, armRot);
            }

            public override void Initialization(Item item, int type)
            {
                Projectile.localNPCHitCooldown = (int)SwingUseTime * Projectile.MaxUpdates;
                Projectile.Opacity = 0.5f;
                GetTextureValues(this, item);
            }

            public override bool AttackPattern(Item item, int type)
            {
                GetEllipse(1f, 1f);
                SwingStats(SwingUseTime, 0.5f);
                DelayTimer = SwingDelayTime;
                return base.AttackPattern(item, type);
            }

            public override float GetProgress(int type) => EaseFunction.EaseCubicOut.Ease(progress);
        }
    }
}