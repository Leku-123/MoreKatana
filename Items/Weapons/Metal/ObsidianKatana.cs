using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MoreKatana.Buffs;
using MoreKatana.Projectiles.Metal;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace MoreKatana.Items.Weapons.Metal
{
    public class ObsidianKatana : KatanaItem
    {
        public override KatanaID ID => KatanaID.Obsidian;

        public static bool Fire(Player player) => player.HasBuff<ObsidianKatanaFire>(); // 着火状態であるか否か 
        public bool FireTrigger = false;    // 着火がトリガーされたか   
        public const int FireTime = 60 * 5; // 着火状態の基礎時間

        public const int DrawFrameLength = 11;
        public int Frame = 1;
        public const int FrameMax = 9;
        public int FrameCount = 0;
        public int Combo = 1;

        public override void SetStaticDefaults()
        {
            Main.RegisterItemAnimation(Type, new DrawAnimationVertical(int.MaxValue, DrawFrameLength));
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
            Item.MKItem().AltDamage = 0;

            Item.value = Item.sellPrice(silver: 55);
            Item.rare = ItemRarityID.Orange;

            Item.MKItem().SetKatanaDefaults(Item, 120, true, ModContent.ProjectileType<ObsidianSwing>(), Combo);
        }

        public override bool AltFunctionUseItem(Player player) => !Fire(player);

        public override void PassiveSkill(Player player, bool equipment)
        {
            player.statDefense += 5;

            if (Fire(player))
            {
                Item.SetNameOverride(Language.GetTextValue("Mods.MoreKatana.Items.ObsidianKatana.AltName"));
                Item.damage = 30;
                Item.useTime = 25;
                Item.useAnimation = 25;
                Combo = 2;
                player.DrawColorEffect(Color.Red.ToVector3());
            }
            else
            {
                Item.SetNameOverride(Language.GetTextValue("Mods.MoreKatana.Items.ObsidianKatana.DisplayName"));
                Item.damage = 20;
                Item.useTime = 20;
                Item.useAnimation = 20;
                Combo = 1;
            }
        }

        public override void ActiveSkill(Player player)
        {
            Item.UseSound = SoundID.DD2_BetsysWrathShot;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.noUseGraphic = false;

            player.AddBuff(ModContent.BuffType<ObsidianKatanaFire>(), FireTime);

            MoreKatanaUtil.DrawRing(player.Center, [DustID.Torch], 30, 5f);
            MoreKatanaUtil.DrawRing(player.Center, [DustID.Torch], 24, 10f, dustScale: 3f);

            FireTrigger = true;
        }

        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            if (!Fire(player))
                return;

            int newDust = Dust.NewDust(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height, DustID.Torch);
            Main.dust[newDust].noGravity = true;
        }

        public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!Fire(player))
                return;

            target.AddBuff(BuffID.OnFire, 120);
        }

        public override void UpdateInventory(Player player)
        {
            if (!Fire(player) && FireTrigger)
            {
                Item.MKItem().ActivateCooldown(player);
                FireTrigger = false;
            }
        }

        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            Texture2D texture = TextureAssets.Item[Type].Value;
            Frame = Fire(Main.LocalPlayer) ? Frame : 10;
            Rectangle rectangle = texture.Frame(1, DrawFrameLength, 0, Frame);
            if (FrameCount >= FrameMax)
            {
                if (Frame < 9)
                    Frame++;
                else
                    Frame = 1;
                FrameCount = 0;
            }
            FrameCount = Fire(Main.LocalPlayer) ? FrameCount + 1 : 0;
            spriteBatch.Draw(texture, position, rectangle, drawColor, 0f, origin, scale, SpriteEffects.None, 0);
            return false;
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D texture = TextureAssets.Item[Type].Value;
            Vector2 position = Item.Center - Main.screenPosition;
            Rectangle rectangle = texture.Frame(1, DrawFrameLength, 0, 10);
            Vector2 origin = rectangle.Size() / 2f;

            spriteBatch.Draw(texture, position, rectangle, lightColor, rotation, origin, scale, SpriteEffects.None, 0);
            return false;
        }
    }
}