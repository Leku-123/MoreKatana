using Terraria;
using Terraria.ModLoader;

namespace MoreKatana.Buffs
{
    public class TerraMark : ModBuff
    {
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Curse Mark");
            Main.buffNoTimeDisplay[Type] = false;
        }
    }
}