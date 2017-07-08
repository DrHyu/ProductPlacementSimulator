using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CollisionMap
{
    // Eeach node has it's own collision infos in it's relevant position in the array

    public float resolution = 0.05f;

    public static float ray_length = 50;

    public List<CollisionBucket>[] perNodeCollision;

    public Vector3[] mDraglines;

    Dictionary<int, List<int>> collisionNote;


    public CollisionMap(Vector3[] dragLines, GameObject[] cubes)
    {
        perNodeCollision = new List<CollisionBucket>[dragLines.Length];

        mDraglines = dragLines;

        for(int i = 0; i < dragLines.Length-1 ; i++)
        {
            perNodeCollision[i] = new List<CollisionBucket>();

            int n_buckets = (int)((dragLines[i + 1] - dragLines[i]).magnitude / resolution) +1;
            for(int p = 0; p < n_buckets; p++)
            {
                CollisionBucket cb = new CollisionBucket();

                cb.left = new CollisionInfo(ray_length, -1);
                cb.right = new CollisionInfo(ray_length, -1);
                perNodeCollision[i].Add(cb);
            }
        }

        collisionNote = new Dictionary<int, List<int>>();

        for (int i = 0; i < cubes.Length; i++)
        {
            collisionNote.Add(cubes[i].GetInstanceID(), new List<int>());
        }

    }

    public void AddCollisionInfo(int dragline, int bucket, float min_height, float max_height, int ID, bool contained)
    {
        CollisionBucket cb = perNodeCollision[dragline][bucket];

        if(cb.left == null)
        {
            cb.left = new CollisionInfo(min_height, ID, contained);
        }
        else if (min_height == ray_length)
        {
            // No collision since ray didn't hit
        }
        else
        {
            if(min_height <= cb.left.height)
            {
                // Collision happened
                if(cb.left.was_contained)
                {
                    collisionNote[cb.left.ID].Add(ID);
                    collisionNote[ID].Add(cb.left.ID);
                }
                cb.left = new CollisionInfo(min_height, ID, contained);
            }
            // min_height > cb.left.heigh && contained
            // aka: I should go here but I am too big
            else if (contained)
            {
                collisionNote[cb.left.ID].Add(ID);
                collisionNote[ID].Add(cb.left.ID);
            }
        }


        if (cb.right == null)
        {
            cb.right = new CollisionInfo(max_height, ID, contained);
        }
        else if (max_height == ray_length)
        {
            // No collision since ray didn't hit
        }
        else
        {
            if (max_height <= cb.right.height)
            {
                // Collision happened
                if (cb.right.was_contained)
                {
                    collisionNote[cb.right.ID].Add(ID);
                    collisionNote[ID].Add(cb.right.ID);
                }
                cb.right = new CollisionInfo(max_height, ID, contained);
            }
            // max_height > cb.right.heigh && contained
            // aka: I should go here but I am too big
            else if (contained)
            {
                collisionNote[cb.right.ID].Add(ID);
                collisionNote[ID].Add(cb.right.ID);
            }
        }



    }

    public bool AmICollided(int ID)
    {
        //foreach(List<CollisionInfo> pc in perIDCollision[ID])
        //{
        //    if (pc.Count > 1 || pc[0].collisionCause != ID)
        //    {
        //        return true;
        //    }
        //}

        return collisionNote[ID].Count > 0;
    }

    public bool AmICollided(int ID, out int[] with)
    {

        with = collisionNote[ID].ToArray();
        return collisionNote[ID].Count > 0;
    }

    public static void CalculateCollisionMap(Vector3[] mDragLine, BoxJSON[] boxes, GameObject[] cubes, Transform t, out CollisionMap cm, BoxJSON this_cube)
    {


        // Loop segment to segment and check if there is any box in the shelf that invades the space defined by the area above and below the segment

        /*         \     |                   |     / 
         *          \    |                   |    /
         *        A  \   |                   |   /  C         
         *            \  |                   |  /                 
         *             \ |                   | /
         *               |-------------------|    
         *                         B
         *                          
         *  The dashed bottom segment (B) is the the one we are currently looping through, 
         *  the inclined side lines (A,C) and the dashed botton line are the shelf 
         *  and the vertical "|" lines is the space defined by the segment we are looping through
         */

        float RESOLUTION = 0.3f;
        cm = new CollisionMap(mDragLine,cubes);

        for (int p = 0; p < boxes.Length; p++)
        {
            UpdateCollisionMap(mDragLine, boxes[p], cubes[p], t, ref cm, this_cube);
        }
    }

    public static void UpdateCollisionMap(Vector3[] mDragLine, BoxJSON box, GameObject cube, Transform t, ref CollisionMap cm, BoxJSON this_cube)
    {

        float RESOLUTION = 0.05f;

        int ID = cube.GetInstanceID();

        // Clear out any conflict info caused by ID
        //for(int i = 0; i < cm.perNodeCollision.Length; i++)
        //{
        //    for(int p =0; p < cm.perNodeCollision[i].Count; p++)
        //    {
        //        if(cm.perNodeCollision[i][p].collisionCause == ID)
        //        {
        //            cm.perNodeCollision[i].RemoveAt(p);
        //            p--;
        //        }
        //    }
        //}


        for (int i = 0; i < mDragLine.Length - 1; i++)
        {
            BoxCollider b = cube.GetComponent<BoxCollider>();

            Vector3 segmentStart = mDragLine[i];
            Vector3 segmentEnd = mDragLine[i + 1];

            segmentStart = cube.transform.parent.TransformPoint(segmentStart);
            segmentEnd = cube.transform.parent.TransformPoint(segmentEnd);


            segmentStart.y = cube.transform.position.y;
            segmentEnd.y = cube.transform.position.y;

            Vector3 segmentDir = segmentEnd - segmentStart;

            // Pointing towards inside
            Vector3 perpVec = new Vector3(segmentDir.z, segmentDir.y, -segmentDir.x);

            // Fire an array of rays towards the cubes
            int num_steps = (int)(segmentDir.magnitude / RESOLUTION)+1;
            Vector3 step = segmentDir / num_steps;


            bool contained = false;

            for (int z = 0; z < num_steps; z++)
            {

                Vector3 point = segmentStart + step * (z + 0.5f);
                Vector3 direction = perpVec.normalized;

                Ray r1 = new Ray();
                Ray r2 = new Ray();

                contained = (point == b.ClosestPointOnBounds(point));

                if (!contained)
                {
                    r1 = new Ray(point, direction);
                    r2 = new Ray(point, -direction);
                }
                // If the point is inside the box, the ray won't hit the it since it is firing it from the inside
                // Inverse the ray to solve this
                else
                {
                    Ray temp = new Ray(point, perpVec.normalized);
                    // One ray in one direction
                    r1 = new Ray(temp.GetPoint(ray_length), -direction);

                    temp = new Ray(point, -perpVec.normalized);
                    // One ray in the oposite one
                    r2 = new Ray(temp.GetPoint(ray_length), direction);
                }

                RaycastHit rch1;
                RaycastHit rch2;

                bool r1hit = b.Raycast(r1, out rch1, ray_length);
                bool r2hit = b.Raycast(r2, out rch2, ray_length);

                if (r1hit || r2hit)
                {
                    float max_dist = r1hit ? contained ? ray_length - rch1.distance : rch1.distance : ray_length;
                    float min_dist = r2hit ? contained ? ray_length - rch2.distance : rch2.distance : ray_length;

                    cm.AddCollisionInfo(i, z, min_dist, max_dist, ID, contained);
                }
            }
        }
    }

}

public class CollisionInfo
{
    public bool was_contained = false;

    public float height;

    // ID of the box causing the collision
    public int ID;


    public CollisionInfo(float h,  int id, bool contained = false)
    {
        height = h;
        ID = id;
        was_contained = contained;
    }

}

public class CollisionBucket
{
    public CollisionInfo right;
    public CollisionInfo left;

}
