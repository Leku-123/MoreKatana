using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MoreKatana.Buffs;
using MoreKatana.Projectiles.Metal;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace MoreKatana.Items.Weapons.Metal
{
    public class ObsidianKatana : KatanaItem
    {
        public override KatanaID ID => KatanaID.Obsidian;

        public const int FireBuffTime = 60 * 5;

        private int SwingType = ProjectileID.None;
        private int Combo = 1;
        private bool FireTrigger = false;

        public static Asset<Texture2D> FireTexture;
       
        public override void SetStaticDefaults()
        {
            FireTexture = ModContent.Request<Texture2D>(Texture + "_Fire");
        }

        public override void SetDefaultsItem()
        {
            Item.width = 58;
            Item.height = 58;

            Item.useTime = 25;
            Item.useAnimation = 25;
            Item.UseSound = SoundID.Item1;

            Item.damage = 20;
            Item.knockBack = 4.5f;
            Item.MKItem().AltDamage = 30;

            Item.value = Item.sellPrice(silver: 55);
            Item.rare = ItemRarityID.Orange;

            Item.MKItem().SetKatanaDefaults(Item, 120, true, SwingType, Combo);
        }

        /// <summary>
        /// プレイヤーに Obsidian Burn が付与されているかどうか
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        private bool Fire(Player player) => player.HasBuff<ObsidianBurnBuff>();

        public override bool AltFunctionUseItem(Player player) => !Fire(player);

        public override void PassiveSkill(Player player, bool equipment)
        {
            player.statDefense += 5;

            if (!equipment)
            {
                if (!Fire(player))
                {
                    Item.useTime = 30;
                    Item.useAnimation = 30;
                    Item.SetNameOverride(MoreKatanaUtil.GetTextValue("Items.ObsidianKatana.DisplayName"));
                    SwingType = ModContent.ProjectileType<ObsidianSwing>();
                    Combo = 1;
                }
                else
                {
                    Item.useTime = 25;
                    Item.useAnimation = 25;
                    Item.SetNameOverride(MoreKatanaUtil.GetTextValue("Items.ObsidianKatana.AltName"));
                    SwingType = ModContent.ProjectileType<ObsidianSwing2>();
                    Combo = 2;
                    FireTrigger = true;
                }
            }
        }

        public override void ActiveSkill(Player player)
        {
            Item.UseSound = null;
            player.ChangeDir(Main.MouseWorld.X - player.Center.X > 0 ? 1 : -1);
            Projectile.NewProjectile(player.GetSource_ItemUse(Item), player.MountedCenter, new Vector2(player.direction, 0), ModContent.ProjectileType<ObsidianKatanaHoldUp>(), Item.MKItem().AltDamage, Item.knockBack, player.whoAmI);
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            if (!Fire(player))
                velocity = new Vector2(player.direction, 0);
            else
                damage = Item.MKItem().AltDamage;
        }

        public override void UpdateInventory(Player player)
        {
            if (!Fire(player) && FireTrigger)
            {
                Item.MKItem().ActivateCooldown(player);
                SoundEngine.PlaySound(MoreKatanaSounds.LiquidsWaterLava, player.Center);

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

                FireTrigger = false;
            }
        }

        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            Texture2D texture = !Fire(Main.LocalPlayer) ? TextureAssets.Item[Type].Value : FireTexture.Value;
            spriteBatch.Draw(texture, position, frame, drawColor, 0f, origin, scale, SpriteEffects.None, 0);
            return false;
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D texture = !Fire(Main.LocalPlayer) ? TextureAssets.Item[Type].Value : FireTexture.Value;
            Vector2 position = Item.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;
            spriteBatch.Draw(texture, position, null, lightColor, rotation, origin, scale, SpriteEffects.None, 0);
            return false;
        }
    }
}