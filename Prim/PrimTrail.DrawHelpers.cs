using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;

namespace MoreKatana.Prim
{
    public partial class PrimTrail
    {
        public interface ITrailShader
        {
            string ShaderPass { get; }

            void ApplyShader<TTrail>(Effect effect, TTrail trail, List<Vector2> positions, string esp, float progressParam);
        }

        public class DefaultShader : ITrailShader
        {
            public string ShaderPass => "DefaultPass";

            public void ApplyShader<T>(Effect effect, T trail, List<Vector2> positions, string esp, float progressParam)
            {
                try
                {
                    effect.Parameters["progress"].SetValue(progressParam);
                    effect.CurrentTechnique.Passes[esp].Apply();
                    effect.CurrentTechnique.Passes[ShaderPass].Apply();
                }
                catch
                {
                    // ここは無視
                }
            }
        }

        protected static Vector2 CurveNormal(List<Vector2> points, int index)
        {
            if (points.Count == 1)
                return points[0];

            if (index == 0)
                return Vector2.Normalize(points[1] - points[0]).TurnRight();

            return (index == points.Count - 1
                ? Vector2.Normalize(points[index] - points[index - 1])
                : Vector2.Normalize(points[index + 1] - points[index - 1]))
                .TurnRight();
        }

        protected void PrepareShader(Effect effects, string passName, float progress, Color? color = null)
        {
            if (color == null)
                color = Color.White;

            int width = GraphicsDevice.Viewport.Width;
            int height = GraphicsDevice.Viewport.Height;

            Vector2 zoom = Main.GameViewMatrix.Zoom;
            Matrix view = Matrix.CreateLookAt(Vector3.Zero, Vector3.UnitZ, Vector3.Up) *
                          Matrix.CreateTranslation(width / 2f, height / -2f, 0) * Matrix.CreateRotationZ(MathHelper.Pi) *
                          Matrix.CreateScale(zoom.X, zoom.Y, 1f);

            Matrix projection = Matrix.CreateOrthographic(width, height, 0, 1000);
            effects.Parameters["WorldViewProjection"].SetValue(view * projection);

            if (effects.HasParameter("uColor"))
                effects.Parameters["uColor"].SetValue(color.Value.ToVector3());

            TrailShader.ApplyShader(effects, this, Points, passName, progress);
        }

        protected void AddVertex(Vector2 position, Color color, Vector2 uv)
        {
            if (CurrentIndex < Vertices.Length)
                Vertices[CurrentIndex++] = new VertexPositionColorTexture(new Vector3(position - Main.screenPosition, 0f), color, uv);
        }

        protected void DrawBasicTrail(Color color)
        {
            for (int i = 0; i < Points.Count; i++)
            {
                float widthVar = (float)Math.Sqrt(i) * Width;
                Color c = Color;
                Color CBT = Color;
                Vector2 normal = CurveNormal(Points, i);
                Vector2 normalAhead = CurveNormal(Points, i + 1);
                float j = (Cap + (float)Math.Sin(Counter / 10f) * 1 - i * 0.1f) / Cap;
                widthVar *= j;
                Vector2 firstUp = Points[i] - normal * widthVar;
                Vector2 firstDown = Points[i] + normal * widthVar;
                Vector2 secondUp = Points[i + 1] - normalAhead * widthVar;
                Vector2 secondDown = Points[i + 1] + normalAhead * widthVar;

                AddVertex(firstDown, c * AlphaValue, new Vector2(i / Cap, 1));
                AddVertex(firstUp, c * AlphaValue, new Vector2(i / Cap, 0));
                AddVertex(secondDown, CBT * AlphaValue, new Vector2((i + 1) / Cap, 1));

                AddVertex(secondUp, CBT * AlphaValue, new Vector2((i + 1) / Cap, 0));
                AddVertex(secondDown, CBT * AlphaValue, new Vector2((i + 1) / Cap, 1));
                AddVertex(firstUp, c * AlphaValue, new Vector2(i / Cap, 0));
            }
        }
    }
}