using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.EventSystems;

public class Drag3D : MonoBehaviour,  IPointerDownHandler, IPointerUpHandler
{

    private bool initialized = false;

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
    public Vector3 last_position;
    public int last_index = 0;
    public float last_pos_rel = 0;

    // The position of the cube is stored as:
    //  1. Index of the vertex in the dragline
    //  2. magnitude(current_3d_pos - dragLine[c_index])/ magnitude(dragLine[c_index+1] - dragLine[c_index])
    // Stored this way to allow the stands to be rotated and/or scaled while maintaining the relative position of this cube

    private float distance;
    private float startDragTime;

    // Drag speed/s
    public float DRAG_SPEED = 5f;


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

        Vector3 right_p = SG.dragLines.points[box.cir] + (SG.dragLines.points[box.cir + 1] - SG.dragLines.points[box.cir]) * box.cpr;
        Vector3 left_p = SG.dragLines.points[box.cil] + (SG.dragLines.points[box.cil + 1] - SG.dragLines.points[box.cil]) * box.cpl;

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

        if (transform.parent != null)
        {
            Gizmos.matrix = transform.parent.localToWorldMatrix;
        
            Gizmos.color = Color.green;
            for (int i = 0; i < SG.dragLines.points.Length - 1; i++)
            {
                Gizmos.DrawLine(SG.dragLines.points[i], SG.dragLines.points[i + 1]);
            }

            Vector3 right_p = SG.dragLines.points[box.cir] + (SG.dragLines.points[box.cir + 1] - SG.dragLines.points[box.cir]) * box.cpr;
            Vector3 left_p = SG.dragLines.points[box.cil] + (SG.dragLines.points[box.cil + 1] - SG.dragLines.points[box.cil]) * box.cpl;

            Gizmos.color = move_right ? Color.red : Color.green;
            Gizmos.DrawLine(right_p, new Vector3(right_p.x, right_p.y + 100, right_p.z));

            Gizmos.color = move_right ? Color.green : Color.red;
            Gizmos.DrawLine(left_p, new Vector3(left_p.x, left_p.y + 100, left_p.z));
        }
        else
        {
            Debug.Log("Unitialized");
        }

    }

    /* - - - - - NON-STATIC METHODS - - - - - */

    public void Initialize(BoxJSON b, ShelfGenerator parent)
    {
        box = b;
        SG = parent;

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

        Vector3 right_p = SG.dragLines.points[b.cir] + (SG.dragLines.points[b.cir + 1] - SG.dragLines.points[b.cir]) * b.cpr;
        Vector3 left_p = SG.dragLines.points[b.cil] + (SG.dragLines.points[b.cil + 1] - SG.dragLines.points[b.cil]) * b.cpl;

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
            SG.dragLines.MoveInDragline(ref c_index, ref c_pos, mousePos3D, ref distance_to_move);

            if (move_budget == distance_to_move) { break; }
        }

        if (move_right)
        {
            b.cir = c_index;
            b.cpr = c_pos;
            SG.dragLines.CalculateMatchingPoint(b.cir, b.cpr, b.actual_width, true, ref b.cil, ref b.cpl);
        }
        else
        {
            b.cil = c_index;
            b.cpl = c_pos;
            SG.dragLines.CalculateMatchingPoint(b.cil, b.cpl, b.actual_width, false, ref b.cir, ref b.cpr);
        }
    }

    

    public void ReturnToLastValidPosition()
    {
        transform.localPosition = last_position;
        box.cir = last_index;
        box.cpr = last_pos_rel;

        SG.dragLines.CalculateMatchingPoint(box.cir, box.cpr, box.actual_width, true, ref box.cil, ref box.cpl);
    }

    private Vector3 CalculateCenterPosition(BoxJSON b)
    {
        Vector3 right_p = SG.dragLines.points[box.cir] + (SG.dragLines.points[box.cir + 1] - SG.dragLines.points[box.cir]) * box.cpr;
        Vector3 left_p = SG.dragLines.points[box.cil] + (SG.dragLines.points[box.cil + 1] - SG.dragLines.points[box.cil]) * box.cpl;
        Vector3 perp_dir = (left_p - right_p).to2DwoY().PerpClockWise().to3DwY(0).normalized;

        return right_p + (perp_dir * -box.actual_depth / 2) + ((left_p - right_p) / 2) + new Vector3(0,box.actual_height/2,0);
    }

    public void GetBottomVertices( out Vector2[] v )
    {
        v = new Vector2[4];
        v[0] = (transform.rotation * new Vector3(box.actual_width / 2.0f, 0, box.actual_depth / 2.0f) + transform.position).to2DwoY();
        v[1] = (transform.rotation * new Vector3(box.actual_width / 2.0f, 0, -box.actual_depth / 2.0f) + transform.position).to2DwoY();
        v[2] = (transform.rotation * new Vector3(-box.actual_width / 2.0f, 0, -box.actual_depth / 2.0f) + transform.position).to2DwoY();
        v[3] = (transform.rotation * new Vector3(-box.actual_width / 2.0f, 0, box.actual_depth / 2.0f) + transform.position).to2DwoY();
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


    public void SetStartingPosition()
    {

        Vector3 vect = SG.dragLines.points[box.cir +1] - SG.dragLines.points[box.cir];
        
                                  /* Normal position calculation */
        transform.localPosition = SG.dragLines.points[box.cir] + vect * box.cpr 
                                /**/
                                + (vect.to2DwoY().PerpAntiClockWise() * box.actual_depth).to3DwY(SG.dragLines.points[box.cir].y);
    }

    public void SetStartingPosition(int c_index, float c_pos)
    {
        box.cir = c_index;
        box.cpr = c_pos;

        SG.dragLines.CalculateMatchingPoint(box.cir, box.cpr, box.actual_width, true, ref box.cil, ref box.cpl);

        last_index = c_index;
        last_pos_rel = c_pos;

        SetStartingPosition();
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

    public static int CompareByPosition(Drag3D a, Drag3D b)
    {
        /* The closer to the initial dragline the higher result */
        if(a.box.cir < b.box.cir)
        {
            return -1;
        }
        else if(a.box.cir > b.box.cir)
        {
            return 1;
        }
        else if(a.box.cpr < b.box.cpr)
        {
            return -1;
        }
        else if (a.box.cpr > b.box.cpr)
        {
            return 1;
        }
        else
        {
            return 0;
        }
    }



}
