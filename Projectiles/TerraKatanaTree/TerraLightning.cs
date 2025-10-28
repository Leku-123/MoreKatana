using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace MoreKatana.Projectiles.TerraKatanaTree
{
    public class TerraLightning : ModProjectile
    {
        private ref float Timer => ref Projectile.ai[0];

        private const int LightningLength = 500;

        public Vector2 BaseVelocity = Vector2.Zero;

        private Vector2[] points;

        private bool fadeOut;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = LightningLength;
        }

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.aiStyle = -1;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = LightningLength;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 30;
            Projectile.extraUpdates = LightningLength / 5;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            if (points != null)
            {
                writer.Write(points.Length);
                for (int i = 0; i < points.Length; i++)
                    writer.WriteVector2(points[i]);
            }

            writer.Write(fadeOut);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            if (points != null)
            {
                int length = reader.ReadInt32();
                for (int i = 0; i < length; i++)
                    points[i] = reader.ReadVector2();
            }

            fadeOut = reader.ReadBoolean();
        }

        public override void AI()
        {
            // BaseVelocityに発射体のベロシティを保存
            if (BaseVelocity == Vector2.Zero)
            {
                BaseVelocity = Vector2.Normalize(Projectile.velocity) * 3;
                Projectile.velocity = BaseVelocity;
            }

            if (Projectile.position.Y > Main.player[Projectile.owner].position.Y)
                Projectile.tileCollide = true;

            Timer++;
            if (Timer > 20 && Main.rand.NextBool(10))
            {
                Timer = 0;

                NPC npc = Projectile.Center.ClosestNPCAt(500);

                const float f = 0.6f;
                if (npc != null && Vector2.Subtract(npc.Center, Projectile.Center).Y > 0 && npc.CanBeChasedBy())
                {
                    float randF = Vector2.Subtract(npc.Center, Projectile.Center).Length() / 300f;
                    if (randF > 0.6f)
                        randF = 0.6f;

                    Vector2 vector = Utils.RotatedBy(Vector2.Subtract(npc.Center, Projectile.Center).Normalized() * Projectile.velocity.Length(), Main.rand.NextFloat(-randF, randF), default);
                    Projectile.velocity = vector;
                }
                else
                {
                    Vector2 vector = Utils.RotatedBy(BaseVelocity.Normalized() * Projectile.velocity.Length(), Main.rand.NextFloat(-f, f), default);
                    Projectile.velocity = vector;
                }

                Projectile.netUpdate = true;
            }

            if (Projectile.timeLeft <= 2 * Projectile.MaxUpdates && !fadeOut)
            {
                if (points == null)
                {
                    points = new Vector2[Projectile.oldPos.Length];
                    for (int i = 0; i < Projectile.oldPos.Length; i++)
                    {
                        points[i] = Projectile.oldPos[i];
                    }
                }

                Projectile.timeLeft = 2 * Projectile.MaxUpdates;
                fadeOut = true;
                Projectile.netUpdate = true;
            }

            if (fadeOut)
            {
                Projectile.extraUpdates = 0;
                Projectile.timeLeft = 2;
                Projectile.Opacity -= 0.025f;
                Projectile.velocity = Vector2.Zero;
                Projectile.damage = 0;

                if (Projectile.Opacity <= 0)
                {
                    Projectile.Kill();
                }
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Projectile.timeLeft = 2 * Projectile.MaxUpdates;
            Projectile.netUpdate = true;
            return false;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 vector = Projectile.Size / 2;

            if (points == null)
            {
                for (int i = 0; i < Projectile.oldPos.Length; i++)
                {
                    if (Projectile.oldPos[i] != Projectile.position)
                    {
                        Vector2 vector2 = Projectile.oldPos[i] + vector - Main.screenPosition;
                        float scale = Projectile.scale / 6 + 0.15f + (float)i / LightningLength;
                        Main.spriteBatch.Draw(texture, vector2, null, new Color(61, 233, 40, 0) * 0.5f * Projectile.Opacity, Projectile.rotation, texture.Size() / 2, scale, SpriteEffects.None, 0f);
                    }
                }
            }
            else
            {
                for (int i = 0; i < points.Length; i++)
                {
                    if (points[i] != Projectile.position)
                    {
                        Vector2 vector2 = points[i] + vector - Main.screenPosition;
                        float scale = Projectile.scale / 6 + 0.15f + (float)i / LightningLength;
                        Main.spriteBatch.Draw(texture, vector2, null, new Color(61, 233, 40, 0) * 0.5f * Projectile.Opacity, Projectile.rotation, texture.Size() / 2, scale, SpriteEffects.None, 0f);
                    }
                }
            }
            return false;
        }
    }
}