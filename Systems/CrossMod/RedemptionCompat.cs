using Terraria;
using Terraria.ModLoader;

namespace MoreKatana.Systems.CrossMod
{
    /// <summary>
    /// Mod of RedemptionのModサポート
    /// wiki: https://modofredemption.wiki.gg/wiki/Mod_Calls
    /// </summary>
    public static class RedemptionCompat
    {
        /// <summary>
        /// Elemental damageの種類
        /// wiki: https://modofredemption.wiki.gg/wiki/Elemental_damage
        /// </summary>
        public const short Arcane = 1;
        public const short Fire = 2;
        public const short Water = 3;
        public const short Ice = 4;
        public const short Earth = 5;
        public const short Wind = 6;
        public const short Thunder = 7;
        public const short Holy = 8;
        public const short Shadow = 9;
        public const short Nature = 10;
        public const short Poison = 11;
        public const short Blood = 12;
        public const short Psychic = 13;
        public const short Celestial = 14;
        public const short Explosive = 15;

        /// <summary>
        /// Elementを設定する
        /// SetStaticDefaults()内で呼び出す
        /// </summary>
        public static void AddElement(this Entity entity, int elementID, bool projsInheritElements = false)
        {
            if (!ModLoader.TryGetMod("Redemption", out Mod redemption))
                return;

            if (entity is Item item)
                redemption.Call("addElementItem", elementID, item.type, projsInheritElements);
            else if (entity is NPC npc)
                redemption.Call("addElementNPC", elementID, npc.type);
            else if (entity is Projectile proj)
                redemption.Call("addElementProj", elementID, proj.type, projsInheritElements);
        }

        /// <summary>
        /// Slash属性を設定する
        /// SetStaticDefaults()内で呼び出す
        /// </summary>
        public static bool SetSlashBonus(this Item item, bool setBonus = true)
        {
            if (!ModLoader.TryGetMod("Redemption", out Mod redemption))
                return false;

            return (bool)redemption.Call("setSlashBonus", item, setBonus);
        }

        /// <summary>
        /// Decapitationの効果を実行する
        /// OnHitNPC()内で呼び出す
        /// wiki: https://modofredemption.wiki.gg/wiki/Elemental_damage#Decapitation
        /// </summary>
        public static bool Decapitation(NPC target, ref int damageDone, ref bool crit, int chance = 200)
        {
            if (!ModLoader.TryGetMod("Redemption", out Mod redemption))
                return false;

            return (bool)redemption.Call("decapitation", target, damageDone, crit, chance);
        }
    }
}