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
        /// <summary>
        /// シフト状態での説明文
        /// </summary>
        public LocalizedText FunctionText => this.GetLocalization(nameof(FunctionText));

        /// <summary>
        /// 刀を追加しているMod名
        /// </summary>
        public string ParentMod => Mod.Name;

        /// <summary>
        /// 刀のレア度（星アイコンの数＝ベスティアリ用）
        /// </summary>
        public virtual int StarRarity => 1;

        /// <summary>
        /// 図鑑上の刀ID
        /// </summary>
        public virtual int KatanaID { get; set; }

        /// <summary>
        /// 図鑑に登録されるか
        /// </summary>
        public virtual bool IsCollectable { get; private set; } = true;

        /*
         今後必要なプロパティ
         ・図鑑用説明文（ローカライゼーションテキストのリスト）
         ・メインカテゴリ（列挙型）
         ・ヒント用説明文（ローカライゼーションテキストのリスト）
         ・サブカテゴリプロパティ（列挙型のリスト）
         */


        public override void AutoStaticDefaults()
        {
            base.AutoStaticDefaults();
            _ = FunctionText;
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

            if (functionTooltip == null) return;

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