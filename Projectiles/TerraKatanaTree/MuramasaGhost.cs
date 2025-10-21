using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MoreKatana.Assets.ExtraTextures;
using MoreKatana.Projectiles.Base;
using MoreKatana.Projectiles.PrimTrails;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static MoreKatana.MoreKatanaUtil;

namespace MoreKatana.Projectiles.TerraKatanaTree
{
    public class MuramasaGhost : ModProjectile
    {
        private ref float DirectionX => ref Projectile.ai[0];
        private ref float DirectionY => ref Projectile.ai[1];

        private const float LifeTime = 10 * 60;
        private const int ComboCount = 2;
        private int swingType;

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
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.noEnchantmentVisuals = true;
            Projectile.ContinuouslyUpdateDamageStats = true;
            Projectile.MKProjectile().SourceIsItemUse = true;
            Projectile.MKProjectile().ActivateCD = true;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {

        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {

        }

        public override void AI()
        {
            // ムラマサをホールドしていない場合消滅
            if (ActiveItem.type != ItemID.Muramasa || !Owner.active || Owner.CCed || Owner == null)
            {
                Projectile.Kill();
                return;
            }

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

            // クローンのスイングが存在するかどうか確認する
            // 存在しない場合、スイングのスポーンと体と腕のフレーム調節を行う
            int proj = ModContent.ProjectileType<MuramasaGhostSwing>(); // スイングとなる発射体
            if (Owner.ownedProjectileCounts[proj] == 0)
            {
                clone.bodyFrame.Y = 0;
                clone.compositeBackArm = Owner.compositeBackArm;
                clone.compositeFrontArm = Owner.compositeFrontArm;

                if (Owner.ItemAnimationJustStarted && !Owner.IsUsingAlt()) // プレイヤーの攻撃が開始された場合
                {
                    Owner.ScreenShake(4, 4);
                    SoundEngine.PlaySound(SoundID.Item71, Owner.Center);

                    // スイングのスポーンを行う
                    // スイングのホストとなる発射体とクローンを設定し、コンボのカウントを変更する
                    if (Projectile.owner == Main.myPlayer)
                    {
                        int p = Projectile.NewProjectile(Owner.GetSource_ItemUse(ActiveItem), Projectile.Center, Projectile.SafeDirectionTo(Owner.MKPlayer().MouseWorld), proj, Projectile.damage * 2, Projectile.knockBack, Projectile.owner, swingType);
                        MuramasaGhostSwing swing = (MuramasaGhostSwing)Main.projectile[p].ModProjectile;
                        swing.hostIndex = Projectile.whoAmI;
                        swing.clone = clone;
                        swingType = (swingType + 1) % ComboCount;
                    }

                    Projectile.netUpdate = true;
                }
            }

            // 発射体の位置
            // XYの向きを線形補完して、XYの値に乗算する
            // そこから上下に揺れるようにする
            DirectionX = MathHelper.Lerp(DirectionX, Owner.direction, 0.045f);
            DirectionY = MathHelper.Lerp(DirectionY, 1, 0.045f);
            Projectile.Center = Owner.MountedCenter - new Vector2(50 * DirectionX, 50 * DirectionY + (float)(Math.Sin(Main.GameUpdateCount / 30f) * 7));

            // ダストのスポーン
            for (int i = 0; i < 3; i++)
            {
                int newDust = Dust.NewDust(new Vector2(Projectile.Center.X - Projectile.width, Projectile.Center.Y + Projectile.height / 2), Projectile.width * 2 - 3, 0, DustID.DungeonWater, 0, Main.rand.Next(-5, -2), 150, default, 0.5f);
                Main.dust[newDust].fadeIn = 0.3f;
                Main.dust[newDust].noGravity = true;
            }

            // 発光
            Lighting.AddLight(Projectile.position, Color.White.ToVector3());
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
            // 円形グラデーションの描画
            Texture2D bloom = MoreKatanaTextures.BloomTexture.Value;
            Vector2 position = Projectile.Center - Main.screenPosition;
            Color bloomColor = Color.Blue;
            Main.EntitySpriteDraw(bloom, position, null, bloomColor with { A = 0 } * 0.5f, Projectile.rotation, bloom.Size() / 2f, new Vector2(1f, 1f), 0, 0);

            // クローンの傾きを制御する
            float rot = Owner.velocity.X * 0.03f;
            rot = Math.Clamp(rot, -0.3f, 0.3f);

            // 拍動するクローンのテクスチャを描画
            float backglowAmount = 12f;
            for (int i = 0; i < backglowAmount; i++)
            {
                Vector2 backglowOffset = (MathHelper.TwoPi * i / backglowAmount).ToRotationVector2() * 6f;
                backglowOffset *= (float)Math.Sin(Main.GameUpdateCount / 30f);

                clone.DrawColorEffect(Color.SkyBlue.ToVector3(), 0.1f);
                Main.PlayerRenderer.DrawPlayer(Main.Camera, clone, Projectile.position + backglowOffset, rot, clone.fullRotationOrigin, 0f, 1f);
            }

            // 本体のクローンのテクスチャを描画
            clone.DrawColorEffect(Color.SkyBlue.ToVector3(), 0.5f);
            Main.PlayerRenderer.DrawPlayer(Main.Camera, clone, Projectile.position, rot, clone.fullRotationOrigin, 0f, 1f);

            // ゲージを描画
            Vector2 gaugePos = Owner.Center + new Vector2(0, 50);
            DrawGauge(gaugePos, Projectile.timeLeft / LifeTime, Color.DeepSkyBlue, dustType: DustID.DungeonWater);

            return false;
        }

        /// <summary>
        /// マルチ対応出来ねぇんだけど
        /// </summary>
        public class MuramasaGhostSwing : CustomSword
        {
            public Player clone;

            public int hostIndex;

            private Projectile HostProj => Main.projectile[hostIndex];

            public override void SetDefaults()
            {
                base.SetDefaults();
                Projectile.ownerHitCheck = false;
            }

            public override void SafeSendExtraAI(BinaryWriter writer) => writer.Write7BitEncodedInt(hostIndex);
            public override void SafeReceiveExtraAI(BinaryReader reader) => hostIndex = reader.Read7BitEncodedInt();

            public override void DrawTrail(int type)
            {
                if (GetProgress(type) >= 0f)
                {
                    // トレイルを描画する
                    if (!PrimsCreated)
                    {
                        PrimsCreated = true;
                        SwordTrail = new CustomSwordPrimTrail(Projectile, TrailColor, SwordLength, (int)(SwingTime * 1.5f));
                        MoreKatana.primitives.CreateTrail(SwordTrail);
                    }

                    // トレイルの情報を更新する
                    UpdateTrail(SwordTrail, GetProgress(type) >= 0.95f, type: 2, width: SwordLength);
                }
            }

            /// <summary>
            /// このスイングはプレイヤー中心ではなく発射体を中心に動くため、このメソッドをオーバーライドする
            /// </summary>
            public override void SetSwordPosition(Vector2 v)
            {
                // 発射体の位置と向き
                Projectile.Center = HostProj.Center + (v * Projectile.scale);
                clone.direction = Projectile.direction;
                Projectile.spriteDirection = Projectile.direction;

                // 発射体の回転を調節する
                Projectile.rotation = (Projectile.Center - HostProj.Center).ToRotation()
                    + (MathHelper.PiOver2 - MathHelper.PiOver4 * Projectile.spriteDirection)
                    * SwingDirection;

                // クローンの腕の回転の設定をする
                float armRot = (HostProj.Center - Projectile.Center).ToRotation() + (float)Math.PI / 2f;
                clone.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, armRot);
                clone.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, armRot);
            }

            public override void Initialization(Item item, int type)
            {
                Projectile.localNPCHitCooldown = -1; // 1振りで同じターゲットに2回ヒットしないようにする
                Projectile.Opacity = 0.5f;
                GetTextureValues();
            }

            public override bool SwingPattern(Item item, int type)
            {
                SwingStats(item.useAnimation * 2, 0.5f, backspin: type % 2 != 0);
                return base.SwingPattern(item, type);
            }

            public CurveSegment execute = new CurveSegment(SineOutEasing, 0f, 0f, 0.95f); // 振りのアニメーション
            public CurveSegment unwind = new CurveSegment(LinearEasing, 0.5f, 0.95f, 0.05f); // 振りの減衰のアニメーション
            public override float GetProgress(int type) => PiecewiseAnimation(Progress, execute, unwind);

            public override void AdditionalAI(Item item, int type, bool onDelay)
            {
                // ホストとなるクローンの発射体が無い場合は消滅
                if (!HostProj.active || HostProj.type != ModContent.ProjectileType<MuramasaGhost>())
                {
                    Projectile.Kill();
                    return;
                }

                // スケールをスイング進行度によって調節する
                if (GetProgress(type) < 0.5f)
                    Projectile.scale = MathHelper.SmoothStep(0.5f, 3f, GetProgress(type) * 2f);
                else
                    Projectile.scale = MathHelper.SmoothStep(3f, 1f, (GetProgress(type) * 2f) - 1f);
            }

            public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) => overPlayers.Add(index);

            public override bool PreDraw(ref Color lightColor)
            {
                Texture2D texture = TextureAssets.Item[SwordItem.type].Value;

                Vector2 position = Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);
                Rectangle rectangle = new Rectangle(0, 0, texture.Width, texture.Height);
                Vector2 origin = rectangle.Size() / 2f;

                Color color = Projectile.GetAlpha(lightColor);
                Color glowColor = Color.White * Projectile.Opacity;
                Color trailColor = TrailColor * Projectile.Opacity;

                SpriteEffects spriteEffects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
                SpriteEffects spriteEffects2 = SwingDirection == -1 ? SpriteEffects.FlipVertically : SpriteEffects.None;

                // 背面にアウトラインを描画
                DrawBackglow(texture, position, rectangle, trailColor with { A = 0 }, Projectile.rotation, 2f, new Vector2(Projectile.scale), spriteEffects | spriteEffects2);

                // 本体の描画
                Main.EntitySpriteDraw(texture, position, rectangle, color, Projectile.rotation, origin, Projectile.scale, spriteEffects | spriteEffects2, 0);

                // 剣先にスパークルを描画
                Vector2 offset = Utils.DirectionTo(HostProj.Center, Projectile.Center) * 40 * Projectile.scale;
                DrawPrettyStarSparkle(1f, SpriteEffects.None, position + offset, glowColor * (1 - Progress), trailColor * (1 - Progress),
                        0.5f, 0f, 0.1f, 0.9f, 1f, 0f, new Vector2(Projectile.scale, Projectile.scale * 2.5f), new Vector2(1f, 1f));

                return false;
            }
        }
    }
}