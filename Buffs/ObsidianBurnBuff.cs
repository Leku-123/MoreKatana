using Microsoft.Xna.Framework;
using MoreKatana.Items.Weapons.Metal;
using MoreKatana.Particles;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MoreKatana.Buffs
{
    public class ObsidianBurnBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            if (player.HeldItem.type != ModContent.ItemType<ObsidianKatana_Fire>())
            {
                player.DelBuff(buffIndex);
                buffIndex--;
            }

            if (player.wet && !player.lavaWet)
            {
                Rectangle textPos = new Rectangle((int)player.position.X, (int)player.position.Y - 20, player.width, player.height);
                CombatText.NewText(textPos, Color.Crimson, "Oops!");

                player.DelBuff(buffIndex);
                buffIndex--;
            }

            player.DrawColorEffect(Color.Crimson.ToVector3());

            if (player.velocity.Y == 0 && !player.mount.Active)
            {
                for (int i = 0; i < 3; i++)
                {
                    int newDust = Dust.NewDust(new Vector2(player.Center.X - player.width, player.Center.Y + player.height / 2), player.width * 2 - 3, 0, DustID.Flare, 0, Main.rand.Next(-10, -4), 150, default, 1f);
                    Main.dust[newDust].fadeIn = 0.3f;
                    Main.dust[newDust].noGravity = true;
                    newDust = Dust.NewDust(new Vector2(player.Center.X - player.width, player.Center.Y + player.height / 2), player.width * 2 - 3, 0, DustID.Torch, 0, Main.rand.Next(-5, -2), 150, default, 1f);
                    Main.dust[newDust].fadeIn = 0.3f;
                    Main.dust[newDust].noGravity = true;
                }
            }

            if (Main.rand.NextBool(4))
            {
                Vector2 position = Main.rand.NextVector2FromRectangle(player.Hitbox);
                Vector2 velocity = Vector2.UnitY.RotatedBy(MathHelper.Pi) * 10f * Main.rand.NextFloat(0.3f);
                ParticleHandler.SpawnParticle(new GlowParticle(position, velocity, Color.OrangeRed, Main.rand.NextFloat(0.3f, 0.5f), 25));
            }
        }

        public override bool RightClick(int buffIndex) => false;
    }
}