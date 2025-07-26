using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;

namespace MoreKatana.Items.Katana
{
    public abstract class KatanaItem : ModItem
    {
        public LocalizedText FunctionText => this.GetLocalization(nameof(FunctionText));

        public abstract KatanaID ID { get; }

        /// <summary>
        /// 刀のレア度（星アイコンの数＝ベスティアリ用）
        /// </summary>
        public virtual int StarRarity => 1;

        public override void AutoStaticDefaults()
        {
            base.AutoStaticDefaults();
            _ = FunctionText;
        }

        public sealed override void SetDefaults()
        {
            SetDefaultsItem();
        }

        public virtual void SetDefaultsItem()
        {

        }

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

            if (functionTooltip == null)
                return;

            if (!ItemSlot.ShiftInUse)
                functionTooltip.Text = (string)Mod.GetLocalization($"{nameof(KatanaItem)}.DefaultText");
            else
                functionTooltip.Text = ILocalizedModTypeExtensions.GetLocalizedValue((ILocalizedModType)(object)this, "FunctionText");
        }
    }
}