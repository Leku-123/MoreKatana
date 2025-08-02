using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;

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

        private string DefaultTip() => (string)Mod.GetLocalization($"{nameof(KatanaItem)}.DefaultText");
        private string FunctionTip() => ILocalizedModTypeExtensions.GetLocalizedValue((ILocalizedModType)(object)this, "FunctionText");

        public override bool PreDrawTooltipLine(DrawableTooltipLine line, ref int yOffset)
        {
            if (line.Text == DefaultTip())
            {
                Vector2 lineposition = new Vector2(line.OriginalX, line.OriginalY);
                Utils.DrawBorderString(Main.spriteBatch, line.Text, lineposition, Color.LightGoldenrodYellow);
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, null, null, null, null, Main.UIScaleMatrix);
                for (int i = 0; i < 4; i++)
                {
                    Vector2 drawpos = lineposition + new Vector2(0, 2 * ((float)Math.Sin(Main.GlobalTimeWrappedHourly * 4) / 2)).RotatedBy(i * MathHelper.PiOver2);
                    Utils.DrawBorderString(Main.spriteBatch, line.Text, drawpos, Color.Goldenrod);
                }
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, Main.UIScaleMatrix);
                return false;
            }
            return base.PreDrawTooltipLine(line, ref yOffset);
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
                functionTooltip.Text = DefaultTip();
            else
                functionTooltip.Text = FunctionTip();
        }
    }
}