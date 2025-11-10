using Microsoft.Xna.Framework;
using MoreKatana.Projectiles.Base;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace MoreKatana.Projectiles.TerraKatanaTree
{
    public class LightsTeleport : ModProjectile
    {
        private Vector2 teleportPos;

        public override string Texture => MoreKatana.EmptyTexture;

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 1;
            Projectile.tileCollide = true;
            Projectile.hide = true;
        }

        public override bool? CanDamage() => false;

        public override void OnKill(int timeLeft)
        {
            teleportPos = Projectile.Center;

            Player player = Main.player[Projectile.owner];
            player.immune = true;
            player.immuneTime = 15;

            player.Teleport(teleportPos, -1);
            NetMessage.SendData(MessageID.TeleportEntity, -1, -1, null, 0, player.whoAmI, teleportPos.X, teleportPos.Y, -1);

            if (Collision.SolidCollision(player.Bottom, player.width, 2))
                player.position.Y -= Player.defaultHeight / 2;

            player.velocity = Vector2.Zero;
            NetMessage.SendData(MessageID.PlayerControls, number: player.whoAmI);

            MoreKatanaUtil.DrawRing(player.MountedCenter, DustID.Demonite, 36, 15f, dustScale: 2f);

            if (Projectile.owner == Main.myPlayer)
                Projectile.NewProjectile(player.GetSource_ItemUse(player.ActiveItem()), player.Center, Vector2.Normalize(Projectile.velocity), ModContent.ProjectileType<LightsTeleportSwing>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
        }
    }

    public class LightsTeleportSwing : CustomSword
    {
        public override void Initialize(int type)
        {
            Projectile.localNPCHitCooldown = OwnerItem.useAnimation * Projectile.MaxUpdates;
            Projectile.MKProj().Bool[0] = false;

            if (type == 0)
                SwingEllipse = new(1.9f, 0.9f);
            else
            {
                SwingEllipse = new(1.1f, 1.1f);
                SwingDirection *= Owner.direction;
            }

            ContinuousSwing = true; // 設定した全てのスイングを連続で行う
            FixedDirection = true; // プレイヤーと発射体の方向を固定する
            CreateSound = false;
            ImpactCharge = 5;

            GetTextureValues();
        }

        // スイングデータを設定する
        public SwingData Down => new SwingData(OwnerItem.useAnimation, 0.65f, 0.2f);
        public SwingData Spin => new SwingData(OwnerItem.useAnimation * 1.5f, 1.5f, 0.2f, Owner.MKPlayer().MouseWorld.X < Owner.Center.X);
        public override SwingData GetSwingData(int type) => SwingData.SwingRegister(type, Down, Spin, new SwingData());

        public override float GetProgress(int type) => type == 0 ? GeneralSwingAnimation(Progress) : MoreKatanaUtil.LinearEasing(Progress, 1);

        public override void AdditionalAI(int type, bool delay)
        {
            // プレイヤーのアイテム使用時間を延長する
            Owner.SetDummyItemTime(2);

            if (Owner.yoraiz0rEye < 2)
                Owner.yoraiz0rEye = 2;

            if (!Projectile.MKProj().Bool[0])
            {
                Projectile.MKProj().Bool[0] = true;
                SoundEngine.PlaySound(MoreKatanaSounds.SwordSlash, Owner.Center);

                Owner.ScreenShake(3, 15);
                Owner.velocity.X = 0;
                Owner.velocity.Y = 0;

                if (type == 1)
                {
                    Owner.UpdateRotation(1, SwingDirection * Owner.direction, 20f);
                    Owner.velocity.X += 8 * SwingDirection * Owner.direction;
                    Owner.velocity.Y -= 8;
                }

                NetMessage.SendData(MessageID.PlayerControls, number: Owner.whoAmI);
            }
        }
    }
}