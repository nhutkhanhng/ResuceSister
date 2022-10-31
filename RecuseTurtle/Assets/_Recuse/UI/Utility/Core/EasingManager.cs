
using System;
using UnityEngine;

namespace UIManager.Easing
{
    public enum Ease
    {
        Unset,
        Linear,
        InSine,
        OutSine,
        InOutSine,
        InQuad,
        OutQuad,
        InOutQuad,
        InCubic,
        OutCubic,
        InOutCubic,
        InQuart,
        OutQuart,
        InOutQuart,
        InQuint,
        OutQuint,
        InOutQuint,
        InExpo,
        OutExpo,
        InOutExpo,
        InCirc,
        OutCirc,
        InOutCirc,
        InElastic,
        OutElastic,
        InOutElastic,
        InBack,
        OutBack,
        InOutBack,
        InBounce,
        OutBounce,
        InOutBounce,
        Flash,
        InFlash,
        OutFlash,
        InOutFlash,
        /// <summary>
        /// Don't assign this! It's assigned automatically when creating 0 duration tweens
        /// </summary>
        INTERNAL_Zero,
        /// <summary>
        /// Don't assign this! It's assigned automatically when setting the ease to an AnimationCurve or to a custom ease function
        /// </summary>
        INTERNAL_Custom,
    }

    public static class EaseManager
    {
        private const float _PiOver2 = 1.570796f;
        private const float _TwoPi = 6.283185f;

        /// <summary>
        /// Used for custom and animationCurve-based ease functions. Must return a value between 0 and 1.
        /// </summary>
        public delegate float EaseFunction(
          float time,
          float duration,
          float overshootOrAmplitude,
          float period);

        /// <summary>
        /// Returns a value between 0 and 1 (inclusive) based on the elapsed time and ease selected
        /// </summary>
        public static float Evaluate(
          Ease easeType,
          EaseFunction customEase,
          float time,
          float duration,
          float overshootOrAmplitude,
          float period)
        {
            switch (easeType)
            {
                case Ease.Linear:
                    return time / duration;
                case Ease.InSine:
                    return (float)(-Math.Cos((double)time / (double)duration * 1.57079637050629) + 1.0);
                case Ease.OutSine:
                    return (float)Math.Sin((double)time / (double)duration * 1.57079637050629);
                case Ease.InOutSine:
                    return (float)(-0.5 * (Math.Cos(3.14159274101257 * (double)time / (double)duration) - 1.0));
                case Ease.InQuad:
                    return (time /= duration) * time;
                case Ease.OutQuad:
                    return (float)(-(double)(time /= duration) * ((double)time - 2.0));
                case Ease.InOutQuad:
                    return (double)(time /= duration * 0.5f) < 1.0 ? 0.5f * time * time : (float)(-0.5 * ((double)--time * ((double)time - 2.0) - 1.0));
                case Ease.InCubic:
                    return (time /= duration) * time * time;
                case Ease.OutCubic:
                    return (float)((double)(time = (float)((double)time / (double)duration - 1.0)) * (double)time * (double)time + 1.0);
                case Ease.InOutCubic:
                    return (double)(time /= duration * 0.5f) < 1.0 ? 0.5f * time * time * time : (float)(0.5 * ((double)(time -= 2f) * (double)time * (double)time + 2.0));
                case Ease.InQuart:
                    return (time /= duration) * time * time * time;
                case Ease.OutQuart:
                    return (float)-((double)(time = (float)((double)time / (double)duration - 1.0)) * (double)time * (double)time * (double)time - 1.0);
                case Ease.InOutQuart:
                    return (double)(time /= duration * 0.5f) < 1.0 ? 0.5f * time * time * time * time : (float)(-0.5 * ((double)(time -= 2f) * (double)time * (double)time * (double)time - 2.0));
                case Ease.InQuint:
                    return (time /= duration) * time * time * time * time;
                case Ease.OutQuint:
                    return (float)((double)(time = (float)((double)time / (double)duration - 1.0)) * (double)time * (double)time * (double)time * (double)time + 1.0);
                case Ease.InOutQuint:
                    return (double)(time /= duration * 0.5f) < 1.0 ? 0.5f * time * time * time * time * time : (float)(0.5 * ((double)(time -= 2f) * (double)time * (double)time * (double)time * (double)time + 2.0));
                case Ease.InExpo:
                    return (double)time != 0.0 ? (float)Math.Pow(2.0, 10.0 * ((double)time / (double)duration - 1.0)) : 0.0f;
                case Ease.OutExpo:
                    return (double)time == (double)duration ? 1f : (float)(-Math.Pow(2.0, -10.0 * (double)time / (double)duration) + 1.0);
                case Ease.InOutExpo:
                    if ((double)time == 0.0)
                        return 0.0f;
                    if ((double)time == (double)duration)
                        return 1f;
                    return (double)(time /= duration * 0.5f) < 1.0 ? 0.5f * (float)Math.Pow(2.0, 10.0 * ((double)time - 1.0)) : (float)(0.5 * (-Math.Pow(2.0, -10.0 * (double)--time) + 2.0));
                case Ease.InCirc:
                    return (float)-(Math.Sqrt(1.0 - (double)(time /= duration) * (double)time) - 1.0);
                case Ease.OutCirc:
                    return (float)Math.Sqrt(1.0 - (double)(time = (float)((double)time / (double)duration - 1.0)) * (double)time);
                case Ease.InOutCirc:
                    if ((double)(time /= duration * 0.5f) >= 1.0)
                        return (float)(0.5 * (Math.Sqrt(1.0 - (double)(time -= 2f) * (double)time) + 1.0));
                    double num1 = (double)time;
                    return (float)(-0.5 * (Math.Sqrt(1.0 - num1 * num1) - 1.0));
                case Ease.InElastic:
                    if ((double)time == 0.0)
                        return 0.0f;
                    if ((double)(time /= duration) == 1.0)
                        return 1f;
                    if ((double)period == 0.0)
                        period = duration * 0.3f;
                    float num2;
                    if ((double)overshootOrAmplitude < 1.0)
                    {
                        overshootOrAmplitude = 1f;
                        num2 = period / 4f;
                    }
                    else
                        num2 = period / 6.283185f * (float)Math.Asin(1.0 / (double)overshootOrAmplitude);
                    return (float)-((double)overshootOrAmplitude * Math.Pow(2.0, 10.0 * (double)--time) * Math.Sin(((double)time * (double)duration - (double)num2) * 6.28318548202515 / (double)period));
                case Ease.OutElastic:
                    if ((double)time == 0.0)
                        return 0.0f;
                    if ((double)(time /= duration) == 1.0)
                        return 1f;
                    if ((double)period == 0.0)
                        period = duration * 0.3f;
                    float num3;
                    if ((double)overshootOrAmplitude < 1.0)
                    {
                        overshootOrAmplitude = 1f;
                        num3 = period / 4f;
                    }
                    else
                        num3 = period / 6.283185f * (float)Math.Asin(1.0 / (double)overshootOrAmplitude);
                    return (float)((double)overshootOrAmplitude * Math.Pow(2.0, -10.0 * (double)time) * Math.Sin(((double)time * (double)duration - (double)num3) * 6.28318548202515 / (double)period) + 1.0);
                case Ease.InOutElastic:
                    if ((double)time == 0.0)
                        return 0.0f;
                    if ((double)(time /= duration * 0.5f) == 2.0)
                        return 1f;
                    if ((double)period == 0.0)
                        period = duration * 0.45f;
                    float num4;
                    if ((double)overshootOrAmplitude < 1.0)
                    {
                        overshootOrAmplitude = 1f;
                        num4 = period / 4f;
                    }
                    else
                        num4 = period / 6.283185f * (float)Math.Asin(1.0 / (double)overshootOrAmplitude);
                    return (double)time < 1.0 ? (float)(-0.5 * ((double)overshootOrAmplitude * Math.Pow(2.0, 10.0 * (double)--time) * Math.Sin(((double)time * (double)duration - (double)num4) * 6.28318548202515 / (double)period))) : (float)((double)overshootOrAmplitude * Math.Pow(2.0, -10.0 * (double)--time) * Math.Sin(((double)time * (double)duration - (double)num4) * 6.28318548202515 / (double)period) * 0.5 + 1.0);
                case Ease.InBack:
                    return (float)((double)(time /= duration) * (double)time * (((double)overshootOrAmplitude + 1.0) * (double)time - (double)overshootOrAmplitude));
                case Ease.OutBack:
                    return (float)((double)(time = (float)((double)time / (double)duration - 1.0)) * (double)time * (((double)overshootOrAmplitude + 1.0) * (double)time + (double)overshootOrAmplitude) + 1.0);
                case Ease.InOutBack:
                    if ((double)(time /= duration * 0.5f) >= 1.0)
                        return (float)(0.5 * ((double)(time -= 2f) * (double)time * (((double)(overshootOrAmplitude *= 1.525f) + 1.0) * (double)time + (double)overshootOrAmplitude) + 2.0));
                    double num5 = (double)time;
                    return (float)(0.5 * (num5 * num5 * (((double)(overshootOrAmplitude *= 1.525f) + 1.0) * (double)time - (double)overshootOrAmplitude)));
                case Ease.InBounce:
                    return Bounce.EaseIn(time, duration, overshootOrAmplitude, period);
                case Ease.OutBounce:
                    return Bounce.EaseOut(time, duration, overshootOrAmplitude, period);
                case Ease.InOutBounce:
                    return Bounce.EaseInOut(time, duration, overshootOrAmplitude, period);
                case Ease.Flash:
                    return Flash.Ease(time, duration, overshootOrAmplitude, period);
                case Ease.InFlash:
                    return Flash.EaseIn(time, duration, overshootOrAmplitude, period);
                case Ease.OutFlash:
                    return Flash.EaseOut(time, duration, overshootOrAmplitude, period);
                case Ease.InOutFlash:
                    return Flash.EaseInOut(time, duration, overshootOrAmplitude, period);
                case Ease.INTERNAL_Zero:
                    return 1f;
                case Ease.INTERNAL_Custom:
                    return customEase(time, duration, overshootOrAmplitude, period);
                default:
                    return (float)(-(double)(time /= duration) * ((double)time - 2.0));
            }
        }

        public static EaseFunction ToEaseFunction(Ease ease)
        {
            switch (ease)
            {
                case Ease.Linear:
                    return (EaseFunction)((time, duration, overshootOrAmplitude, period) => time / duration);
                case Ease.InSine:
                    return (EaseFunction)((time, duration, overshootOrAmplitude, period) => (float)(-Math.Cos((double)time / (double)duration * 1.57079637050629) + 1.0));
                case Ease.OutSine:
                    return (EaseFunction)((time, duration, overshootOrAmplitude, period) => (float)Math.Sin((double)time / (double)duration * 1.57079637050629));
                case Ease.InOutSine:
                    return (EaseFunction)((time, duration, overshootOrAmplitude, period) => (float)(-0.5 * (Math.Cos(3.14159274101257 * (double)time / (double)duration) - 1.0)));
                case Ease.InQuad:
                    return (EaseFunction)((time, duration, overshootOrAmplitude, period) => (time /= duration) * time);
                case Ease.OutQuad:
                    return (EaseFunction)((time, duration, overshootOrAmplitude, period) => (float)(-(double)(time /= duration) * ((double)time - 2.0)));
                case Ease.InOutQuad:
                    return (EaseFunction)((time, duration, overshootOrAmplitude, period) => (double)(time /= duration * 0.5f) < 1.0 ? 0.5f * time * time : (float)(-0.5 * ((double)--time * ((double)time - 2.0) - 1.0)));
                case Ease.InCubic:
                    return (EaseFunction)((time, duration, overshootOrAmplitude, period) => (time /= duration) * time * time);
                case Ease.OutCubic:
                    return (EaseFunction)((time, duration, overshootOrAmplitude, period) => (float)((double)(time = (float)((double)time / (double)duration - 1.0)) * (double)time * (double)time + 1.0));
                case Ease.InOutCubic:
                    return (EaseFunction)((time, duration, overshootOrAmplitude, period) => (double)(time /= duration * 0.5f) < 1.0 ? 0.5f * time * time * time : (float)(0.5 * ((double)(time -= 2f) * (double)time * (double)time + 2.0)));
                case Ease.InQuart:
                    return (EaseFunction)((time, duration, overshootOrAmplitude, period) => (time /= duration) * time * time * time);
                case Ease.OutQuart:
                    return (EaseFunction)((time, duration, overshootOrAmplitude, period) => (float)-((double)(time = (float)((double)time / (double)duration - 1.0)) * (double)time * (double)time * (double)time - 1.0));
                case Ease.InOutQuart:
                    return (EaseFunction)((time, duration, overshootOrAmplitude, period) => (double)(time /= duration * 0.5f) < 1.0 ? 0.5f * time * time * time * time : (float)(-0.5 * ((double)(time -= 2f) * (double)time * (double)time * (double)time - 2.0)));
                case Ease.InQuint:
                    return (EaseFunction)((time, duration, overshootOrAmplitude, period) => (time /= duration) * time * time * time * time);
                case Ease.OutQuint:
                    return (EaseFunction)((time, duration, overshootOrAmplitude, period) => (float)((double)(time = (float)((double)time / (double)duration - 1.0)) * (double)time * (double)time * (double)time * (double)time + 1.0));
                case Ease.InOutQuint:
                    return (EaseFunction)((time, duration, overshootOrAmplitude, period) => (double)(time /= duration * 0.5f) < 1.0 ? 0.5f * time * time * time * time * time : (float)(0.5 * ((double)(time -= 2f) * (double)time * (double)time * (double)time * (double)time + 2.0)));
                case Ease.InExpo:
                    return (EaseFunction)((time, duration, overshootOrAmplitude, period) => (double)time != 0.0 ? (float)Math.Pow(2.0, 10.0 * ((double)time / (double)duration - 1.0)) : 0.0f);
                case Ease.OutExpo:
                    return (EaseFunction)((time, duration, overshootOrAmplitude, period) => (double)time == (double)duration ? 1f : (float)(-Math.Pow(2.0, -10.0 * (double)time / (double)duration) + 1.0));
                case Ease.InOutExpo:
                    return (EaseFunction)((time, duration, overshootOrAmplitude, period) =>
                    {
                        if ((double)time == 0.0)
                            return 0.0f;
                        if ((double)time == (double)duration)
                            return 1f;
                        return (double)(time /= duration * 0.5f) < 1.0 ? 0.5f * (float)Math.Pow(2.0, 10.0 * ((double)time - 1.0)) : (float)(0.5 * (-Math.Pow(2.0, -10.0 * (double)--time) + 2.0));
                    });
                case Ease.InCirc:
                    return (EaseFunction)((time, duration, overshootOrAmplitude, period) => (float)-(Math.Sqrt(1.0 - (double)(time /= duration) * (double)time) - 1.0));
                case Ease.OutCirc:
                    return (EaseFunction)((time, duration, overshootOrAmplitude, period) => (float)Math.Sqrt(1.0 - (double)(time = (float)((double)time / (double)duration - 1.0)) * (double)time));
                case Ease.InOutCirc:
                    return (EaseFunction)((time, duration, overshootOrAmplitude, period) =>
                    {
                        if ((double)(time /= duration * 0.5f) >= 1.0)
                            return (float)(0.5 * (Math.Sqrt(1.0 - (double)(time -= 2f) * (double)time) + 1.0));
                        double num = (double)time;
                        return (float)(-0.5 * (Math.Sqrt(1.0 - num * num) - 1.0));
                    });
                case Ease.InElastic:
                    return (EaseFunction)((time, duration, overshootOrAmplitude, period) =>
                    {
                        if ((double)time == 0.0)
                            return 0.0f;
                        if ((double)(time /= duration) == 1.0)
                            return 1f;
                        if ((double)period == 0.0)
                            period = duration * 0.3f;
                        float num;
                        if ((double)overshootOrAmplitude < 1.0)
                        {
                            overshootOrAmplitude = 1f;
                            num = period / 4f;
                        }
                        else
                            num = period / 6.283185f * (float)Math.Asin(1.0 / (double)overshootOrAmplitude);
                        return (float)-((double)overshootOrAmplitude * Math.Pow(2.0, 10.0 * (double)--time) * Math.Sin(((double)time * (double)duration - (double)num) * 6.28318548202515 / (double)period));
                    });
                case Ease.OutElastic:
                    return (EaseFunction)((time, duration, overshootOrAmplitude, period) =>
                    {
                        if ((double)time == 0.0)
                            return 0.0f;
                        if ((double)(time /= duration) == 1.0)
                            return 1f;
                        if ((double)period == 0.0)
                            period = duration * 0.3f;
                        float num;
                        if ((double)overshootOrAmplitude < 1.0)
                        {
                            overshootOrAmplitude = 1f;
                            num = period / 4f;
                        }
                        else
                            num = period / 6.283185f * (float)Math.Asin(1.0 / (double)overshootOrAmplitude);
                        return (float)((double)overshootOrAmplitude * Math.Pow(2.0, -10.0 * (double)time) * Math.Sin(((double)time * (double)duration - (double)num) * 6.28318548202515 / (double)period) + 1.0);
                    });
                case Ease.InOutElastic:
                    return (EaseFunction)((time, duration, overshootOrAmplitude, period) =>
                    {
                        if ((double)time == 0.0)
                            return 0.0f;
                        if ((double)(time /= duration * 0.5f) == 2.0)
                            return 1f;
                        if ((double)period == 0.0)
                            period = duration * 0.45f;
                        float num;
                        if ((double)overshootOrAmplitude < 1.0)
                        {
                            overshootOrAmplitude = 1f;
                            num = period / 4f;
                        }
                        else
                            num = period / 6.283185f * (float)Math.Asin(1.0 / (double)overshootOrAmplitude);
                        return (double)time < 1.0 ? (float)(-0.5 * ((double)overshootOrAmplitude * Math.Pow(2.0, 10.0 * (double)--time) * Math.Sin(((double)time * (double)duration - (double)num) * 6.28318548202515 / (double)period))) : (float)((double)overshootOrAmplitude * Math.Pow(2.0, -10.0 * (double)--time) * Math.Sin(((double)time * (double)duration - (double)num) * 6.28318548202515 / (double)period) * 0.5 + 1.0);
                    });
                case Ease.InBack:
                    return (EaseFunction)((time, duration, overshootOrAmplitude, period) => (float)((double)(time /= duration) * (double)time * (((double)overshootOrAmplitude + 1.0) * (double)time - (double)overshootOrAmplitude)));
                case Ease.OutBack:
                    return (EaseFunction)((time, duration, overshootOrAmplitude, period) => (float)((double)(time = (float)((double)time / (double)duration - 1.0)) * (double)time * (((double)overshootOrAmplitude + 1.0) * (double)time + (double)overshootOrAmplitude) + 1.0));
                case Ease.InOutBack:
                    return (EaseFunction)((time, duration, overshootOrAmplitude, period) =>
                    {
                        if ((double)(time /= duration * 0.5f) >= 1.0)
                            return (float)(0.5 * ((double)(time -= 2f) * (double)time * (((double)(overshootOrAmplitude *= 1.525f) + 1.0) * (double)time + (double)overshootOrAmplitude) + 2.0));
                        double num = (double)time;
                        return (float)(0.5 * (num * num * (((double)(overshootOrAmplitude *= 1.525f) + 1.0) * (double)time - (double)overshootOrAmplitude)));
                    });
                case Ease.InBounce:
                    return (EaseFunction)((time, duration, overshootOrAmplitude, period) => Bounce.EaseIn(time, duration, overshootOrAmplitude, period));
                case Ease.OutBounce:
                    return (EaseFunction)((time, duration, overshootOrAmplitude, period) => Bounce.EaseOut(time, duration, overshootOrAmplitude, period));
                case Ease.InOutBounce:
                    return (EaseFunction)((time, duration, overshootOrAmplitude, period) => Bounce.EaseInOut(time, duration, overshootOrAmplitude, period));
                case Ease.Flash:
                    return (EaseFunction)((time, duration, overshootOrAmplitude, period) => Flash.Ease(time, duration, overshootOrAmplitude, period));
                case Ease.InFlash:
                    return (EaseFunction)((time, duration, overshootOrAmplitude, period) => Flash.EaseIn(time, duration, overshootOrAmplitude, period));
                case Ease.OutFlash:
                    return (EaseFunction)((time, duration, overshootOrAmplitude, period) => Flash.EaseOut(time, duration, overshootOrAmplitude, period));
                case Ease.InOutFlash:
                    return (EaseFunction)((time, duration, overshootOrAmplitude, period) => Flash.EaseInOut(time, duration, overshootOrAmplitude, period));
                default:
                    return (EaseFunction)((time, duration, overshootOrAmplitude, period) => (float)(-(double)(time /= duration) * ((double)time - 2.0)));
            }
        }
    }


    /// <summary>
    /// This class contains a C# port of the easing equations created by Robert Penner (http://robertpenner.com/easing).
    /// </summary>
    public static class Bounce
    {
        /// <summary>
        /// Easing equation function for a bounce (exponentially decaying parabolic bounce) easing in: accelerating from zero velocity.
        /// </summary>
        /// <param name="time">Current time (in frames or seconds).</param>
        /// <param name="duration">
        /// Expected easing duration (in frames or seconds).
        /// </param>
        /// <param name="unusedOvershootOrAmplitude">Unused: here to keep same delegate for all ease types.</param>
        /// <param name="unusedPeriod">Unused: here to keep same delegate for all ease types.</param>
        /// <returns>The eased value.</returns>
        public static float EaseIn(
          float time,
          float duration,
          float unusedOvershootOrAmplitude,
          float unusedPeriod)
        {
            return 1f - Bounce.EaseOut(duration - time, duration, -1f, -1f);
        }

        /// <summary>
        /// Easing equation function for a bounce (exponentially decaying parabolic bounce) easing out: decelerating from zero velocity.
        /// </summary>
        /// <param name="time">Current time (in frames or seconds).</param>
        /// <param name="duration">
        /// Expected easing duration (in frames or seconds).
        /// </param>
        /// <param name="unusedOvershootOrAmplitude">Unused: here to keep same delegate for all ease types.</param>
        /// <param name="unusedPeriod">Unused: here to keep same delegate for all ease types.</param>
        /// <returns>The eased value.</returns>
        public static float EaseOut(
          float time,
          float duration,
          float unusedOvershootOrAmplitude,
          float unusedPeriod)
        {
            if ((double)(time /= duration) < 0.363636374473572)
                return 121f / 16f * time * time;
            if ((double)time < 0.727272748947144)
                return (float)(121.0 / 16.0 * (double)(time -= 0.5454546f) * (double)time + 0.75);
            return (double)time < 0.909090936183929 ? (float)(121.0 / 16.0 * (double)(time -= 0.8181818f) * (double)time + 15.0 / 16.0) : (float)(121.0 / 16.0 * (double)(time -= 0.9545454f) * (double)time + 63.0 / 64.0);
        }

        /// <summary>
        /// Easing equation function for a bounce (exponentially decaying parabolic bounce) easing in/out: acceleration until halfway, then deceleration.
        /// </summary>
        /// <param name="time">Current time (in frames or seconds).</param>
        /// <param name="duration">
        /// Expected easing duration (in frames or seconds).
        /// </param>
        /// <param name="unusedOvershootOrAmplitude">Unused: here to keep same delegate for all ease types.</param>
        /// <param name="unusedPeriod">Unused: here to keep same delegate for all ease types.</param>
        /// <returns>The eased value.</returns>
        public static float EaseInOut(
          float time,
          float duration,
          float unusedOvershootOrAmplitude,
          float unusedPeriod)
        {
            return (double)time < (double)duration * 0.5 ? Bounce.EaseIn(time * 2f, duration, -1f, -1f) * 0.5f : (float)((double)Bounce.EaseOut(time * 2f - duration, duration, -1f, -1f) * 0.5 + 0.5);
        }
    }

    public static class Flash
    {
        public static float Ease(float time, float duration, float overshootOrAmplitude, float period)
        {
            int stepIndex = Mathf.CeilToInt(time / duration * overshootOrAmplitude);
            float stepDuration = duration / overshootOrAmplitude;
            time -= stepDuration * (float)(stepIndex - 1);
            float dir = stepIndex % 2 != 0 ? 1f : -1f;
            if ((double)dir < 0.0)
                time -= stepDuration;
            float res = time * dir / stepDuration;
            return Flash.WeightedEase(overshootOrAmplitude, period, stepIndex, stepDuration, dir, res);
        }

        public static float EaseIn(
          float time,
          float duration,
          float overshootOrAmplitude,
          float period)
        {
            int stepIndex = Mathf.CeilToInt(time / duration * overshootOrAmplitude);
            float stepDuration = duration / overshootOrAmplitude;
            time -= stepDuration * (float)(stepIndex - 1);
            float dir = stepIndex % 2 != 0 ? 1f : -1f;
            if ((double)dir < 0.0)
                time -= stepDuration;
            time *= dir;
            float res = (time /= stepDuration) * time;
            return Flash.WeightedEase(overshootOrAmplitude, period, stepIndex, stepDuration, dir, res);
        }

        public static float EaseOut(
          float time,
          float duration,
          float overshootOrAmplitude,
          float period)
        {
            int stepIndex = Mathf.CeilToInt(time / duration * overshootOrAmplitude);
            float stepDuration = duration / overshootOrAmplitude;
            time -= stepDuration * (float)(stepIndex - 1);
            float dir = stepIndex % 2 != 0 ? 1f : -1f;
            if ((double)dir < 0.0)
                time -= stepDuration;
            time *= dir;
            float res = (float)(-(double)(time /= stepDuration) * ((double)time - 2.0));
            return Flash.WeightedEase(overshootOrAmplitude, period, stepIndex, stepDuration, dir, res);
        }

        public static float EaseInOut(
          float time,
          float duration,
          float overshootOrAmplitude,
          float period)
        {
            int stepIndex = Mathf.CeilToInt(time / duration * overshootOrAmplitude);
            float stepDuration = duration / overshootOrAmplitude;
            time -= stepDuration * (float)(stepIndex - 1);
            float dir = stepIndex % 2 != 0 ? 1f : -1f;
            if ((double)dir < 0.0)
                time -= stepDuration;
            time *= dir;
            float res = (double)(time /= stepDuration * 0.5f) < 1.0 ? 0.5f * time * time : (float)(-0.5 * ((double)--time * ((double)time - 2.0) - 1.0));
            return Flash.WeightedEase(overshootOrAmplitude, period, stepIndex, stepDuration, dir, res);
        }

        private static float WeightedEase(
          float overshootOrAmplitude,
          float period,
          int stepIndex,
          float stepDuration,
          float dir,
          float res)
        {
            float num1 = 0.0f;
            float num2 = 0.0f;
            if ((double)period > 0.0)
            {
                float num3 = (float)Math.Truncate((double)overshootOrAmplitude);
                float num4 = overshootOrAmplitude - num3;
                if ((double)num3 % 2.0 > 0.0)
                    num4 = 1f - num4;
                num2 = num4 * (float)stepIndex / overshootOrAmplitude;
                num1 = res * (overshootOrAmplitude - (float)stepIndex) / overshootOrAmplitude;
            }
            else if ((double)period < 0.0)
            {
                period = -period;
                num1 = res * (float)stepIndex / overshootOrAmplitude;
            }
            float num5 = num1 - res;
            res += num5 * period + num2;
            if ((double)res > 1.0)
                res = 1f;
            return res;
        }
    }
}