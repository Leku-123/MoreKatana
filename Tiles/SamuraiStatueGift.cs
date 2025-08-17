using Microsoft.Xna.Framework;
using MoreKatana.Items.Weapons.Misc;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace MoreKatana.Tiles
{
    public class SamuraiStatueGift : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileObsidianKill[Type] = true;
            TileID.Sets.HasOutlines[Type] = true;
            Main.tileNoAttach[Type] = true;
            TileID.Sets.PreventsTileRemovalIfOnTopOfIt[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style3x3);
            TileObjectData.newTile.LavaDeath = false;
            TileObjectData.addTile(Type);

            LocalizedText name = CreateMapEntryName();
            AddMapEntry(new Color(144, 144, 144), name);
        }

        public override void MouseOver(int i, int j)
        {
            Player player = Main.LocalPlayer;
            player.noThrow = 2;
            player.cursorItemIconEnabled = true;
            player.cursorItemIconID = ModContent.ItemType<EnchantedKatana>();
        }

        public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings)
        {
            return true;
        }

        public override bool RightClick(int i, int j)
        {
            Tile tile = Framing.GetTileSafely(i, j);
            i -= tile.TileFrameX / 18;
            j -= tile.TileFrameY / 18;
            i += 1;
            j += 2;
            WorldGen.KillTile(i, j, noItem: true);
            return true;
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            Tile tile = Framing.GetTileSafely(i, j);
            i -= tile.TileFrameX / 18;
            j -= tile.TileFrameY / 18;
            i += 1;
            j += 2;
            WorldGen.PlaceTile(i, j, ModContent.TileType<SamuraiStatue>());
        }

        public override IEnumerable<Item> GetItemDrops(int i, int j)
        {
            yield return new Item(ModContent.ItemType<EnchantedKatana>());
        }

        public override void EmitParticles(int i, int j, Tile tileCache, short tileFrameX, short tileFrameY, Color tileLight, bool visible)
        {
            Tile tile = Main.tile[i, j];
            if (Main.rand.NextBool(20))
            {
                Dust dust = Dust.NewDustDirect(new Vector2(i * 16 + 2, j * 16 - 4), 4, 8, Utils.SelectRandom(Main.rand, EnchantedKatana.EnchantedDustType), 0f, 0f, 100);
                dust.position.X += Main.rand.Next(8);
                dust.alpha += Main.rand.Next(100);
                dust.velocity *= 0.2f;
                dust.velocity.Y -= 0.5f + Main.rand.Next(10) * 0.1f;
                dust.fadeIn = 0.5f + Main.rand.Next(10) * 0.1f;
            }
        }
    }
}