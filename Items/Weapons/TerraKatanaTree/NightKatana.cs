using Microsoft.Xna.Framework;
using MoreKatana.Projectiles.TerraKatanaTree;
using MoreKatana.Systems.CrossMod;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;

namespace MoreKatana.Items.Weapons.TerraKatanaTree
{
    public class NightKatana : KatanaItem, IAddDrawLayer
    {
        public const int MaxComboCount = 10;
        public const int MaxComboTime = 60;

        public override void SetStaticDefaults()
        {
            Item.AddElement(RedemptionCompat.Shadow, true);
            Item.SetSlashBonus();
        }

        public override void SetDefaultsItem()
        {
            Item.width = 60;
            Item.height = 70;

            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.MKItem().UseSound = MoreKatanaSounds.SwordSlash;

            Item.damage = 34;
            Item.knockBack = 4.5f;
            Item.MKItem().AltDamage = 50;

            Item.value = Item.sellPrice(gold: 4);
            Item.rare = ItemRarityID.Orange;

            Item.MKItem().SetKatanaDefaults(Item, 60, type: ModContent.ProjectileType<NightKatanaSwing>());
        }

        public override void PassiveSkill(Player player, bool equipment)
        {
            player.GetDamage(DamageClass.Melee) += (float)Item.MKItem().AttackType / MaxComboCount;

            if (player.MKPlayer().NightComboTimer <= 0)
                Item.MKItem().AttackType = 0;
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

            if (drawPlayer.ActiveItem().type != ModContent.ItemType<NightKatana>())
                return;

            if (Main.myPlayer == drawPlayer.whoAmI)
            {
                Vector2 gaugePos = new Vector2(drawinfo.Center.X, drawinfo.Center.Y) + new Vector2(0, 35);
                float ratio = (float)drawPlayer.MKPlayer().NightComboTimer / MaxComboTime;
                MoreKatanaUtil.DrawGauge(gaugePos, ratio, Color.Violet);
            }
        }
    }
}