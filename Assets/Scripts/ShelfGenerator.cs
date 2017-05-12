using UnityEngine;
using UnityEditor;

public class ShelfGenerator : MonoBehaviour
{

    public int n_cubes = 2;

    public GameObject[] current_cubes;

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
        current_cubes = new GameObject[n_cubes];
        for (int p = 0; p < n_cubes; p++)
        {
            current_cubes[p] = GameObject.CreatePrimitive(PrimitiveType.Cube);
            current_cubes[p].transform.SetParent(transform);
            current_cubes[p].transform.localPosition = new Vector3(0, 0, 0);

            Drag3D d3d = current_cubes[p].AddComponent(typeof(Drag3D)) as Drag3D;
            d3d.setDragline(offsettedDragline);

            //Rigidbody rgbd = current_cubes[p].AddComponent<Rigidbody>();
            //rgbd.isKinematic = true;
            //rgbd.useGravity = false;

        }
        
    }



}