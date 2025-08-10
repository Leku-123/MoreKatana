using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MoreKatana.Items.Weapons.Metal;
using MoreKatana.Projectiles.Base;
using Terraria;
using Terraria.GameContent;

namespace MoreKatana.Projectiles.Metal
{
    public class ObsidianSwing : CustomSword
    {
        public override string Texture => (GetType().Namespace + "." + Name).Replace('.', '/');

        public override void SafeSetStaticDefaults()
        {
            Main.projFrames[Type] = 2;
        }

        public override void Initialization(Item item, int type)
        {
            Projectile.localNPCHitCooldown = Owner.itemAnimationMax * Projectile.MaxUpdates;
            SwordSize(48, 54);

            // 通常状態と着火状態
            // テクスチャフレームとトレイルの色を変更する
            if (!ObsidianKatana.Fire(Owner))
            {
                Projectile.frame = 0;
                TrailColor = new Color(43, 40, 84);
            }
            else
            {
                Projectile.frame = 1;
                TrailColor = new Color(83, 5, 1);
            }
        }

        public override bool AttackPattern(Item item, int type)
        {
            float x = Utils.SelectRandom(Main.rand, 1f, 1.3f);
            float y = Utils.SelectRandom(Main.rand, 0.7f, 0.9f);
            GetEllipse(x, y);

            float swingRange = Main.rand.NextFloat(0.7f, 0.8f);
            float num = Owner.itemAnimationMax / 4f;
            SwingStats(num * 3, swingRange, (0.9f - swingRange) / 2f, type % 2 != 0);

            DelayTimer = num;

            return base.AttackPattern(item, type);
        }

        public override float GetProgress(int type) => EaseFunction.EaseCubicOut.Ease(progress);

        public override void AdditionalAI(Item item, int type, bool delay)
        {
            Owner.SetDummyItemTime(2);

            // 例えばこんな感じで遊んでみたりとか
            if (type == 1 && Projectile.localAI[0] == 0)
            {
                Projectile.localAI[0] = 1;

                if (Projectile.owner == Main.myPlayer)
                {
                    Projectile.velocity.Normalize();
                    Vector2 v = Projectile.velocity * 6f;
                    int newProj = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Owner.Center + v, v, 684, Projectile.damage / 2, Projectile.knockBack, Projectile.owner);
                    Main.projectile[newProj].penetrate = 1;
                    Main.projectile[newProj].timeLeft = 30;
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Rectangle rectangle = texture.Frame(1, Main.projFrames[Projectile.type], 0, Projectile.frame);
            Vector2 origin = rectangle.Size() / 2f;
            Vector2 position = Projectile.Center - Main.screenPosition;
            SpriteEffects spriteEffects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            SpriteEffects spriteEffects2 = Backspin ? SpriteEffects.FlipVertically : SpriteEffects.None;

            Main.EntitySpriteDraw(texture, position, rectangle, Projectile.GetAlpha(lightColor), Projectile.rotation, origin, Projectile.scale, spriteEffects | spriteEffects2, 0);
            return false;
        }
    }
}
