using Terraria;
using Terraria.ModLoader;

namespace MoreKatana
{
    internal class MoreKatanaSystem : ModSystem
    {
        public override void PreUpdateItems()
        {
            if (!Main.dedServ)
                MoreKatana.primitives.UpdateTrails();
        }
    }
}