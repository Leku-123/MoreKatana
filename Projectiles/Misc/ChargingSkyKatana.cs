using Microsoft.Xna.Framework;
using MoreKatana.Projectiles.Base;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace MoreKatana.Projectiles.Misc
{
    public class ChargingSkyKatana : CustomSword
    {
        public override void Initialize(int type)
        {
            Projectile.localNPCHitCooldown = -1;
            Projectile.MKProj().ActivateCD = true;
            Projectile.MKProj().Bool[0] = false;

            SwingEllipse = new(0.5f, 0.2f);
            ContinuousSwing = true;
            FixedDirection = true;
            CreateSound = false;

            GetTextureValues();
        }

        public override SwingData GetSwingData(int type) => new SwingData(45 / (Math.Clamp(type, 0, 6) * 0.3f + 1), 1);

        public override void SetSwordPosition(Vector2 v)
        {
            // 発射体の位置と向き
            Projectile.Center = Owner.MountedCenter - new Vector2(0, 45) + v;
            Projectile.spriteDirection = Owner.direction;

            // 発射体の回転を調節する
            Projectile.rotation = (Projectile.Center - Owner.MountedCenter).ToRotation()
                + (MathHelper.PiOver2 - MathHelper.PiOver4 * Projectile.spriteDirection)
                * SwingDirection;

            // プレイヤーの保持する発射体のIDを更新する
            Owner.heldProj = Projectile.whoAmI;

            // 腕の回転の設定をする
            Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, (Owner.MountedCenter - Projectile.Center).ToRotation() + (float)Math.PI / 2f);
        }

        public override void AdditionalAI(int type, bool onDelay)
        {
            Owner.SetDummyItemTime(2);
            Projectile.timeLeft = 2;

            if (Projectile.owner == Main.myPlayer && !Main.mouseRight)
                Projectile.Kill();

            if (GetProgress(type) > 0f)
            {
                if (!Projectile.MKProj().Bool[0])
                {
                    Projectile.MKProj().Bool[0] = true;
                    SoundEngine.PlaySound(SoundID.Item1 with { Pitch = +0.5f }, Owner.Center);
                }
            }

            if (type >= 1)
            {
                if (!Projectile.MKProj().Bool[1])
                {
                    Projectile.MKProj().Bool[1] = true;
                    if (Projectile.owner == Main.myPlayer)
                    {
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<SkyTornado>(), Projectile.damage, Projectile.knockBack, Projectile.owner, Projectile.whoAmI);
                    }
                }
            }
        }

        public override void OnKill(int timeLeft)
        {
            if (Projectile.owner == Main.myPlayer && Projectile.MKProj().Bool[1])
                Projectile.NewProjectile(Owner.GetSource_ItemUse(OwnerItem), Owner.Center, Owner.Center.DirectionTo(Owner.MKPlayer().MouseWorld), ModContent.ProjectileType<GeneralKatanaSwing>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
        }

        public override void DrawTrail(int type) { }
    }
}