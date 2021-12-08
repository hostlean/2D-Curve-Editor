using UnityEngine;

public static class CurveCalculator2D
{
    public static Vector2 Lerp2D(Vector2 origin, Vector2 to, float time)
    {
        return origin + (to - origin) * time;
    }

    public static Vector2 QuadraticCurve2D(Vector2 point1, Vector2 point2, Vector2 point3, float time)
    {
        var p0 = Lerp2D(point1, point2, time);
        var p1 = Lerp2D(point2, point3, time);

        return Lerp2D(p0, p1, time);
    }

    public static Vector2 CubicCurve2D(Vector2 point1, Vector2 point2, Vector2 point3, Vector2 point4, float time)
    {
        var p0 = QuadraticCurve2D(point1, point2, point3, time);
        var p1 = QuadraticCurve2D(point2, point3, point4, time);

        return Lerp2D(p0, p1, time);
    }

    public static Vector2 QuadraticPolynomial2D(Vector2 point1, Vector2 point2, Vector2 point3, float time)
    {
        return Mathf.Pow(1 - time, 2) * point1 + 
               2 * (1 - time) * time * point2 + 
               Mathf.Pow(time, 2) * point3;
    }

    public static Vector2 CubicPolynomial(Vector2 point1, Vector2 point2, Vector2 point3, Vector2 point4, float time)
    {
        return Mathf.Pow(1 - time, 3) * point1 + 
               3 * Mathf.Pow(1 - time, 2) * time * point2 +
               3 * (1 - time) * Mathf.Pow(time, 2) * point3 + 
               Mathf.Pow(time, 3) * point4;
    }
}
