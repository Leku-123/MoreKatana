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
                    // 無視
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

        protected void DrawBasicTrail(Color color, float widthVar)
        {
            for (int i = 0; i < Points.Count; i++)
            {
                if (i == 0)
                {
                    Vector2 normalAhead = CurveNormal(Points, i + 1);
                    Vector2 secondUp = Points[i + 1] - normalAhead * widthVar;
                    //Vector2 secondDown = Points[i + 1] + normalAhead * widthVar;

                    AddVertex(Points[i], color * AlphaValue, new Vector2(0, 0.5f));
                    AddVertex(secondUp, color * AlphaValue * ((i + 1) / (float)Points.Count), new Vector2((i + 1) / (float)Points.Count, 0));
                    AddVertex(secondUp, color * AlphaValue * ((i + 1) / (float)Points.Count), new Vector2((i + 1) / (float)Points.Count, 1));
                }
                else
                {
                    if (i == Points.Count - 1)
                        continue;

                    Vector2 normal = CurveNormal(Points, i);
                    Vector2 normalAhead = CurveNormal(Points, i + 1);
                    Vector2 firstUp = Points[i] - normal * widthVar;
                    Vector2 firstDown = Points[i] + normal * widthVar;
                    Vector2 secondUp = Points[i + 1] - normalAhead * widthVar;
                    Vector2 secondDown = Points[i + 1] + normalAhead * widthVar;

                    AddVertex(firstDown, color * AlphaValue * (i / (float)Points.Count), new Vector2(i / (float)Points.Count, 1));
                    AddVertex(firstUp, color * AlphaValue * (i / (float)Points.Count), new Vector2(i / (float)Points.Count, 0));
                    AddVertex(secondDown, color * AlphaValue * ((i + 1) / (float)Points.Count), new Vector2((i + 1) / (float)Points.Count, 1));

                    AddVertex(secondUp, color * AlphaValue * ((i + 1) / (float)Points.Count), new Vector2((i + 1) / (float)Points.Count, 0));
                    AddVertex(secondDown, color * AlphaValue * ((i + 1) / (float)Points.Count), new Vector2((i + 1) / (float)Points.Count, 1));
                    AddVertex(firstUp, color * AlphaValue * (i / (float)Points.Count), new Vector2(i / (float)Points.Count, 0));
                }
            }
        }
    }
}