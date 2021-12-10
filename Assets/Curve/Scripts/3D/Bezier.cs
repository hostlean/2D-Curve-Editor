using UnityEngine;

namespace Curve.Scripts
{
    public static class Bezier
    {
        public static Vector3 EvaluateQuadratic(Vector3 a, Vector3 b, Vector3 c, float time)
        {
            var p0 = Vector3.Lerp(a, b, time);
            var p1 = Vector3.Lerp(b, c, time);
            return Vector3.Lerp(p0, p1, time);
        }

        public static Vector3 EvaluateCubic(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float time)
        {
            var p0 = EvaluateQuadratic(a, b, c, time);
            var p1 = EvaluateQuadratic(b, c, d, time);
            return Vector3.Lerp(p0, p1, time);
        }
    }
}