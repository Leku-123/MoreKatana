using Terraria;
using Terraria.ModLoader;

namespace MoreKatana.Systems
{
    internal class PrimsUpdateSystem : ModSystem
    {
        public override void PreUpdateItems()
        {
            if (!Main.dedServ)
                MoreKatana.primitives.UpdateTrails();
        }
    }
}