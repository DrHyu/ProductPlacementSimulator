using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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

        for (int p = 0; p < cubes.Length; p++)
        {
            if (cubes[p].gameObject.GetInstanceID() != ID)
            {
                collisionNote[ID].Clear();

                vertices_a = new Vector2[4];
                vertices_b = new Vector2[4];

                vertices_a[0] = (cube.gameObject.transform.rotation * new Vector3(cube.box.actual_width / 2.0f, 0, cube.box.actual_depth / 2.0f) + cube.transform.position).to2DwoY();
                vertices_a[1] = (cube.gameObject.transform.rotation * new Vector3(cube.box.actual_width / 2.0f, 0, -cube.box.actual_depth / 2.0f) + cube.transform.position).to2DwoY();
                vertices_a[2] = (cube.gameObject.transform.rotation * new Vector3(-cube.box.actual_width / 2.0f, 0, -cube.box.actual_depth / 2.0f) + cube.transform.position).to2DwoY();
                vertices_a[3] = (cube.gameObject.transform.rotation * new Vector3(-cube.box.actual_width / 2.0f, 0, cube.box.actual_depth / 2.0f) + cube.transform.position).to2DwoY();

                vertices_b[0] = (cubes[p].gameObject.transform.rotation * new Vector3( cubes[p].box.actual_width / 2.0f, 0,  cubes[p].box.actual_depth / 2.0f) + cubes[p].transform.position).to2DwoY();
                vertices_b[1] = (cubes[p].gameObject.transform.rotation * new Vector3( cubes[p].box.actual_width / 2.0f, 0, -cubes[p].box.actual_depth / 2.0f) + cubes[p].transform.position).to2DwoY();
                vertices_b[2] = (cubes[p].gameObject.transform.rotation * new Vector3(-cubes[p].box.actual_width / 2.0f, 0, -cubes[p].box.actual_depth / 2.0f) + cubes[p].transform.position).to2DwoY();
                vertices_b[3] = (cubes[p].gameObject.transform.rotation * new Vector3(-cubes[p].box.actual_width / 2.0f, 0,  cubes[p].box.actual_depth / 2.0f) + cubes[p].transform.position).to2DwoY();

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
}


