using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGenerator {

    public Vector2[] v;

    public MeshGenerator(Vector2[] _v)
    {
        v = _v;
    }

    public MeshGenerator(float[] x, float[] y)
    {
        v = new Vector2[x.Length];
        for(int i = 0; i < x.Length; i++)
        {
            v[i].x = x[i];
            v[i].y = y[i];
        }
    }

    // Takes in a set of vertices representing a flat surface on the X axis
    // Returns the mesh a "cuboid" created offesting the original surface by offset on the Y axis + closing the side
    public Mesh get3DMeshFrom2D(float yOffset)
    {
        Triangulator t = new Triangulator(v);
        int[] top_index = t.Triangulate();

        Vector3[] v3D_top = new Vector3[v.Length];
        Vector3[] v3D_bot = new Vector3[v.Length];

        for (int i = 0; i < v.Length; i++)
        {
            v3D_top[i] = new Vector3(v[i].x, 0, v[i].y);
            v3D_bot[i] = new Vector3(v[i].x, yOffset, v[i].y);
        }

        Vector3[] v3D_all = new Vector3[v.Length*2];

        v3D_top.CopyTo(v3D_all, 0);
        v3D_bot.CopyTo(v3D_all, v3D_top.Length);


        int[] bottom_index = new int[top_index.Length];

        // Essentially coppying the triangulation result but offesting the indices to point to the "bottom" side
        // Reorder the order of the vertices within the same triangle to invert the surface normal

        for (int i = 0; i < top_index.Length; i ++)
        {
            bottom_index[top_index.Length - i -1] = top_index[i] + v.Length;
        }


        List<int> tmp = new List<int>();

        // Caclulate the indices for the sides
        for (int i = 0; i < v3D_top.Length; i++)
        {
            if( i+1 < v3D_top.Length)
            {
                // First Triangle
                tmp.Add(i + 1);
                tmp.Add(i);
                tmp.Add(i + v3D_top.Length);

                // Second Triangle
                tmp.Add(i + v3D_top.Length);
                tmp.Add(i+1+v3D_top.Length);
                tmp.Add(i + 1);
            }
            else
            {
                // First Triangle
                tmp.Add(0);
                tmp.Add(i);
                tmp.Add(i + v3D_top.Length);

                // Second Triangle
                tmp.Add(i + v3D_top.Length);
                tmp.Add(0 + v3D_top.Length);
                tmp.Add(0);
            }

        }

        int[] side_index = tmp.ToArray();


        var all_index = new int[top_index.Length + bottom_index.Length + side_index.Length];
        //var all_index = new int[top_index.Length + bottom_index.Length];

        // Merge all the indices in the same array
        top_index.CopyTo(all_index, 0);
        bottom_index.CopyTo(all_index, top_index.Length );
        side_index.CopyTo(all_index, top_index.Length + bottom_index.Length);



        Mesh msh = new Mesh();
        msh.vertices = v3D_all;
        msh.triangles = all_index;
        msh.RecalculateNormals();
        msh.RecalculateBounds();

        return msh;
    }
	
}
