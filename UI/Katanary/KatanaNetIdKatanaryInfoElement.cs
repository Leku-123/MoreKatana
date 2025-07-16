using Terraria.GameContent.Bestiary;
using Terraria.UI;

namespace MoreKatana.UI.Katanary
{
    public class KatanaNetIdKatanaryInfoElement : IBestiaryInfoElement, IBestiaryEntryDisplayIndex
    {
        public int NetId { get; private set; }

        public int BestiaryDisplayIndex => NetId;

        public KatanaNetIdKatanaryInfoElement(int katanaNetId)
        {
            NetId = katanaNetId;
        }

        public UIElement ProvideUIElement(BestiaryUICollectionInfo info) => null;
    }

}
