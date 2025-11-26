using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace MoreKatana.Projectiles.TerraKatanaTree
{
    public class SacredEdge : ModProjectile
    {
        private ref float Timer => ref Projectile.ai[0];

        public const int FadeOutLength = 15;

        private bool fadeOut;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
        }

        public override void SetDefaults()
        {
            Projectile.width = 45;
            Projectile.height = 45;
            Projectile.aiStyle = -1;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 60;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void SendExtraAI(BinaryWriter writer) => writer.Write(fadeOut);
        public override void ReceiveExtraAI(BinaryReader reader) => fadeOut = reader.ReadBoolean();

        public override void AI()
        {
            Projectile.spriteDirection = (Vector2.Dot(Projectile.velocity, Vector2.UnitX) >= 0f).ToDirectionInt();
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2 - MathHelper.PiOver4 * Projectile.spriteDirection;

            if (fadeOut)
            {
                Projectile.friendly = false;
                Projectile.velocity *= 0.78f;
                Projectile.Opacity = Utils.GetLerpValue(FadeOutLength, 0f, Timer, true);

                if (Projectile.Opacity == 0f)
                    Projectile.Kill();

                Timer++;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Projectile.netUpdate = true;
            fadeOut = true;
            Timer = 0;
        }

        public override Color? GetAlpha(Color lightColor) => new Color(255, 255, 255, 100) * Projectile.Opacity;

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Texture2D glowTex = ModContent.Request<Texture2D>(GlowTexture).Value;

            Rectangle rectangle = new Rectangle(0, 0, texture.Width, texture.Height);
            Vector2 origin = rectangle.Size() / 2f;
            Vector2 position = Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);
            Color color = Projectile.GetAlpha(lightColor);
            Color glowColor = Color.Gold * Projectile.Opacity;
            SpriteEffects spriteEffects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            for (int i = 0; i < ProjectileID.Sets.TrailCacheLength[Projectile.type]; i++)
            {
                float fade = (float)(ProjectileID.Sets.TrailCacheLength[Projectile.type] - i) / ProjectileID.Sets.TrailCacheLength[Projectile.type];
                Color color2 = color;
                color2 *= fade;
                glowColor *= fade;
                Vector2 pos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition + new Vector2(0, Projectile.gfxOffY);
                Main.EntitySpriteDraw(glowTex, pos, rectangle, glowColor with { A = 0 }, Projectile.rotation, origin, Projectile.scale, spriteEffects, 0);
                Main.EntitySpriteDraw(texture, pos, rectangle, color2, Projectile.rotation, origin, Projectile.scale, spriteEffects, 0);
            }

            Main.EntitySpriteDraw(texture, position, rectangle, color, Projectile.rotation, origin, Projectile.scale, spriteEffects, 0);
            return false;
        }
    }
}