using Terraria;
using Terraria.ModLoader;

namespace MoreKatana
{
    public class MKNPC : GlobalNPC
    {
        public override void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns)
        {
            if (player.GetModPlayer<MKPlayer>().EquipMuramasa)
                spawnRate += spawnRate / 4;
        }
    }
}
