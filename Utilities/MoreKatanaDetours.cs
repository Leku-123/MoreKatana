using Microsoft.Xna.Framework;
using Terraria;

namespace MoreKatana
{
    public static class MoreKatanaDetours
    {
        public static void Initialize()
        {
            On_Main.DrawNPCs += Main_DrawNPCs;
            On_Main.DrawProjectiles += Main_DrawProjectiles;
            On_Main.Update += Main_Update;
        }

        public static void Unload()
        {
            On_Main.DrawNPCs -= Main_DrawNPCs;
            On_Main.DrawProjectiles -= Main_DrawProjectiles;
            On_Main.Update -= Main_Update;
        }

        private static void Main_DrawNPCs(On_Main.orig_DrawNPCs orig, Main self, bool behindTiles)
        {
            if (!Main.dedServ)
                MoreKatana.primitives.DrawTargetNPC(Main.spriteBatch);

            orig(self, behindTiles);
        }

        private static void Main_DrawProjectiles(On_Main.orig_DrawProjectiles orig, Main self)
        {
            if (!Main.dedServ)
                MoreKatana.primitives.DrawTargetProj(Main.spriteBatch);

            orig(self);
        }

        private static void Main_Update(On_Main.orig_Update orig, Main self, GameTime gameTime)
        {
            MoreKatana.Instance?.CheckScreenSize();

            orig(self, gameTime);
        }
    }
}