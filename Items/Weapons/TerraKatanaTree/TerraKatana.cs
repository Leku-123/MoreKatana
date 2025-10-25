using Microsoft.Xna.Framework;
using MoreKatana.Projectiles.TerraKatanaTree;
using Terraria;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;

namespace MoreKatana.Items.Weapons.TerraKatanaTree
{
    public class TerraKatana : KatanaItem
    {
        public override KatanaID ID => KatanaID.None;

        public override void SetDefaultsItem()
        {
            Item.width = 62;
            Item.height = 68;

            Item.useTime = 18;
            Item.useAnimation = 18;
            Item.MKItem().UseSound = SoundID.Item1;

            Item.damage = 80;
            Item.knockBack = 4.5f;
            Item.MKItem().AltDamage = 22;

            Item.value = Item.sellPrice(silver: 55);
            Item.rare = ItemRarityID.Yellow;

            Item.MKItem().SetKatanaDefaults(Item, 60, true, ModContent.ProjectileType<TerraKatanaSwing>(), 5);
        }

        public override void PassiveSkill(Player player, bool equipment)
        {
        }

        public override void ActiveSkill(Player player)
        {
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
            if (Main.rand.NextBool(3))
                Dust.NewDust(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height, DustID.Terra);
        }
    }
}