using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace MoreKatana.Projectiles
{
    /// <summary>
    /// [WIP] ダッシュ切り予定
    /// </summary>
    public class CustomDashSlash : ModProjectile
    {
        protected Player Owner => Main.player[Projectile.owner];

        protected Item OwnerItem => Owner.ActiveItem();

        public override string Texture => MoreKatana.EmptyTexture;

        private Vector2 v;

        public override void OnSpawn(IEntitySource source)
        {
            v = Owner.Center.DirectionTo(Main.MouseWorld);
            v *= 10f;
            Owner.velocity += v;
        }

        public override void SetDefaults()
        {
            Projectile.width = Owner.width;
            Projectile.height = Owner.height;
            Projectile.aiStyle = -1;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 45;
            //Projectile.ownerHitCheck = true;
            //Projectile.usesLocalNPCImmunity = true;
            //Projectile.noEnchantmentVisuals = true;
        }

        public override void AI()
        {
            SetSwordPosition();
            Owner.SetDummyItemTime(2);
        }

        private void SetSwordPosition()
        {
            // 発射体の位置と向き
            Projectile.Center = Owner.MountedCenter;
            Projectile.spriteDirection = Owner.direction;

            // 発射体の回転を調節する。Backspinも考慮する
            if (Projectile.spriteDirection == 1)
                Projectile.rotation = v.ToRotation() + MathHelper.ToRadians(45f);
            else
                Projectile.rotation = v.ToRotation() + MathHelper.ToRadians(135f);

            // プレイヤーの保持する発射体のIDを更新する
            Owner.heldProj = Projectile.whoAmI;

            // 腕の回転の設定をする
            Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, v.ToRotation() - MathHelper.ToRadians(90f));
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Item[OwnerItem.type].Value;

            DrawAnimation anim = Main.itemAnimations[OwnerItem.type];

            Vector2 position = Projectile.Center + v * 2 - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);
            Rectangle? rectangle = new Rectangle?(anim == null ? texture.Frame(1, 1, 0, 0, 0, 0) : anim.GetFrame(texture, -1));

            float frame = anim == null ? 1 : anim.FrameCount;
            Vector2 origin = new(texture.Width / 2, texture.Height / frame / 2);

            SpriteEffects spriteEffects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            Main.EntitySpriteDraw(texture, position, rectangle, Projectile.GetAlpha(lightColor), Projectile.rotation, origin, Projectile.scale, spriteEffects, 0);

            return false;
        }
    }
}
