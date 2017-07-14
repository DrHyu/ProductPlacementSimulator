﻿using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

[Serializable]
public class ShelfGenerator : MonoBehaviour
{

    public int n_cubes;
    public List<Drag3D> cubes;
    public List<BoxJSON> cubesJSON;
    Dictionary<int, GameObject> id2cube;

    public ShelfJSON this_shelf;
    private GameObject shelf_mesh;

    public bool initialized = false;
    public Boolean selected = false;

    private Vector3[] offsettedDragline;

    public CollisionMap tempCollisionMap;    // Debugging

    public void Initialize(ShelfJSON s)
    {
        onItemAttachedCallBacks = new List<OnItemAttachedCallback>();
        onItemDeattachedCallBacks = new List<OnItemDeattachedCallback>();

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

        shelf_mesh.GetComponent<Renderer>().material = Resources.Load("Materials/StandardTransparent", typeof(Material)) as Material;
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
            dragline[a] = new Vector3(s.x_points[s.front_index[a]], 0, s.y_points[s.front_index[a]]);
        }

        int[] vertexR;
        offsettedDragline = Drag3D.CalculateDragLines(dragline, 0.2f, out vertexR, false);

        cubes       = new List<Drag3D>();
        cubesJSON = new List<BoxJSON>();
        id2cube     = new Dictionary<int, GameObject>();

        if (this_shelf.boxes != null)
        {
            for (int p = 0; p < this_shelf.boxes.Length; p++)
            {
                GameObject new_cube = GenerateProduct(this_shelf.boxes[p], offsettedDragline);
                new_cube.transform.SetParent(this.transform);
                AttatchProduct(this_shelf.boxes[p], new_cube);
            }
        }

        initialized = true;
    }

    // Generation of products //

    public GameObject GenerateProduct(BoxJSON box)
    {
        return GenerateProduct(box, offsettedDragline);
    }

    public static GameObject GenerateProduct(BoxJSON box, Vector3[] shelf_draglines)
    {
        GameObject gocube;
        if (box.x_repeats == 1 && box.y_repeats == 1 && box.z_repeats == 1)
        {
            gocube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            gocube.transform.localScale = new Vector3(box.width, box.height, box.depth);

            Drag3D d3d = gocube.AddComponent(typeof(Drag3D)) as Drag3D;
            ProductAesthetics pa = gocube.AddComponent<ProductAesthetics>();

            d3d.Initialize(box, shelf_draglines);

            // Make it so there is always at least a very small gap in betwwen cubes
            gocube.GetComponent<BoxCollider>().size *= 1.05f;

            pa.Initialize(box, d3d);
            gocube.name = box.name;
        }
        else
        {
            gocube = new GameObject();
            List<GameObject> gos = new List<GameObject>();



            //Vector3 final_size = new Vector3(box.width * box.x_repeats + ProductAesthetics.BOX_STACK_X_SPACING * box.x_repeats,
            //                                box.height * box.y_repeats + ProductAesthetics.BOX_STACK_Y_SPACING * box.y_repeats, 
            //                                box.depth * box.z_repeats + ProductAesthetics.BOX_STACK_Z_SPACING * box.z_repeats);

            Vector3 final_size = new Vector3(box.actual_width, box.actual_height, box.actual_depth);
            Vector3 original_size = new Vector3(box.width, box.height, box.depth);

            //box.actual_width = final_size.x;
            //box.actual_height = final_size.y;
            //box.actual_depth = final_size.z;

            for (int x = 0; x < box.x_repeats; x++)
            {
                for (int y = 0; y < box.y_repeats; y++)
                {
                    for (int z = 0; z < box.z_repeats; z++)
                    {
                        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        go.transform.localPosition = new Vector3(box.width * x + ProductAesthetics.BOX_STACK_X_SPACING * x,
                                                                    box.height * y + ProductAesthetics.BOX_STACK_Y_SPACING * y,
                                                                    box.depth * z + ProductAesthetics.BOX_STACK_Y_SPACING * z) - final_size / 2 + original_size / 2;
                        go.transform.localScale = original_size;
                        go.transform.SetParent(gocube.transform);
                        go.GetComponent<BoxCollider>().enabled = false;
                        gos.Add(go);
                    }
                }
            }


            Drag3D d3d = gocube.AddComponent(typeof(Drag3D)) as Drag3D;
            ProductAesthetics pae = gocube.AddComponent<ProductAesthetics>();

            d3d.Initialize(box, shelf_draglines);

            // Make it so there is always at least a very small gap in betwwen cubes
            //gocube.GetComponent<BoxCollider>().size *= 1.05f;

            foreach (GameObject go in gos)
            {
                ProductAesthetics pa = go.AddComponent<ProductAesthetics>();
                pa.Initialize(box, d3d);
            }

            pae.InitializeAsGroupController(box, d3d);

            gocube.AddComponent<BoxCollider>();
            
            //Bounds b = new Bounds();

            //foreach (GameObject go in gos)
            //{
            //    b.Encapsulate(go.GetComponent<BoxCollider>().bounds);
            //}
            ////FitToChildren(gocube);

            //gocube.GetComponent<BoxCollider>().center = final_size / 2 - new Vector3(box.width, box.height, box.depth)/2;
            gocube.GetComponent<BoxCollider>().center = Vector3.zero;
            gocube.GetComponent<BoxCollider>().size = final_size;
            gocube.transform.localScale = Vector3.one;
            gocube.name = box.name;


        }
        return gocube;
    }

    // Collision maps related methods //

    public void InvalidateChildCollisionMaps()
    {
        foreach (Drag3D go in cubes)
        {
            go.cm = null;
        }
    }

    public void InvalideChildCollisionMaps(int exceptID)
    {
        foreach (Drag3D go in cubes)
        {
            if (go.GetInstanceID() != exceptID)
            {
                go.cm = null;
            }
        }
    }

    public void NotifyCollision(int[] IDs, int srcID)
    {
        foreach (int id in IDs)
        {
            id2cube[id].GetComponent<Drag3D>().collided_upon = true;
            id2cube[id].GetComponent<Drag3D>().ExecOnMyCollisionEnterCallbacks();
        }
    }

    public void ClearCollision()
    {
        for (int i = 0; i < cubes.Count; i++)
        {
            cubes[i].GetComponent<Drag3D>().collided_upon = false;
            cubes[i].GetComponent<Drag3D>().ExecOnMyCollisionExitCallbacks();
        }
    }

    // Attatching/de-attatching products to this shelf //

    public void AttatchProduct(BoxJSON b, GameObject cube)
    {
        cube.transform.SetParent(this.transform);
        cubes.Add(cube.GetComponent<Drag3D>());
        cubesJSON.Add(b);
        id2cube.Add(cube.GetInstanceID(), cube);
        InvalidateChildCollisionMaps();


        // Only need to update the UI for the products added after Start() has been executed 
        if(initialized == true)
        {
            ExecOnItemAttachedCallbacks(transform.parent.GetComponent<StandGenerator>(), this, cube.GetComponent<Drag3D>());
        }
    }


    public void DeattachProduct(Drag3D leaving_product, bool trigger_callback = false)
    {
        InvalidateChildCollisionMaps();
        cubes.Remove(leaving_product);
        cubesJSON.Remove(leaving_product.this_box);
        id2cube.Remove(leaving_product.gameObject.GetInstanceID());

        if (trigger_callback)
            ExecOnItemDeattachedCallbacks(transform.parent.GetComponent<StandGenerator>(), this, leaving_product);
    }

    // Callbacks //

    public delegate void OnItemDeattachedCallback(StandGenerator stand, ShelfGenerator shelf, Drag3D cube);
    private List<OnItemDeattachedCallback> onItemDeattachedCallBacks;
    public void RegisterOnItemDeattachedCallback(OnItemDeattachedCallback f)
    {
        onItemDeattachedCallBacks.Add(f);
    }
    public void UnRegisterOnItemDeattachedCallback(OnItemDeattachedCallback f)
    {
        onItemDeattachedCallBacks.Remove(f);
    }
    public void ExecOnItemDeattachedCallbacks(StandGenerator stand, ShelfGenerator shelf, Drag3D cube)
    {
        foreach (OnItemDeattachedCallback f in onItemDeattachedCallBacks) { f(stand, shelf, cube); }
    }

    public delegate void OnItemAttachedCallback(StandGenerator stand, ShelfGenerator shelf, Drag3D cube);
    private List<OnItemAttachedCallback> onItemAttachedCallBacks;
    public void RegisterOnItemAttachedCallback(OnItemAttachedCallback f)
    {
        onItemAttachedCallBacks.Add(f);
    }
    public void UnRegisterOnItemAttachedCallback(OnItemAttachedCallback f)
    {
        onItemAttachedCallBacks.Remove(f);
    }
    public void ExecOnItemAttachedCallbacks(StandGenerator stand, ShelfGenerator shelf, Drag3D cube)
    {
        foreach (OnItemAttachedCallback f in onItemAttachedCallBacks) { f(stand, shelf, cube); }
    }


    // Others/Misc //

    public void UpdateColor()
    {
        float alpha = shelf_mesh.GetComponent<Renderer>().material.color.a;
        if (selected)
        {
            shelf_mesh.GetComponent<MeshRenderer>().material = Resources.Load("Materials/StandardNonTransparent", typeof(Material)) as Material;
            shelf_mesh.GetComponent<MeshRenderer>().material.color = new Color(0.4f, 1f, 0.8f, alpha);
        }
        else
        {
            shelf_mesh.GetComponent<MeshRenderer>().material = Resources.Load("Materials/StandardTransparent", typeof(Material)) as Material;
            shelf_mesh.GetComponent<MeshRenderer>().material.color = new Color(1f, 1f, 1f, alpha);
        }
    }

    private void OnDrawGizmos()
    {

        Gizmos.matrix = transform.localToWorldMatrix;

        Gizmos.color = Color.red;

        if (tempCollisionMap != null)
        {
            for (int i = 0; i < tempCollisionMap.perNodeCollision.Length - 1; i++)
            {
                for (int p = 0; p < tempCollisionMap.perNodeCollision[i].Count; p++)
                {

                    CollisionBucket cb = tempCollisionMap.perNodeCollision[i][p];

                    if (cb == null || cb.left == null || cb.right == null) { continue; }

                    Vector3 sStart = tempCollisionMap.mDraglines[i];
                    Vector3 sDir = (tempCollisionMap.mDraglines[i + 1] - sStart);
                    Vector3 sPerp = (new Vector3(sDir.z, sDir.y, -sDir.x)).normalized;

                    // Need to find the 4 poins that define the area of the collision

                    Vector3 c00 = sStart + (tempCollisionMap.resolution * (p + 0.5f)) * sDir.normalized;
                    Vector3 c01 = c00 + cb.right.height * sPerp.normalized;

                    if (cb.right.height < 30)
                    {
                        Gizmos.DrawLine(c00, c01);
                    }

                    c01 = c00 + cb.left.height * -sPerp.normalized;

                    if (cb.left.height < 30)
                    {
                        Gizmos.DrawLine(c00, c01);
                    }
                }
            }

        }
    }



}
