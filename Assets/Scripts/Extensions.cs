using System;
using UnityEngine;

public static class Extensions
{
    private const double Epsilon = 1e-10;

    public static bool IsZero(this double d)
    {
        return Math.Abs(d) < Epsilon;
    }

    public static bool IsZero(this float d)
    {
        return Math.Abs(d) < Epsilon;
    }

    public static Vector2 WithX(this Vector2 v, float x)
    {
        return new Vector2(x, v.y);
    }

    public static float Cross(this Vector2 v1, Vector2 v2)
    {
        return (v1.x * v2.y) - (v1.y * v2.x);
    }

    public static Vector3 to3DwY(this Vector2 v2, float y)
    {
        return new Vector3(v2.x, y, v2.y);
    }

    public static Vector2 to2DwoY(this Vector3 v3)
    {
        return new Vector2(v3.x,v3.z);
    }

    public static Vector2 toV2(this Vector v)
    {
        return new Vector2((float)v.X,(float)v.Y);
    }
    public static Vector toV(this Vector2 v)
    {
        return new Vector(v.x, v.y);
    }


}