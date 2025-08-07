using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MoreKatana.Assets.ExtraTextures;
using MoreKatana.Projectiles.TerraKatanaTree;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI.Chat;

namespace MoreKatana.Items.Weapons.TerraKatanaTree
{
    public class TrueSacredNaginata : KatanaItem
    {
        public static int ShieldRechargeTime = 30 * 60;
        public static int ShieldDurabilityMax = 75;
        public const int ShieldDefenseBoost = 10;

        public override KatanaID ID => KatanaID.TrueHallowed;

        public override void SetDefaultsItem()
        {
            Item.width = 70;
            Item.height = 80;

            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.UseSound = SoundID.Item169;

            Item.damage = 45;
            Item.knockBack = 4.5f;
            Item.MKItem().AltDamage = 45;

            Item.value = Item.sellPrice(0, 4, 60);
            Item.rare = ItemRarityID.Pink;

            Item.MKItem().SetKatanaDefaults(Item, 60, false, ModContent.ProjectileType<TrueSacredNaginataSwing>(), 3);
        }

        public override void PassiveSkill(Player player, bool equipment)
        {
            player.MKPlayer().trueHolyShield = true;

            if (player.MKPlayer().TrueHolyShieldDurability > 0)
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
            Item.UseSound = SoundID.MaxMana;
            Projectile.NewProjectile(player.GetSource_ItemUse(Item), player.Center, player.SafeDirectionTo(Main.MouseWorld), ModContent.ProjectileType<SacredNaginataHoldout>(), Item.MKItem().AltDamage, Item.knockBack, player.whoAmI);
        }

        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            if (Main.rand.NextBool(3))
                Dust.NewDust(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height, DustID.HallowedWeapons);
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<SacredNaginata>())
                .AddIngredient(ItemID.ChlorophyteBar, 24)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }

        private static Vector2 ShieldCenter;

        public static void DrawTrueHolyShield(ref PlayerDrawSet drawInfo)
        {
            Player drawPlayer = drawInfo.drawPlayer;

            if (drawPlayer.dead || drawPlayer.ghost || !drawPlayer.active)
                return;

            if (drawInfo.shadow != 0f)
                return;

            if (!drawPlayer.MKPlayer().trueHolyShield)
                return;

            // シールド
            Texture2D texture = MoreKatanaTextures.ShieldTexture.Value;
            Rectangle rectangle = new Rectangle(0, 0, texture.Width, texture.Height);
            Vector2 origin = rectangle.Size() / 2f;

            const int amount = 4;
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
                shieldScale *= new Vector2(1.2f - distanceCompletion, 1.2f);

                // シールドの色
                Color shieldColor = Color.Gold;
                if (i % 2 == 0)
                    shieldColor = Color.Crimson;
                if (completion < 0f)
                    shieldColor *= distanceCompletion;

                // クールダウンがない場合はシールドを描画する
                if (drawPlayer.MKPlayer().ShieldCooldown <= 0)
                    Main.spriteBatch.Draw(texture, ShieldCenter - Main.screenPosition, rectangle, shieldColor with { A = 0 }, 0f, origin, shieldScale, SpriteEffects.None, 0);
            }

            // ゲージの描画位置を計算する
            Vector2 spriteSize = new Vector2(50, 50);
            Vector2 ownerPos = drawInfo.Center - Main.screenPosition;
            Vector2 pos = new Vector2(ownerPos.X - spriteSize.X * 0.5f, ownerPos.Y + spriteSize.Y * 0.7f);

            // ゲージごとの色
            Color c1 = Color.Black;
            Color c2 = Color.Gold;
            Color c3 = Color.Red;

            // ゲージの充填率
            float durabilityRatio = (float)drawPlayer.MKPlayer().TrueHolyShieldDurability / ShieldDurabilityMax;
            float cooldownRatio = drawPlayer.MKPlayer().ShieldCooldown / (float)ShieldRechargeTime;

            // シールドの耐久率が下がった時ゲージを揺らす
            if (durabilityRatio < 0.3f && cooldownRatio == 0)
                pos += Main.rand.NextVector2Unit();

            // ゲージを描画する
            Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, pos, new Rectangle(0, 0, 1, 1), c1, 0f, Vector2.Zero, new Vector2(spriteSize.X, 4f), SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, pos, new Rectangle(0, 0, 1, 1), c2, 0f, Vector2.Zero, new Vector2(spriteSize.X * durabilityRatio, 4f), SpriteEffects.None, 0f);
            if (cooldownRatio != 0)
                Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, pos, new Rectangle(0, 0, 1, 1), c3, 0f, Vector2.Zero, new Vector2(spriteSize.X * (1 - cooldownRatio), 4f), SpriteEffects.None, 0f);

            // テキストを描画する
            var font = FontAssets.MouseText.Value;
            int numerator = cooldownRatio == 0 ? drawPlayer.MKPlayer().TrueHolyShieldDurability : (int)(ShieldDurabilityMax * (1 - cooldownRatio));
            string text = Language.GetTextValue("Mods.MoreKatana.Tooltips.Life") + ":" + $"{numerator}" + "/" + $"{ShieldDurabilityMax}";
            Vector2 textPos = pos + new Vector2(0, spriteSize.Y * 0.2f);
            ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, font, text, textPos, cooldownRatio != 0 ? c3 : Color.White, 0f, new Vector2(0.5f, 0.5f), Vector2.One);
        }
    }
}