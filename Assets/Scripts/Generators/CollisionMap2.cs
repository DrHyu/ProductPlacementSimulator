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

    private ShelfGenerator SG;

    public CollisionMap2(Drag3D[] c, DragLines _draglines, ShelfGenerator parent)
    {
        cubes = c;
        draglines = _draglines;
        SG = parent;
        collisionNote = new Dictionary<int, List<int>>();

        for (int i = 0; i < c.Length; i++)
        {
            collisionNote.Add(c[i].gameObject.GetInstanceID(), new List<int>());
        }

        for (int p = 0; p < cubes.Length; p++)
        {
            UpdateCollisionMap(cubes[p]);
        }
    }

    public void UpdateProducts(Drag3D[] c)
    {
        cubes = c;
        /* Recalculate everything*/
        collisionNote.Clear();
        for (int i = 0; i < c.Length; i++)
        {
            collisionNote.Add(c[i].gameObject.GetInstanceID(), new List<int>());
        }

        for (int p = 0; p < cubes.Length; p++)
        {
            UpdateCollisionMap(cubes[p]);
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
        vertices[0] = (draglines.points[cube.cil] + (draglines.points[cube.cil + 1] - draglines.points[cube.cil]) * cube.cpl).to2DwoY();
        vertices[3] = (draglines.points[cube.cir] + (draglines.points[cube.cir + 1] - draglines.points[cube.cir]) * cube.cpr).to2DwoY();
 

        /* Calculate the remaning ones */
        Vector2 normal = (vertices[0] - vertices[3]).PerpClockWise();

        vertices[1] = vertices[0] + (-normal.normalized) * cube.actual_depth;
        vertices[2] = vertices[3] + (-normal.normalized) * cube.actual_depth;

        vertices[0] = SG.transform.TransformPoint(vertices[0].to3DwY(0)).to2DwoY();
        vertices[1] = SG.transform.TransformPoint(vertices[1].to3DwY(0)).to2DwoY();
        vertices[2] = SG.transform.TransformPoint(vertices[2].to3DwY(0)).to2DwoY();
        vertices[3] = SG.transform.TransformPoint(vertices[3].to3DwY(0)).to2DwoY();

        /* Use the shelf transform to get world coordinates */


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



    public bool FindNextEmptySpace(BoxJSON sizes, out BoxJSON out_box)
    {
        /* Find the next empty space in the draglines */
        out_box = null;
        if (cubes.Length == 0)
        {
            out_box = sizes.Copy();

            out_box.cir = 0;
            out_box.cpr = 0.01f;

            draglines.CalculateMatchingPoint(out_box.cir, out_box.cpr, out_box.actual_width, true, ref out_box.cil, ref out_box.cpl);

            return true;
        }
        else
        {
            /* Sort the cubes according to their positions right to left */
            List<Drag3D> ordered = new List<Drag3D>();

            for (int i = 0; i < cubes.Length; i++)
            {
                ordered.Add(cubes[i]);
            }
            ordered.Sort(Drag3D.CompareByPosition);

            /* Check if it ispossible to add it to the right of the first product */

            for (int i = 0; i < ordered.Count; i++)
            {
                out_box = sizes.Copy();
                out_box.cir = ordered[i].box.cil;
                out_box.cpr = ordered[i].box.cpl;

                draglines.CalculateMatchingPoint(out_box.cir, out_box.cpr, out_box.actual_width, true, ref out_box.cil, ref out_box.cpl);

                /* While we haven't reached the next box ... or the end of the draglines */
                while (
                    i == ordered.Count - 1 ?
                    /* last index in draglines and position is at the end */
                    !(out_box.cil == draglines.points.Length - 1 && out_box.cpl < 0.99f):
                    /* stepped into the next product */
                    (out_box.cil < ordered[i+1].box.cir || (out_box.cil == ordered[i + 1].box.cir && out_box.cpl < ordered[i + 1].box.cpr)))
                {

                    // OPTION TO CONSIDER draglines.MoveInDragline(ref out_box.cil,ref out_box.cpl, draglines.points[out_box.cil], ref 5);

                    /* If we can fit the rigth vertex of the product in this dragline */
                    if (out_box.cpl <= 0.99f)
                    {
                        out_box.cpl += 0.01f;
                    }
                    /* Use the next dragline if it is not the last one */
                    else if (out_box.cil != draglines.points.Length - 2)
                    {
                        out_box.cil += 1;
                        out_box.cpl = 0;
                    }
                    /* No draglines left, can't fit the product */
                    else
                    {
                        break;
                    }

                    draglines.CalculateMatchingPoint(out_box.cil, out_box.cpl, out_box.actual_width, false, ref out_box.cir, ref out_box.cpr);


                    /* Temporarily position it to the right of the product and check if it fits */

                    if (!WouldBoxColide(out_box))
                    {
                        /* Found a position which does't colide */
                        return true;
                    }
                }
            }
            return false;
        }
    }

}


