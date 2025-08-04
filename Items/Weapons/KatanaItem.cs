using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace MoreKatana.Items.Weapons
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
        /// パッシブスキル
        /// </summary>
        /// <param name="player"></param>
        /// <param name="equipment"> trueの場合、アクセサリーとしての効果 </param>
        public virtual void PassiveSkill(Player player, bool equipment)
        {

        }

        /// <summary>
        /// アクティブスキル
        /// </summary>
        /// <param name="player"></param>
        public virtual void ActiveSkill(Player player)
        {

        }
    }
}