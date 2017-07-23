using UnityEngine;
using System.Collections.Generic;

public class MiscFunc
{

    public static bool PointOnTopOfSegment(Vector2 point, Vector2 segmentStart, Vector2 segmentEnd, out Vector2 intersection)
    {
        Vector2 dir = segmentEnd - segmentStart;
        Vector2 perpendicular = new Vector2(-dir.y, dir.x);


        if (!IntersectRay2DvsSegment(point, perpendicular, segmentStart, segmentEnd, out intersection))
        {
            return false;
        }
        // Check if the intersection point is within the segment start and end
        // Check this to avoid a division by 0

        Vector2 temp = (intersection - segmentStart);

        if ((dir.x != 0 && temp.x / dir.x <= 1)
         || (dir.y != 0 && temp.y / dir.y <= 1))
        //if (Math.Abs((intersection - segmentStart).x / dir.x) < 1)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public static Vector3[] GetColliderVertexPositions(GameObject obj)
    {
        Vector3[] vertices = new Vector3[8];

        Vector3 temp_scale = obj.transform.localScale;
        obj.transform.localScale = Vector3.one;
        Matrix4x4 thisMatrix = obj.transform.localToWorldMatrix;
        obj.transform.localScale = temp_scale;

        Quaternion storedRotation = obj.transform.rotation;
        obj.transform.rotation = Quaternion.identity;

        Vector3 extents = obj.GetComponent<BoxCollider>().bounds.extents;
        vertices[0] = thisMatrix.MultiplyPoint3x4(extents);
        vertices[1] = thisMatrix.MultiplyPoint3x4(new Vector3(-extents.x, extents.y, extents.z));
        vertices[2] = thisMatrix.MultiplyPoint3x4(new Vector3(extents.x, extents.y, -extents.z));
        vertices[3] = thisMatrix.MultiplyPoint3x4(new Vector3(-extents.x, extents.y, -extents.z));
        vertices[4] = thisMatrix.MultiplyPoint3x4(new Vector3(extents.x, -extents.y, extents.z));
        vertices[5] = thisMatrix.MultiplyPoint3x4(new Vector3(-extents.x, -extents.y, extents.z));
        vertices[6] = thisMatrix.MultiplyPoint3x4(new Vector3(extents.x, -extents.y, -extents.z));
        vertices[7] = thisMatrix.MultiplyPoint3x4(-extents);

        obj.transform.rotation = storedRotation;
        return vertices;
    }

    public static bool PointInObject(Vector3 p, GameObject go)
    {

        p = go.transform.parent.TransformPoint(p);

        Vector3[] v = GetColliderVertexPositions(go);

        Vector3 max = new Vector3(-9999f,-9999f,- 9999f);
        Vector3 min = new Vector3(9999f, 9999f, 9999f);

        foreach(Vector3 i in v)
        {
            if (i.x < min.x) { min.x = i.x; }
            if (i.y < min.y) { min.y = i.y; }
            if (i.z < min.z) { min.z = i.z; }

            if (i.x > max.x) { max.x = i.x; }
            if (i.y > max.y) { max.y = i.y; }
            if (i.z > max.z) { max.z = i.z; }
        }

        bool ret = (p.x <= max.x && p.y <= max.y && p.z <= max.z && p.x >= min.x && p.y >= min.y && p.z >= min.z);

        return ret;

    }

    public static Vector3 RotatePointAroundPivot(Vector3 point , Vector3 pivot ,Vector3 angles )
    {
       Vector3 dir = point - pivot; // get point direction relative to pivot
       dir = Quaternion.Euler(angles) * dir; // rotate it
       point = dir + pivot; // calculate rotated point
       return point; // return it
    }


    public static bool Intersects(Vector p, Vector p2, Vector q, Vector q2, out Vector intersection, bool considerCollinearOverlapAsIntersect = false)
    {
        intersection = new Vector();

        var r = p2 - p;
        var s = q2 - q;
        var rxs = r.Cross(s);
        var qpxr = (q - p).Cross(r);

        // If r x s = 0 and (q - p) x r = 0, then the two lines are collinear.
        if (rxs.IsZero() && qpxr.IsZero())
        {
            // 1. If either  0 <= (q - p) * r <= r * r or 0 <= (p - q) * s <= * s
            // then the two lines are overlapping,
            if (considerCollinearOverlapAsIntersect)
                if ((0 <= (q - p) * r && (q - p) * r <= r * r) || (0 <= (p - q) * s && (p - q) * s <= s * s))
                    return true;

            // 2. If neither 0 <= (q - p) * r = r * r nor 0 <= (p - q) * s <= s * s
            // then the two lines are collinear but disjoint.
            // No need to implement this expression, as it follows from the expression above.
            return false;
        }

        // 3. If r x s = 0 and (q - p) x r != 0, then the two lines are parallel and non-intersecting.
        if (rxs.IsZero() && !qpxr.IsZero())
            return false;

        // t = (q - p) x s / (r x s)
        var t = (q - p).Cross(s) / rxs;

        // u = (q - p) x r / (r x s)

        var u = (q - p).Cross(r) / rxs;

        // 4. If r x s != 0 and 0 <= t <= 1 and 0 <= u <= 1
        // the two line segments meet at the point p + t r = q + u s.
        if (!rxs.IsZero() && (0 <= t && t <= 1) && (0 <= u && u <= 1))
        {
            // We can calculate the intersection point using either t or u.
            intersection = p + t * r;

            // An intersection was found.
            return true;
        }

        // 5. Otherwise, the two line segments are not parallel but do not intersect.
        return false;
    }

    public static Vector2[] DoBezier(Vector2[] v, int _order, int _resolution)
    {

        List<Vector2> result = new List<Vector2>();

        Ray2D[] r1 = new Ray2D[_order];
        Ray2D[] r2 = new Ray2D[_order];

        Vector2[] v1 = new Vector2[_order + 1];
        Vector2[] v2 = new Vector2[_order + 1];

        for (int i = 0; i < v.Length - _order; i += 2)
        {
            for (int x = 0; x < _resolution; x++)
            {
                // Calculate for the initial "order" iteration
                for (int o = 0; o < _order; o++)
                {
                    Vector2 deb = v[o + i + 1] - v[o + i];
                    r1[o] = new Ray2D(v[o + i], deb);
                }
                for (int o = 0; o < _order + 1; o++)
                {
                    v1[o] = v[i + o];
                }

                for (int order = _order; order > 0; order--)
                {
                    for (int p = 0; p < order; p++)
                    {
                        v2[p] = r1[p].GetPoint((v1[p + 1] - v1[p]).magnitude * (float)((float)x / _resolution));
                    }
                    for (int p = 0; p < order - 1; p++)
                    {
                        r2[p] = new Ray2D(v2[p], v2[p + 1] - v2[p]);
                    }

                    //Clean up for next iteration
                    for (int p = 0; p < order; p++) { v1[p] = v2[p]; }
                    for (int p = 0; p < order - 1; p++) { r1[p] = r2[p]; }
                }

                result.Add(v1[0]);
            }
        }

        return result.ToArray();
    }


    public static bool IntersectRay2D(Vector2 p, Vector2 r, Vector2 q, Vector2 s, out Vector2 isc)
    {
        float rxs = r.Cross(s);
        float qminpxr = (q - p).Cross(r);

        // They are coolinear
        if (rxs == 0 && qminpxr == 0)
        {
            isc = p + r;
            return true;
        }
        // They are parallel
        else if (rxs == 0 && qminpxr != 0)
        {
            isc = new Vector2(0, 0);
            return false;
        }
        else
        {
            float u = qminpxr / (rxs);

            isc = q + u * s;
            return true;
        }
    }

    public static bool IntersectRay2DvsSegment(Vector2 p, Vector2 r, Vector2 a, Vector2 b, out Vector2 isc)
    {
        Vector2 dir = b - a;

        if (IntersectRay2D(p, r, a, dir, out isc))
        {
            // if (a <= isc <= b || a >= isc >= b)
            if (((a.x <= isc.x && b.x >= isc.x) || (a.x >= isc.x && b.x <= isc.x)) && ((a.y <= isc.y && b.y >= isc.y) || (a.y >= isc.y && b.y <= isc.y)))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            return false;
        }

    }

    public static bool PointIsInSegment(Vector2 a, Vector2 b, Vector2 c)
    {

        float crossproduct = (c.y - a.y) * (b.x - a.x) - (c.x - a.x) * (b.y - a.y);
        if (Mathf.Abs(crossproduct) > Mathf.Epsilon)
            return false;

        float dotproduct = (c.x - a.x) * (b.x - a.x) + (c.y - a.y) * (b.y - a.y);
        if (dotproduct < 0)
            return false;

        float squaredlengthba = (b.x - a.x) * (b.x - a.x) + (b.y - a.y) * (b.y - a.y);
        if (dotproduct > squaredlengthba)
            return false;

        return true;
    }



}
