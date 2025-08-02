using MoreKatana.Items.Materials;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace MoreKatana
{
    public class MoreKatanaGlobalNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
        {
            if (npc.type == NPCID.Mothron)
            {
                npcLoot.Add(ItemDropRule.ExpertGetsRerolls(ModContent.ItemType<BrokenHeroKatana>(), 4, 1));
            }
        }
    }
}
