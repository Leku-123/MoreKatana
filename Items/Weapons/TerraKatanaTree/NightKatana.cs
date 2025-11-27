using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MoreKatana.Projectiles.TerraKatanaTree;
using MoreKatana.Systems.CrossMod;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;

namespace MoreKatana.Items.Weapons.TerraKatanaTree
{
    public class NightKatana : KatanaItem, IAddDrawLayer
    {
        public const int MaxHitCount = 10;

        public override void SetStaticDefaults()
        {
            Item.AddElement(RedemptionCompat.Shadow, true);
            Item.SetSlashBonus();
        }

        public override void SetDefaultsItem()
        {
            Item.width = 60;
            Item.height = 70;

            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.MKItem().UseSound = MoreKatanaSounds.SwordSlash;

            Item.damage = 34;
            Item.knockBack = 4.5f;
            Item.MKItem().AltDamage = 50;

            Item.value = Item.sellPrice(gold: 4);
            Item.rare = ItemRarityID.Orange;

            Item.MKItem().SetKatanaDefaults(Item, 60, ModContent.ProjectileType<NightKatanaSwing>(), 2);
        }

        public override void PassiveSkill(Player player, bool equipment)
        {
            float ratio = (float)player.MKPlayer().nightHitCount / MaxHitCount;
            player.MKPlayer().nightAuraEffect = true;
            player.statDefense += (int)(5 * ratio);
            player.GetDamage(DamageClass.Melee) += 0.5f * ratio;
            player.DrawColorEffect(Color.Lerp(Color.White, Color.Indigo, ratio).ToVector3());

            if (ratio > 0.5f)
                player.tipsy = true;
            if (ratio == 1f)
            {
                player.moveSpeed += 0.35f;

                if (player.yoraiz0rEye < 2)
                    player.yoraiz0rEye = 2;

                int[] triggers = [MoreKatanaPlayer.Right, MoreKatanaPlayer.Left];
                for (int i = 0; i < triggers.Length; i++)
                {
                    int dashDirection = triggers[i] == MoreKatanaPlayer.Right ? 1 : -1;
                    float dashVelocity = 10f;

                    if (player.MKPlayer().DoubleTap[triggers[i]] && player.MKPlayer().DoubleTapDelay == 0 && !player.mount.Active)
                    {
                        player.immune = true;
                        player.immuneTime = 30;
                        player.UpdateRotation(1, dashDirection, 15);
                        player.MKPlayer().DoubleTapDelay = 120;

                        Vector2 newVelocity = player.velocity;
                        newVelocity.X = dashVelocity * dashDirection;
                        player.velocity = newVelocity;
                        NetMessage.SendData(MessageID.PlayerControls, number: player.whoAmI);
                    }
                }

                if (player.MKPlayer().DoubleTapDelay == 1)
                {
                    SoundEngine.PlaySound(SoundID.MaxMana, player.Center);
                    MoreKatanaUtil.DrawRing(player.Center, DustID.Shadowflame, 24, 10, dustScale: 1.5f);
                }
            }

            if (player.velocity.Y == 0 && !player.mount.Active)
            {
                for (int i = 0; i < 3; i++)
                {
                    int newDust = Dust.NewDust(new Vector2(player.Center.X - player.width, player.Center.Y + player.height / 2), player.width * 2 - 3, 0, Utils.SelectRandom(Main.rand, DustID.Demonite, DustID.Shadowflame), 0, Main.rand.Next(-5, -2), 150, default, 0.8f);
                    Main.dust[newDust].fadeIn = 0.3f;
                    Main.dust[newDust].noGravity = true;
                }
            }
        }

        public override void ActiveSkill(Player player)
        {

        }

        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            if (Main.rand.NextBool(3))
                Dust.NewDust(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height, Utils.SelectRandom(Main.rand, DustID.Demonite, DustID.Shadowflame));
        }

        public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            ParticleOrchestraSettings particleOrchestraSettings = default;
            particleOrchestraSettings.PositionInWorld = Main.rand.NextVector2FromRectangle(target.Hitbox);
            ParticleOrchestrator.RequestParticleSpawn(false, ParticleOrchestraType.NightsEdge, particleOrchestraSettings, player.whoAmI);
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<LightsSlasher>())
                .AddIngredient(ItemID.Muramasa)
                .AddIngredient(ModContent.ItemType<GrassKatana>())
                .AddIngredient(ModContent.ItemType<VolcanoKatana>())
                .AddTile(TileID.DemonAltar)
                .Register();
        }

        public void AdditiveDrawLayer(ref PlayerDrawSet drawinfo)
        {
            Player drawPlayer = drawinfo.drawPlayer;

            if (drawPlayer.dead || drawPlayer.ghost || !drawPlayer.active)
                return;

            if (drawinfo.shadow != 0f)
                return;

            if (drawPlayer.ActiveItem().type != Type || !drawPlayer.MKPlayer().nightAuraEffect)
                return;

            Texture2D texture = ModContent.Request<Texture2D>($"Terraria/Images/Projectile_{ProjectileID.HallowBossDeathAurora}").Value;
            Vector2 drawPosition = drawPlayer.Center - new Vector2(0, 30) + new Vector2(0, drawPlayer.gfxOffY) - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;

            float time = Main.GlobalTimeWrappedHourly % 10f / 10f;
            int drawnAmt = 30;
            float[] posX = new float[drawnAmt];
            float[] posY = new float[drawnAmt];
            float[] size = new float[drawnAmt];
            float sizeScale = 0.8f;
            float sizeScalar = (1f - sizeScale) / drawnAmt;
            float yPosOffset = 10f;
            float xPosOffset = 80f;

            Vector2 scale = new Vector2(0.6f, 1.5f);

            float ratio = (float)drawPlayer.MKPlayer().nightHitCount / MaxHitCount;

            for (int i = 0; i < drawnAmt; i++)
            {
                float timeScalar = (float)Math.Sin(time * MathHelper.TwoPi + (float)Math.PI / 2f + i / 2f);

                posX[i] = timeScalar * (xPosOffset - i * 3f);
                posY[i] = (float)Math.Sin(time * MathHelper.TwoPi * 2f + (float)Math.PI / 3f + i) * yPosOffset;

                size[i] = sizeScale + (i + 1) * sizeScalar;
                size[i] *= 0.3f;

                float rotation = MathHelper.PiOver2 + timeScalar * MathHelper.PiOver4 * -0.3f + (float)Math.PI * i;

                Main.spriteBatch.Draw(texture, drawPosition + new Vector2(posX[i], posY[i]), null, Color.Indigo * 0.2f * ratio, rotation, origin, new Vector2(size[i], size[i]) * scale, SpriteEffects.None, 0);
            }

            if (Main.myPlayer == drawPlayer.whoAmI)
            {
                Vector2 gaugePos = new Vector2(drawinfo.Center.X, drawinfo.Center.Y) + new Vector2(0, 35);
                MoreKatanaUtil.DrawGauge(gaugePos, ratio, Color.Purple);
            }
        }
    }
}