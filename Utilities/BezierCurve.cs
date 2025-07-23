using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace MoreKatana
{
    public class BezierCurve
    {
        private Vector2[] _controlPoints;

        public BezierCurve(params Vector2[] controls) => _controlPoints = controls;

        /// <summary>
        /// ベジェ曲線に沿った<see cref="Vector2"/>を返す
        /// </summary>
        /// <param name="T"> ベジェ曲線のどの辺りまでポイントを返すか </param>
        /// <returns></returns>
        public Vector2 Evaluate(float T)
        {
            if (T < 0f) T = 0f;
            if (T > 1f) T = 1f;

            return PrivateEvaluate(_controlPoints, T);
        }

        /// <summary>
        /// ベジェ曲線に沿ったポイントのリストを取得する。少なくとも2になるようにする
        /// </summary>
        /// <param name="amount"> 取得するポイント </param>
        /// <returns> ポイントを表すリスト </returns>
        public List<Vector2> GetPoints(int amount)
        {
            if (amount < 2)
                amount = 2;

            float perStep = 1f / (amount - 1);

            List<Vector2> points = new List<Vector2>();

            for (int i = 0; i < amount; i++)
                points.Add(Evaluate(perStep * i));

            return points;
        }

        private Vector2 PrivateEvaluate(Vector2[] points, float T)
        {
            if (points.Length > 2)
            {
                Vector2[] nextPoints = new Vector2[points.Length - 1];
                for (int k = 0; k < points.Length - 1; k++)
                    nextPoints[k] = Vector2.Lerp(points[k], points[k + 1], T);

                return PrivateEvaluate(nextPoints, T);
            }
            else
            {
                return Vector2.Lerp(points[0], points[1], T);
            }
        }

        public Vector2 this[int x]
        {
            get
            {
                return _controlPoints[x];
            }
            set
            {
                _controlPoints[x] = value;
            }
        }
    }
}