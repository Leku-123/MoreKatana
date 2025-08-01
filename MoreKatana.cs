using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MoreKatana.Prim;
using ReLogic.Content;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace MoreKatana
{
    public class MoreKatana : Mod
    {
        public const string EmptyTexture = "MoreKatana/Empty";

        private Vector2 _lastScreenSize;
        private Vector2 _lastViewSize;
        private Viewport _lastViewPort;

        public static MoreKatana Instance;

        public static BasicEffect basicEffect;
        public static Effect PrimitiveTextureMap;
        public static PrimTrailManager primitives;

        public MoreKatana()
        {
            Instance = this;
        }

        public override void Load()
        {
            MoreKatanaDetours.Initialize();

            if (Main.netMode != NetmodeID.Server)
            {
                int width = Main.graphics.GraphicsDevice.Viewport.Width;
                int height = Main.graphics.GraphicsDevice.Viewport.Height;
                Vector2 zoom = Main.GameViewMatrix.Zoom;
                Matrix view = Matrix.CreateLookAt(Vector3.Zero, Vector3.UnitZ, Vector3.Up) * Matrix.CreateTranslation(width / 2, height / -2, 0) * Matrix.CreateRotationZ(MathHelper.Pi) * Matrix.CreateScale(zoom.X, zoom.Y, 1f);
                Matrix projection = Matrix.CreateOrthographic(width, height, 0, 1000);

                Main.QueueMainThreadAction(() =>
                {
                    basicEffect = new BasicEffect(Main.graphics.GraphicsDevice)
                    {
                        VertexColorEnabled = true,
                        View = view,
                        Projection = projection
                    };
                });

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

                    if ((_lastViewPort.Bounds != Main.graphics.GraphicsDevice.Viewport.Bounds || _lastScreenSize != new Vector2(Main.screenWidth, Main.screenHeight) || _lastViewSize != Main.ViewSize)
                        && basicEffect != null && primitives != null)
                    {
                        Helpers.SetBasicEffectMatrices(ref basicEffect, Main.GameViewMatrix.Zoom);
                    }

                    _lastScreenSize = new Vector2(Main.screenWidth, Main.screenHeight);
                    _lastViewSize = Main.ViewSize;
                    _lastViewPort = Main.graphics.GraphicsDevice.Viewport;
                });
            }
        }

        public override void Unload()
        {
            PrimitiveTextureMap = null;
            primitives = null;

            MoreKatanaDetours.Unload();
        }
    }
}