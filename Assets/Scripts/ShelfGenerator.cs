using UnityEngine;
using UnityEditor;

public class ShelfGenerator : MonoBehaviour
{

    public int n_cubes = 2;

    public GameObject[] cubes;

    public void Initialize(Shelf s, Mesh msh)
    {

        transform.localPosition = new Vector3(s.x_start, s.y_start, s.z_start);

        GameObject mesh_object = new GameObject("mesh");


        mesh_object.AddComponent(typeof(MeshRenderer));
        MeshFilter filter = mesh_object.AddComponent(typeof(MeshFilter)) as MeshFilter;
        filter.mesh = msh;
        mesh_object.GetComponent<Renderer>().material.color = Color.white;
        mesh_object.GetComponent<Transform>().localScale = new Vector3(1, 1, 1);
        
        mesh_object.transform.SetParent(transform);

        mesh_object.GetComponent<Transform>().localPosition = new Vector3(0, 0, 0);


        // Calculate the the points that belong to the front face of the shelf
        // If none ar given, they are all front face by default
        if (s.front_index == null)
        {
            s.front_index = new int[s.x_points.Length];

            for (int p = 0; p < s.front_index.Length; p++)
            {
                s.front_index[p] = p;
            }
        }
        Vector3[] dragline = new Vector3[s.front_index.Length];

        for (int a = 0; a < dragline.Length; a++)
        {
            dragline[a] = new Vector3(s.x_points[s.front_index[a]],
                                        0,
                                        s.y_points[s.front_index[a]]);
        }

        int[] vertexR;
        Vector3[] offsettedDragline = Drag3D.CalculateDragLines(dragline, 0.2f, out vertexR, false);

        // Add the products to the shelf
        cubes = new GameObject[n_cubes];
        for (int p = 0; p < n_cubes; p++)
        {
            cubes[p] = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cubes[p].transform.SetParent(transform);
            cubes[p].transform.localPosition = new Vector3(0, 0, 0);

            Drag3D d3d = cubes[p].AddComponent(typeof(Drag3D)) as Drag3D;
            d3d.setDragline(offsettedDragline);

            // Make it so there is always at least a very small gap inw betwwen cubes
            cubes[p].GetComponent<BoxCollider>().size *= 1.05f;

        }
        
    }


    public bool cubeIsColided(int cubeMoved)
    {
        GameObject g = cubes[cubeMoved];

        for (int i = 0; i < cubes.Length; i++)
        {
            if (i == cubeMoved) { continue; }

            Vector3 oc_pos = cubes[i].transform.position;
            Quaternion oc_rot = cubes[i].transform.rotation;

            if (cubes[i].GetComponent<BoxCollider>().bounds.Intersects(g.GetComponent<BoxCollider>().bounds))
            {
                return true;
            }

        }


        return false;
    }
}