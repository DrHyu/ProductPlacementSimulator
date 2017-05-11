using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StandfGenerator : MonoBehaviour {


    // Hieratchy
    // Stand (Sets the x_start and y_start, rest should have 0,y,0 as localpos)
    //      Shelf1
    //            Mesh
    //            Cubes(with draglines)
    //      Shelf2
    //            Mesh
    //            Cubes(with draglines)
    //      Wall1
    //      Wall2

    public Shelf[] mShelves;

    private ShelfJSON mJSONData;

    public float thickness = 0.5f;

    public Vector2[] mesh_points;

    public void initialize(ShelfJSON s)
    {
        mJSONData = s;
        MeshGenerator meshGen = new MeshGenerator(s.x_points, s.y_points);
        Mesh msh = meshGen.get3DMeshFrom2D(-thickness);

        mesh_points = meshGen.v;

        transform.localPosition = new Vector3(s.x_start, 0, s.z_start);

        mShelves = new Shelf[s.n_levels];

        for(int i =0; i < s.n_levels; i++)
        {
            mShelves[i] = new Shelf(0, s.y_start + i* s.lvl_offset, 0, s.x_points, s.y_points, s.front_index);

            GameObject g = new GameObject("Shelf_" + i);

            // Register this GameObject as the parent
            g.transform.parent = transform;

            ShelfGenerator SG =  g.AddComponent<ShelfGenerator>();
            SG.Initialize(mShelves[i], msh);
        }
    }

    private void Start()
    {
        //Draw the backpanels
        float lowest_y = mShelves[0].y_start;
        float highest_y = mShelves[mShelves.Length - 1].y_start + mJSONData.lvl_offset;
        if (mJSONData.back_index != null)
        {


            for (int i = 0; i < mJSONData.back_index.Length - 1; i++)
            {
                GameObject w = GameObject.CreatePrimitive(PrimitiveType.Cube);

                int ind1 = mJSONData.back_index[i + 1];
                int ind2 = mJSONData.back_index[i];

                Vector2 v = new Vector2(mJSONData.x_points[ind2] - mJSONData.x_points[ind1], mJSONData.y_points[ind2] - mJSONData.y_points[ind1]);


                float magnitude = v.magnitude;


                float width = 0.1f;

                w.transform.parent = transform;

                w.GetComponent<Transform>().localPosition = new Vector3(mJSONData.x_points[ind1] + v.x / 2,
                                                                   (highest_y - lowest_y) / 2,
                                                                   mJSONData.y_points[ind1] + v.y / 2);


                //float ang = Mathf.Atan(v.y / v.x) * (180.0f / Mathf.PI);
                float ang = Vector2.Angle(v, Vector2.right);

                if(v.x < 0 && v.y > 0)
                {
                    ang = ang * -1;
                }
                else if(v.x > 0 && v.y > 0)
                {
                    ang = ang * -1;
                }

                w.GetComponent<Transform>().localRotation = Quaternion.AngleAxis(ang, Vector3.up);

                w.GetComponent<Transform>().localScale = new Vector3(magnitude,
                                                                    highest_y - lowest_y,
                                                                   width);

            }
        }
    }

    private void Update()
    {
        
    }
}
