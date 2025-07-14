using Terraria;
using Terraria.ModLoader;

namespace MoreKatana.Items.Katana
{
    public abstract class KatanaItem : ModItem
    {
        /// <summary>
        /// アクティブスキル
        /// </summary>
        /// <param name="player"></param>
        public virtual void ActiveSkill(Player player)
        {

        }

        /// <summary>
        /// パッシブスキル
        /// </summary>
        /// <param name="player"></param>
        /// <param name="equipment"> trueの場合、アクセサリーとしての効果 </param>
        public virtual void PassiveSkill(Player player, bool equipment)
        {

        }
    }
}