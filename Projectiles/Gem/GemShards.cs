using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MoreKatana.Assets.ExtraTextures;
using MoreKatana.Items.Weapons.Gem;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace MoreKatana.Projectiles.Gem
{
    public abstract class GemShards : ModProjectile
    {
        private readonly Color GlowColor;
        private readonly int DustType;
        private readonly int ItemType;

        public GemShards(Color glowColor, int dustType, int itemType)
        {
            GlowColor = glowColor;
            DustType = dustType;
            ItemType = itemType;
        }

        private ref float RotTimer => ref Projectile.ai[1];
        private ref float SkillTimer => ref Projectile.ai[0];

        public const float AssembleTime = 120f;
        public float AssembleCompletion => MathHelper.Clamp(SkillTimer / AssembleTime, 0f, 1f);

        private bool activateSkill;
        private bool flyaway;

        public override void SetStaticDefaults() => Main.projFrames[Projectile.type] = 3;

        public override void SetDefaults()
        {
            Projectile.width = 18;
            Projectile.height = 18;
            Projectile.DamageType = DamageClass.Melee;
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

        public override void SendExtraAI(BinaryWriter writer) => writer.Write(activateSkill);

        public override void ReceiveExtraAI(BinaryReader reader) => activateSkill = reader.ReadBoolean();

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            if (!player.active || player.dead)
            {
                Projectile.Kill();
                return;
            }
            if (player.HeldItem.type != ItemType)
            {
                Projectile.Kill();
                return;
            }

            if (Projectile.frameCounter == 0)
            {
                Projectile.frameCounter = 1;
                Projectile.frame = Main.rand.Next(3);
                Projectile.rotation = Main.rand.NextFloat() * ((float)Math.PI * 2f);
            }

            AI_GetMyGroupIndexAndFillBlackList(null, out var index, out var totalIndexesInGroup);

            if (player.IsUsingAlt())
            {
                activateSkill = true;
                Projectile.netUpdate = true;
            }

            if (SkillTimer == 1)
            {
                if (index == 0)
                {
                    SoundEngine.PlaySound(SoundID.MaxMana, player.Center);
                    MoreKatanaUtil.DrawRing(player.Center, [DustType], 24, 10f);
                }
            }

            RotTimer++;

            if (activateSkill)
                SkillTimer++;

            if (AssembleCompletion != 1f)
            {
                float aroundTime = !activateSkill ? 90 : 45;
                float f = (index / (float)totalIndexesInGroup + (RotTimer / aroundTime)) * ((float)Math.PI * 2f);
                float scaleFactor = 18f + totalIndexesInGroup * 7f;
                Vector2 vector = player.position - player.oldPosition;
                Projectile.Center += vector;
                Vector2 value = f.ToRotationVector2();
                Projectile.localAI[0] = value.Y;
                Vector2 value2 = player.Center + new Vector2(0, -50 * AssembleCompletion) + (value * new Vector2(2f * (1 - AssembleCompletion), 0.1f) * scaleFactor);
                Projectile.Center = Vector2.Lerp(Projectile.Center, value2, 0.3f);
                Projectile.scale = 1f + (Projectile.localAI[0] / 4f);
                Projectile.scale += 0.5f * AssembleCompletion;
                Projectile.rotation += (float)Math.PI / (!activateSkill ? 200f : 50f);
                Projectile.timeLeft = 60;
                Projectile.ExpandHitboxBy((int)(18 * Projectile.scale));

                if (activateSkill)
                {
                    player.SetDummyItemTime(2);

                    float armRot = player.DirectionTo(Projectile.Top).ToRotation() - ((float)Math.PI / 2f);
                    if (index == 0)
                        player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, armRot);
                    else if (index == 1)
                        player.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, armRot);
                }
            }
            else
            {
                if (AssembleCompletion == 1f && SkillTimer == AssembleTime)
                {
                    Projectile.penetrate = 1;
                    Projectile.damage = player.HeldItem.MKItem().AltDamage;

                    Vector2 direct = Projectile.DirectionTo(player.MKPlayer().MouseWorld);
                    float speed = 25f;
                    Projectile.velocity += direct * speed;

                    if (index == 0)
                    {
                        player.ScreenShake(5, 6);
                        player.HeldItem?.MKItem().ActivateCooldown(player);
                        SoundEngine.PlaySound(SoundID.Item29, player.Center);
                        SoundEngine.PlaySound(MoreKatanaSounds.SwordSlash, player.Center);

                        if (Projectile.owner == Main.myPlayer)
                            Projectile.NewProjectile(player.GetSource_ItemUse(player.ActiveItem()), player.Center, Vector2.Normalize(Projectile.velocity), ModContent.ProjectileType<GeneralKatanaSwing>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
                    }

                    Projectile.netUpdate = true;
                }

                if (!flyaway)
                {
                    flyaway = true;
                    for (int i = 0; i < 3; i++)
                    {
                        int newDust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustType, 0f, 0f, 100, default, 1.5f);
                        Main.dust[newDust].scale *= Main.rand.NextFloat(1, 2.5f);
                        Main.dust[newDust].noGravity = true;
                        Main.dust[newDust].velocity = Vector2.Normalize(Projectile.velocity) * 10f;
                        Main.dust[newDust].velocity = Main.dust[newDust].velocity.RotatedByRandom(MathHelper.ToRadians(30));
                        Main.dust[newDust].velocity *= Main.rand.NextFloat(1f, 3f);
                    }
                }
                else
                {
                    int fourConst = 4;
                    for (int i = 0; i < 2; i++)
                    {
                        float shortXVel = Projectile.velocity.X / 3f * i;
                        float shortYVel = Projectile.velocity.Y / 3f * i;
                        int newDust = Dust.NewDust(new Vector2(Projectile.position.X + fourConst, Projectile.position.Y + fourConst), Projectile.width - (fourConst * 2), Projectile.height - (fourConst * 2), DustType, 0f, 0f, 100, default, 1.2f);
                        Main.dust[newDust].noGravity = true;
                        Main.dust[newDust].velocity *= 0.1f;
                        Main.dust[newDust].velocity += Projectile.velocity * 0.1f;
                        Main.dust[newDust].position.X -= shortXVel;
                        Main.dust[newDust].position.Y -= shortYVel;
                    }
                    if (Main.rand.NextBool(5))
                    {
                        int newDust2 = Dust.NewDust(new Vector2(Projectile.position.X + fourConst, Projectile.position.Y + fourConst), Projectile.width - (fourConst * 2), Projectile.height - (fourConst * 2), DustType, 0f, 0f, 100, default, 0.6f);
                        Main.dust[newDust2].velocity *= 0.25f;
                        Main.dust[newDust2].velocity += Projectile.velocity * 0.5f;
                    }
                }
            }
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
                        index++;

                    totalIndexesInGroup++;
                }
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!activateSkill || AssembleCompletion != 1f)
                return;

            SoundEngine.PlaySound(SoundID.DD2_WitherBeastCrystalImpact, target.Center);
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
            Texture2D bloomTex = MoreKatanaTextures.BloomTexture.Value;

            Rectangle rectangle = texture.Frame(1, Main.projFrames[Projectile.type], 0, Projectile.frame);
            Vector2 origin = rectangle.Size() / 2f;
            Vector2 position = Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);
            SpriteEffects spriteEffects = (Projectile.spriteDirection == -1) ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            Main.EntitySpriteDraw(bloomTex, position, null, GlowColor with { A = 0 }, Projectile.rotation, bloomTex.Size() / 2f, Projectile.scale * 0.15f, SpriteEffects.None, 0);
            MoreKatanaUtil.DrawBackglow(texture, position, rectangle, Color.White with { A = 0 }, Projectile.rotation, 4f * AssembleCompletion, new Vector2(Projectile.scale), spriteEffects);
            Main.EntitySpriteDraw(texture, position, rectangle, Projectile.GetAlpha(lightColor), Projectile.rotation, origin, Projectile.scale, spriteEffects, 0);
            return false;
        }
    }

    public class GemShards_Amethyst : GemShards
    {
        public GemShards_Amethyst() : base(Color.Magenta, DustID.GemAmethyst, ModContent.ItemType<AmethystKatana>()) { }
    }
    public class GemShards_Topaz : GemShards
    {
        public GemShards_Topaz() : base(Color.Orange, DustID.GemTopaz, ModContent.ItemType<TopazKatana>()) { }
    }
    public class GemShards_Sapphire : GemShards
    {
        public GemShards_Sapphire() : base(Color.DeepSkyBlue, DustID.GemSapphire, ModContent.ItemType<SapphireKatana>()) { }
    }
    public class GemShards_Emerald : GemShards
    {
        public GemShards_Emerald() : base(Color.SpringGreen, DustID.GemEmerald, ModContent.ItemType<EmeraldKatana>()) { }
    }
    public class GemShards_Ruby : GemShards
    {
        public GemShards_Ruby() : base(Color.Red, DustID.GemRuby, ModContent.ItemType<RubyKatana>()) { }
    }
    public class GemShards_Diamond : GemShards
    {
        public GemShards_Diamond() : base(Color.White, DustID.GemDiamond, ModContent.ItemType<DiamondKatana>()) { }
    }
}