using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MoreKatana.Buffs;
using MoreKatana.Projectiles.Metal;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace MoreKatana.Items.Weapons.Metal
{
    public class ObsidianKatana : KatanaItem
    {
        public override KatanaID ID => KatanaID.Obsidian;

        public static bool Fire(Player player) => player.HasBuff<ObsidianKatanaFire>(); // 着火状態であるか否か 
        public bool FireTrigger = false;     // 着火がトリガーされたか   
        public const int FireTime = 60 * 5;// 着火状態の基礎時間

        private Rectangle[] DrawFrame =
        [
            new(0, 0, 48, 54),
            new(0, 56, 48, 110)
        ];

        private Vector2 katanaOrigin => DrawFrame[1].Size() / 2f;

        public override void SetStaticDefaults()
        {
            Main.RegisterItemAnimation(Type, new DrawAnimationVertical(int.MaxValue, 2));
        }

        public override void SetDefaultsItem()
        {
            Item.width = 48;
            Item.height = 54;

            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.UseSound = SoundID.Item1;

            Item.damage = 20;
            Item.knockBack = 4.5f;

            Item.value = Item.sellPrice(silver: 55);
            Item.rare = ItemRarityID.Orange;

            Item.MKItem().SetKatanaDefaults(Item, 120, true, ModContent.ProjectileType<ObsidianSwing>());
        }

        public override void PassiveSkill(Player player, bool equipment)
        {
            player.statDefense += 5;
        }

        public override void UpdateInventory(Player player)
        {
            if (FireTrigger)
            {
                if (!Fire(player))
                {
                    Item.MKItem().ActivateCooldown(player);
                    FireTrigger = false;
                }
            }
        }

        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            if (FireTrigger)
                frame = DrawFrame[0];
            else
                frame = DrawFrame[1];

            spriteBatch.Draw(TextureAssets.Item[Type].Value, position, frame, drawColor, 0f, origin, scale, SpriteEffects.None, default);

            return false;
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            Vector2 position = Item.Bottom - Main.screenPosition - new Vector2(0, katanaOrigin.Y);

            spriteBatch.Draw(TextureAssets.Item[Type].Value, position, DrawFrame[1], Item.color, 0f, katanaOrigin, scale, SpriteEffects.None, default);
            return false;
        }

        public override bool AltFunctionUse(Player player)
        {
            return !Fire(player) && !FireTrigger;
        }

        public override void ActiveSkill(Player player)
        {
            if (!Fire(player) && !FireTrigger)
            {
                Item.noUseGraphic = false;
                Item.useStyle = ItemUseStyleID.HoldUp;
                MoreKatanaUtil.DrawRing(player.Center, DustID.Torch, 30, 1f, dustSize: 3f);
                MoreKatanaUtil.DrawRing(player.Center, DustID.Torch, 24, 10f, dustSize: 3f);
                SoundEngine.PlaySound(SoundID.DD2_BetsysWrathShot, player.Center);
                player.AddBuff(ModContent.BuffType<ObsidianKatanaFire>(), FireTime);
                FireTrigger = true;
            }
        }

        public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Fire(player))
            {
                target.AddBuff(BuffID.OnFire, 120);
            }
        }
    }
}
