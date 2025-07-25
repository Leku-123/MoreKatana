using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MoreKatana.Assets.ExtraTextures;
using MoreKatana.Items.Katana.Gem;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace MoreKatana.Projectiles.Gem
{
    public class AmethystShards : ModProjectile
    {
        public override void SetStaticDefaults() => Main.projFrames[Projectile.type] = 3;

        public override void SetDefaults()
        {
            Projectile.width = 18;
            Projectile.height = 18;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 36000;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.scale = 1f;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 30;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            if (!player.active || player.dead)
            {
                Projectile.Kill();
                return;
            }
            if (player.HeldItem.type != ModContent.ItemType<AmethystKatana>())
            {
                Projectile.Kill();
                return;
            }

            Projectile.timeLeft = 2;

            if (Projectile.frameCounter == 0)
            {
                Projectile.frameCounter = 1;
                Projectile.frame = Main.rand.Next(3);
                Projectile.rotation = Main.rand.NextFloat() * ((float)Math.PI * 2f);
            }

            float aroundTime = 90;
            float globalTimer = Main.GlobalTimeWrappedHourly * 24 * 2;

            Projectile.rotation += (float)Math.PI / 200f;
            AI_GetMyGroupIndexAndFillBlackList(null, out var index, out var totalIndexesInGroup);
            float f = (index / (float)totalIndexesInGroup + (globalTimer / aroundTime)) * ((float)Math.PI * 2f);
            float scaleFactor = 18f + totalIndexesInGroup * 7f;
            Vector2 vector = player.position - player.oldPosition;
            Projectile.Center += vector;
            Vector2 value = f.ToRotationVector2();
            Projectile.localAI[0] = value.Y;
            Vector2 value2 = player.Center + (value * new Vector2(2f, 0.1f) * scaleFactor);
            Projectile.Center = Vector2.Lerp(Projectile.Center, value2, 0.3f);
            Projectile.scale = 1f + (Projectile.localAI[0] / 4f);
        }

        private void AI_GetMyGroupIndexAndFillBlackList(List<int> blackListedTargets, out int index, out int totalIndexesInGroup)
        {
            index = 0;
            totalIndexesInGroup = 0;
            for (int i = 0; i < 1000; i++)
            {
                Projectile projectile = Main.projectile[i];
                if (projectile.active && projectile.owner == Projectile.owner && projectile.type == Projectile.type && (projectile.type != 759 || projectile.frame == Main.projFrames[projectile.type] - 1))
                {
                    if (Projectile.whoAmI > i)
                    {
                        index++;
                    }
                    totalIndexesInGroup++;
                }
            }
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            if (Projectile.localAI[0] >= 0f)
                overPlayers.Add(index);
            else
                behindNPCs.Add(index);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Texture2D bloomTex = MoreKatanaTextureRegistry.BloomTexture.Value;

            Rectangle rectangle = texture.Frame(1, Main.projFrames[Projectile.type], 0, Projectile.frame);
            Vector2 origin = rectangle.Size() / 2f;
            Vector2 position = Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);
            SpriteEffects spriteEffects = (Projectile.spriteDirection == -1) ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            Main.EntitySpriteDraw(bloomTex, position, null, Color.Magenta with { A = 0 }, Projectile.rotation, bloomTex.Size() / 2f, Projectile.scale * 0.15f, 0, 0);
            Main.EntitySpriteDraw(texture, position, new Rectangle?(rectangle), Projectile.GetAlpha(lightColor), Projectile.rotation, origin, Projectile.scale, spriteEffects, 0);
            return false;
        }
    }
}