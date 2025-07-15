using MoreKatana.Items.Katana;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.UI;

namespace MoreKatana.UI.Katanary
{
    public class KatanaryEntry : UIPanel
    {
        public KatanaItem entryItem;

        // 新しく入手した刀を強調表示するフラグ（未実装）
        //public bool katanaShiny;

        public KatanaryEntry(KatanaItem katana)
        {
            entryItem = katana;
            Width.Pixels = 100f;
            Height.Pixels = 100f;
        }

        public override void MouseOver(UIMouseEvent evt)
        {
            base.MouseOver(evt);
            SoundEngine.PlaySound(SoundID.MenuTick);
        }
    }
}
