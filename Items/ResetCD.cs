using Terraria;

namespace MoreKatana.Items
{
    public class ResetCD : DebugItem
    {
        public override bool? UseItem(Player player)
        {
            player.MKPlayer().ActiveSkillCD = 1;
            return true;
        }
    }
}