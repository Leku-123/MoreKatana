using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MoreKatana.Assets.ExtraTextures;
using MoreKatana.Projectiles.TerraKatanaTree;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI.Chat;

namespace MoreKatana.Items.Weapons.TerraKatanaTree
{
    public class SacredNaginata : KatanaItem
    {
        public static int ShieldRechargeTime = 30 * 60;
        public static int ShieldDurabilityMax = 50;
        public const int ShieldDefenseBoost = 10;

        public override LocalizedText FunctionText => base.FunctionText.WithFormatArgs(ShieldDurabilityMax, ShieldDefenseBoost, ShieldRechargeTime / 60);

        public override KatanaID ID => KatanaID.Hallowed;

        public override void SetDefaultsItem()
        {
            Item.width = 70;
            Item.height = 80;

            Item.useTime = 35;
            Item.useAnimation = 35;
            Item.MKItem().UseSound = SoundID.Item169;

            Item.damage = 45;
            Item.knockBack = 4.5f;
            Item.MKItem().AltDamage = 45;

            Item.value = Item.sellPrice(0, 4, 60);
            Item.rare = ItemRarityID.Pink;

            Item.MKItem().SetKatanaDefaults(Item, 60, false, ModContent.ProjectileType<SacredNaginataSwing>(), 3);
        }

        public override void PassiveSkill(Player player, bool equipment)
        {
            player.MKPlayer().holyShield = true;

            if (player.MKPlayer().HolyShieldDurability > 0)
                player.statDefense += ShieldDefenseBoost;

            if (player.velocity.Y == 0 && !player.mount.Active)
            {
                for (int i = 0; i < 3; i++)
                {
                    int newDust = Dust.NewDust(new Vector2(player.Center.X - player.width, player.Center.Y + player.height / 2), player.width * 2 - 3, 0, DustID.HallowedWeapons, 0, Main.rand.Next(-5, -2), 150, default, 0.5f);
                    Main.dust[newDust].fadeIn = 0.3f;
                    Main.dust[newDust].noGravity = true;
                }
            }
        }

        public override void ActiveSkill(Player player)
        {
            SoundEngine.PlaySound(SoundID.MaxMana, player.Center);
            Projectile.NewProjectile(player.GetSource_ItemUse(Item), player.Center, player.SafeDirectionTo(player.MKPlayer().MouseWorld), ModContent.ProjectileType<SacredNaginataHoldout>(), Item.MKItem().AltDamage, Item.knockBack, player.whoAmI);
        }

        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            if (Main.rand.NextBool(3))
                Dust.NewDust(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height, DustID.HallowedWeapons);
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.HallowedBar, 12)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }

        private static Vector2 ShieldCenter;

        public static void DrawHolyShield(ref PlayerDrawSet drawInfo)
        {
            Player drawPlayer = drawInfo.drawPlayer;

            if (drawPlayer.dead || drawPlayer.ghost || !drawPlayer.active)
                return;

            if (drawInfo.shadow != 0f)
                return;

            if (!drawPlayer.MKPlayer().holyShield)
                return;

            // シールド
            Texture2D texture = MoreKatanaTextures.ShieldTexture.Value;
            Rectangle rectangle = new Rectangle(0, 0, texture.Width, texture.Height);
            Vector2 origin = rectangle.Size() / 2f;

            const int amount = 3;
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
                shieldScale *= new Vector2(1f - distanceCompletion, 1f);

                // シールドの色
                Color shieldColor = Color.Gold;
                if (completion < 0f)
                    shieldColor *= distanceCompletion;

                // クールダウンがない場合はシールドを描画する
                if (drawPlayer.MKPlayer().ShieldCD <= 0)
                    Main.spriteBatch.Draw(texture, ShieldCenter - Main.screenPosition, rectangle, shieldColor with { A = 0 }, 0f, origin, shieldScale, SpriteEffects.None, 0);
            }

            if (Main.myPlayer == drawPlayer.whoAmI)
            {
                // ゲージの充填率
                float durabilityRatio = (float)drawPlayer.MKPlayer().HolyShieldDurability / ShieldDurabilityMax;
                float cooldownRatio = (float)drawPlayer.MKPlayer().ShieldCD / ShieldRechargeTime;

                // ゲージの位置
                Vector2 gaugePos = new Vector2(drawInfo.Center.X, drawInfo.Center.Y) + new Vector2(0, 35);

                // ゲージとテキストの色
                Color c1 = Color.Gold;
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
                int numerator = cooldownRatio == 0 ? drawPlayer.MKPlayer().HolyShieldDurability : (int)(ShieldDurabilityMax * (1 - cooldownRatio));
                string text = MoreKatanaUtil.GetTextValue("Tooltips.Life") + ":" + $"{numerator}" + "/" + $"{ShieldDurabilityMax}";
                Vector2 textPos = gaugePos + new Vector2(-25, 5) - Main.screenPosition;
                ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, font, text, textPos, cooldownRatio != 0 ? c2 : c3, 0f, new Vector2(0.5f, 0.5f), Vector2.One);
            }
        }
    }
}