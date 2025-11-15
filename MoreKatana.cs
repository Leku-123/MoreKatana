using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MoreKatana.Assets.ExtraTextures;
using MoreKatana.Assets.ItemTextures;
using MoreKatana.Items;
using MoreKatana.Particles;
using MoreKatana.Prim;
using MoreKatana.Projectiles.PrimTrails;
using MoreKatana.Utilities;
using ReLogic.Content;
using System;
using System.IO;
using System.Reflection;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace MoreKatana
{
    public class MoreKatana : Mod
    {
        public static MoreKatana Instance { get; private set; }

        public const string EmptyTexture = "MoreKatana/Empty";

        public static Effect PrimitiveTextureMap;
        public static PrimTrailManager primitives;
        public static TrailManager TrailManager;

        private Vector2 _lastScreenSize;

        public MoreKatana()
        {
            Instance = this;
        }

        public override void Load()
        {
            if (!Main.dedServ)
            {
                ParticleHandler.RegisterParticles();
            }

            MoreKatanaDetours.Initialize();
            MoreKatanaTextures.LoadTextures();
            MoreKatanaItemTextures.LoadItemTextures();

            if (Main.netMode != NetmodeID.Server)
            {
                Filters.Scene["Shockwave"] = new Filter(new ScreenShaderData(ModContent.Request<Effect>("MoreKatana/Effects/ShockwaveEffect", AssetRequestMode.ImmediateLoad), "Shockwave"));
                Filters.Scene["Shockwave"].Load();

                GameShaders.Misc["Compression"] = new MiscShaderData(ModContent.Request<Effect>("MoreKatana/Effects/Compression", AssetRequestMode.ImmediateLoad), "ShieldPass");

                PrimitiveTextureMap = ModContent.Request<Effect>("MoreKatana/Effects/PrimitiveTextureMap", AssetRequestMode.ImmediateLoad).Value;
                primitives = new PrimTrailManager();
                primitives.LoadContent(Main.graphics.GraphicsDevice);

                TrailManager = new TrailManager();

                AddDrawLayerManager.Load();
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
            TrailManager = null;
            AddDrawLayerManager.Unload();
            ParticleHandler.Unload();
            MoreKatanaDetours.Unload();
            MoreKatanaTextures.UnloadTextures();
            MoreKatanaItemTextures.UnloadItemTextures();
        }

        public enum MessageType : byte
        {
            MouseWorld = 0,
            SpawnTrail,
        }

        public static void SyncData(MessageType msgType, int whoAmI, int toClient = -1, int ignoreClient = -1, object value = null, object value2 = null, object value3 = null)
        {
            if (Main.netMode == NetmodeID.SinglePlayer)
            {
                return;
            }
            Mod mod = MoreKatana.Instance;
            ModPacket packet;
            try
            {
                switch (msgType)
                {
                    case MessageType.MouseWorld:
                        packet = mod.GetPacket(256);
                        packet.Write((byte)MessageType.MouseWorld);
                        packet.Write((byte)whoAmI);
                        packet.WriteVector2(Main.player[whoAmI].MKPlayer().MouseWorld);
                        packet.Write(Main.player[whoAmI].controlUseTile);
                        packet.Send(toClient, ignoreClient);
                        break;
                    case MessageType.SpawnTrail:
                        packet = mod.GetPacket(256);
                        packet.Write((byte)MessageType.SpawnTrail);
                        packet.Write(whoAmI);
                        packet.Send(toClient, ignoreClient);
                        break;
                    default:
                        mod.Logger.Error(string.Format("MoreKatana: Unknown Packet type: {0}", msgType));
                        throw new Exception("MoreKatana: FInvalid Synchronization Data Packet type");
                }
            }
            catch (Exception e)
            {
                EndOfStreamException eose;
                ObjectDisposedException ode;
                if ((eose = (e as EndOfStreamException)) != null)
                {
                    mod.Logger.Error("MoreKatana: FInvalid Synchronization Data Packet type", eose);
                }
                else if ((ode = (e as ObjectDisposedException)) != null)
                {
                    mod.Logger.Error("MoreKatana: FInvalid Synchronization Data Packet type", ode);
                }
                else
                {
                    IOException ioe;
                    if ((ioe = (e as IOException)) == null)
                    {
                        throw;
                    }
                    mod.Logger.Error("MoreKatana: FInvalid Synchronization Data Packet type", ioe);
                }
            }
        }

        public override void HandlePacket(BinaryReader reader, int whoAmI)
        {
            MessageType msgType = (MessageType)reader.ReadByte();
            int proj;

            switch (msgType)
            {
                case MessageType.MouseWorld:
                    MoreKatanaPlayer.SyncMouseWorld(this, reader, whoAmI);
                    break;
                case MessageType.SpawnTrail:
                    proj = reader.ReadInt32();

                    if (Main.netMode == NetmodeID.Server)
                    {
                        SyncData(msgType, proj);
                        break;
                    }

                    if (Main.projectile[proj].ModProjectile is IManualTrailProjectile trailProj)
                        trailProj.DoTrailCreation(TrailManager);
                    break;
                default:
                    Logger.WarnFormat("MoreKatana: Unknown Message type: {0}", msgType);
                    break;
            }
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
                Instance.Logger.Error("An error occurred while manually saving MoreKatana configuration. It is safe to ignore this error.");
            }
        }
    }
}