using Microsoft.Xna.Framework;
using MoreKatana.Projectiles.PrimTrails;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MoreKatana.Projectiles.Misc
{
    public class KatanaSlash : ModProjectile
    {
        private ref float Timer => ref Projectile.ai[0];

        private const int projPenet = 6;
        private readonly NPC[] hit = new NPC[projPenet];

        private Vector2 teleportPos;

        private bool primsCreated;

        private KatanaSlashPrimTrail trail;

        private Player Owner => Main.player[Projectile.owner];

        public override string Texture => MoreKatana.EmptyTexture;

        public override void SetDefaults()
        {
            Projectile.width = 4;
            Projectile.height = 4;
            Projectile.aiStyle = -1;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = projPenet;
            Projectile.timeLeft = 36000;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.hide = true;
            Projectile.noEnchantmentVisuals = true;
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
            }
            else
            {
                Projectile.extraUpdates = 7;
            }

            Owner.immune = true;
            Owner.immuneTime = 120;
            Owner.immuneAlpha = 255;
            Owner.AddBuff(BuffID.Swiftness, 120);

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
            NPC target = null;
            var center = Projectile.Center;
            for (int i = 0; i < Main.maxNPCs; ++i)
            {
                NPC npc = Main.npc[i];
                if (npc != current && npc.active && npc.CanBeChasedBy(null) && CanTarget(npc))
                {
                    float dist = Vector2.Distance(center, npc.Center);
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
            if (target.type == NPCID.TargetDummy || target.friendly)
                Projectile.Kill();

            Owner.ScreenShake(5, 15);
            hit[Projectile.penetrate - 1] = target;
            Timer = 0;

            if (Main.netMode != NetmodeID.Server)
                trail.Points.Add(target.Center);

            if (Main.myPlayer == Projectile.owner)
            {
                float maxOffset = target.width * 0.4f;
                if (maxOffset > 300f)
                    maxOffset = 300f;

                Vector2 spawnOffset = (MathHelper.Pi + Main.rand.NextFloatDirection() * 0.2f).ToRotationVector2() * Main.rand.NextFloatDirection() * maxOffset;
                Vector2 sliceVelocity = spawnOffset.SafeNormalize(Vector2.UnitY) * 0.1f;
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.Center + spawnOffset, sliceVelocity, ModContent.ProjectileType<KatanaSlashEffect>(), Projectile.damage, 0f, Projectile.owner);
            }

            teleportPos = new Vector2(target.Center.X, target.Center.Y - (Owner.height / 2));
            target = TargetNext(target);
            if (target != null)
            {
                Projectile.velocity = Projectile.SafeDirectionTo(target.Center, Vector2.UnitY);
                Projectile.velocity *= 15f;
                Projectile.netUpdate = true;
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
                // テレポート
                Owner.Teleport(teleportPos, -1);
                NetMessage.SendData(MessageID.TeleportEntity, -1, -1, null, 0, Owner.whoAmI, teleportPos.X, teleportPos.Y, -1);

                // プレイヤーの反動
                if (Projectile.velocity != Vector2.Zero)
                {
                    Owner.velocity = Vector2.Normalize(Projectile.velocity) * 8f;
                    NetMessage.SendData(MessageID.PlayerControls, number: Owner.whoAmI);
                }
            }
        }
    }
}