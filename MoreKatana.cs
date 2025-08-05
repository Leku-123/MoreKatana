using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MoreKatana.Assets.ExtraTextures;
using MoreKatana.Assets.ItemTextures;
using MoreKatana.Prim;
using MoreKatana.Utilities;
using ReLogic.Content;
using System.Reflection;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace MoreKatana
{
    public class MoreKatana : Mod
    {
        public const string EmptyTexture = "MoreKatana/Empty";

        public static Effect PrimitiveTextureMap;
        public static PrimTrailManager primitives;

        private Vector2 _lastScreenSize;
     
        public static MoreKatana Instance;

        public MoreKatana()
        {
            Instance = this;
        }

        public override void Load()
        {
            MoreKatanaDetours.Initialize();
            MoreKatanaTextures.LoadTextures();
            MoreKatanaItemTextures.LoadItemTextures();

            if (Main.netMode != NetmodeID.Server)
            {
                GameShaders.Misc["Compression"] = new MiscShaderData(ModContent.Request<Effect>("MoreKatana/Effects/Compression", AssetRequestMode.ImmediateLoad), "ShieldPass");
                
                PrimitiveTextureMap = ModContent.Request<Effect>("MoreKatana/Effects/PrimitiveTextureMap", AssetRequestMode.ImmediateLoad).Value;
                primitives = new PrimTrailManager();
                primitives.LoadContent(Main.graphics.GraphicsDevice);
            }
        }

        public void CheckScreenSize()
        {
            if (!Main.dedServ && !Main.gameMenu)
            {
                Main.QueueMainThreadAction(() =>
                {
                    if (_lastScreenSize != new Vector2(Main.screenWidth, Main.screenHeight) && primitives != null)
                        primitives.InitializeTargets(Main.graphics.GraphicsDevice);

                    _lastScreenSize = new Vector2(Main.screenWidth, Main.screenHeight);
                });
            }
        }

        public override void Unload()
        {
            PrimitiveTextureMap = null;
            primitives = null;

            MoreKatanaDetours.Unload();
            MoreKatanaTextures.UnloadTextures();
            MoreKatanaItemTextures.UnloadItemTextures();
        }

        internal static void SaveConfig(MoreKatanaConfig cfg)
        {
            // There is no current way to manually save a mod configuration file in tModLoader.
            // The method which saves mod config files is private in ConfigManager, so reflection is used to invoke it.
            try
            {
                MethodInfo saveMethodInfo = typeof(ConfigManager).GetMethod("Save", BindingFlags.Static | BindingFlags.NonPublic);
                if (saveMethodInfo is not null)
                    saveMethodInfo.Invoke(null, new object[] { cfg });
                else
                    Instance.Logger.Error("TML ConfigManager.Save reflection failed. Method signature has changed.");
            }
            catch
            {
                Instance.Logger.Error("An error occurred while manually saving WeaponsOverhaul configuration. It is safe to ignore this error.");
            }
        }
    }
}