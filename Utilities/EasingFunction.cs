using Microsoft.Xna.Framework;
using System;

namespace MoreKatana
{
    /// <summary>
    /// イージング関数です
    /// もっといいのがあったら変更してください
    /// </summary>
    /*public abstract class EaseFunction
    {
        public static readonly EaseFunction Linear = new PolynomialEase((float x) => { return x; });

        public static readonly EaseFunction EaseQuadIn = new PolynomialEase((float x) => { return x * x; });
        public static readonly EaseFunction EaseQuadOut = new PolynomialEase((float x) => { return 1f - EaseQuadIn.Ease(1f - x); });
        public static readonly EaseFunction EaseQuadInOut = new PolynomialEase((float x) => { return (x < 0.5f) ? 2f * x * x : -2f * x * x + 4f * x - 1f; });

        public static readonly EaseFunction EaseCubicIn = new PolynomialEase((float x) => { return x * x * x; });
        public static readonly EaseFunction EaseCubicOut = new PolynomialEase((float x) => { return 1f - EaseCubicIn.Ease(1f - x); });
        public static readonly EaseFunction EaseCubicInOut = new PolynomialEase((float x) => { return (x < 0.5f) ? 4f * x * x * x : 4f * x * x * x - 12f * x * x + 12f * x - 3f; });

        public static readonly EaseFunction EaseQuarticIn = new PolynomialEase((float x) => { return x * x * x * x; });
        public static readonly EaseFunction EaseQuarticOut = new PolynomialEase((float x) => { return 1f - EaseQuarticIn.Ease(1f - x); });
        public static readonly EaseFunction EaseQuarticInOut = new PolynomialEase((float x) => { return (x < 0.5f) ? 8f * x * x * x * x : -8f * x * x * x * x + 32f * x * x * x - 48f * x * x + 32f * x - 7f; });

        public static readonly EaseFunction EaseQuinticIn = new PolynomialEase((float x) => { return x * x * x * x * x; });
        public static readonly EaseFunction EaseQuinticOut = new PolynomialEase((float x) => { return 1f - EaseQuinticIn.Ease(1f - x); });
        public static readonly EaseFunction EaseQuinticInOut = new PolynomialEase((float x) => { return (x < 0.5f) ? 16f * x * x * x * x * x : 16f * x * x * x * x * x - 80f * x * x * x * x + 160f * x * x * x - 160f * x * x + 80f * x - 15f; });

        public static readonly EaseFunction EaseCircularIn = new PolynomialEase((float x) => { return 1f - (float)Math.Sqrt(1.0 - Math.Pow(x, 2)); });
        public static readonly EaseFunction EaseCircularOut = new PolynomialEase((float x) => { return (float)Math.Sqrt(1.0 - Math.Pow(x - 1.0, 2)); });
        public static readonly EaseFunction EaseCircularInOut = new PolynomialEase((float x) => { return (x < 0.5f) ? (1f - (float)Math.Sqrt(1.0 - Math.Pow(x * 2, 2))) * 0.5f : (float)((Math.Sqrt(1.0 - Math.Pow(-2 * x + 2, 2)) + 1) * 0.5); });

        public abstract float Ease(float time);
    }

    public class PolynomialEase : EaseFunction
    {
        private Func<float, float> _function;

        public PolynomialEase(Func<float, float> func)
        {
            _function = func;
        }

        public override float Ease(float time)
        {
            return _function(time);
        }
    }*/

    public static partial class MoreKatanaUtil
    {
        // ありがとうCalamity
        // すごく参考になりました

        /// <summary>
        /// 0から1までの値から、イージング関数処理された値を返します
        /// </summary>
        /// <param name="amount"></param>
        /// <param name="degree"></param>
        /// <returns></returns>
        public delegate float EasingFunction(float amount, int degree);

        public static float LinearEasing(float amount, int degree) => amount;
        //  Sines
        public static float SineInEasing(float amount, int degree) => 1f - (float)Math.Cos(amount * MathHelper.Pi / 2f);
        public static float SineOutEasing(float amount, int degree) => (float)Math.Sin(amount * MathHelper.Pi / 2f);
        public static float SineInOutEasing(float amount, int degree) => -((float)Math.Cos(amount * MathHelper.Pi) - 1) / 2f;
        public static float SineBumpEasing(float amount, int degree) => (float)Math.Sin(amount * MathHelper.Pi);
        // Polynomials
        public static float PolyInEasing(float amount, int degree) => (float)Math.Pow(amount, degree);
        public static float PolyOutEasing(float amount, int degree) => 1f - (float)Math.Pow(1f - amount, degree);
        public static float PolyInOutEasing(float amount, int degree) => amount < 0.5f ? (float)Math.Pow(2, degree - 1) * (float)Math.Pow(amount, degree) : 1f - (float)Math.Pow(-2 * amount + 2, degree) / 2f;
        // Exponential
        public static float ExpInEasing(float amount, int degree) => amount == 0f ? 0f : (float)Math.Pow(2, 10f * amount - 10f);
        public static float ExpOutEasing(float amount, int degree) => amount == 1f ? 1f : 1f - (float)Math.Pow(2, -10f * amount);
        public static float ExpInOutEasing(float amount, int degree) => amount == 0f ? 0f : amount == 1f ? 1f : amount < 0.5f ? (float)Math.Pow(2, 20f * amount - 10f) / 2f : (2f - (float)Math.Pow(2, -20f * amount - 10f)) / 2f;
        // Circular
        public static float CircInEasing(float amount, int degree) => (1f - (float)Math.Sqrt(1 - Math.Pow(amount, 2f)));
        public static float CircOutEasing(float amount, int degree) => (float)Math.Sqrt(1 - Math.Pow(amount - 1f, 2f));
        public static float CircInOutEasing(float amount, int degree) => amount < 0.5 ? (1f - (float)Math.Sqrt(1 - Math.Pow(2 * amount, 2f))) / 2f : ((float)Math.Sqrt(1 - Math.Pow(-2f * amount - 2f, 2f)) + 1f) / 2f;

        /// <summary>
        /// 区分線形関数
        /// </summary>
        public struct CurveSegment
        {
            /// <summary>
            /// セグメントで使用されるイージング関数の種類
            /// </summary>
            public EasingFunction easing;
            /// <summary>
            /// 全体のアニメーション上でセグメントが開始するタイミング
            /// </summary>
            public float startingX;
            /// <summary>
            /// セグメントが開始する高さ
            /// </summary>
            public float startingHeight;
            /// <summary>
            /// これはセグメント中に発生する高さの変化を表します。0にするとセグメントは平坦な線になります。
            /// 通常、この高さのシフトはセグメントの終わりで完全に適用されますが、SineBumpEasingでは曲線の頂点で到達するように設定されています。
            /// </summary>
            public float elevationShift;
            /// <summary>
            /// 選択されたイージングモードが多項式(Poly)­の場合の次数
            /// </summary>
            public int degree;

            /// <summary>
            /// 高さの変化を考慮した後のセグメントの高さ
            /// </summary>
            public float EndingHeight => startingHeight + elevationShift;

            public CurveSegment(EasingFunction MODE, float startX, float startHeight, float elevationShift, int degree = 1)
            {
                easing = MODE;
                startingX = startX;
                startingHeight = startHeight;
                this.elevationShift = elevationShift;
                this.degree = degree;
            }
        }

        /// <summary>
        /// これにより、任意のX値に対するカスタムの区分関数の高さが得られます。これにより、複雑なアニメーション曲線を簡単に作成できます。
        /// X値は自動的に0から1の間に制限されますが、関数の高さは0から1の範囲を超える場合があります。
        /// </summary>
        /// <param name="progress"> 曲線上の位置。自動的に0から1の間に固定されます </param>
        /// <param name="segments"> アニメーション曲線全体を構成する一連の曲線セグメント </param>
        /// <returns></returns>
        public static float PiecewiseAnimation(float progress, params CurveSegment[] segments)
        {
            if (segments.Length == 0)
                return 0f;

            if (segments[0].startingX != 0)
                segments[0].startingX = 0;

            progress = MathHelper.Clamp(progress, 0f, 1f);
            float ratio = 0f;

            for (int i = 0; i <= segments.Length - 1; i++)
            {
                CurveSegment segment = segments[i];
                float startPoint = segment.startingX;
                float endPoint = 1f;

                if (progress < segment.startingX)
                    continue;

                if (i < segments.Length - 1)
                {
                    if (segments[i + 1].startingX <= progress)
                        continue;
                    endPoint = segments[i + 1].startingX;
                }

                float segmentLength = endPoint - startPoint;
                float segmentProgress = (progress - segment.startingX) / segmentLength;
                ratio = segment.startingHeight;

                if (segment.easing != null)
                    ratio += segment.easing(segmentProgress, segment.degree) * segment.elevationShift;

                else
                    ratio += LinearEasing(segmentProgress, segment.degree) * segment.elevationShift;

                break;
            }
            return ratio;
        }
    }
}