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

        private float Degrees = 0;

        private int Meteo = 0;

        public const int PrepareTime = 60;
        public const int FireTime = 100;
        public const int DisappearTime = 30;

        public Vector2 FireBallDistance = new(0, -250);

        public Texture2D meteoTexture = ModContent.Request<Texture2D>("MoreKatana/Projectiles/TerraKatanaTree/Meteo").Value;

        public float PrepareCompletion => MathHelper.Clamp(Timer / PrepareTime, 0f, 1f);
        public float FireCompletion => MathHelper.Clamp((Timer - PrepareTime) / FireTime, 0f, 1f);
        public float DisappearCompletion => MathHelper.Clamp((Timer - PrepareTime - FireTime) / DisappearTime, 0f, 1f);

        public Player Owner => Main.player[Projectile.owner];

        public override string Texture => ModContent.GetInstance<VolcanoKatana>().Texture;

        public override void SetStaticDefaults() => ProjectileID.Sets.HeldProjDoesNotUsePlayerGfxOffY[Type] = true;

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.aiStyle = -1;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.timeLeft = PrepareTime + FireTime + DisappearTime;
            Projectile.penetrate = -1;
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
            else
                Projectile.timeLeft = 3;

            if (Timer < 190f)
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
            }

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

            // 発射体の速度をマウス方向への線形補完した速度の単位ベクトルに変換
            // 発射体の位置を単位ベクトル方向にオフセット分移動させる
            const float lerp = 0.08f;
            float offset = 50f;
            Vector2 normalizeVel = Vector2.Normalize(Projectile.velocity); //発射体の速度の単位ベクトル
            Projectile.velocity = Vector2.Lerp(normalizeVel, Vector2.Normalize(Owner.Top - Owner.MountedCenter), lerp);
            Projectile.velocity.Normalize();
            Projectile.position += Projectile.velocity * offset;

            // 発射体の回転と向き
            Projectile.spriteDirection = Projectile.direction;
            float rot = (Projectile.spriteDirection == 1) ? MathHelper.ToRadians(45f) : MathHelper.ToRadians(135f);
            Projectile.rotation = Projectile.velocity.ToRotation() + rot;
        }


        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Rectangle rectangle = new Rectangle(0, 0, texture.Width, texture.Height);
            Vector2 position = Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);
            Color color = Projectile.GetAlpha(lightColor);
            Color glowColor = Color.Red * Projectile.Opacity;
            SpriteEffects spriteEffects = (Projectile.spriteDirection == -1) ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            // 拍動
            MoreKatanaUtil.DrawBackglow(texture, position, rectangle, glowColor with { A = 0 }, Projectile.rotation, 3f * ((float)Math.Sin(Main.GameUpdateCount / 30f) + 0.3f), new Vector2(Projectile.scale), spriteEffects);

            // 本体の描画
            Main.EntitySpriteDraw(texture, position, rectangle, color, Projectile.rotation, texture.Size() / 2, Projectile.scale, spriteEffects, 0);

            //Texture2D meteoTexture = TextureAssets.Projectile[ProjectileID.Meteor1].Value;

            Vector2 meteoPosition = Owner.Center - Main.screenPosition + (Projectile.gfxOffY * FireBallDistance);

            meteoPosition += FireBallDistance;

            Rectangle meteoRectangle = new Rectangle(0, 0, meteoTexture.Width, meteoTexture.Height);

            // 火山弾の描画（拍動付き）
            MoreKatanaUtil.DrawBackglow(meteoTexture, meteoPosition, meteoRectangle, Color.DarkRed with { A = 0 }, MathHelper.ToRadians(Degrees), 3f * ((float)Math.Sin(Main.GameUpdateCount / 30f) + 0.3f), new Vector2(1.1f + (Timer / 100f)), spriteEffects);
            Main.EntitySpriteDraw(meteoTexture, meteoPosition, meteoRectangle, Color.White, MathHelper.ToRadians(Degrees), meteoTexture.Size() / 2, 1f + (Timer / 100f), spriteEffects, 0);

            Degrees++;
            if (Degrees > 360f)
            {
                Degrees = 0f;
            }

            return false;
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.DD2_BetsysWrathImpact, Projectile.position);

            int MeteoWidth = meteoTexture.Width * (Meteo + 1);
            int MeteoHeight = meteoTexture.Height * (Meteo + 1);

            Vector2 meteoPosition = Owner.Center + FireBallDistance;

            for (int i = 0; i < 30; i++)
            {
                var smoke = Dust.NewDustDirect(meteoPosition, MeteoWidth, MeteoHeight, DustID.Smoke, 0f, 0f, 100, default, 1.5f);
                smoke.velocity *= 1.4f;
            }

            for (int j = 0; j < 20; j++)
            {
                var fireDust = Dust.NewDustDirect(meteoPosition, MeteoWidth, MeteoHeight, DustID.Torch, 0f, 0f, 100, default, 3.5f);
                fireDust.noGravity = true;
                fireDust.velocity *= 7f;
                fireDust = Dust.NewDustDirect(meteoPosition, MeteoWidth, MeteoHeight, DustID.Torch, 0f, 0f, 100, default, 1.5f);
                fireDust.velocity *= 3f;
            }

            for (int k = 0; k < 2; k++)
            {
                float speedMulti = 0.4f;
                if (k == 1)
                {
                    speedMulti = 0.8f;
                }

                var smokeGore = Gore.NewGoreDirect(Projectile.GetSource_Death(), meteoPosition, default, Main.rand.Next(GoreID.Smoke1, GoreID.Smoke3 + 1));
                smokeGore.velocity *= speedMulti;
                smokeGore.velocity += Vector2.One;
                smokeGore = Gore.NewGoreDirect(Projectile.GetSource_Death(), meteoPosition, default, Main.rand.Next(GoreID.Smoke1, GoreID.Smoke3 + 1));
                smokeGore.velocity *= speedMulti;
                smokeGore.velocity.X -= 1f;
                smokeGore.velocity.Y += 1f;
                smokeGore = Gore.NewGoreDirect(Projectile.GetSource_Death(), meteoPosition, default, Main.rand.Next(GoreID.Smoke1, GoreID.Smoke3 + 1));
                smokeGore.velocity *= speedMulti;
                smokeGore.velocity.X += 1f;
                smokeGore.velocity.Y -= 1f;
                smokeGore = Gore.NewGoreDirect(Projectile.GetSource_Death(), meteoPosition, default, Main.rand.Next(GoreID.Smoke1, GoreID.Smoke3 + 1));
                smokeGore.velocity *= speedMulti;
                smokeGore.velocity -= Vector2.One;
            }

            for (int volcanoNum = Meteo; Meteo > 0; Meteo--)
            {
                Vector2 newVelocity = Projectile.velocity.RotatedByRandom(MathHelper.ToRadians(Degrees));

                newVelocity.X *= Main.rand.NextBool() ? 1 : -1;

                Projectile.NewProjectile(Projectile.GetSource_FromThis(), meteoPosition, newVelocity, ModContent.ProjectileType<MeteoBullet>(), Projectile.damage, Projectile.knockBack, Owner.whoAmI);
            }
        }
    }
}
