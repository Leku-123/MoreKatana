using System;

namespace MoreKatana
{
    public abstract class EaseFunction
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
    }
}