using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;

[Serializable]
public class ShelfGenerator : MonoBehaviour
{
    public int n_cubes;
    public List<GameObject> cubes;

    public ShelfJSON this_shelf;

    private GameObject shelf_mesh;
    public Boolean selected = false;

    private Vector3[] offsettedDragline;

    public void Initialize(ShelfJSON s)
    {
        name = s.name;

        this_shelf = s;

        transform.localPosition = new Vector3(0,s.absolute_height,0);
        transform.localRotation = Quaternion.identity;

        // Generate new mesh game object
        shelf_mesh = new GameObject("mesh");

        // Calculate the mesh from the raw data
        MeshGenerator meshGen = new MeshGenerator(s.x_points, s.y_points);
        Mesh msh = meshGen.get3DMeshFrom2D(-s.thickness);

        // Render the mesh
        shelf_mesh.AddComponent(typeof(MeshRenderer));
        MeshFilter meshRenderer = shelf_mesh.AddComponent(typeof(MeshFilter)) as MeshFilter;
        meshRenderer.mesh = msh;

        shelf_mesh.GetComponent<Renderer>().material.color = Color.white;
        shelf_mesh.GetComponent<Transform>().localScale = new Vector3(1, 1, 1);
        shelf_mesh.GetComponent<Transform>().SetParent(transform);
        shelf_mesh.GetComponent<Transform>().localPosition = new Vector3(0, 0, 0);
        shelf_mesh.GetComponent<Transform>().localRotation = Quaternion.identity;

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
        offsettedDragline = Drag3D.CalculateDragLines(dragline, 0.2f, out vertexR, false);

        // Make some dummy boxes if none available
        //if(this_shelf.boxes == null || this_shelf.boxes.Length == 0)
        //{
        //    // Add the products to the shelf
        //    n_cubes = 1 + (int)(UnityEngine.Random.value * 5);
        //    this_shelf.boxes = new BoxJSON[n_cubes];

        //    for (int p = 0; p < n_cubes; p++)
        //    {
        //        this_shelf.boxes[p] = new BoxJSON();
        //        this_shelf.boxes[p].current_index = 0;
        //        this_shelf.boxes[p].current_pos_relative = UnityEngine.Random.value;
        //        this_shelf.boxes[p].width = 0.5f + UnityEngine.Random.value * 2f;
        //        this_shelf.boxes[p].height = 0.5f + UnityEngine.Random.value * 2f;
        //        this_shelf.boxes[p].depth = 0.5f + UnityEngine.Random.value * 2f;
        //    }
        //}

        cubes = new List<GameObject>();

        if (this_shelf.boxes != null)
        {
            for (int p = 0; p < this_shelf.boxes.Length; p++)
            {
                //cubes.Add(GameObject.CreatePrimitive(PrimitiveType.Cube));
                //cubes[p].transform.SetParent(transform);

                //Drag3D d3d = cubes[p].AddComponent(typeof(Drag3D)) as Drag3D;
                //d3d.Initialize(this_shelf.boxes[p], offsettedDragline, p);

                //// Make it so there is always at least a very small gap inw betwwen cubes
                //cubes[p].GetComponent<BoxCollider>().size *= 1.05f;

                GenerateProduct(this_shelf.boxes[p]);
            }
        }
        
    }

    public void GenerateProduct(BoxJSON box)
    {
        // TODO: Need to actually edit the boxes array
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = box.name;
        cubes.Add(go);
        go.transform.SetParent(transform);

        Drag3D d3d = go.AddComponent(typeof(Drag3D)) as Drag3D;
        // TODO Child ID needs to be revised
        d3d.Initialize(box, offsettedDragline, cubes.Count - 1);

        // Make it so there is always at least a very small gap in betwwen cubes
        go.GetComponent<BoxCollider>().size *= 1.05f;

    }

    public void UpdateColor()
    {
        if (selected)
        {
            shelf_mesh.GetComponent<Renderer>().material.color = new Color(0.4f, 1f, 0.8f);
        }
        else
        {
            shelf_mesh.GetComponent<Renderer>().material.color = Color.white;
        }
    }

    // What we want is to "flatten out" all the draglines in a shelf into a line representing the entire length of the dragable space 

    // Methods used by childs to check/clear collisions with other childs //
    public bool CubeIsColided(Bounds cubeMovedBounds, int insanceID)
    {
        bool colision_happened = false;
        for (int i = 0; i < cubes.Count; i++)
        {
            if (cubes[i].GetInstanceID() == insanceID) { continue; }

            if (cubes[i].GetComponent<BoxCollider>().bounds.Intersects(cubeMovedBounds))
            {
                // Update the other cube's color to show collision aswell.
                cubes[i].GetComponent<Drag3D>().collided_upon = true;
                cubes[i].GetComponent<Drag3D>().updateColor();
                colision_happened = true;
            }
            else
            {
                cubes[i].GetComponent<Drag3D>().collided_upon = false;
                cubes[i].GetComponent<Drag3D>().updateColor();
            }

        }
        return colision_happened;
    }

    public void clearCollision()
    {
        for (int i = 0; i < cubes.Count; i++)
        {
            cubes[i].GetComponent<Drag3D>().collided_upon = false;
            cubes[i].GetComponent<Drag3D>().updateColor();
        }
    }
}