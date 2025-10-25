using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MoreKatana.Buffs;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace MoreKatana.Projectiles.TerraKatanaTree
{
    public class TerraHitMark : ModProjectile
    {
        private NPC Target => Main.npc[(int)Projectile.ai[0]];

        private float counter;
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Curse");
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 3;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.hostile = false;
            Projectile.width = 32;
            Projectile.height = 28;
            Projectile.aiStyle = -1;
            Projectile.friendly = false;
            Projectile.penetrate = 1;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 12;
            Projectile.ignoreWater = true;
        }

        private readonly Color Red = new Color(242, 41, 58);
        private readonly Color Black = new Color(38, 10, 12);

        public override Color? GetAlpha(Color lightColor) => Color.White * .6f;

        public override void AI()
        {
            Lighting.AddLight(Projectile.Center, Color.Red.ToVector3() * 0.3f);
            counter += 0.025f;
            if (Target.active)
            {
                if (Target.HasBuff(ModContent.BuffType<TerraMark>()))
                {
                    Projectile.scale = MathHelper.Clamp(counter * 3, 0, 1);
                    Projectile.timeLeft = 12;
                }
                else
                    Projectile.scale -= 0.083f;
                Projectile.Center = Target.Center;
            }
            else
                Projectile.active = false;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = TextureAssets.Projectile[Projectile.type].Value;
            float progress = counter % 1;
            float transparency = (float)Math.Pow(1 - progress, 2);
            float scale = 1 + progress;

            Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, Color.White * transparency, Projectile.rotation, tex.Size() / 2, scale * Projectile.scale, SpriteEffects.None, 0f);
            return true;
        }

        public override void PostDraw(Color lightColor) => DrawBloom(Main.spriteBatch, new Color(96, 248, 96) * 0.33f, 0.48f);

        protected void DrawBloom(SpriteBatch spriteBatch, Color color, float scale)
        {
            Texture2D glow = TextureAssets.Extra[ExtrasID.ScreenObfuscation].Value;
            color.A = 0;

            float glowScale = 1 + (float)Math.Sin(counter) / 4;

            spriteBatch.Draw(glow, Projectile.Center - Main.screenPosition, null,
                color * glowScale, 0, glow.Size() / 2, Projectile.scale * scale, SpriteEffects.None, 0f);
        }
        public override void OnKill(int timeLeft)
        {
            if (timeLeft > 4)
            {
                Target.SimpleStrikeNPC(Projectile.damage, 0, false, 0);

                //Projectile.NewProjectile(Projectile.GetSource_Death(), Target.Center, Vector2.Zero, ModContent.ProjectileType<CurseBreak>(), 0, 0, Projectile.owner, Target.whoAmI);
                SoundEngine.PlaySound(SoundID.DD2_BetsyFireballImpact, Projectile.Center);
            }
        }
    }
}