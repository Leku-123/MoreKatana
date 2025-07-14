using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace KatanaTest.Buffs
{
    public class KatanaArtsCD : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.buffNoSave[Type] = true;
            BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
        }
    }
}
