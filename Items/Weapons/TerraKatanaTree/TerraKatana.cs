using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MoreKatana.Assets.ExtraTextures;
using MoreKatana.Items.Materials;
using MoreKatana.Projectiles.TerraKatanaTree;
using MoreKatana.Systems.CrossMod;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI.Chat;

namespace MoreKatana.Items.Weapons.TerraKatanaTree
{
    public class TerraKatana : KatanaItem, IAddDrawLayer
    {
        public const int ShieldRechargeTime = 30 * 60;
        public const int ShieldDurabilityMax = 100;
        public const int ShieldDefenseBoost = 10;

        public const int DustType = DustID.Terra;
        public static Color[] TerraColor = [new Color(96, 248, 96), new Color(0, 162, 230)];

        public override LocalizedText FunctionText => base.FunctionText.WithFormatArgs(ShieldDurabilityMax, ShieldDefenseBoost, ShieldRechargeTime / 60);

        public override void SetStaticDefaults()
        {
            Item.AddElement(RedemptionCompat.Nature, true);
            Item.SetSlashBonus();
        }

        public override void SetDefaultsItem()
        {
            Item.width = 62;
            Item.height = 68;

            Item.useTime = 18;
            Item.useAnimation = 18;
            Item.MKItem().UseSound = MoreKatanaSounds.SwordSlash;

            Item.damage = 80;
            Item.knockBack = 4.5f;
            Item.MKItem().AltDamage = 800;

            Item.value = Item.sellPrice(gold: 20);
            Item.rare = ItemRarityID.Yellow;

            Item.MKItem().SetKatanaDefaults(Item, 10 * 60, false, ModContent.ProjectileType<TerraKatanaSwing>(), 5);
        }

        public override void PassiveSkill(Player player, bool equipment)
        {
            player.MKPlayer().terraShield = true;

            if (player.MKPlayer().TerraShieldDurability > 0)
                player.statDefense += ShieldDefenseBoost;
        }

        public override void ActiveSkill(Player player)
        {
            player.ChangeDir(Main.MouseWorld.X - player.Center.X > 0 ? 1 : -1);
            Projectile.NewProjectile(player.GetSource_ItemUse(Item), player.Center, new Vector2(player.direction, 0), ModContent.ProjectileType<TerraDestructionHoldout>(), Item.MKItem().AltDamage / 10, Item.knockBack, player.whoAmI);
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            if (Item.MKItem().AttackType == 5)
                velocity = new Vector2(player.direction, 0);
        }

        public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            // テラブレードのパーティクル
            ParticleOrchestraSettings particleOrchestraSettings = default;
            particleOrchestraSettings.PositionInWorld = Main.rand.NextVector2FromRectangle(target.Hitbox);
            ParticleOrchestrator.RequestParticleSpawn(false, ParticleOrchestraType.TerraBlade, particleOrchestraSettings, player.whoAmI);
        }

        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            if (Main.rand.NextBool(4))
                Dust.NewDust(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height, DustType);
        }

        public override void AddRecipes()
        {
            if (ModLoader.TryGetMod("CalamityMod", out Mod calamity))
            {
                CreateRecipe()
                    .AddIngredient(ItemID.TrueNightsEdge)
                    .AddIngredient<TrueSacredNaginata>()
                    .AddIngredient<BrokenHeroKatana>()
                    .AddIngredient(calamity, "LivingShard", 12)
                    .AddTile(TileID.MythrilAnvil)
                    .Register();
            }
            else
            {
                CreateRecipe()
                    .AddIngredient(ItemID.TrueNightsEdge)
                    .AddIngredient<TrueSacredNaginata>()
                    .AddIngredient<BrokenHeroKatana>()
                    .AddTile(TileID.MythrilAnvil)
                    .Register();
            }
        }

        private Vector2 ShieldCenter;
        public void AdditiveDrawLayer(ref PlayerDrawSet drawinfo)
        {
            Player drawPlayer = drawinfo.drawPlayer;

            if (drawPlayer.dead || drawPlayer.ghost || !drawPlayer.active)
                return;

            if (drawinfo.shadow != 0f)
                return;

            if (!drawPlayer.MKPlayer().terraShield)
                return;

            // シールド
            Texture2D texture = MoreKatanaTextures.ShieldTexture.Value;
            Rectangle rectangle = new Rectangle(0, 0, texture.Width, texture.Height);
            Vector2 origin = rectangle.Size() / 2f;

            const int amount = 5;
            for (int i = 0; i < amount; i++)
            {
                float aroundTime = 20 * amount;
                float globalTimer = Main.GlobalTimeWrappedHourly * 24 * 2;
                float f = (i / (float)amount + (globalTimer / aroundTime)) * ((float)Math.PI * 2f);
                float scaleFactor = 3f + amount * 3f;

                Vector2 value = f.ToRotationVector2();
                Vector2 value2 = drawPlayer.MountedCenter + (value * new Vector2(10f, 0.1f) * scaleFactor);
                ShieldCenter = Vector2.Lerp(ShieldCenter, value2, 0.3f);

                float completion = value.Y;
                float distanceCompletion = drawPlayer.MountedCenter.Distance(ShieldCenter) / ((scaleFactor * 3f) - 5f);

                // シールドのスケール
                Vector2 shieldScale = new Vector2(0.5f + (completion / 10f));
                shieldScale *= new Vector2(1.5f - distanceCompletion, 1.5f);

                // シールドの色
                float lerp = (float)Math.Sin(Main.GameUpdateCount / 15f * i) / 2 + 0.5f;
                Color shieldColor = Color.Lerp(TerraColor[0], TerraColor[1], MoreKatanaUtil.CircInEasing(lerp, 1));
                if (completion < 0f)
                    shieldColor *= distanceCompletion;

                // クールダウンがない場合はシールドを描画する
                if (drawPlayer.MKPlayer().ShieldCD <= 0)
                    Main.spriteBatch.Draw(texture, ShieldCenter + new Vector2(0, drawPlayer.gfxOffY) - Main.screenPosition, rectangle, shieldColor with { A = 0 }, 0f, origin, shieldScale, SpriteEffects.None, 0);
            }

            if (Main.myPlayer == drawPlayer.whoAmI)
            {
                // ゲージの充填率
                float durabilityRatio = (float)drawPlayer.MKPlayer().TerraShieldDurability / ShieldDurabilityMax;
                float cooldownRatio = (float)drawPlayer.MKPlayer().ShieldCD / ShieldRechargeTime;

                // ゲージの位置
                Vector2 gaugePos = new Vector2(drawinfo.Center.X, drawinfo.Center.Y) + new Vector2(0, 35);

                // ゲージとテキストの色
                Color c1 = Color.Lerp(TerraColor[0], TerraColor[1], cooldownRatio);
                Color c2 = Color.Red;
                Color c3 = Color.White;

                // シールドの耐久率が下がった時ゲージを揺らす
                if (durabilityRatio < 0.3f && cooldownRatio == 0)
                    gaugePos += Main.rand.NextVector2Unit();

                // ゲージを描画する
                if (cooldownRatio != 0)
                    MoreKatanaUtil.DrawGauge(gaugePos, 1 - cooldownRatio, c2, c2);
                else
                    MoreKatanaUtil.DrawGauge(gaugePos, durabilityRatio, c1);

                // テキストを描画する
                var font = FontAssets.MouseText.Value;
                int numerator = cooldownRatio == 0 ? drawPlayer.MKPlayer().TerraShieldDurability : (int)(ShieldDurabilityMax * (1 - cooldownRatio));
                string text = MoreKatanaUtil.GetTextValue("Tooltips.Life") + ":" + $"{numerator}" + "/" + $"{ShieldDurabilityMax}";
                Vector2 textPos = gaugePos + new Vector2(-25, 5) - Main.screenPosition;
                ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, font, text, textPos, cooldownRatio != 0 ? c2 : c3, 0f, new Vector2(0.5f, 0.5f), Vector2.One);
            }
        }
    }
}