using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

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

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            if (tooltips == null)
                return;

            if (Main.player[Main.myPlayer] == null)
                return;

            TooltipLine functionTooltip = Enumerable.FirstOrDefault(tooltips, (TooltipLine x) => x.Text.Contains("[FUNC]") && x.Mod == "Terraria");
            if (!ItemSlot.ShiftInUse)
            {
                functionTooltip.Text = (string)Mod.GetLocalization($"{nameof(KatanaItem)}.DefaultText");
            }
            else
            {
                functionTooltip.Text = ILocalizedModTypeExtensions.GetLocalizedValue((ILocalizedModType)(object)this, "FunctionText");
            }
        }
    }
}