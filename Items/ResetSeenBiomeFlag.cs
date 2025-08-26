using MoreKatana.Worlds;
using Terraria;

namespace MoreKatana.Items
{
    public class ResetSeenBiomeFlag : DebugItem
    {
        public override bool? UseItem(Player player)
        {
            MoreKatanaWorld.seenForgottenAltarBiome = false;
            return true;
        }
    }
}