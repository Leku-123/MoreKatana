using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

namespace MoreKatana.Items.Weapons.TerraKatanaTree
{
    public class VolcanoKatana : KatanaItem
    {
        public override KatanaID ID => KatanaID.Volcano;

        public override void SetDefaultsItem()
        {
            Item.width = 54;
            Item.height = 64;
            Item.useTime = 40;
            Item.useAnimation = 40;
            Item.UseSound = SoundID.Item1;
            Item.damage = 40;
            Item.knockBack = 6.5f;
            Item.value = Item.sellPrice(silver: 55);
            Item.rare = ItemRarityID.Orange;

            //WIP 仮で発射
            Item.MKItem().SetKatanaDefaults(Item, 0, type: ProjectileID.BallofFire);
        }

        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            for (int i = 0; i < 2; i++)
            {
                int moyasu = Dust.NewDust(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height, DustID.Torch, player.velocity.X * 0.2f + (float)(player.direction * 3), player.velocity.Y * 0.2f, 100, default(Color), 2.5f);
                Main.dust[moyasu].noGravity = true;
                Main.dust[moyasu].velocity.X *= 2f;
                Main.dust[moyasu].velocity.Y *= 2f;
            }
        }

        public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Main.rand.NextBool(2))
                target.AddBuff(BuffID.OnFire, 60 * 3);
        }
    }
}
