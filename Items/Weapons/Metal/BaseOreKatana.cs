using MoreKatana.Projectiles;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

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

        public abstract int HeadID { get; }
        public abstract int BodyID { get; }
        public abstract int LegID { get; }

        public virtual int[] AltHeadIDs => [];
        public virtual int[] AltBodyIDs => []; // クロスMod対応で使用する可能性あり
        public virtual int[] AltLegIDs => []; // クロスMod対応で使用する可能性あり

        public const int ArmorSetDefenseBonus = 2;

        public override LocalizedText FunctionText => base.FunctionText.WithFormatArgs(DefenseBonus, ArmorSetDefenseBonus);

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
            // 防御力のボーナス
            player.statDefense += DefenseBonus;

            // 対応するアーマーが揃っている場合さらにボーナスを与える
            if ((HeadID == player.armor[0].type || AltHeadIDs.Contains(player.armor[0].type))
                && (BodyID == player.armor[1].type || AltBodyIDs.Contains(player.armor[1].type))
                && (LegID == player.armor[2].type || AltLegIDs.Contains(player.armor[2].type)))
            {
                player.statDefense += 2;
            }
        }

        public override void ActiveSkill(Player player)
        {
            // サウンド
            SoundEngine.PlaySound(MoreKatanaSounds.SwordSlash_2, player.Center);

            // クールダウンを有効化
            Item.MKItem().ActivateCooldown(player);

            // ダッシュ切りの発射体
            Projectile.NewProjectile(player.GetSource_ItemUse(Item), player.Center, player.SafeDirectionTo(player.MKPlayer().MouseWorld), ModContent.ProjectileType<GeneralDashSlash>(), Item.MKItem().AltDamage, Item.knockBack, player.whoAmI, DashSlashDistance, DashSlashTime);
        }
    }
}