using Microsoft.Xna.Framework;
using MoreKatana.Projectiles.PrimTrails;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace MoreKatana.Projectiles.Misc
{
    public class KatanaSlash : ModProjectile
    {
        public override string Texture => MoreKatana.EmptyTexture;

        private ref float Timer => ref Projectile.ai[0];

        private readonly NPC[] hit = new NPC[5];

        private Vector2 teleportPos;

        private bool primsCreated;

        private KatanaSlashPrimTrail trail;

        private Player Owner => Main.player[Projectile.owner];

        public override void SetDefaults()
        {
            Projectile.width = 18;
            Projectile.height = 18;
            Projectile.aiStyle = -1;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 5;
            Projectile.timeLeft = 36000;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 255;
        }

        public override void AI()
        {
            if (Timer >= 60)
                Projectile.Kill();

            if (!primsCreated)
            {
                primsCreated = true;
                if (Main.netMode != NetmodeID.Server)
                {
                    trail = new KatanaSlashPrimTrail(Projectile, Color.White);
                    MoreKatana.primitives.CreateTrail(trail);
                }

                NPC target = Owner.Center.ClosestNPCAt(KatanaSlashHoldout.AttackRange);
                if (target != null)
                {
                    Vector2 vector = Projectile.SafeDirectionTo(target.Center, Vector2.UnitY);

                    // 発射体に速度を加算する
                    float speed = 15f;
                    Projectile.velocity = vector * speed;

                    SoundEngine.PlaySound(MoreKatanaSounds.SlashEffect, Owner.position);

                    Owner.ScreenShake(10, 15);

                    for (int i = 0; i < 12; i++)
                    {
                        int newDust = Dust.NewDust(Owner.MountedCenter, 32, 32, DustID.Smoke, 0f, 0f, 100, default, 2f);
                        Main.dust[newDust].velocity -= Vector2.Normalize(Projectile.velocity) * 2f;
                        Main.dust[newDust].velocity = Main.dust[newDust].velocity.RotatedByRandom(MathHelper.ToRadians(15));
                        Main.dust[newDust].velocity *= Main.rand.NextFloat(1f, 3f);
                    }
                }
                else
                {
                    Projectile.Kill();
                    return;
                }
            }
            else
            {
                Projectile.extraUpdates = 7;
            }

            //for (int i = 0; i < 3; i++)
            //{
            //    Vector2 vector = Projectile.Center;
            //    vector -= Projectile.velocity * (i * 0.25f);
            //    int newDust = Dust.NewDust(vector, 1, 1, DustID.GemDiamond, 0f, 0f, 0, default, 0.9f);
            //    Main.dust[newDust].position = vector;
            //    Main.dust[newDust].noGravity = true;
            //    Dust dust = Main.dust[newDust];
            //    dust.velocity *= 0.2f;
            //}

            Owner.immune = true;
            Owner.immuneTime = 120;
            Owner.immuneAlpha = 255;

            Timer++;
        }

        private bool CanTarget(NPC target)
        {
            foreach (var npc in hit)
                if (target == npc)
                    return false;
            return true;
        }

        private NPC TargetNext(NPC current)
        {
            float range = KatanaSlashHoldout.AttackRange;
            range *= range;
            NPC target = null;
            var center = Projectile.Center;
            for (int i = 0; i < 200; ++i)
            {
                NPC npc = Main.npc[i];
                if (npc != current && npc.active && npc.CanBeChasedBy(null) && CanTarget(npc))
                {
                    float dist = Vector2.DistanceSquared(center, npc.Center);
                    if (dist < range)
                    {
                        range = dist;
                        target = npc;
                    }
                }
            }
            return target;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hitInfo, int damageDone)
        {
            Projectile.velocity = Vector2.Zero;
            hit[Projectile.penetrate - 1] = target;
            Timer = 0;

            if (Main.netMode != NetmodeID.Server)
                trail.Points.Add(Projectile.Center);

            if (target.type == NPCID.TargetDummy || target.friendly)
                Projectile.Kill();

            teleportPos = new Vector2(target.Center.X, target.Center.Y - (Owner.height / 2));
            target = TargetNext(target);
            if (target != null)
            {
                Projectile.velocity = Projectile.SafeDirectionTo(target.Center, Vector2.UnitY);
                Projectile.velocity *= 15f;
            }
            else
            {
                Projectile.Kill();
            }
        }

        public override void OnKill(int timeLeft)
        {
            if (teleportPos != Vector2.Zero)
            {
                Owner.Teleport(teleportPos, -1);
                NetMessage.SendData(MessageID.TeleportEntity, -1, -1, null, 0, Owner.whoAmI, teleportPos.X, teleportPos.Y, 1);
                Owner.AddBuff(BuffID.Swiftness, 120);
            }
        }
    }
}