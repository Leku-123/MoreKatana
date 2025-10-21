using Microsoft.Xna.Framework;
using MoreKatana.Projectiles.PrimTrails;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;

namespace MoreKatana.Utilities
{
    public static class MoreKatanaDetours
    {
        public static void Initialize()
        {
            On_PlayerDrawLayers.DrawPlayer_RenderAllLayers += DrawPlayer_RenderAllLayers;
            On_PlayerDrawLayers.DrawPlayer_TransformDrawData += DrawPlayer_TransformDrawData;
            On_Projectile.NewProjectile_IEntitySource_float_float_float_float_int_int_float_int_float_float_float += Projectile_NewProjectile;
            On_Main.DrawNPCs += Main_DrawNPCs;
            On_Main.DrawProjectiles += Main_DrawProjectiles;
            On_Main.Update += Main_Update;
            On_Main.DrawInfernoRings += On_Main_DrawInfernoRings;
        }

        public static void Unload()
        {
            On_PlayerDrawLayers.DrawPlayer_RenderAllLayers -= DrawPlayer_RenderAllLayers;
            On_PlayerDrawLayers.DrawPlayer_TransformDrawData -= DrawPlayer_TransformDrawData;
            On_Projectile.NewProjectile_IEntitySource_float_float_float_float_int_int_float_int_float_float_float -= Projectile_NewProjectile;
            On_Main.DrawNPCs -= Main_DrawNPCs;
            On_Main.DrawProjectiles -= Main_DrawProjectiles;
            On_Main.Update -= Main_Update;
            On_Main.DrawInfernoRings -= On_Main_DrawInfernoRings;
        }

        private static void DrawPlayer_RenderAllLayers(On_PlayerDrawLayers.orig_DrawPlayer_RenderAllLayers orig, ref PlayerDrawSet drawinfo)
        {
            orig.Invoke(ref drawinfo);
            MoreKatanaPlayer.AddRenderDrawLayers(ref drawinfo);
        }

        private static void DrawPlayer_TransformDrawData(On_PlayerDrawLayers.orig_DrawPlayer_TransformDrawData orig, ref PlayerDrawSet drawinfo)
        {
            orig.Invoke(ref drawinfo);
            for (int i = 0; i < drawinfo.DrawDataCache.Count; i++)
                drawinfo.DrawDataCache[i] = MoreKatanaPlayer.ManipulateDrawInfo(drawinfo.DrawDataCache[i], drawinfo.drawPlayer);
        }

        private static int Projectile_NewProjectile(On_Projectile.orig_NewProjectile_IEntitySource_float_float_float_float_int_int_float_int_float_float_float orig, IEntitySource spawnSource, float X, float Y, float SpeedX, float SpeedY, int Type, int Damage, float KnockBack, int Owner, float ai0, float ai1, float ai2)
        {
            int index = orig(spawnSource, X, Y, SpeedX, SpeedY, Type, Damage, KnockBack, Owner, ai0, ai1, ai2);
            Projectile projectile = Main.projectile[index];

            if (projectile.ModProjectile is ITrailProjectile)
            {
                if (Main.netMode == NetmodeID.SinglePlayer)
                    (projectile.ModProjectile as ITrailProjectile).DoTrailCreation(MoreKatana.TrailManager);
                else
                {
                    MoreKatana.SyncData(MoreKatana.MessageType.SpawnTrail, index);
                }
            }

            return index;
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

        private static void On_Main_DrawInfernoRings(On_Main.orig_DrawInfernoRings orig, Main self)
        {
            orig.Invoke(self);
            MoreKatanaPlayer.AddRenderUI(Main.spriteBatch, Main.LocalPlayer);
        }
    }
}