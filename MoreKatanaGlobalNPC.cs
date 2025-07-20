using Terraria;
using Terraria.ModLoader;

namespace MoreKatana
{
    public class MoreKatanaGlobalNPC : GlobalNPC
    {
        public override void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns)
        {
            if (player.MKPlayer().EquipMuramasa)
                spawnRate += spawnRate / 4;
        }
    }
}
