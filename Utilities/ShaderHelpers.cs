using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace MoreKatana
{
    static partial class Helpers
    {
        public static bool HasParameter(this Effect effect, string parameterName)
        {
            foreach (EffectParameter parameter in effect.Parameters)
            {
                if (parameter.Name == parameterName)
                {
                    return true;
                }
            }

            return false;
        }

        public static void SetBasicEffectMatrices(ref BasicEffect effect, Vector2 zoom)
        {
            GetWorldViewProjection(zoom, out Matrix view, out Matrix projection);

            effect.View = view;
            effect.Projection = projection;
        }

        public static void GetWorldViewProjection(Vector2 zoom, out Matrix view, out Matrix projection)
        {
            int width = Main.graphics.GraphicsDevice.Viewport.Width;
            int height = Main.graphics.GraphicsDevice.Viewport.Height;

            view = Matrix.CreateLookAt(Vector3.Zero, Vector3.UnitZ, Vector3.Up) *
                          Matrix.CreateTranslation(width / 2f, height / -2f, 0) * Matrix.CreateRotationZ(MathHelper.Pi) *
                          Matrix.CreateScale(zoom.X, zoom.Y, 1f);

            projection = Matrix.CreateOrthographic(width, height, 0, 1000);
        }

        public static void CalculatePerspectiveMatricies(out Matrix viewMatrix, out Matrix projectionMatrix, int MatrixType)
        {
            if (MatrixType == 0)
            {
                Vector2 zoom = Main.GameViewMatrix.Zoom;
                Matrix zoomScaleMatrix = Matrix.CreateScale(zoom.X, zoom.Y, 1f);
                int width = Main.instance.GraphicsDevice.Viewport.Width;
                int height = Main.instance.GraphicsDevice.Viewport.Height;
                viewMatrix = Matrix.CreateLookAt(Vector3.Zero, Vector3.UnitZ, Vector3.Up);
                viewMatrix *= Matrix.CreateTranslation(0f, (float)(-(float)height), 0f);
                viewMatrix *= Matrix.CreateRotationZ(3.1415927f);
                if (Main.LocalPlayer.gravDir == -1f)
                {
                    viewMatrix *= Matrix.CreateScale(1f, -1f, 1f) * Matrix.CreateTranslation(0f, height, 0f);
                }
                viewMatrix *= zoomScaleMatrix;
                projectionMatrix = Matrix.CreateOrthographicOffCenter(0f, width * zoom.X, 0f, height * zoom.Y, 0f, 1f) * zoomScaleMatrix;
            }
            else
            {
                Vector2 zoom = new Vector2(Main.UIScale, Main.UIScale);
                Matrix zoomScaleMatrix = Main.UIScaleMatrix;
                int width = Main.screenWidth;
                int height = Main.screenHeight;
                viewMatrix = Matrix.CreateLookAt(Vector3.Zero, Vector3.UnitZ, Vector3.Up);
                viewMatrix *= Matrix.CreateTranslation(0f, (float)(-(float)height), 0f);
                viewMatrix *= Matrix.CreateRotationZ(3.1415927f);
                viewMatrix *= zoomScaleMatrix;
                projectionMatrix = Matrix.CreateOrthographicOffCenter(0f, width * zoom.X, 0f, height * zoom.Y, 0f, 1f);
            }
        }
    }
}