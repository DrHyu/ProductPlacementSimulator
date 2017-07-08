using UnityEngine;
using System.Collections;

public class MiscFunc : MonoBehaviour
{

    public static bool PointOnTopOfSegment(Vector2 point, Vector2 segmentStart, Vector2 segmentEnd, out Vector2 intersection)
    {
        Vector2 dir = segmentEnd - segmentStart;
        Vector2 perpendicular = new Vector2(-dir.y, dir.x);


        if (!Drag3D.IntersectRay2DvsSegment(point, perpendicular, segmentStart, segmentEnd, out intersection))
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

        BoxCollider b = go.GetComponent<BoxCollider>();

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

}
