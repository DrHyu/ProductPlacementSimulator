
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.EventSystems;

class Drag3D : MonoBehaviour, IPointerUpHandler, IPointerDownHandler
{
    public CollisionMap cm;

    private BoxJSON this_box;

    private Color dragColor     = new Color(1,1,1,0.5f);
    private Color collidedColor = new Color(1, 0, 0, 0.5f);
    private Color collidedUponColor = new Color(0, 0, 1, 0.5f);
    private Color selectedColor = new Color(0, 1, 0, 0.5f);
    private Color originalColor = new Color(1, 0.91f, 0.62f, 0.5f);

    // Used to keep state machine
    public bool selected = false;
    public bool dragging = false;
    public bool collided = false;
    public bool collided_upon = false;

    // Used to keep history of the last valid position and index to recover in case of a failed drag
    private Vector3 last_position;
    private int last_index = 0;

    // The position of the cube is stored as:
    //  1. Index of the vertex in the dragline
    //  2. magnitude(current_3d_pos - dragLine[c_index])/ magnitude(dragLine[c_index+1] - dragLine[c_index])
    // Stored this way to allow the stands to be rotated and/or scaled while maintaining the relative position of this cube


    private float distance;

    public Vector3[] globalDragLines;
    public Vector3[] localDragLines;
    public int[] global_to_public_relation;

    // The face of the cube should always be oriented in this direction
    private Vector2[] normals;

    /* - - - - - OVERRIDE METHODS - - - - - */

    private void Start()
    {
        // Make sure it is at the very start 
        GetComponent<Renderer>().material  = Resources.Load("Materials/StandardTransparent", typeof(Material)) as Material;
        GetComponent<Renderer>().material.color = originalColor;

        last_position = transform.localPosition;
    }

    void Update()
    {

        if(dragging)
        {
            //Calcualte the estimaded mouse position in the 3D space
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Vector3 mousePos3D = ray.GetPoint(distance);

            // Transform to local coordinates
            mousePos3D = transform.parent.InverseTransformPoint(mousePos3D);

            int next_index;
            // Try the new position
            transform.localPosition = getClosestPointInCurrentLine(mousePos3D, this_box.current_index, out next_index);

            this_box.current_index = next_index;

            // This box moved so the other collision maps are no longer valid
            GetComponentInParent<ShelfGenerator>().InvalideChildCollisionMaps(gameObject.GetInstanceID());

            // This could be done when initializing this object on the ShelfGenerator. 
            // However, It is likely that not every cube will be moved during a sesion, therefore it makes sense to calculate the collision map only when a cube is moved
            //if (cm == null)
            //{
                ShelfGenerator sg = GetComponentInParent<ShelfGenerator>();
                CollisionMap.CalculateCollisionMap(localDragLines, sg.productList.ToArray(), sg.cubes.ToArray(), transform.parent, out cm, this_box);
            //}
            //// Further calls will merely update the map with the movement of this cube whithout recalculating the impact of the other boxes in the shelf
            //else
            //{
            //    CollisionMap.UpdateCollisionMap(localDragLines, this_box, gameObject, transform.parent, ref cm);
            //}

            GetComponentInParent<ShelfGenerator>().tempCollisionMap = cm;

            int[] collided_with;
            if (cm.AmICollided(gameObject.GetInstanceID(), out collided_with))
            {
                collided = true;
                updateColor();

                GetComponentInParent<ShelfGenerator>().NotifyCollision(collided_with, gameObject.GetInstanceID());
            }
            else
            {
                // If transitioning from collided to non collided
                if (collided)
                {
                    // Notify that we are not longer in collision and cubes that are "collided upon" can reset to default state
                    GetComponentInParent<ShelfGenerator>().ClearCollision();
                }
                collided = false;
                updateColor();

                // This are used to reover the last valid position if, for example, the user stops dragging a block during an intercception
                last_index = this_box.current_index;
                last_position = transform.localPosition;

                // The position of the cube is stored as:
                //  1. Index of the vertex in the dragline
                //  2. magnitude(current_3d_pos - dragLine[c_index])/ magnitude(dragLine[c_index+1] - dragLine[c_index])
                // Stored this way to allow the stands to be rotated and/or scaled while maintaining the relative position of this cube
                this_box.current_pos_relative = Vector3.Magnitude(transform.localPosition - localDragLines[this_box.current_index]) / Vector3.Magnitude(localDragLines[this_box.current_index + 1] - localDragLines[this_box.current_index]);
            }
            
        }

    }

    public void updateColor()
    {
        if (collided_upon)
        {
            GetComponent<Renderer>().material.color = collidedUponColor;

        }
        else if (collided)
        {
            GetComponent<Renderer>().material.color = collidedColor;
        }
        else if (dragging)
        {
            GetComponent<Renderer>().material.color = dragColor;
        }
        else if (selected)
        {
            GetComponent<Renderer>().material.color = selectedColor;
        }
        else
        {
            GetComponent<Renderer>().material.color = originalColor;
        }
    }

    private void LateUpdate()
    {
        Vector3 n = normals[this_box.current_index].to3DwY(0);
        transform.localRotation = Quaternion.LookRotation(n);
    }

    public void OnValidate()
    {
        
        // Re-calulate the local drag lines based on the new scale of the object
        offsetDraglineByCubeSize();
        CalculateNormals();

        transform.localPosition = localDragLines[this_box.current_index];
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (collided)
        {
            // Notify that we are not longer in collision and cubes that are "collided upon" can reset to default state
            GetComponentInParent<ShelfGenerator>().ClearCollision();
            transform.localPosition = last_position;
            this_box.current_index = last_index;
            collided = false;
        }
        dragging = false;
        updateColor();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        distance = Vector3.Distance(GetComponent<Transform>().position, Camera.main.transform.position);
        dragging = true;
        updateColor();
    }

    private void OnCollisionEnter(Collision c)
    {
        // Only do something if we are the ones causing the colision

        //collided = true;
    }

    private void OnCollisionExit(Collision collision)
    {
        //collided = false;
    }

    private void OnCollisionStay(Collision collision)
    {
        //collided = true;

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

            //Gizmos.color = Color.magenta;
            //Gizmos.DrawLine(globalDragLines[i], globalDragLines[i]+debug[i]);


        }

    }

    /* - - - - - NON-STATIC METHODS - - - - - */

    public void PrepareForDelete()
    {
        if (collided)
        {

        }
    }

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

    public void Initialize(BoxJSON b, Vector3[] _dragLines)
    {
        this_box = b;
        transform.localScale = new Vector3(b.width, b.height, b.depth);

        globalDragLines = _dragLines;
        offsetDraglineByCubeSize();
        CalculateNormals();

        transform.localPosition = localDragLines[this_box.current_index] + (localDragLines[this_box.current_index + 1] - localDragLines[this_box.current_index]) * this_box.current_pos_relative;

    }

    public void setDragline(Vector3[] _dragLines)
    {
        globalDragLines = _dragLines;
        offsetDraglineByCubeSize();
        CalculateNormals();
    }

    private void offsetDraglineByCubeSize()
    {

        /* Diferent size of cubes requires diferent draglines
         * so, take the common dragline for all cubes in the shelf
         * and tweak it slightly to fit the size of this gameobject
        */

        Vector3 scale = transform.localScale/2;

        localDragLines = CalculateDragLines(globalDragLines, scale.z, out global_to_public_relation, true);

        for(int i = 0; i < localDragLines.Length; i++)
        {
            localDragLines[i].y += scale.y;
        }

        // Shorten start and end points //
        Vector3 temp = localDragLines[1] - localDragLines[0];
        temp.Normalize();
        localDragLines[0] = localDragLines[0] + temp * (scale.x);

        temp = localDragLines[localDragLines.Length - 2] - localDragLines[localDragLines.Length - 1];
        temp.Normalize();
        localDragLines[localDragLines.Length - 1] = localDragLines[localDragLines.Length - 1] + temp * (scale.x);

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

    private Vector3 getClosestPointInCurrentLine(Vector3 point, int c_index, out int n_index)
    {
        //Do some trigonometry to find out the closes point in the vector to the mouse position
        Vector3 SE = localDragLines[c_index + 1] - localDragLines[c_index];
        Vector3 CS = point - localDragLines[c_index];

        float alpha = Mathf.Acos(Vector3.Dot(SE, CS) / (SE.magnitude * CS.magnitude));
        float con = CS.magnitude * Mathf.Cos(alpha);

        Ray r = new Ray(localDragLines[c_index], SE);

        // This is the point in the segment where we want the mouse to move
        Vector3 insc = r.GetPoint(con);

        // The code below makes sure that the obect can't move out of the ends of the segment
        // Without it, the obect could move in a (infinite) line. 
        // This bounds the object movement to the ends to the segment.
        // Also, if either ends of the segment is reached, it will move on the respective next segment in the localDragLines array
        Vector3 big_x = (localDragLines[c_index + 1].x > localDragLines[c_index].x) ? localDragLines[c_index + 1] : localDragLines[c_index];
        Vector3 small_x = (localDragLines[c_index + 1].x < localDragLines[c_index].x) ? localDragLines[c_index + 1] : localDragLines[c_index];

        Vector3 big_y = (localDragLines[c_index + 1].y > localDragLines[c_index].y) ? localDragLines[c_index + 1] : localDragLines[c_index];
        Vector3 small_y = (localDragLines[c_index + 1].y < localDragLines[c_index].y) ? localDragLines[c_index + 1] : localDragLines[c_index];

        Vector3 big_z = (localDragLines[c_index + 1].z > localDragLines[c_index].z) ? localDragLines[c_index + 1] : localDragLines[c_index];
        Vector3 small_z = (localDragLines[c_index + 1].z < localDragLines[c_index].z) ? localDragLines[c_index + 1] : localDragLines[c_index];

        int big_x_incr = (localDragLines[c_index + 1].x > localDragLines[c_index].x) ? 1 : -1;
        int small_x_incr = (localDragLines[c_index + 1].x < localDragLines[c_index].x) ? 1 : -1;

        int big_y_incr = (localDragLines[c_index + 1].y > localDragLines[c_index].y) ? 1 : -1;
        int small_y_incr = (localDragLines[c_index + 1].y < localDragLines[c_index].y) ? 1 : -1;

        int big_z_incr = (localDragLines[c_index + 1].z > localDragLines[c_index].z) ? 1 : -1;
        int small_z_incr = (localDragLines[c_index + 1].z < localDragLines[c_index].z) ? 1 : -1;

        if (insc.x > big_x.x)
        {
            insc = big_x;

            c_index += big_x_incr;
        }
        else if (insc.x < small_x.x)
        {
            insc = small_x;

            c_index += small_x_incr;
        }
        if (insc.y > big_y.y)
        {
            insc = big_y;

            c_index += big_y_incr;
        }
        else if (insc.y < small_y.y)
        {
            insc = small_y;

            c_index += small_y_incr;
        }
        else if (insc.z > big_z.z)
        {
            insc = big_z;

            c_index += big_z_incr;
        }
        else if (insc.z < small_z.z)
        {
            insc = small_z;

            c_index += small_z_incr;
        }

        if (c_index < 0)
        {
            c_index = 0;
            insc = localDragLines[0];
        }
        else if (c_index >= localDragLines.Length - 1)
        {
            c_index = localDragLines.Length - 2;
            insc = localDragLines[localDragLines.Length - 1];
        }
        n_index = c_index;
        return insc;
    }

    /* - - - - - STATIC METHODS - - - - - */

    public static Vector2 PerpendicularClockwise(Vector2 vector2)
    {
        return new Vector2(-vector2.y, vector2.x);
    }

    public static Vector2 PerpendicularCounterClockwise(Vector2 vector2)
    {
        return new Vector2(vector2.y, -vector2.x);
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

    public static Vector3[] CalculateDragLines(Vector3[] points, float offset, out int[] vtxRltn, bool doBeizer = false )
    {

        if (points.Length == 2)
        {

            Vector2 o1 = points[0].to2DwoY();
            Vector2 o2 = points[1].to2DwoY();

            Vector2 o1o2 = o2 - o1;

            Vector2 po1o2 = PerpendicularClockwise(o1o2);
            po1o2.Normalize();

            Vector2 n1 = o1 - po1o2 * offset;
            Vector2 n2 = o2 - po1o2 * offset;

            List<Vector3> newDragline = new List<Vector3>();
            List<int> vertexRelation = new List<int>();

            newDragline.Add(n1.to3DwY(points[0].y));
            vertexRelation.Add(0);
            newDragline.Add(n2.to3DwY(points[1].y));
            vertexRelation.Add(1);

            vtxRltn = vertexRelation.ToArray();

            return newDragline.ToArray();
        }
        else
        {


            Vector2 o1 = points[0].to2DwoY();
            Vector2 o2 = points[1].to2DwoY();

            Vector2 o1o2 = o2 - o1;

            Vector2 po1o2 = PerpendicularClockwise(o1o2);
            po1o2.Normalize();

            Vector2 n1 = o1 - po1o2 * offset;
            Vector2 n2 = o2 - po1o2 * offset;

            List<Vector3> newDragline = new List<Vector3>();
            List<int> vertexRelation = new List<int>();

            newDragline.Add(n1.to3DwY(points[0].y));
            vertexRelation.Add(0);

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

                    if (doBeizer)
                    {
                        Vector2[] res = doBezier(new Vector2[] { n5, isc, n6 }, 2, 40);

                        for (int p = 0; p < res.Length; p++)
                        {
                            newDragline.Add(res[p].to3DwY(points[i].y));
                            vertexRelation.Add(i);
                        }

                        n1 = res[res.Length - 1];    //This one is correct for sure
                        n2 = n4;    //This one might get fixed next iteration
                    }
                    else
                    {
                        newDragline.Add(isc.to3DwY(points[i].y));
                        vertexRelation.Add(i);
                        n1 = isc;    //This one is correct for sure
                        n2 = n4;    //This one might get fixed next iteration
                    }
                }
                else
                {
                    Vector2 isc;
                    if (!IntersectRay2D(n1, n2 - n1, n4, n3 - n4, out isc))
                    {
                        Debug.LogError("Did not intersect");
                    }

                    if (doBeizer)
                    {

                        Vector2[] res = doBezier(new Vector2[] { n2, isc, n3 }, 2, 20);

                        for (int p = 0; p < res.Length; p++)
                        {
                            newDragline.Add(res[p].to3DwY(points[i].y));
                            vertexRelation.Add(i);
                        }

                        n1 = res[res.Length - 1]; //This one is correct for sure
                        n2 = n4;    //This one might get fixed next iteration
                    }
                    else
                    {
                        newDragline.Add(isc.to3DwY(points[i].y));
                        vertexRelation.Add(i);

                        n1 = isc;    //This one is correct for sure
                        n2 = n4;    //This one might get fixed next iteration
                    }
                }


                if (i == points.Length - 2)
                {
                    newDragline.Add(n4.to3DwY(points[i + 1].y));
                    vertexRelation.Add(i + 1);
                }
            }

            vtxRltn = vertexRelation.ToArray();

            return newDragline.ToArray();
        }
    }

    public static bool IntersectRay2D(Vector2 p, Vector2 r, Vector2 q, Vector2 s, out Vector2 isc)
    {
        float rxs = r.Cross(s);
        float qminpxr = (q - p).Cross(r);

        // They are coolinear
        if(rxs == 0 && qminpxr == 0)
        {
            isc = p+r;
            return true;
        }
        // They are parallel
        else if(rxs == 0 && qminpxr != 0)
        {
            isc = new Vector2(0,0);
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
            if(((a.x <= isc.x && b.x >= isc.x) || (a.x >= isc.x && b.x <= isc.x)) && ((a.y <= isc.y && b.y >= isc.y) || (a.y >= isc.y && b.y <= isc.y)))
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



    /* - - - - - STATIC METHODS - - - - - */
}
