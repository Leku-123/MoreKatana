using Terraria;
using Terraria.ModLoader;

namespace MoreKatana.System
{
    public class MKPlayer : ModPlayer
    {
        public bool Equip_WoodenKatana = false;

        public override void ResetEffects()
        {
            Equip_WoodenKatana = false;
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (Equip_WoodenKatana)
            {
                WoodenKatana(in target, ref modifiers);
            }
        }

        public void WoodenKatana(in NPC target, ref NPC.HitModifiers modifiers)
        {
            if (target.life < 1)
                return;
            modifiers.SetMaxDamage(target.life - 1);
        }

    }
}
