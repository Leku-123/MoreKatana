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

        //private Rectangle[] DrawFrame =
        //[
        //    new(0, 0, 48, 54),
        //    new(0, 54, 48, 112)
        //];

        public override void SafeSetStaticDefaults()
        {
            Main.projFrames[Type] = 2;
        }

        public override void Initialization(Item item, int type)
        {
            Projectile.localNPCHitCooldown = Owner.itemAnimationMax * Projectile.MaxUpdates;
            SwordSize(48, 54);

            if (!ObsidianKatana.Fire(Owner))//非活性時
                TrailColor = new(43, 40, 84);
            else//活性時
            {
                Projectile.frame = 1;        //活性時はテクスチャが変わり、
                TrailColor = new(83, 5, 1);  //軌跡の色が変わり、
                Projectile.damage *= 2;      //ダメージが２倍になる。
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D proj = TextureAssets.Projectile[Type].Value;
            Rectangle projRect = proj.Frame(1, 2, 0, Projectile.frame);
            Vector2 position = Projectile.Center - Main.screenPosition;
            SpriteEffects direction = Owner.direction == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            Main.EntitySpriteDraw(proj, position, projRect, Color.White, Projectile.rotation, projRect.Size() / 2f, Projectile.scale, direction);
            return false;
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

        public override void AdditionalAI(Item item, int type, bool delay) => Owner.SetDummyItemTime(2);
    }
}
