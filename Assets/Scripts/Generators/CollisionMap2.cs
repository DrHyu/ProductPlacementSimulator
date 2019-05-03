using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class CollisionMap2
{
    // Eeach node has it's own collision infos in it's relevant position in the array
    public Drag3D[] cubes;
    public DragLines draglines;

    Dictionary<int, List<int>> collisionNote;

    public Vector2[] vertices_a ;
    public Vector2[] vertices_b ;

    public CollisionMap2(Drag3D[] c, DragLines _draglines)
    {
        cubes = c;
        draglines = _draglines;
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

    public bool WouldBoxColide(BoxJSON cube)
    {

        /* Check if this new hypothetical box would colide */
        /* This new box is not placed in the world yet, so we need to obtain the vertices positon in a new way */

        Vector2[] vertices = new Vector2[4];

        /* Vertices 0 and 3 we can know straigh away since they are on the dragline */
        vertices[0] = (draglines.points[cube.cir] + (draglines.points[cube.cir + 1] - draglines.points[cube.cir]) * cube.cpr).to2DwoY();
        vertices[3] = (draglines.points[cube.cil] + (draglines.points[cube.cil + 1] - draglines.points[cube.cil]) * cube.cpl).to2DwoY();

        /* Calculate the remaning ones */
        Vector2 normal = (vertices[0] - vertices[3]).PerpClockWise();

        vertices[1] = vertices[0] + (-normal.normalized) * cube.actual_depth;
        vertices[2] = vertices[2] + (-normal.normalized) * cube.actual_depth;

        for (int i = 0; i < cubes.Length; i ++)
        {
            Vector2[] other_vertices;
            cubes[i].GetBottomVertices(out other_vertices);
            if(MiscFunc.BoxesColide2D(vertices, other_vertices))
            {
                return true;
            }
        }

        return false;
    }

    public static void GenerateCollisionMap(Drag3D[] cubes, DragLines dlines, out CollisionMap2 cm)
    {
        cm = new CollisionMap2(cubes, dlines);

        for (int p = 0; p < cubes.Length; p++)
        {
            cm.UpdateCollisionMap(cubes[p]);
        }
    }

    public bool FindNextEmptySpace(ref Drag3D other_cube)
    {
        /* Find the next empty space in the draglines */

        /* Sort the cubes according to their positions right to left */
        List<Drag3D> ordered = new List<Drag3D>();

        for (int i = 0; i < cubes.Length; i ++)
        {
            ordered.Add(cubes[i]);
        }
        ordered.Sort(Drag3D.CompareByPosition);

        /* Check if it ispossible to add it to the right of the first product */

        for (int i = 0; i < ordered.Count; i ++)
        {
            BoxJSON bx = other_cube.box.Copy();

            bx.cir = other_cube.box.cir;
            bx.cpr = other_cube.box.cpr + 0.01f;

            draglines.CalculateMatchingPoint(bx.cir, bx.cpr, bx.actual_width, true, ref bx.cil, ref bx.cpl);

            /* Temporarily position it to the right of the product and check if it fits */

            if(!WouldBoxColide(bx))
            {
                /* Found a position which does't colide */
                other_cube.box = bx.Copy();
                return true;
            }

        }
        return false;

    }

}


