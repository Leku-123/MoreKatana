using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace MoreKatana.Projectiles.Base
{
    /// <summary>
    /// [WIP] ダッシュ切り予定
    /// </summary>
    public class CustomDashSlash : ModProjectile
    {
        protected Player Owner => Main.player[Projectile.owner];

        protected Item OwnerItem => Owner.HeldItem;

        public override string Texture => MoreKatana.EmptyTexture;

        public override void OnSpawn(IEntitySource source)
        {
            TextureAssets.Projectile[Projectile.type] = TextureAssets.Item[OwnerItem.type];
            Vector2 v = Utils.DirectionTo(Owner.Center, Main.MouseWorld);
            v *= 10f;
            Owner.velocity += v;
        }

        public override void SetDefaults()
        {
            Projectile.width = 2;
            Projectile.height = 2;
            Projectile.aiStyle = -1;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 5;
            Projectile.ownerHitCheck = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.noEnchantmentVisuals = true;
            Projectile.MKProjectile().SourceIsItemUse = true;
        }

        public override void AI()
        {
            SetSwordPosition();
        }

        private void SetSwordPosition()
        {
            // 発射体の位置と向き
            Projectile.Center += Owner.MountedCenter;
            Projectile.spriteDirection = Owner.direction;

            // プレイヤーの保持する発射体のIDを更新する
            Owner.heldProj = Projectile.whoAmI;

            // 腕の回転の設定をする
            Owner.SetCompositeArmFront(true, 0, (Owner.MountedCenter - Projectile.Center).ToRotation() + (float)Math.PI / 2f);
        }
    }
}
