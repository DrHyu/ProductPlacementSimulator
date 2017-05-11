
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System;

class Drag3D : MonoBehaviour
{
    private Color mouseOverColor = Color.blue;
    private Color originalColor = Color.yellow;
    private bool dragging = false;

    private float distance;

    private Vector3 startPos;

    public Vector3[] globalDragLines;
    public  Vector3[] localDragLines;

    // The face of the cube should always be oriented in this direction
    private Vector2[] normals;

    public  Vector3[] debug;

    public int cPointStart = 0;

    /* - - - - - OVERRIDE METHODS - - - - - */

    private void Start()
    {
        startPos = GetComponent<Transform>().localPosition;

        // Make sure it is at the very start 
        transform.localPosition = localDragLines[0];
    }

    void Update()
    {
        if (dragging)
        {
            //Calcualte the estimaded mouse position in the 3D space
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Vector3 mousePos3D = ray.GetPoint(distance);

            // Transform to local coordinates
            mousePos3D = transform.parent.InverseTransformPoint(mousePos3D);
            //We don't care about the Y pos, since we will always be moving on a plane
            //mousePos3D.y = startPos.y;

            GetComponent<Transform>().localPosition = getClosestPointInCurrentLine(mousePos3D);
        }
    }

    private void LateUpdate()
    {
        Vector3 n = normals[cPointStart].to3DwY(0);

        transform.localRotation = Quaternion.LookRotation(n);
    }

    public void OnValidate()
    {
        
        // Re-calulate the local drag lines based on the new scale of th object
        offsetDraglineByCubeSize();
        CalculateNormals();

        transform.localPosition = localDragLines[cPointStart];
    }

    void OnMouseEnter()
    {
        GetComponent<Renderer>().material.color = mouseOverColor;
    }

    void OnMouseExit()
    {
        GetComponent<Renderer>().material.color = originalColor;
    }

    void OnMouseDown()
    {
        distance = Vector3.Distance(GetComponent<Transform>().position, Camera.main.transform.position);
        dragging = true;
    }

    void OnMouseUp()
    {
        dragging = false;
    }

    private void OnCollisionEnter(Collision c)
    {
        // Only do something if we are the ones causing the colision
        if (dragging)
        {
            GameObject other_cube = c.gameObject;
            
            Vector3 other_cube_pos = other_cube.transform.localPosition;

        }
        
    }

    private void OnGUI()
    {
        //Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        //Vector3 mousePos3D = ray.GetPoint(distance);

        //GUILayout.BeginArea(new Rect(20, 20, 250, 120));
        //GUILayout.Label("mouse position: " + Input.mousePosition);
        //GUILayout.Label("world position: " + mousePos3D.ToString("f3"));
        //GUILayout.Label("local position: " + transform.parent.InverseTransformPoint(mousePos3D).ToString("f3")); ;
        //GUILayout.EndArea();
    }

    public void OnDrawGizmos()
    {
        Gizmos.matrix = transform.parent.localToWorldMatrix;


        Gizmos.color = Color.green;
        for (int i = 0; i < localDragLines.Length - 1; i++)
        {
            Gizmos.DrawLine(localDragLines[i], localDragLines[i + 1]);
        }

        Gizmos.color = Color.red;
        for (int i = 0; i < globalDragLines.Length - 1; i++)
        {
            Gizmos.DrawLine(globalDragLines[i], globalDragLines[i + 1]);
        }

        for (int i = 0; i < globalDragLines.Length - 2; i++)
        {

            Vector3 norm = Vector3.Cross(globalDragLines[i + 1] - globalDragLines[i], Vector3.up);
            norm.Normalize();

            Gizmos.color = Color.blue;
            Gizmos.DrawLine(globalDragLines[i], globalDragLines[i]+ norm);
            Gizmos.color = Color.black;
            Gizmos.DrawLine(globalDragLines[i+1], globalDragLines[i+1] + norm);

            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(globalDragLines[i], globalDragLines[i]+debug[i]);


        }


    }

    /* - - - - - NON-STATIC METHODS - - - - - */

    private void findNextEmptySpace(GameObject other_cube)
    {
        //Figure out what direction we are going
        Vector3 dir = other_cube.transform.position - transform.position;

        Ray MyRay = new Ray(transform.position,dir);
        RaycastHit MyRayHit;
        Physics.Raycast(MyRay, out MyRayHit);
        Vector3 MyNormal = MyRayHit.normal;
        MyNormal = MyRayHit.transform.TransformDirection(MyNormal);

        // Hit right
        if (MyNormal == MyRayHit.transform.right)
        {
            
        }
        // Hit left
        else if (MyNormal == -MyRayHit.transform.right)
        {
            
        }
        else
        {
            Debug.LogError("Din't hit either right or left", this);
        }


        
    }

    public void setDragline(Vector3[] _dragLines)
    {
        globalDragLines = _dragLines;
        offsetDraglineByCubeSize();
    }

    private void offsetDraglineByCubeSize()
    {

        /* Diferent sizae of cubes requires diferent draglines
         * so, take the common dragline for all cubes in the shelf
         * and tweak it slightly to fit the size of this gameobject
        */

        Vector3 scale = transform.localScale/2;


        //Vector3[] n = new Vector3[globalDragLines.Length];
        //for (int i = 1; i < globalDragLines.Length - 2; i++)
        //{
        //    n[i] = Vector3.Cross(globalDragLines[i + 1] - globalDragLines[i], Vector3.up);
        //    n[i].Normalize();
        //}
        //n[globalDragLines.Length - 1] = n[globalDragLines.Length - 2];

        localDragLines = new Vector3[globalDragLines.Length];
        debug = new Vector3[globalDragLines.Length];
        for(int i = 1; i < localDragLines.Length -1; i++)
        {
            Vector3 ab = globalDragLines[i] - globalDragLines[i-1];
            Vector3 bc = globalDragLines[i] - globalDragLines[i+1];

            // Angle between vectors: ang = acos(a.b)
            float ang = Mathf.Acos(Vector3.Dot(ab, bc)/(ab.magnitude + bc.magnitude)) * 180.0f/(float)Math.PI;
            Vector3 axis = Vector3.Cross(ab, bc);
            Quaternion q = Quaternion.AngleAxis(ang/2, axis.normalized);

            debug[i] = q*new Vector3(1,0,1);


            Vector3 scle = axis.normalized;
            //localDragLines[i] = globalDragLines[i] - scle;
        }


        //// Shorten start and end points //
        //Vector3 temp = localDragLines[1] - localDragLines[0];
        //temp.Normalize();
        //localDragLines[0] = localDragLines[0] + temp * ( scale.x / 2);

        //temp = localDragLines[localDragLines.Length - 2] - localDragLines[localDragLines.Length - 1];
        //temp.Normalize();
        //localDragLines[localDragLines.Length - 1] = localDragLines[localDragLines.Length - 1] + temp * (scale.x / 2);

    }

    private void CalculateNormals()
    {
        normals = new Vector2[localDragLines.Length];
        for (int i = 0; i < localDragLines.Length - 1; i++)
        {
            Vector2 perpendicular = PerpendicularClockwise(localDragLines[i + 1].to2DwoY() - localDragLines[i].to2DwoY());
            perpendicular.Normalize();

            normals[i] = perpendicular;
        }
    }

    private Vector3 getClosestPointInCurrentLine(Vector3 point)
    {
        //Do some trigonometry to find out the closes point in the vector to the mouse position
        Vector3 SE = localDragLines[cPointStart + 1] - localDragLines[cPointStart];
        Vector3 CS = point - localDragLines[cPointStart];

        float alpha = Mathf.Acos(Vector3.Dot(SE, CS) / (SE.magnitude * CS.magnitude));
        float con = CS.magnitude * Mathf.Cos(alpha);

        Ray r = new Ray(localDragLines[cPointStart], SE);

        // This is the point in the segment where we want the mouse to move
        Vector3 insc = r.GetPoint(con);

        // The code below makes sure that the obect can't move out of the ends of the segment
        // Without it, the obect could move in a (infinite) line. 
        // This bounds the object movement to the ends to the segment.
        // Also, if either ends of the segment is reached, it will move on the respective next segment in the localDragLines array
        Vector3 big_x = (localDragLines[cPointStart + 1].x > localDragLines[cPointStart].x) ? localDragLines[cPointStart + 1] : localDragLines[cPointStart];
        Vector3 small_x = (localDragLines[cPointStart + 1].x < localDragLines[cPointStart].x) ? localDragLines[cPointStart + 1] : localDragLines[cPointStart];

        Vector3 big_y = (localDragLines[cPointStart + 1].y > localDragLines[cPointStart].y) ? localDragLines[cPointStart + 1] : localDragLines[cPointStart];
        Vector3 small_y = (localDragLines[cPointStart + 1].y < localDragLines[cPointStart].y) ? localDragLines[cPointStart + 1] : localDragLines[cPointStart];

        Vector3 big_z = (localDragLines[cPointStart + 1].z > localDragLines[cPointStart].z) ? localDragLines[cPointStart + 1] : localDragLines[cPointStart];
        Vector3 small_z = (localDragLines[cPointStart + 1].z < localDragLines[cPointStart].z) ? localDragLines[cPointStart + 1] : localDragLines[cPointStart];

        int big_x_incr = (localDragLines[cPointStart + 1].x > localDragLines[cPointStart].x) ? 1 : -1;
        int small_x_incr = (localDragLines[cPointStart + 1].x < localDragLines[cPointStart].x) ? 1 : -1;

        int big_y_incr = (localDragLines[cPointStart + 1].y > localDragLines[cPointStart].y) ? 1 : -1;
        int small_y_incr = (localDragLines[cPointStart + 1].y < localDragLines[cPointStart].y) ? 1 : -1;

        int big_z_incr = (localDragLines[cPointStart + 1].z > localDragLines[cPointStart].z) ? 1 : -1;
        int small_z_incr = (localDragLines[cPointStart + 1].z < localDragLines[cPointStart].z) ? 1 : -1;

        if (insc.x > big_x.x)
        {
            insc = big_x;

            cPointStart += big_x_incr;
        }
        else if (insc.x < small_x.x)
        {
            insc = small_x;

            cPointStart += small_x_incr;
        }
        if (insc.y > big_y.y)
        {
            insc = big_y;

            cPointStart += big_y_incr;
        }
        else if (insc.y < small_y.y)
        {
            insc = small_y;

            cPointStart += small_y_incr;
        }
        else if (insc.z > big_z.z)
        {
            insc = big_z;

            cPointStart += big_z_incr;
        }
        else if (insc.z < small_z.z)
        {
            insc = small_z;

            cPointStart += small_z_incr;
        }

        if (cPointStart < 0)
        {
            cPointStart = 0;
            insc = localDragLines[0];
        }
        else if (cPointStart >= localDragLines.Length - 1)
        {
            cPointStart = localDragLines.Length - 2;
            insc = localDragLines[localDragLines.Length - 1];
        }

        return insc;
    }

    /* - - - - - STATIC METHODS - - - - - */

    private static Vector2 PerpendicularClockwise(Vector2 vector2)
    {
        return new Vector2(-vector2.y, vector2.x);
    }

    private static Vector2 PerpendicularCounterClockwise(Vector2 vector2)
    {
        return new Vector2(vector2.y, -vector2.x);
    }

    public static bool Intersects(Vector p, Vector p2, Vector q, Vector q2,
    out Vector intersection, bool considerCollinearOverlapAsIntersect = false)
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

    public static Vector2[] doBezier(Vector2[] v, int _order, int _resolution)
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
                    Vector2 deb = v[o + i + 1] - v[o+i];
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
                        v2[p] = r1[p].GetPoint((v1[p + 1] - v1[p]).magnitude * (float) ((float)x / _resolution));
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

    public static Vector3[] CalculateDragLines(Vector3[] points, float offset)
    {
        
        Vector2 o1 = points[0].to2DwoY();
        Vector2 o2 = points[1].to2DwoY();

        Vector2 o1o2 = o2 - o1;

        Vector2 po1o2 = PerpendicularClockwise(o1o2);
        po1o2.Normalize();

        Vector2 n1 = o1 - po1o2 * offset;
        Vector2 n2 = o2 - po1o2 * offset;

        List<Vector3> newDragline = new List<Vector3>();
        newDragline.Add(n1.to3DwY(points[0].y));

        for (int i = 1; i < points.Length - 1; i++)
        {

            o2 = points[i].to2DwoY();
            Vector2 o3 = points[i + 1].to2DwoY();

            Vector2 o2o3 = o3 - o2;

            Vector2 po2o3 = PerpendicularClockwise(o2o3);

            po2o3.Normalize();

            Vector2 n3 = o2 - po2o3 * offset;
            Vector2 n4 = o3 - po2o3 * offset;

            Vector output;
            if (Intersects(n1.toV(), n2.toV(), n3.toV(), n4.toV(), out output))
            {
                Vector2 isc = output.toV2();
                Vector2 n5 = new Ray2D(n1, isc - n1).GetPoint((isc - n1).magnitude - (n2 - isc).magnitude);
                Vector2 n6 = new Ray2D(n4, isc - n4).GetPoint((isc - n4).magnitude - (n3 - isc).magnitude);

                Vector2[] res = doBezier(new Vector2[] { n5, isc, n6 }, 2, 5);

                for (int p = 0; p < res.Length; p++)
                {
                    newDragline.Add(res[p].to3DwY(points[i].y));
                }

                n1 = res[res.Length - 1];    //This one is correct for sure
                n2 = n4;    //This one might get fixed next iteration
            }
            else
            {
                newDragline.Add(n2.to3DwY(points[i].y));
                newDragline.Add(n3.to3DwY(points[i + 1].y));

                n1 = n3;    //This one is correct for sure
                n2 = n4;    //This one might get fixed next iteration
            }

            if (i == points.Length - 2)
            {
                newDragline.Add(n4.to3DwY(points[i + 1].y));
            }
        }

        return newDragline.ToArray();
    }
    
    /* - - - - - STATIC METHODS - - - - - */
}
