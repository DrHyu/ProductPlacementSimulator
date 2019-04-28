using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CollisionMap2
{
    // Eeach node has it's own collision infos in it's relevant position in the array
    public Drag3D[] cubes;
    Dictionary<int, List<int>> collisionNote;

    public CollisionMap2(Drag3D[] c)
    {
        cubes = c;
        collisionNote = new Dictionary<int, List<int>>();

        for (int i = 0; i < c.Length; i++)
        {
            collisionNote.Add(c[i].gameObject.GetInstanceID(), new List<int>());
        }
    }

    public bool AmICollided(int ID)
    {
        return collisionNote[ID].Count > 0;
    }

    public bool AmICollided(int ID, out int[] with)
    {
        if (collisionNote.ContainsKey(ID))
        {
            with = collisionNote[ID].ToArray();
            return collisionNote[ID].Count > 0;
        }
        else
        {
            with = new int[] { };
            return false;
        }
    }

    public void UpdateCollisionMap(Drag3D cube)
    {
        int ID = cube.gameObject.GetInstanceID();

        for (int p = 0; p < cubes.Length; p++)
        {
            collisionNote[ID].Clear();

            if (cubes[p].gameObject.GetInstanceID() != ID)
            {
                if(MiscFunc.BoxesColide2D(cube.transform.position.to2DwoY(), 
                                        new Vector2(cube.box.actual_width, cube.box.actual_depth), 
                                        cubes[p].transform.position.to2DwoY(), 
                                        new Vector2(cubes[p].box.actual_width, cubes[p].box.actual_depth)))
                {
                    collisionNote[ID].Add(cubes[p].gameObject.GetInstanceID());
                    collisionNote[cubes[p].gameObject.GetInstanceID()].Add(ID);
                }
            }
        }        
    }

    public static void GenerateCollisionMap(Drag3D[] cubes, out CollisionMap2 cm)
    {
        cm = new CollisionMap2(cubes);

        for (int p = 0; p < cubes.Length; p++)
        {
            cm.UpdateCollisionMap(cubes[p]);
        }
    }
}


