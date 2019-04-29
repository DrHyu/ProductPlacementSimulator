using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.EventSystems;

public class Drag3D : MonoBehaviour,  IPointerDownHandler, IPointerUpHandler
{

    private bool initialized = false;
    public CollisionMap2 cm;

    public BoxJSON box;

    public ProductAesthetics PA;
    public FloatingProducts floatingObj;
    public ShelfGenerator SG;

    public bool move_right = true;

    // Used to keep state machine
    public bool selected = false;
    public bool dragging = false;
    public bool collided = false;
    public bool collided_upon = false;
    public bool deattached = false;

    // Used to keep history of the last valid position and index to recover in case of a failed drag
    private Vector3 last_position;
    private int last_index = 0;
    private float last_pos_rel = 0;

    // The position of the cube is stored as:
    //  1. Index of the vertex in the dragline
    //  2. magnitude(current_3d_pos - dragLine[c_index])/ magnitude(dragLine[c_index+1] - dragLine[c_index])
    // Stored this way to allow the stands to be rotated and/or scaled while maintaining the relative position of this cube

    private float distance;
    private float startDragTime;

    // Drag speed/s
    public float DRAG_SPEED = 5f;

    public Vector3[] dLines;


    /* - - - - - OVERRIDE METHODS - - - - - */

    private void Update()
    {
        if((dragging || (Input.GetKey(KeyCode.M) && selected)) && ! deattached)
        {

            CalculateNextPosition(ref box);
            transform.localPosition = CalculateCenterPosition(box);

            /* Update the collision AFTER the position has been updated */
            SG.sharedCollisionMap.UpdateCollisionMap(this);

            int[] collided_with;
            if (SG.sharedCollisionMap.AmICollided(gameObject.GetInstanceID(), out collided_with))
            {
                if (!collided) { ExecOnMyCollisionEnterCallbacks(); }
                collided = true;
                SG.NotifyCollision(collided_with, gameObject.GetInstanceID());
            }
            else
            {
                // If transitioning from collided to non collided
                if (collided)
                {
                    // Notify that we are not longer in collision and cubes that are "collided upon" can reset to default state
                    SG.ClearCollision();
                    ExecOnMyCollisionExitCallbacks();
                }
                collided = false;

                // This are used to recover the last valid position if, for example, the user stops dragging a block during an intercception
                last_index = box.cir;
                last_pos_rel = box.cpr;
                last_position = transform.localPosition;
            }
        }
    }

    private void Start()
    {   
        initialized = true;
        transform.localPosition = CalculateCenterPosition(box);
        last_position = transform.localPosition;

        floatingObj = GameObject.Find("FloatingProducts").GetComponent<FloatingProducts>();
        if (floatingObj == null)
            Debug.LogError("Couldnt't find floating products object !");
    }

    private void LateUpdate()
    {

        Vector3 right_p = dLines[box.cir] + (dLines[box.cir + 1] - dLines[box.cir]) * box.cpr;
        Vector3 left_p = dLines[box.cil] + (dLines[box.cil + 1] - dLines[box.cil]) * box.cpl;

        Vector3 n = (left_p - right_p).to2DwoY().PerpClockWise().to3DwY(0);

        if(n != Vector3.zero)
            transform.localRotation = Quaternion.LookRotation(n);

        // Start D-attachment
        if (selected && Input.GetKeyDown(KeyCode.D) && !deattached)
        {
            Deattach();
        }
        // Cancel deattatchment
        else if (deattached && !selected || deattached && Input.GetKeyDown(KeyCode.D))
        {
            ReAttach();
        }
        // Move while deattached
        else if (deattached && dragging && selected)
        {
            // When deattatched move wherever the mouse is
            transform.localPosition = CalculateMousePosition();
        }
    }

    public void OnValidate()
    {
        if(initialized)
        {
            // Re-calulate the local drag lines based on the new scale of the object
            transform.localPosition = CalculateCenterPosition(box);
        }
    }

    /* private void OnMouseUp() */
    public void OnPointerUp(PointerEventData pointerEventData)
    {
        if (collided)
        {
            // Notify that we are not longer in collision and cubes that are "collided upon" can reset to default state
            SG.ClearCollision();
            ReturnToLastValidPosition();
            collided = false;
            ExecOnMyCollisionExitCallbacks();
        }
        dragging = false;
        ExecOnDragEndCallbaks();
    }

    /* private void OnMouseDown() */
    public void OnPointerDown(PointerEventData pointerEventData)
    {
        distance = Vector3.Distance(GetComponent<Transform>().position, Camera.main.transform.position);
        dragging = true;
        ExecOnDragStartCallbaks();

        // TODO: Implications ?
        //if (!selected)
        {
            if(SG != null)
            {
                SG.ChildWasClicked(this);
            }
        }

        startDragTime = Time.time;
    }

    public void OnCollisionEnter(Collision collision)
    {
        if (deattached && selected)
        {
            ShelfGenerator sg =  collision.collider.gameObject.GetComponent<ShelfGenerator>();
            if(sg != null || sg != floatingObj.floatingProdOrigShelf)
            {
                ReAttach(sg);
            }
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        if (deattached && selected)
        {
            if (SG != null && SG != floatingObj.floatingProdOrigShelf)
            {
                ReAttach(SG);
            }
        }
    }

    public void SetSelected(bool sel)
    {
        selected = sel;
        PA.SetSelected(sel);
    }

    public void Deattach()
    {
        if(!deattached)
        {
            transform.parent.GetComponent<ShelfGenerator>().DeattachProduct(this, true);
            floatingObj.AddFloatingProduct(this, transform.parent.GetComponent<ShelfGenerator>(), transform.parent.parent.GetComponent<StandGenerator>());
            deattached = true;
        }
    }

    public void ReAttach(ShelfGenerator sg = null)
    {
        FloatingProducts fp = transform.GetComponentInParent<FloatingProducts>();
        if(fp != null)
        {
            this.deattached = false;
            fp.ReAttach(sg);
        }
        else
        {
            Debug.LogError("Attempting to re-attach without beeing in floating products");
        }
    }

    public void OnDrawGizmos()
    {
        Gizmos.matrix = transform.parent.localToWorldMatrix;


        Gizmos.color = Color.green;
        for (int i = 0; i < dLines.Length - 1; i++)
        {
            Gizmos.DrawLine(dLines[i], dLines[i + 1]);
        }

        Vector3 right_p = dLines[box.cir] + (dLines[box.cir + 1] - dLines[box.cir]) * box.cpr;
        Vector3 left_p = dLines[box.cil] + (dLines[box.cil + 1] - dLines[box.cil]) * box.cpl;

        Gizmos.color = move_right ? Color.red : Color.green;
        Gizmos.DrawLine(right_p, new Vector3(right_p.x, right_p.y + 100, right_p.z));

        Gizmos.color = move_right ? Color.green : Color.red;
        Gizmos.DrawLine(left_p, new Vector3(left_p.x, left_p.y + 100, left_p.z));


    }

    /* - - - - - NON-STATIC METHODS - - - - - */

    public void Initialize(BoxJSON b, Vector3[] _dragLines)
    {
        box = b;
        dLines = _dragLines;

        onMyCollisionEnterCallbacks = new List<OnMyCollisionEnterCallback>();
        onMyCollisionExitCallbacks = new List<OnMyCollisionExitCallback>();
        onDragStartCallBacks = new List<OnDragStartCallback>();
        onDragEndCallBacks = new List<OnDragEndCallback>();

        PA = gameObject.GetComponent<ProductAesthetics>();

        if (PA == null)
            Debug.LogError("Product Aesthetics not present");
    }

    private Vector3 CalculateMousePosition()
    {
        //Calculate the estimaded mouse position in the 3D space
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Vector3 mousePos3D = ray.GetPoint(distance);

        // Transform to local coordinates
        return transform.parent.InverseTransformPoint(mousePos3D);
    }

    private void CalculateNextPosition(ref BoxJSON b)
    {

        Vector3 mousePos3D = CalculateMousePosition();

        //transform.localPosition = localDragLines[this_box.current_index] + (localDragLines[this_box.current_index + 1] - localDragLines[this_box.current_index]) * this_box.current_pos_relative;
        //Vector3 right_p = dLines[b.cir] + (dLines[b.cir + 1] - dLines[b.cir]) * b.cpr;
        //Vector3 left_p = dLines[b.cil] + (dLines[b.cil + 1] - dLines[b.cil]) * b.cpl;


        Vector3 right_p = dLines[b.cir] + (dLines[b.cir + 1] - dLines[b.cir]) * b.cpr;
        Vector3 left_p = dLines[b.cil] + (dLines[b.cil + 1] - dLines[b.cil]) * b.cpl;

        Vector2 v1 = (right_p - left_p).to2DwoY();
        Vector2 v2 = (mousePos3D - transform.localPosition).to2DwoY();

        /* Check if the mouse is on top of the box. Don't move if that is the case */
        if( MiscFunc.IntersectRay2DvsSegment(v2, v1.PerpClockWise(), right_p, v1, out _ ))
        {
            return;
        }

        float angle = Vector2.Angle(v1, v2);

        move_right = angle <= 90;

        //bool move_right = false;
        int c_index = move_right ? b.cir : b.cil;
        float c_pos = move_right ? b.cpr : b.cpl;

        Vector3 pos = move_right ? right_p : left_p;

        // Speed calculations

        float time_since_start_drag = (Time.time - startDragTime);
        // if more than 1 seconds passed -> 1 factor
        // else scaling factor
        float time_speed_factor = time_since_start_drag > 1 ? 1 : time_since_start_drag;

        float mouse_distance_speed_factor = (mousePos3D - pos).magnitude > 10 ? 1 : (mousePos3D - pos).magnitude / 10f;

        // distance_to_move_budget_in_current_frame = drag_speed * time_since_last_frame * time_speed_factor
        // this will also be multiplied bye the mouse_position_speed_factor for each dragline
        float distance_to_move = DRAG_SPEED * Time.deltaTime * 2 * time_speed_factor * 3 * mouse_distance_speed_factor;


        while (distance_to_move > 0)
        {
            float move_budget = distance_to_move;
            MoveInDragline(ref c_index, ref c_pos, mousePos3D, ref distance_to_move);

            if (move_budget == distance_to_move) { break; }
        }

        if (move_right)
        {
            b.cir = c_index;
            b.cpr = c_pos;
            CalculateMatchingPoint(b.cir, b.cpr, b.actual_width, true, ref b.cil, ref b.cpl);
        }
        else
        {
            b.cil = c_index;
            b.cpl = c_pos;
            CalculateMatchingPoint(b.cil, b.cpl, b.actual_width, false, ref b.cir, ref b.cpr);
        }
    }

    private void MoveInDragline(ref int c_index, ref float c_pos, Vector3 towards_p, ref float move_budget)
    {

        Vector3 currentPos = dLines[c_index] + (dLines[c_index + 1] - dLines[c_index]) * c_pos;

        // Direction of the dragline
        Vector3 draglineDir = (dLines[c_index + 1] - dLines[c_index]).normalized;
        // curent_point to towards_dir
        Vector3 currentpToTowardspDir = (towards_p - currentPos).normalized;
        // Angle inbetween both vectors
        float alpha = Mathf.Acos(Vector3.Dot(draglineDir, currentpToTowardspDir) / (draglineDir.magnitude * currentpToTowardspDir.magnitude));


        // This representa how well the position of the cursor fits the direction of the current line
        float towards_position_speed_factor = Mathf.Abs(Mathf.Cos(alpha));

        // This represens if we are moiving towards the start (left) or end (right) of this dragline
        int right_or_left = (Mathf.Cos(alpha)) >= 0 ? 1 : -1;

        Vector3 dir_vector = right_or_left == 1 ? draglineDir : -draglineDir;

        // If we are going to the right the distance left will be measured agains the end (c_index +1) of this dragline
        // If we are going to the left the distance left will be measured against the start (c_index) of this dragline
        float dist_left_in_current_dragline = right_or_left == 1 ? (dLines[c_index + 1] - currentPos).magnitude : (dLines[c_index] - currentPos).magnitude;


        //Debug.Log("Actual move :" + move_budget * towards_position_speed_factor / (localDragLines[c_index + 1] - localDragLines[c_index]).magnitude + " RoL: " + right_or_left +" Speed fact: " + towards_position_speed_factor + " Dist left: " + dist_left_in_current_dragline + " Dist/fact: " + dist_left_in_current_dragline / towards_position_speed_factor + " budget: " + move_budget);


        // If there isn't enough move budget to finish this dragline
        // if towards_position_speed_factor == 0 it means that the towards point is in 90 (or 270) degrees, so we shouldnt move
        if (towards_position_speed_factor == 0 || dist_left_in_current_dragline / towards_position_speed_factor > move_budget)
        {
            //c_index = c_index;
            float moved_distance = move_budget * towards_position_speed_factor;

            c_pos = c_pos + right_or_left * (moved_distance / (dLines[c_index + 1] - dLines[c_index]).magnitude);
            move_budget = 0;
        }
        // If there is
        else
        {
            if (c_index == 0 && right_or_left == -1)
            {
                //c_index = c_index;
                c_pos = 0;
                move_budget = 0;
            }
            else if (c_index == dLines.Length - 2 && right_or_left == 1)
            {
                //c_index == c_index
                c_pos = 1;
                move_budget = 0;
            }
            else
            {
                c_index = c_index + right_or_left;
                c_pos = right_or_left == -1 ? 1 : 0;
                move_budget = move_budget - (dist_left_in_current_dragline / towards_position_speed_factor);
            }
        }
    }

    public void SetStartingPosition(Vector3 pos, int c_index, float c_pos)
    {
        transform.localPosition = pos;

        box.cir = c_index;
        box.cpr = c_pos;

        CalculateMatchingPoint(box.cir, box.cpr, box.actual_width, true, ref box.cil, ref box.cpl);

        last_position = pos;
        last_index = c_index;
        last_pos_rel = c_pos;
    }

    public void ReturnToLastValidPosition()
    {
        transform.localPosition = last_position;
        box.cir = last_index;
        box.cpr = last_pos_rel;
    }

    private void FindNextEmptySpace(GameObject other_cube)
    {
        //TODO
    }

    private Vector3 CalculateCenterPosition(BoxJSON b)
    {
        Vector3 right_p = dLines[box.cir] + (dLines[box.cir + 1] - dLines[box.cir]) * box.cpr;
        Vector3 left_p = dLines[box.cil] + (dLines[box.cil + 1] - dLines[box.cil]) * box.cpl;
        Vector3 perp_dir = (left_p - right_p).to2DwoY().PerpClockWise().to3DwY(0).normalized;

        return right_p + (perp_dir * -box.actual_depth / 2) + ((left_p - right_p) / 2) + new Vector3(0,box.actual_height/2,0);
    }


    /* - - - - - CALLLBACK REGISTER - - - - - */

    public delegate void OnMyCollisionEnterCallback(bool collided_upon);
    public delegate void OnMyCollisionExitCallback();
    public delegate void OnDragStartCallback();
    public delegate void OnDragEndCallback();

    private List<OnMyCollisionEnterCallback> onMyCollisionEnterCallbacks;
    private List<OnMyCollisionExitCallback> onMyCollisionExitCallbacks;
    private List<OnDragStartCallback> onDragStartCallBacks;
    private List<OnDragEndCallback> onDragEndCallBacks;

    public void RegisterOnMyCollisionEnterCallback(OnMyCollisionEnterCallback f)
    {
        onMyCollisionEnterCallbacks.Add(f);
    }
    public void RegisterOnMyCollisionExitCallback(OnMyCollisionExitCallback f)
    {
        onMyCollisionExitCallbacks.Add(f);
    }
    public void RegisterOnDragStartCallback(OnDragStartCallback f)
    {
        onDragStartCallBacks.Add(f);
    }
    public void RegisterOnDragEndCallback(OnDragEndCallback f)
    {
        onDragEndCallBacks.Add(f);
    }

    public void ExecOnMyCollisionEnterCallbacks()
    {
        foreach (OnMyCollisionEnterCallback f in onMyCollisionEnterCallbacks) { f(collided_upon); }
    }
    public void ExecOnMyCollisionExitCallbacks()
    {
        foreach (OnMyCollisionExitCallback f in onMyCollisionExitCallbacks) { f(); }
    }
    public void ExecOnDragStartCallbaks()
    {
        foreach (OnDragStartCallback f in onDragStartCallBacks) { f(); }
    }
    public void ExecOnDragEndCallbaks()
    {
        foreach (OnDragEndCallback f in onDragEndCallBacks) { f(); }
    }


    /* - - - - - STATIC METHODS - - - - - */

    public static Vector3[] CalculateDragLines(Vector3[] points, float offset, out int[] vtxRltn, bool doBeizer = false)
    {

        if (points.Length == 2)
        {

            Vector2 o1 = points[0].to2DwoY();
            Vector2 o2 = points[1].to2DwoY();

            Vector2 o1o2 = o2 - o1;

            Vector2 po1o2 = o1o2.PerpClockWise();
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

            Vector2 po1o2 = o1o2.PerpClockWise();
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

                Vector2 po2o3 = o2o3.PerpClockWise();

                po2o3.Normalize();

                Vector2 n3 = o2 - po2o3 * offset;
                Vector2 n4 = o3 - po2o3 * offset;

                Vector output;
                if (MiscFunc.Intersects(n1.toV(), n2.toV(), n3.toV(), n4.toV(), out output))
                {
                    Vector2 isc = output.toV2();
                    Vector2 n5 = new Ray2D(n1, isc - n1).GetPoint((isc - n1).magnitude - (n2 - isc).magnitude);
                    Vector2 n6 = new Ray2D(n4, isc - n4).GetPoint((isc - n4).magnitude - (n3 - isc).magnitude);

                    if (doBeizer)
                    {
                        Vector2[] res = MiscFunc.DoBezier(new Vector2[] { n5, isc, n6 }, 2, 40);

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
                    if (!MiscFunc.IntersectRay2D(n1, n2 - n1, n4, n3 - n4, out isc))
                    {
                        Debug.LogError("Did not intersect");
                    }

                    if (doBeizer)
                    {

                        Vector2[] res = MiscFunc.DoBezier(new Vector2[] { n2, isc, n3 }, 2, 20);

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


    /* Return a Vector Facing outwards from the face side of the product */
    public bool GetFrontVector (out Vector3 result)
    {
        /* Check if Box has a position already */
        if(PA != null && PA.transform.childCount > 0)
        {
            /* Get the Normals of the mesh of the plane used to hold the image of the product*/
            Vector3[] normals = PA.transform.GetChild(0).GetComponent<MeshFilter>().mesh.normals;
            result = PA.transform.GetChild(0).transform.TransformDirection(normals[0]);
            return  true;
        }
        result = Vector3.zero;
        return false;
    }
    /* - - - - - STATIC METHODS - - - - - */

    public Vector3 CalculateMatchingPoint(int c_index, float c_pos, float p_width, bool given_is_right, ref int index, ref float pos)
    {
        // We have the position of one of the vertices of the product in the dragline, calculate the other one

        Vector3 P = (dLines[c_index] + (dLines[c_index + 1] - dLines[c_index]) * c_pos);

        // The loop has to go in difrent directions depending if we are searching left or right
        int from = given_is_right ? c_index + 1 : c_index;
        int to = given_is_right ? dLines.Length : -1;
        int incr = given_is_right ? 1 : -1;

        int r_or_l = given_is_right ? 1 : 0;
        // The second point will be contained inside the same segment
        if((P-dLines[c_index + r_or_l]).magnitude > p_width)
        {
            Vector3 dir = dLines[c_index + 1] - dLines[c_index];
            Vector3 C = c_pos * dir + p_width * (dir.normalized * incr);
            index = c_index;
            pos = C.magnitude / dir.magnitude;

            return C + dLines[c_index];
        }
        else
        {
            int near_index = -1;
            for (int i = from; i != to; i += incr)
            {
                float dist = (dLines[i] - P).magnitude;

                if (dist >= p_width)
                {
                    near_index = i;
                    break;
                }
            }

            // TODO: should give an error ?
            if(near_index == -1)
            {
                if (given_is_right) { index = c_index; pos = 1; return dLines[c_index + 1]; }
                else { index = c_index; pos = 0; return dLines[index]; }
            }


            // We now know  that the point must be found inbetween close_index - 1 and close_index
            Vector2 A = (given_is_right ? dLines[near_index - 1] : dLines[near_index + 1]).to2DwoY(); ;
            Vector2 B = dLines[near_index].to2DwoY();
            Vector2 D;   // Closest point in the line to P
            Vector2 C1;  // Poitn we want to find
            Vector2 C2;

            MiscFunc.PointOnTopOfSegment(P.to2DwoY(), A, B, out D);

            // Cos alpha = |PD|/|PC|
            float alpha = Mathf.Acos((D - P.to2DwoY()).magnitude / p_width);

            // Sin alpha = |CD|/|PC|
            float CDm = p_width * Mathf.Sin(alpha);

            //There can be two solutions to this, C = D + AB.normalized * (+ or -) |CD|
            // Chose the one closest to A ( if it is inside the semgment)

            C1 = D + (B - A).normalized * -CDm;
            C2 = D + (B - A).normalized * +CDm;


            bool C1inSeg = MiscFunc.PointIsInSegment(A, B, C1);
            bool C2inSeg = MiscFunc.PointIsInSegment(A, B, C2);

            Vector2 C;
            if (C1inSeg && C2inSeg)
            {
                float d1 = (C1 - A).magnitude;
                float d2 = (C2 - A).magnitude;

                C = d1 < d2 ? C1 : C2;
            }
            else if (C1inSeg)
            {
                C = C1;
            }
            else
            {
                C = C2;
            }


            //float PAm = (P - A).magnitude;
            //float PBm = (P - B).magnitude;

            //if(near_index == 0)
            //{
            //    string bug = "here";
            //}

            index = given_is_right ? near_index - 1 : near_index;

            pos = given_is_right ? (A - C).magnitude / (A - B).magnitude: 1 - (A - C).magnitude / (A - B).magnitude;
            return A + pos * (B - A);
            //pos = given_is_right ? (p_width - PAm) / (PBm - PAm) : 1 - (p_width - PAm) / (PBm - PAm);

            //return A + (B - A) * (p_width - PAm) / (PBm - PAm);
        }
    }


}
