using Terraria.ModLoader;

namespace MoreKatana.Items
{
    public class SetupFirstStatue : DebugItem
    {
        public override void SetDefaults()
        {
            base.SetDefaults();
            Item.consumable = true;
            Item.createTile = ModContent.TileType<Tiles.SamuraiStatueGift>();
        }
    }
}