using Microsoft.Xna.Framework;
using MoreKatana.Buffs;
using MoreKatana.Projectiles.Metal;
using MoreKatana.Systems.CrossMod;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace MoreKatana.Items.Weapons.Metal
{
    public class ObsidianKatana : KatanaItem
    {
        public const int DefenseBonus = 5;
        public const int FireBuffTime = 10 * 60;

        public override LocalizedText FunctionText => base.FunctionText.WithFormatArgs(DefenseBonus, FireBuffTime / 60);

        public override void SetStaticDefaults()
        {
            Item.AddElement(RedemptionCompat.Fire, true);
            Item.SetSlashBonus();
        }

        public override void SetDefaultsItem()
        {
            Item.width = 58;
            Item.height = 58;

            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.MKItem().UseSound = SoundID.Item1;

            Item.damage = 20;
            Item.knockBack = 4.5f;
            Item.MKItem().AltDamage = 30;

            Item.value = Item.sellPrice(silver: 55);
            Item.rare = ItemRarityID.Orange;

            Item.MKItem().SetKatanaDefaults(Item, 120, ModContent.ProjectileType<ObsidianSwing>());
        }

        public override void PassiveSkill(Player player, bool equipment)
        {
            // 防御力のボーナス
            player.statDefense += 5;

            // 燃えるブロックへの耐性
            player.fireWalk = true;
        }

        public override void ActiveSkill(Player player)
        {
            // プレイヤーの向きをマウスの方向に向けて、発射体をスポーンさせる
            player.ChangeDir(Main.MouseWorld.X - player.Center.X > 0 ? 1 : -1);
            Projectile.NewProjectile(player.GetSource_ItemUse(Item), player.Center, new Vector2(player.direction, 0), ModContent.ProjectileType<ObsidianKatanaHoldout>(), Item.MKItem().AltDamage, Item.knockBack, player.whoAmI);
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            // ベロシティをプレイヤーのX軸の向きにする
            velocity = new Vector2(player.direction, 0);
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.Obsidian, 15)
                .AddIngredient(ItemID.LavaBucket, 5)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }

    public class ObsidianKatana_Fire : ObsidianKatana
    {
        public override LocalizedText Tooltip => MoreKatanaUtil.GetText("Items.ObsidianKatana.Tooltip");
        public override LocalizedText FunctionText => MoreKatanaUtil.GetText("Items.ObsidianKatana.FunctionText").WithFormatArgs(DefenseBonus, FireBuffTime / 60);

        public override void SetDefaultsItem()
        {
            base.SetDefaultsItem();
            Item.useTime = 25;
            Item.useAnimation = 25;
            Item.MKItem().SetKatanaDefaults(Item, 120, ModContent.ProjectileType<ObsidianSwing2>(), 2);
        }

        public override bool AltFunctionUseItem(Player player) => false;

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            // アクティブスキル中のなためダメージはAltDamageにする
            damage = Item.MKItem().AltDamage;
        }

        public override void UpdateInventory(Player player)
        {
            // アクティブスキルのバフが無くなった場合
            if (!player.HasBuff<ObsidianBurnBuff>())
            {
                // クールダウンを有効化
                Item.MKItem().ActivateCooldown(player);

                // サウンド
                SoundEngine.PlaySound(MoreKatanaSounds.LiquidsWaterLava, player.Center);

                // ダスト
                for (int i = 0; i < 5; i++)
                {
                    int newDust = Dust.NewDust(player.position, player.width, player.height, DustID.Torch);
                    Main.dust[newDust].scale = Main.rand.NextFloat(0.9f, 1.75f);
                    Main.dust[newDust].noGravity = true;
                    Main.dust[newDust].velocity.Y = -1f;
                    Main.dust[newDust].velocity = Main.dust[newDust].velocity.RotatedByRandom(MathHelper.ToRadians(10));
                    Main.dust[newDust].velocity *= Main.rand.NextFloat(1f, 3f);
                }
                for (int i = 0; i < 18; i++)
                {
                    int newDust = Dust.NewDust(player.position, player.width, player.height, DustID.Smoke, 0.0f, 0f, 150, default, 0.5f);
                    Main.dust[newDust].fadeIn = 1.25f;
                    Main.dust[newDust].noLight = true;
                    Main.dust[newDust].velocity = new Vector2(0f, Main.rand.Next(-2, -1));
                    Main.dust[newDust].velocity = Main.dust[newDust].velocity.RotatedByRandom(MathHelper.ToRadians(15));
                    Main.dust[newDust].velocity *= Main.rand.NextFloat(0.5f, 2f);
                }

                // アイテムを再設置する
                player.ReplaceItem(Item, ModContent.ItemType<ObsidianKatana>());
            }
        }

        public override void AddRecipes()
        {
            // 何もしない
        }
    }
}