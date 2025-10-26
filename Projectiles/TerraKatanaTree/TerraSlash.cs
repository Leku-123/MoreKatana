using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MoreKatana.Assets.ExtraTextures;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace MoreKatana.Projectiles.TerraKatanaTree
{
    public class TerraSlash : ModProjectile
    {
        public Player clone;

        private const int FrameCount = 2;

        public override void SetStaticDefaults() => Main.projFrames[Projectile.type] = 3;

        public override void SetDefaults()
        {
            Projectile.width = 0;
            Projectile.height = 0;
            Projectile.timeLeft = 9999;
            Projectile.tileCollide = false;
            Projectile.extraUpdates = 5;
        }

        public override bool? CanDamage() => false;

        public override void AI()
        {
            if (Projectile.velocity != Vector2.Zero)
            {
                Projectile.rotation = Projectile.velocity.ToRotation();
                Projectile.velocity = Vector2.Zero;
            }

            Projectile.frameCounter++;
            if (Projectile.frameCounter > FrameCount * Projectile.MaxUpdates)
            {
                Projectile.frameCounter = 0;
                if (Projectile.frame < Main.projFrames[Projectile.type] - 1)
                    Projectile.frame++;
            }

            if (Projectile.ai[0] > 40)
            {
                Projectile.Kill();
            }

            Projectile.ai[0]++;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (Projectile.velocity != Vector2.Zero)
                return false;

            // プレイヤーのクローンを作成する
            // プレイヤーの見た目を引き継ぐ
            clone ??= new Player();
            clone.CopyVisuals(Main.player[Projectile.owner]);

            // クローンのデータを更新する
            clone.ResetEffects();
            clone.ResetVisibleAccessories();
            clone.DisplayDollUpdate();
            clone.UpdateSocialShadow();
            clone.UpdateDyes();
            clone.PlayerFrame();

            // 脚をジャンプ時のものにする
            clone.legFrame.Y = 56 * 5;

            clone.direction = (Projectile.rotation.ToRotationVector2().X > 0).ToDirectionInt();

            float armRot = Projectile.rotation - (float)Math.PI / 2f;
            clone.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, armRot);
            clone.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, armRot);

            clone.MKPlayer().FullBright = true;

            // 本体のクローンのテクスチャを描画
            float denominator = (float)FrameCount * (Main.projFrames[Projectile.type] + 1) * Projectile.MaxUpdates;
            float progress = MoreKatanaUtil.SineOutEasing(MathHelper.Clamp((float)(Projectile.ai[0] / denominator), 0f, 1f), 1);
            Vector2 clonePos = Projectile.Center + (Projectile.rotation.ToRotationVector2() * 250 * progress);

            Texture2D bloom = MoreKatanaTextures.BloomTexture.Value;
            Color color = new Color(96, 248, 96) with { A = 0 };
            Main.EntitySpriteDraw(bloom, clonePos - Main.screenPosition, null, color * 0.5f, Projectile.rotation, bloom.Size() / 2f, new Vector2(1f, 1f), SpriteEffects.None, 0f);

            if (clone != null)
                Main.PlayerRenderer.DrawPlayer(Main.Camera, clone, clonePos, 0f, clone.fullRotationOrigin, 0f, 1f);
            Lighting.AddLight(clonePos, new Color(96, 248, 96).ToVector3());

            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Rectangle rectangle = texture.Frame(1, Main.projFrames[Projectile.type], 0, Projectile.frame);
            Vector2 origin = rectangle.Size() / 2f;
            Vector2 position = Projectile.Center - Main.screenPosition;
            Main.EntitySpriteDraw(texture, position, rectangle, color, Projectile.rotation, origin, 0.8f, SpriteEffects.None, 0f);
            return false;
        }
    }
}