using Terraria;
using Terraria.ID;

namespace MoreKatana.Items.Weapons.Metal
{
    public abstract class BaseOreKatana : KatanaItem
    {
        public readonly int DefenseBonus;
        public readonly int DashSlashDistance;
        public readonly float DashSlashTime;

        public BaseOreKatana(int defenseBonus, int dashSlashDistance, float dashSlashTime)
        {
            DefenseBonus = defenseBonus;
            DashSlashDistance = dashSlashDistance;
            DashSlashTime = dashSlashTime;
        }

        public override void SetDefaultsItem()
        {
            Item.width = 56;
            Item.height = 56;
            Item.rare = ItemRarityID.White;
            Item.MKItem().AltDamage = Item.damage * 2;
            Item.MKItem().UseSound = SoundID.Item1;
            Item.MKItem().SetKatanaDefaults(Item, 60, true);
        }

        public override void PassiveSkill(Player player, bool equipment)
        {
            player.statDefense += DefenseBonus;
        }

        public override void ActiveSkill(Player player)
        {
            // クールダウンを有効化
            Item.MKItem().ActivateCooldown(player);

            // ダッシュ切りを実行
            player.CreateDashSlash(player.GetSource_ItemUse(Item), Item.MKItem().AltDamage, Item.knockBack, DashSlashDistance, DashSlashTime);
        }
    }
}