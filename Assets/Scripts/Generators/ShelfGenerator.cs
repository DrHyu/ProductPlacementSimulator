﻿using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;

[Serializable]
public class ShelfGenerator : MonoBehaviour
{
    public int n_cubes;
    public List<GameObject> cubes;

    public ShelfJSON this_shelf;

    private void Start()
    {
        
    }

    public void Initialize(ShelfJSON s)
    {
        name = s.name;

        this_shelf = s;

        transform.localPosition = new Vector3(0,s.height,0);
        transform.localRotation = Quaternion.identity;

        // Generate new mesh game object
        GameObject mesh_object = new GameObject("mesh");

        // Calculate the mesh from the raw data
        MeshGenerator meshGen = new MeshGenerator(s.x_points, s.y_points);
        Mesh msh = meshGen.get3DMeshFrom2D(-s.thickness);

        // Render the mesh
        mesh_object.AddComponent(typeof(MeshRenderer));
        MeshFilter meshRenderer = mesh_object.AddComponent(typeof(MeshFilter)) as MeshFilter;
        meshRenderer.mesh = msh;

        mesh_object.GetComponent<Renderer>().material.color = Color.white;
        mesh_object.GetComponent<Transform>().localScale = new Vector3(1, 1, 1);
        mesh_object.GetComponent<Transform>().SetParent(transform);
        mesh_object.GetComponent<Transform>().localPosition = new Vector3(0, 0, 0);
        mesh_object.GetComponent<Transform>().localRotation = Quaternion.identity;

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

        // Make some dummy boxes if none available
        if(this_shelf.boxes == null || this_shelf.boxes.Length == 0)
        {
            // Add the products to the shelf
            n_cubes = 1 + (int)(UnityEngine.Random.value * 5);
            this_shelf.boxes = new BoxJSON[n_cubes];

            for (int p = 0; p < n_cubes; p++)
            {
                this_shelf.boxes[p] = new BoxJSON();
                this_shelf.boxes[p].current_index = 0;
                this_shelf.boxes[p].current_pos_relative = 0;
                this_shelf.boxes[p].width = 0.5f + UnityEngine.Random.value * 2f;
                this_shelf.boxes[p].height = 0.5f + UnityEngine.Random.value * 2f;
                this_shelf.boxes[p].depth = 0.5f + UnityEngine.Random.value * 2f;
            }
        }

        cubes = new List<GameObject>();

        for (int p = 0; p < this_shelf.boxes.Length; p++)
        {
            cubes.Add(GameObject.CreatePrimitive(PrimitiveType.Cube));
            cubes[p].transform.SetParent(transform);

            Drag3D d3d = cubes[p].AddComponent(typeof(Drag3D)) as Drag3D;
            d3d.Initialize(this_shelf.boxes[p], offsettedDragline,p);

            // Make it so there is always at least a very small gap inw betwwen cubes
            cubes[p].GetComponent<BoxCollider>().size *= 1.05f;
        }
        
    }

    // Methods used by childs to check/clear collisions with other childs //
    public bool cubeIsColided(int cubeMoved)
    {
        Bounds cubeMovedBounds = cubes[cubeMoved].GetComponent<BoxCollider>().bounds;

        bool colision_happened = false;
        for (int i = 0; i < cubes.Count; i++)
        {
            if (i == cubeMoved) { continue; }

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