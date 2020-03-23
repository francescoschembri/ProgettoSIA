using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Bezier
{
    /*
    public static Vector3 EvaluateQuadratic(Vector3 a, Vector3 b, Vector3 c, float t) 
    {
        Vector3 p0 = Vector3.Lerp(a, b, t);
        Vector3 p1 = Vector3.Lerp(b, c, t);
        return Vector3.Lerp(p0, p1, t);
    }
    */
    public static Vector3 EvaluateCubic(Vector3[] p, float t)
    {
        float deltaT = 1f - t;
        return p[0] * deltaT * deltaT * deltaT
             + p[1] * 3 * deltaT * deltaT * t
             + p[2] * 3 * deltaT * t * t
             + p[3] * t * t * t;
    }

    public static float ApproximateLengthCubic(Vector3[] points) {
        float controlPolyLength = Vector3.Distance(points[0], points[1]) 
                                + Vector3.Distance(points[1], points[2]) 
                                + Vector3.Distance(points[2], points[3]);
        return Vector3.Distance(points[0], points[3]) + controlPolyLength / 2f;
    }

}
