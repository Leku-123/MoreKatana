using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MoreKatana.Assets.ExtraTextures;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;

namespace MoreKatana.Projectiles.TerraKatanaTree
{
    public class TrueSacredNaginataHoldout : ModProjectile
    {
        private ref float Timer => ref Projectile.ai[0];

        public const int PrepareTime = 60;
        public const int FireTime = 150;
        public const int DisappearTime = 30;

        public float PrepareCompletion => MathHelper.Clamp(Timer / PrepareTime, 0f, 1f);
        public float FireCompletion => MathHelper.Clamp((Timer - PrepareTime) / FireTime, 0f, 1f);
        public float DisappearCompletion => MathHelper.Clamp((Timer - PrepareTime - FireTime) / DisappearTime, 0f, 1f);

        public Player Owner => Main.player[Projectile.owner];

        public override string Texture => ModContent.GetInstance<TrueSacredNaginataSwing>().Texture;

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
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (projHitbox.Intersects(targetHitbox))
                return true;

            float _ = float.NaN;
            float length = 124;
            Vector2 offset = length / 2 * Projectile.scale * Vector2.Normalize(Projectile.velocity);
            Vector2 tip = Projectile.Center + offset;
            Vector2 end = Projectile.Center - offset;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), tip, end, Projectile.scale, ref _);
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

            if (Projectile.owner == Main.myPlayer && !Main.mouseRight && PrepareCompletion != 1f)
            {
                Projectile.Kill();
                return;
            }

            // チェイン部分の挙動
            ChainPhysics();

            // プレイヤーの保持する発射体のIDを更新して、プレイヤーの使用時間を延長する
            Owner.heldProj = Projectile.whoAmI;
            Owner.SetDummyItemTime(10);

            // プレイヤーのアイテムローテーションと向き
            float itemrotate = Projectile.direction < 0 ? MathHelper.Pi : 0;
            Owner.itemRotation = Projectile.velocity.ToRotation() + itemrotate;
            Owner.itemRotation = MathHelper.WrapAngle(Owner.itemRotation);
            Owner.ChangeDir(Math.Sign(Projectile.velocity.X));

            // 発射体の基本位置
            Projectile.Center = Owner.RotatedRelativePoint(Owner.MountedCenter);

            // 発射体の速度をマウス方向への線形補完した速度の単位ベクトルに変換
            // 発射体の位置を単位ベクトル方向にオフセット分移動させる
            const float lerp = 0.05f;
            float offset = 60f;
            Vector2 normalizeVel = Vector2.Normalize(Projectile.velocity); //発射体の速度の単位ベクトル
            Projectile.velocity = Vector2.Lerp(normalizeVel, Vector2.Normalize(Main.MouseWorld - Owner.MountedCenter), lerp);
            Projectile.velocity.Normalize();
            Projectile.position += Projectile.velocity * offset;

            // 発射体の回転と向き
            Projectile.spriteDirection = Projectile.direction;
            float rot = (Projectile.spriteDirection == 1) ? MathHelper.ToRadians(45f) : MathHelper.ToRadians(135f);
            Projectile.rotation = Projectile.velocity.ToRotation() + rot;

            // 発射位置を剣先に調節する
            float fireOffset = 100f;
            Vector2 firePos = Projectile.position + (Vector2.Normalize(Projectile.velocity) * fireOffset);

            // 最初のフレームで魔法陣の発射体を2つスポーンさせる
            if (Timer == 0 && Projectile.owner == Main.myPlayer)
            {
                // iで魔法陣の配置向きを決める
                // 0ならばスポーンしない
                for (int i = -1; i <= 1; i++)
                {
                    if (i != 0)
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), firePos, Vector2.Normalize(Projectile.velocity), ModContent.ProjectileType<TrueSacredSecondaryMagicCircle>(), Projectile.damage, Projectile.knockBack, Projectile.owner, Projectile.whoAmI, i);
                }
            }

            if (PrepareCompletion < 0.8f) // 準備
            {
                int newDust = Dust.NewDust(firePos - new Vector2(4), 32, 32, DustID.HallowedWeapons, Projectile.oldVelocity.X, Projectile.oldVelocity.Y, 100, default);
                Main.dust[newDust].noGravity = true;
                Main.dust[newDust].scale *= 2;
                Main.dust[newDust].velocity = Main.rand.NextVector2Unit() * Main.rand.NextFloat(2.5f, 4.5f);
            }
            else if (PrepareCompletion == 1f && FireCompletion < 1f) // 発射
            {
                Owner.ScreenShake(5, 2); // スクリーンシェイク
                Projectile.MKProjectile().ActivateCD = true; // この発射体消滅後にクールダウンを有効化する

                if (Timer % 10 == 0)
                {
                    // 最初のフレームでサウンドとビーム発射
                    if (Projectile.ai[1] == 0)
                    {
                        Projectile.ai[1] = 1;
                        SoundEngine.PlaySound(SoundID.Zombie104, Owner.position);

                        if (Projectile.owner == Main.myPlayer)
                        {
                            Vector2 beamVelocity = Vector2.Normalize(Projectile.velocity);
                            if (beamVelocity.HasNaNs())
                                beamVelocity = -Vector2.UnitY;

                            // このUUIDはマルチプレイヤーモードで全てのプレイヤー間で共通となり、プリズムにビームが正しく固定されるようにします...らしいよ
                            int uuid = Projectile.GetByUUID(Projectile.owner, Projectile.whoAmI);
                            Projectile.NewProjectile(Projectile.GetSource_FromThis(), firePos, beamVelocity, ModContent.ProjectileType<TrueSacredBeam>(), Projectile.damage, Projectile.knockBack, Projectile.owner, uuid);
                            Projectile.netUpdate = true;
                        }
                    }

                    for (int i = 0; i < 8; i++)
                    {
                        int newDust = Dust.NewDust(firePos, 32, 32, DustID.HallowedWeapons, 0f, 0f, 100, default, 1.5f);
                        Main.dust[newDust].scale *= Main.rand.NextFloat(1, 2.5f);
                        Main.dust[newDust].noGravity = true;
                        Main.dust[newDust].velocity += Vector2.Normalize(Projectile.velocity) * 2;
                        Main.dust[newDust].velocity = Main.dust[newDust].velocity.RotatedByRandom(MathHelper.ToRadians(15)) * 6f;
                        Main.dust[newDust].velocity *= Main.rand.NextFloat(1f, 3f);
                        Main.dust[newDust].velocity += Owner.velocity / 2;
                        newDust = Dust.NewDust(firePos, 32, 32, DustID.HallowedWeapons, 0f, 0f, 100, default, 1.5f);
                        Main.dust[newDust].velocity += Vector2.Normalize(Projectile.velocity) * 2;
                        Main.dust[newDust].velocity *= 5f;
                        Main.dust[newDust].velocity *= Main.rand.NextFloat(1f, 2f);
                        Main.dust[newDust].velocity += Owner.velocity / 2;
                    }

                    for (int i = 0; i < 2; i++)
                    {
                        Vector2 vector = Main.rand.NextVector2Unit() * 40;
                        ParticleOrchestraSettings particleOrchestraSettings = default;
                        particleOrchestraSettings.PositionInWorld = firePos + vector;
                        ParticleOrchestrator.RequestParticleSpawn(false, ParticleOrchestraType.TrueExcalibur, particleOrchestraSettings, Projectile.owner);
                    }

                    Projectile.netUpdate = true;
                }
            }
            else // 消滅
            {
                // DisappearCompletionをもとに抑揚をつけてフェードアウト
                Projectile.Opacity = 1 - EaseFunction.EaseCubicOut.Ease(DisappearCompletion);
            }

            Timer++;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Rectangle rectangle = new Rectangle(0, 0, texture.Width, texture.Height);
            Vector2 position = Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);
            Color color = Projectile.GetAlpha(lightColor);
            Color glowColor = Color.White * Projectile.Opacity;
            Color circleColor = Color.Gold * Projectile.Opacity;
            SpriteEffects spriteEffects = (Projectile.spriteDirection == -1) ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            // 拍動
            MoreKatanaUtil.DrawBackglow(texture, position, rectangle, glowColor with { A = 0 }, Projectile.rotation, 3f * ((float)Math.Sin(Main.GameUpdateCount / 30f) + 0.3f), new Vector2(Projectile.scale), spriteEffects);

            // 本体の描画
            Main.EntitySpriteDraw(texture, position, rectangle, color, Projectile.rotation, texture.Size() / 2, Projectile.scale, spriteEffects, 0);

            // チェインの描画
            DrawChain(glowColor);

            // 魔法陣の描画
            Texture2D bloom = MoreKatanaTextures.BloomTexture.Value;
            Texture2D circle = MoreKatanaTextures.MagicCircleTexture.Value;
            Texture2D ring = MoreKatanaTextures.MagicRingTexture.Value;
            Vector2 offset = Vector2.Normalize(Projectile.velocity) * 100f;
            float rot = Vector2.Normalize(Projectile.velocity).ToRotation();

            Main.EntitySpriteDraw(bloom, position + offset, null, circleColor with { A = 0 } * 0.5f, rot, bloom.Size() / 2f, new Vector2(0.25f * PrepareCompletion, 0.5f * PrepareCompletion), 0, 0);

            MoreKatanaUtil.DrawCompression(circle, circleColor, rot, Projectile.Opacity, new Vector2(2, 1), 1, Timer, BlendState.Additive);
            Main.spriteBatch.Draw(circle, position + offset, null, circleColor, 0f, circle.Size() / 2, 0.5f * PrepareCompletion, SpriteEffects.None, 0);

            MoreKatanaUtil.DrawCompression(ring, circleColor, rot, Projectile.Opacity, new Vector2(2, 1), 1, Timer / 10f, BlendState.Additive);
            Main.spriteBatch.Draw(ring, position + offset, null, circleColor, 0f, ring.Size() / 2, 0.5f * PrepareCompletion, SpriteEffects.None, 0);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            // ゲージの描画
            if (PrepareCompletion == 1f && FireCompletion != 1f)
            {
                Vector2 spriteSize = new Vector2(50, 50);
                Vector2 ownerPos = Owner.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);
                Vector2 pos = new Vector2(ownerPos.X - spriteSize.X * 0.5f, ownerPos.Y - spriteSize.Y * 0.9f);
                Color c1 = Color.Black;
                Color c2 = Color.Gold;

                Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, pos, new Rectangle(0, 0, 1, 1), c1, 0f, Vector2.Zero, new Vector2(spriteSize.X, 4f), SpriteEffects.None, 0f);
                Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, pos, new Rectangle(0, 0, 1, 1), c2, 0f, Vector2.Zero, new Vector2(spriteSize.X * (1 - FireCompletion), 4f), SpriteEffects.None, 0f);
            }

            return false;
        }

        private Vector2[] chainVels;
        private Vector2[] chainPoints;

        public void ChainPhysics()
        {
            int length = 16;
            if (chainVels != null)
            {
                for (int i = 0; i < chainVels.Length; i++)
                    chainVels[i] = (MathHelper.PiOver2 - (i * 0.01f)).ToRotationVector2() * 2f;
            }
            else
                chainVels = new Vector2[length];

            if (chainPoints != null)
            {
                Vector2 offset = Projectile.velocity * 18f;
                chainPoints[0] = Projectile.Center + offset;

                for (int i = 1; i < chainPoints.Length; i++)
                {
                    chainPoints[i] += chainVels[i];
                    if (chainPoints[i].Distance(chainPoints[i - 1]) > 10)
                        chainPoints[i] = Vector2.Lerp(chainPoints[i], chainPoints[i - 1] + new Vector2(5, 0).RotatedBy(chainPoints[i - 1].AngleTo(chainPoints[i])), 0.8f);
                }
            }
            else
            {
                chainPoints = new Vector2[length];
                for (int i = 0; i < chainPoints.Length; i++)
                    chainPoints[i] = Projectile.Center;
            }
        }

        private void DrawChain(Color lightColor)
        {
            if (chainPoints != null)
            {
                for (int i = 0; i < chainPoints.Length - 1; i++)
                {
                    Texture2D chainTex = ModContent.Request<Texture2D>(Texture + "_Chain").Value;

                    int style = 0;
                    if (i == chainPoints.Length - 3)
                        style = 1;
                    if (i > chainPoints.Length - 3)
                        style = 2;
                    Rectangle frame = chainTex.Frame(1, 3, 0, style);
                    float rotation = chainPoints[i].AngleTo(chainPoints[i + 1]);
                    Vector2 stretch = new Vector2(0.3f + Utils.GetLerpValue(0, chainPoints.Length - 2, i, true) * 0.2f, chainPoints[i].Distance(chainPoints[i + 1]) / (frame.Height - 5));
                    Main.EntitySpriteDraw(chainTex, chainPoints[i] - Main.screenPosition, frame, lightColor.MultiplyRGBA(Color.Lerp(Color.DimGray, Color.White, (float)i / chainPoints.Length)), rotation - MathHelper.PiOver2, frame.Size() * new Vector2(0.5f, 0f), stretch, 0, 0);
                }
            }
        }
    }
}