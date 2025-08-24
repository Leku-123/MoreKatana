using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace MoreKatana.Assets.ItemTextures
{
    public static class MoreKatanaItemTextures
    {
        public static Asset<Texture2D>[] Item = new Asset<Texture2D>[ItemID.Count - 1];

        public static void LoadItemTextures()
        {
            if (Main.dedServ)
                return;

            void LoadTexture(int type)
            {
                Item[type] = TextureAssets.Item[type];
                TextureAssets.Item[type] = ModContent.Request<Texture2D>("MoreKatana/Assets/ItemTextures/Item_" + type);
            }

            LoadTexture(155);
            LoadTexture(2273);
        }

        public static void UnloadItemTextures()
        {
            if (Main.dedServ)
                return;

            void UnloadTexture(int type)
            {
                TextureAssets.Item[type] = Item[type];
            }

            UnloadTexture(155);
            UnloadTexture(2273);
            Item = null;
        }
    }
}