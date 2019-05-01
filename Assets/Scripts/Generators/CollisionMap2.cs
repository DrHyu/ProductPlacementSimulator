using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class CollisionMap2
{
    // Eeach node has it's own collision infos in it's relevant position in the array
    public Drag3D[] cubes;
    Dictionary<int, List<int>> collisionNote;

    public Vector2[] vertices_a ;
    public Vector2[] vertices_b ;

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

        collisionNote[ID].Clear();

        for (int p = 0; p < cubes.Length; p++)
        {
            if (cubes[p].gameObject.GetInstanceID() != ID)
            {
                cube.GetBottomVertices(out vertices_a);
                cubes[p].GetBottomVertices(out vertices_b);

                if (MiscFunc.BoxesColide2D(vertices_a,vertices_b))
                {
                    collisionNote[ID].Add(cubes[p].gameObject.GetInstanceID());

                    if (!collisionNote[cubes[p].gameObject.GetInstanceID()].Contains(ID))
                    {
                        collisionNote[cubes[p].gameObject.GetInstanceID()].Add(ID);
                    }
                }
                /* If did not colide and was previosuly colided clear the colision Note */
                else if(collisionNote[cubes[p].gameObject.GetInstanceID()].Contains(ID))
                {
                    collisionNote[cubes[p].gameObject.GetInstanceID()].Remove(ID);
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

    public void FindNextEmptySpace(GameObject other_cube)
    {
        /* Find the next empty space in the draglines */

        /* Check if it ispossible to add it to the right of the first product */

        /* Sort the cubes according to their positions right to left */

        List<Drag3D> ordered = new List<Drag3D>();

        for(int i = 0; i < cubes.Length; i ++)
        {
            ordered.Add(cubes[i]);
        }
        ordered.Sort(Drag3D.CompareByPosition);




        /* If not possible check the right of the next prodctucts */

    }

}


