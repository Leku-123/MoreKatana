using Terraria.GameContent.Bestiary;
using Terraria.UI;

namespace MoreKatana.UI.Katanary
{
    public class KatanaUICollectionInfoProvider : IBestiaryUICollectionInfoProvider
    {

        public BestiaryUICollectionInfo GetEntryUICollectionInfo()
        {
            return default;
        }

        public UIElement ProvideUIElement(BestiaryUICollectionInfo info) => null;
    }
}
