
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.EventSystems;

public class Drag3D : MonoBehaviour, IPointerUpHandler, IPointerDownHandler
{
    public CollisionMap cm;

    public BoxJSON this_box;

    public ProductAesthetics PA; 
    public FloatingProducts floatingObj;

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

    public Vector3[] globalDragLines;
    public Vector3[] localDragLines;
    public int[] global_to_public_relation;

    // The face of the cube should always be oriented in this direction
    private Vector2[] normals;


    /* - - - - - OVERRIDE METHODS - - - - - */

    private void Update()
    {
        if(dragging && ! deattached)
        {
            //int next_index;
            //// Try the new position
            //transform.localPosition = getClosestPointInCurrentLine(mousePos3D, this_box.current_index, out next_index);

            //this_box.current_index = next_index;

            //Debug.Log("3 C_INDEX: " + this_box.current_index + " C_POS: " + this_box.current_pos_relative );
            CalculateNextPosition(this_box.current_index, this_box.current_pos_relative, ref this_box.current_index, ref this_box.current_pos_relative);
            //Debug.Log("4 C_INDEX: " + this_box.current_index + " C_POS: " + this_box.current_pos_relative);


            transform.localPosition = localDragLines[this_box.current_index] + (localDragLines[this_box.current_index + 1] - localDragLines[this_box.current_index]) * this_box.current_pos_relative;

            // This box moved so the other collision maps are no longer valid
            GetComponentInParent<ShelfGenerator>().InvalideChildCollisionMaps(gameObject.GetInstanceID());

            // This could be done when initializing this object on the ShelfGenerator. 
            // However, It is likely that not every cube will be moved during a sesion, therefore it makes sense to calculate the collision map only when a cube is moved
            //if (cm == null)
            //{
                ShelfGenerator sg = GetComponentInParent<ShelfGenerator>();
                CollisionMap.CalculateCollisionMap(localDragLines, sg.cubesJSON.ToArray(), sg.cubes.ToArray(), transform.parent, out cm, this_box);
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
                if (!collided) { ExecOnMyCollisionEnterCallbacks(); }
                collided = true;
                GetComponentInParent<ShelfGenerator>().NotifyCollision(collided_with, gameObject.GetInstanceID());
            }
            else
            {
                // If transitioning from collided to non collided
                if (collided)
                {
                    // Notify that we are not longer in collision and cubes that are "collided upon" can reset to default state
                    GetComponentInParent<ShelfGenerator>().ClearCollision();
                    ExecOnMyCollisionExitCallbacks();
                }
                collided = false;

                // This are used to reover the last valid position if, for example, the user stops dragging a block during an intercception
                last_index = this_box.current_index;
                last_pos_rel = this_box.current_pos_relative;
                last_position = transform.localPosition;

                // The position of the cube is stored as:
                //  1. Index of the vertex in the dragline
                //  2. magnitude(current_3d_pos - dragLine[c_index])/ magnitude(dragLine[c_index+1] - dragLine[c_index])
                // Stored this way to allow the stands to be rotated and/or scaled while maintaining the relative position of this cube
                this_box.current_pos_relative = Vector3.Magnitude(transform.localPosition - localDragLines[this_box.current_index]) / Vector3.Magnitude(localDragLines[this_box.current_index + 1] - localDragLines[this_box.current_index]);
            }
            
        }

        // Start D-attachment
        if(selected && Input.GetKeyDown(KeyCode.D) && !deattached)
        {
            SetDeattached(true);
        }
        // Cancel deattatchment
        else if (deattached && !selected || deattached && Input.GetKeyDown(KeyCode.D))
        {
            SetDeattached(false);
        }
        // Move while deattached
        else if (deattached && dragging && selected)
        {
            // When deattatched move wherever the mouse is 
            transform.localPosition = CalculateMousePosition();
        }


    }

    private void Start()
    {


        transform.localPosition = localDragLines[this_box.current_index] + (localDragLines[this_box.current_index + 1] - localDragLines[this_box.current_index]) * this_box.current_pos_relative;
        last_position = transform.localPosition;

        floatingObj = GameObject.Find("FloatingProducts").GetComponent<FloatingProducts>();
        if (floatingObj == null)
            Debug.LogError("Couldnt't find floating products object !");
    }

    private void LateUpdate()
    {
        Vector3 n = normals[this_box.current_index].to3DwY(0);
        transform.localRotation = Quaternion.LookRotation(n);
    }

    public void OnValidate()
    {
        
        // Re-calulate the local drag lines based on the new scale of the object
        OffsetDraglineByCubeSize();
        CalculateNormals();

        transform.localPosition = localDragLines[this_box.current_index];
    }

    public void OnPointerUp(PointerEventData eventData) 
    {
        if (collided)
        {
            // Notify that we are not longer in collision and cubes that are "collided upon" can reset to default state
            GetComponentInParent<ShelfGenerator>().ClearCollision();
            ReturnToLastValidPosition();
            collided = false;
            ExecOnMyCollisionExitCallbacks();
        }
        dragging = false;
        ExecOnDragEndCallbaks();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        distance = Vector3.Distance(GetComponent<Transform>().position, Camera.main.transform.position);
        dragging = true;
        ExecOnDragStartCallbaks();

        if (!selected)
        {
            ExecOnClickCallbacks(transform.parent.parent.GetComponent<StandGenerator>(), transform.parent.gameObject.GetComponent<ShelfGenerator>(), this);
        }

        startDragTime = Time.time;
    }

    public void SetSelected(bool sel)
    {
        selected = sel;
        PA.SetSelected(sel);
    }

    public void SetDeattached(bool deattached)
    {
        if(deattached && !this.deattached)
        {
            transform.parent.GetComponent<ShelfGenerator>().DeattachProduct(this, true);
            floatingObj.AddFloatingProduct(this, transform.parent.GetComponent<ShelfGenerator>(), transform.parent.parent.GetComponent<StandGenerator>());
            this.deattached = true;
        }
        else if(!deattached && this.deattached)
        {
            floatingObj.ReturnFloatingProduct();
            //ReturnToLastValidPosition();
            this.deattached = false;
        }
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

    public void Initialize(BoxJSON b, Vector3[] _dragLines)
    {
        this_box = b;

        globalDragLines = _dragLines;
        OffsetDraglineByCubeSize();
        CalculateNormals();


        onClickCallBacks = new List<OnClickCallback>();
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

    private void CalculateNextPosition(int c_index, float current_pos, ref int new_index, ref float new_pos)
    {

        //Calcualte the estimaded mouse position in the 3D space
        //Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        //Vector3 mousePos3D = ray.GetPoint(distance);

        //// Transform to local coordinates
        //mousePos3D = transform.parent.InverseTransformPoint(mousePos3D);

        Vector3 mousePos3D = CalculateMousePosition();

        // Speed calculations

        float time_since_start_drag = (Time.time - startDragTime);
        // if more than 1 seconds passed -> 1 factor
        // else scaling factor
        float time_speed_factor = time_since_start_drag > 1 ? 1 : time_since_start_drag;

        float mouse_distance_speed_factor = (mousePos3D - transform.localPosition).magnitude > 10 ? 1 : (mousePos3D - transform.localPosition).magnitude / 10f;
        // distance_to_move_budget_in_current_frame = drag_speed * time_since_last_frame * time_speed_factor
        // this will also be multiplied bye the mouse_position_speed_factor for each dragline
        float distance_to_move = DRAG_SPEED * Time.deltaTime * 2 * time_speed_factor * 3 * mouse_distance_speed_factor;


        while (distance_to_move > 0)
        {
            float move_budget = distance_to_move;
            //Debug.Log("1 C_INDEX: "+c_index+ " C_POS: " +current_pos+ " BUDGET: "+ distance_to_move);
            MoveInDragline(ref c_index, ref current_pos, mousePos3D, ref distance_to_move);
            //Debug.Log("2 C_INDEX: " + c_index + " C_POS: " + current_pos + " BUDGET: " + distance_to_move);

            if (move_budget == distance_to_move) { break; }
        }
        //Debug.Log("3 C_INDEX: " + c_index + " C_POS: " + current_pos + " BUDGET: " + distance_to_move);


        new_index = c_index;
        new_pos = current_pos;
    }

    private void MoveInDragline(ref int c_index, ref float c_pos, Vector3 towards_p, ref float move_budget)
    {

        Vector3 currentPos = localDragLines[c_index] + (localDragLines[c_index + 1] - localDragLines[c_index]) * c_pos;

        // Direction of the dragline
        Vector3 draglineDir = (localDragLines[c_index + 1] - localDragLines[c_index]).normalized;
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
        float dist_left_in_current_dragline = right_or_left == 1 ? (localDragLines[c_index + 1] - currentPos).magnitude : (localDragLines[c_index] - currentPos).magnitude;


        //Debug.Log("Actual move :" + move_budget * towards_position_speed_factor / (localDragLines[c_index + 1] - localDragLines[c_index]).magnitude + " RoL: " + right_or_left +" Speed fact: " + towards_position_speed_factor + " Dist left: " + dist_left_in_current_dragline + " Dist/fact: " + dist_left_in_current_dragline / towards_position_speed_factor + " budget: " + move_budget);


        // If there isn't enough move budget to finish this dragline
        // if towards_position_speed_factor == 0 it means that the towards point is in 90 (or 270) degrees, so we shouldnt move
        if (towards_position_speed_factor == 0 || dist_left_in_current_dragline / towards_position_speed_factor > move_budget)
        {
            //c_index = c_index;
            float moved_distance = move_budget * towards_position_speed_factor;

            c_pos = c_pos + right_or_left * (moved_distance / (localDragLines[c_index + 1] - localDragLines[c_index]).magnitude);
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
            else if (c_index == localDragLines.Length - 2 && right_or_left == 1)
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

    public void ReturnToLastValidPosition()
    {
        transform.localPosition = last_position;
        this_box.current_index = last_index;
        this_box.current_pos_relative = last_pos_rel;
    }

    private void FindNextEmptySpace(GameObject other_cube)
    {

        
    }

    private void OffsetDraglineByCubeSize()
    {

        /* Diferent size of cubes requires diferent draglines
         * so, take the common dragline for all cubes in the shelf
         * and tweak it slightly to fit the size of this gameobject
        */

        localDragLines = CalculateDragLines(globalDragLines, this_box.actual_depth/2, out global_to_public_relation, true);

        for(int i = 0; i < localDragLines.Length; i++)
        {
            localDragLines[i].y += this_box.actual_height/2;
        }

        // Shorten start and end points //
        Vector3 temp = localDragLines[1] - localDragLines[0];
        temp.Normalize();
        localDragLines[0] = localDragLines[0] + temp * (this_box.actual_width);

        temp = localDragLines[localDragLines.Length - 2] - localDragLines[localDragLines.Length - 1];
        temp.Normalize();
        localDragLines[localDragLines.Length - 1] = localDragLines[localDragLines.Length - 1] + temp * (this_box.actual_width);

    }

    private void CalculateNormals()
    {
        normals = new Vector2[localDragLines.Length];
        for (int i = 0; i < localDragLines.Length - 1; i++)
        {
            Vector2 perpendicular = (localDragLines[i + 1].to2DwoY() - localDragLines[i].to2DwoY()).PerpClockWise();
            perpendicular.Normalize();

            normals[i] = perpendicular;
        }
    }


    /* - - - - - CALLLBACK REGISTER - - - - - */

    public delegate void OnClickCallback(StandGenerator stand, ShelfGenerator shelf, Drag3D box);
    public delegate void OnMyCollisionEnterCallback(bool collided_upon);
    public delegate void OnMyCollisionExitCallback();
    public delegate void OnDragStartCallback();
    public delegate void OnDragEndCallback();

    private List<OnClickCallback> onClickCallBacks;
    private List<OnMyCollisionEnterCallback> onMyCollisionEnterCallbacks;
    private List<OnMyCollisionExitCallback> onMyCollisionExitCallbacks;
    private List<OnDragStartCallback> onDragStartCallBacks;
    private List<OnDragEndCallback> onDragEndCallBacks;

    public void RegisterOnClickCallback(OnClickCallback f)
    {
        onClickCallBacks.Add(f);
    }
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

    public void UnregisterOnClickCallback(OnClickCallback f)
    {
        //foreach(OnClickCallback ff in onClickCallBacks)
        //{
        //    if(ff == f) { onClickCallBacks.Remove(f); }
        //}
        onClickCallBacks.Remove(f);
    }

    public void ExecOnClickCallbacks(StandGenerator stand, ShelfGenerator shelf, Drag3D box)
    {
        foreach (OnClickCallback f in onClickCallBacks) { f(stand, shelf, box);}
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

    /* - - - - - STATIC METHODS - - - - - */
}
