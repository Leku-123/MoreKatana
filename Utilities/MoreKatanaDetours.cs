using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;

namespace MoreKatana.Utilities
{
    public static class MoreKatanaDetours
    {
        public static void Initialize()
        {
            On_Player.ItemCheck_Inner += On_Player_ItemCheck_Inner;
            On_PlayerDrawLayers.DrawPlayer_TransformDrawData += DrawPlayer_TransformDrawData;
            On_Main.DrawNPCs += Main_DrawNPCs;
            On_Main.DrawProjectiles += Main_DrawProjectiles;
            On_Main.Update += Main_Update;
        }

        public static void Unload()
        {
            On_Player.ItemCheck_Inner -= On_Player_ItemCheck_Inner;
            On_PlayerDrawLayers.DrawPlayer_TransformDrawData -= DrawPlayer_TransformDrawData;
            On_Main.DrawNPCs -= Main_DrawNPCs;
            On_Main.DrawProjectiles -= Main_DrawProjectiles;
            On_Main.Update -= Main_Update;
        }

        private static void On_Player_ItemCheck_Inner(On_Player.orig_ItemCheck_Inner orig, Player self)
        {
            orig.Invoke(self);
            MoreKatanaPlayer.SetBuffImmuneEffects(self);
        }

        private static void DrawPlayer_TransformDrawData(On_PlayerDrawLayers.orig_DrawPlayer_TransformDrawData orig, ref PlayerDrawSet drawinfo)
        {
            orig.Invoke(ref drawinfo);
            for (int i = 0; i < drawinfo.DrawDataCache.Count; i++)
                drawinfo.DrawDataCache[i] = MoreKatanaPlayer.ManipulateDrawInfo(drawinfo.DrawDataCache[i], drawinfo.drawPlayer);
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