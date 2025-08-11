using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MoreKatana.Items.Weapons.TerraKatanaTree;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace MoreKatana.Projectiles.TerraKatanaTree
{
    public class VolcanoKatanaHoldUp : ModProjectile
    {
        private ref float Timer => ref Projectile.ai[0];

        private const int CinderCountMax = 5; // チャージできる噴石の最大数
        private int cinderCount;              // チャージした噴石の数
        private Vector2 cinderPosition;       // 噴石の位置
        private float cinderScale;            // 噴石のスケール

        public Player Owner => Main.player[Projectile.owner];

        public override string Texture => ModContent.GetInstance<VolcanoKatana>().Texture;

        public override void SetStaticDefaults() => ProjectileID.Sets.HeldProjDoesNotUsePlayerGfxOffY[Type] = true;

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.aiStyle = -1;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 36000;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.hide = true;
            Projectile.noEnchantmentVisuals = true;
            Projectile.MKProjectile().SourceIsItemUse = true;
            Projectile.MKProjectile().ActivateCD = true;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (projHitbox.Intersects(targetHitbox))
                return true;

            float dummy = 0f;
            float length = 124;
            Vector2 offset = length / 2 * Projectile.scale * Vector2.Normalize(Projectile.velocity);
            Vector2 tip = Projectile.Center + offset;
            Vector2 end = Projectile.Center - offset;

            if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), tip, end, Projectile.scale, ref dummy))
                return true;

            return false;
        }

        public override void CutTiles()
        {
            float length = 124;
            Vector2 offset = length / 2 * Projectile.scale * Vector2.Normalize(Projectile.velocity);
            Vector2 top = Projectile.Center + offset;
            Vector2 end = Projectile.Center - offset;
            Utils.PlotTileLine(top, end, Projectile.scale, DelegateMethods.CutTiles);
        }

        public override void AI()
        {
            if (Owner.CantUseHoldout(false))
            {
                Projectile.Kill();
                return;
            }

            if (Projectile.owner == Main.myPlayer && !Main.mouseRight)
            {
                Projectile.Kill();
                return;
            }

            Projectile.timeLeft = 2;

            // プレイヤーの保持する発射体のIDを更新して、プレイヤーの使用時間を延長する
            Owner.heldProj = Projectile.whoAmI;
            Owner.SetDummyItemTime(2);

            // プレイヤーのアイテムローテーションと向き
            float itemrotate = Projectile.direction < 0 ? MathHelper.Pi : 0;
            Owner.itemRotation = Projectile.velocity.ToRotation() + itemrotate;
            Owner.itemRotation = MathHelper.WrapAngle(Owner.itemRotation);
            Owner.ChangeDir(Math.Sign(Projectile.velocity.X));

            // 発射体の基本位置
            Projectile.Center = Owner.RotatedRelativePoint(Owner.MountedCenter);

            // 発射体の速度を上方向への線形補完した速度の単位ベクトルに変換
            // 発射体の位置を単位ベクトル方向にオフセット分移動させる
            const float lerp = 0.08f;
            float offset = 50f;
            Vector2 normalizeVel = Vector2.Normalize(Projectile.velocity); //発射体の速度の単位ベクトル

            // 変更：縦方向のベクトルはVector2.UnitYで表せる。
            //Projectile.velocity = Vector2.Lerp(normalizeVel, Vector2.Normalize(Owner.Top - Owner.MountedCenter), lerp);
            Projectile.velocity = Vector2.Lerp(normalizeVel, -Vector2.UnitY, lerp);

            Projectile.velocity.Normalize();
            Projectile.position += Projectile.velocity * offset;

            // 発射体の回転と向き
            Projectile.spriteDirection = Projectile.direction;
            float rot = (Projectile.spriteDirection == 1) ? MathHelper.ToRadians(45f) : MathHelper.ToRadians(135f);
            Projectile.rotation = Projectile.velocity.ToRotation() + rot;

            // 追加：噴石の位置の動き
            const float cinderLerp = 0.045f;
            float dirX = MathHelper.Lerp(Owner.velocity.X, Owner.direction, cinderLerp);
            float dirY = MathHelper.Lerp(Owner.velocity.Y, Owner.direction, cinderLerp);
            float cinderOffset = 180f;
            cinderPosition = Owner.MountedCenter - new Vector2(dirX * 2, dirY + cinderOffset + (float)(Math.Sin(Main.GameUpdateCount / 30f) * 2));
            cinderPosition = new Vector2((int)cinderPosition.X, (int)cinderPosition.Y);

            // 追加：最初のフレームでダストをスポーンさせる
            if (Timer == 0)
            {
                for (int i = 0; i < 20; ++i)
                {
                    int newDust = Dust.NewDust(cinderPosition, 16, 16, DustID.Torch, 0.0f, 0.0f, 0, default, 1f);
                    Main.dust[newDust].velocity *= 5f;
                    Main.dust[newDust].fadeIn = 1f;
                    Main.dust[newDust].scale = 1 + Main.rand.NextFloat() + Main.rand.Next(4) * 0.3f;
                    if (!Main.rand.NextBool(3))
                    {
                        Main.dust[newDust].noGravity = true;
                        Main.dust[newDust].velocity *= 3f;
                        Main.dust[newDust].scale *= 2f;
                    }
                }
            }

            // 変更：処理の簡略化と定数の追加、ダストの追加
            // 4つめまでは普通にカウント増やして、それ以外はマックスになるようにする
            /*if (Timer < 190f)
            {
                if (Timer % 38 == 0 && Timer != 0f)
                {
                    SoundEngine.PlaySound(SoundID.DD2_BetsysWrathShot, Owner.Center);
                    Meteo++;
                }
                else if (Timer == 189f)
                {
                    SoundEngine.PlaySound(SoundID.DD2_BetsysWrathImpact, Owner.Center);
                    Meteo = 5;
                }
                Timer++;
            }*/

            const int chargeTime = 38;
            if (Timer % chargeTime == 0 && cinderCount < CinderCountMax)
            {
                if (cinderCount < 4)
                {
                    SoundEngine.PlaySound(SoundID.DD2_BetsysWrathShot, Owner.Center);
                    MoreKatanaUtil.DrawRing(cinderPosition, DustID.Torch, 24, 10f + (cinderCount * 2));
                    cinderCount++;
                }
                else
                {
                    SoundEngine.PlaySound(SoundID.DD2_BetsysWrathImpact, Owner.Center);
                    MoreKatanaUtil.DrawRing(cinderPosition, DustID.Torch, 24, 25f, dustSize: 3f);
                    cinderCount = CinderCountMax;
                }
            }

            // 追加：スケールの処理
            cinderScale = 1 + (2f * MathHelper.Clamp(Timer / (chargeTime * 4), 0f, 1f));

            Timer++;
        }

        public override void OnKill(int timeLeft)
        {
            Owner.ScreenShake(5, 6);
            SoundEngine.PlaySound(SoundID.DD2_BetsysWrathImpact, Owner.Center);

            for (int i = 0; i < 24; i++)
            {
                int newDust = Dust.NewDust(cinderPosition, Projectile.width, Projectile.height, DustID.Torch, 0f, 0f, 100, default, 1.5f);
                Main.dust[newDust].scale *= Main.rand.NextFloat(1, 2.5f);
                Main.dust[newDust].noGravity = true;
                Main.dust[newDust].velocity = -Vector2.UnitY * 10f;
                Main.dust[newDust].velocity = Main.dust[newDust].velocity.RotatedByRandom(MathHelper.ToRadians(90));
                Main.dust[newDust].velocity *= Main.rand.NextFloat(1f, 3f);
            }

            if (Projectile.owner == Main.myPlayer)
            {
                for (int i = 0; i < cinderCount; i++)
                {
                    float speed = 10f;
                    Vector2 cinderVel = -Vector2.UnitY.RotatedByRandom(MathHelper.ToRadians(90)) * speed;
                    cinderVel *= 1f - Main.rand.NextFloat(0.3f);
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), cinderPosition, cinderVel, ModContent.ProjectileType<VolcanicCinder>(), Projectile.damage, Projectile.knockBack, Owner.whoAmI);
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Rectangle rectangle = new Rectangle(0, 0, texture.Width, texture.Height);
            Vector2 position = Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);
            Color color = Projectile.GetAlpha(lightColor);
            Color backColor = Color.Red;
            SpriteEffects spriteEffects = (Projectile.spriteDirection == -1) ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            float backArea = 3f * ((float)Math.Sin(Main.GameUpdateCount / 30f) + 0.3f);

            // 拍動
            MoreKatanaUtil.DrawBackglow(texture, position, rectangle, backColor with { A = 0 }, Projectile.rotation, backArea, new Vector2(Projectile.scale), spriteEffects);

            // 本体の描画
            Main.EntitySpriteDraw(texture, position, rectangle, color, Projectile.rotation, texture.Size() / 2, Projectile.scale, spriteEffects, 0);

            // 噴石
            Texture2D cinderTex = ModContent.Request<Texture2D>("Terraria/Images/Projectile_" + ProjectileID.Meteor1).Value;
            Rectangle cinderRect = new Rectangle(0, 0, cinderTex.Width, cinderTex.Height);
            Vector2 cinderPos = cinderPosition - Main.screenPosition;

            // 噴石の描画
            MoreKatanaUtil.DrawBackglow(cinderTex, cinderPos, cinderRect, backColor with { A = 0 }, Main.GlobalTimeWrappedHourly, backArea, new Vector2(cinderScale), spriteEffects);
            Main.EntitySpriteDraw(cinderTex, cinderPos, cinderRect, Color.White, Main.GlobalTimeWrappedHourly, cinderTex.Size() / 2, cinderScale, spriteEffects, 0);

            return false;
        }
    }
}