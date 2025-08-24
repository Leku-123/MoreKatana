using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace MoreKatana.Gores
{
    public class ObsidianGore : ModGore
    {
        public override void OnSpawn(Gore gore, IEntitySource source)
        {
            gore.Frame = new SpriteFrame(1, 3, 0, (byte)Main.rand.Next(3));
        }
    }
}