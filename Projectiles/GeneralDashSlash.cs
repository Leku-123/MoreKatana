using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MoreKatana.Projectiles.PrimTrails;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace MoreKatana.Projectiles
{
    public class GeneralDashSlash : ModProjectile
    {
        public Vector2 DashDirection;
        public int DashDistance;
        public float DashTimerMax;
        public bool SuddenStop;
        public Color TrailColor;

        private bool primsCreated;

        private KatanaSlashPrimTrail trail;

        private Player Owner => Main.player[Projectile.owner];
        private Item ActiveItem => Owner.ActiveItem();

        public override string Texture => MoreKatana.EmptyTexture;

        public override void SetDefaults()
        {
            Projectile.width = Player.defaultWidth;
            Projectile.height = Player.defaultHeight;
            Projectile.aiStyle = -1;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.timeLeft = (int)DashTimerMax + 10;
            Projectile.penetrate = -1;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.ownerHitCheck = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.noEnchantmentVisuals = true;
            Projectile.MKProjectile().SourceIsItemUse = true;
            Projectile.MKProjectile().DashProjectile = true;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.WriteVector2(DashDirection);
            writer.Write7BitEncodedInt(DashDistance);
            writer.Write(DashTimerMax);
            writer.WriteFlags(SuddenStop);
            writer.Write(TrailColor.R);
            writer.Write(TrailColor.G);
            writer.Write(TrailColor.B);
            writer.Write(TrailColor.A);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            DashDirection = reader.ReadVector2();
            DashDistance = reader.Read7BitEncodedInt();
            DashTimerMax = reader.ReadSingle();
            SuddenStop = reader.ReadBoolean();
            TrailColor.R = (byte)reader.Read7BitEncodedInt();
            TrailColor.G = (byte)reader.Read7BitEncodedInt();
            TrailColor.B = (byte)reader.Read7BitEncodedInt();
            TrailColor.A = (byte)reader.Read7BitEncodedInt();
        }

        public override void AI()
        {
            if (Projectile.ai[0] == 0)
            {
                Projectile.ai[0] = 1;

                Owner.GeneralDashEffect(DashDirection, DashDistance, DashTimerMax, SuddenStop);

                Color[] colors = MoreKatanaUtil.GetColors(TextureAssets.Item[ActiveItem.type].Value);
                int a = 0;
                Vector4 vector4 = new Vector4(0, 0, 0, 0);
                for (int i = 0; i < colors.Length; i++)
                {
                    if (colors[i] != new Color(0, 0, 0, 0))
                    {
                        a++;
                        vector4 += colors[i].ToVector4();
                    }
                }
                vector4 /= a * 2;
                TrailColor = new Color(vector4.X, vector4.Y, vector4.Z, 0);

                Projectile.netUpdate = true;
            }

            if (!primsCreated)
            {
                primsCreated = true;
                if (Main.netMode != NetmodeID.Server)
                {
                    trail = new KatanaSlashPrimTrail(Projectile, TrailColor);
                    MoreKatana.primitives.CreateTrail(trail);
                }

                SoundEngine.PlaySound(SoundID.Item71, Owner.position);

                for (int i = 0; i < 12; i++)
                {
                    int newDust = Dust.NewDust(Owner.MountedCenter, 32, 32, DustID.Smoke, 0f, 0f, 100, default, 2f);
                    Main.dust[newDust].velocity -= DashDirection * 2f;
                    Main.dust[newDust].velocity = Main.dust[newDust].velocity.RotatedByRandom(MathHelper.ToRadians(15));
                    Main.dust[newDust].velocity *= Main.rand.NextFloat(1f, 3f);
                }
            }

            Projectile.Center = Owner.MountedCenter;
            Projectile.spriteDirection = Owner.direction;
            Projectile.rotation = DashDirection.ToRotation() + (Projectile.spriteDirection == 1 ? MathHelper.ToRadians(45f) : MathHelper.ToRadians(135f));

            Owner.heldProj = Projectile.whoAmI;
            Owner.SetDummyItemTime(2);
            Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, DashDirection.ToRotation() - MathHelper.ToRadians(90f));
            Owner.armorEffectDrawShadow = true;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Item[ActiveItem.type].Value;

            DrawAnimation animation = Main.itemAnimations[ActiveItem.type];

            Vector2 offset = DashDirection * (texture.Width - 4);
            Vector2 position = Projectile.Center + offset - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);
            Rectangle? rectangle = new Rectangle?(animation == null ? texture.Frame(1, 1, 0, 0, 0, 0) : animation.GetFrame(texture, -1));

            float frame = animation == null ? 1 : animation.FrameCount;
            Vector2 origin = new Vector2(texture.Width / 2, texture.Height / frame / 2);

            SpriteEffects spriteEffects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            MoreKatanaUtil.DrawBackglow(texture, position, (Rectangle)rectangle, Color.White with { A = 0 }, Projectile.rotation, 2f, new Vector2(Projectile.scale), spriteEffects);

            Main.EntitySpriteDraw(texture, position, rectangle, Projectile.GetAlpha(lightColor), Projectile.rotation, origin, Projectile.scale, spriteEffects, 0);

            return false;
        }
    }
}