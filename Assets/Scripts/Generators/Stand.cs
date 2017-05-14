using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class Stand : MonoBehaviour
{

    public Vector2[] wall;

    public ShelfGenerator[] shelves;

    public StandJSON this_stand;


    public void Initialize(StandJSON s)
    {
        this_stand = s;

        transform.localPosition = new Vector3(s.x_start, s.y_start, s.z_start);


        shelves = new ShelfGenerator[s.shelves.Length];

        for (int i = 0; i < s.shelves.Length; i++)
        {
            GameObject g = new GameObject("shelf " + i);

            ShelfGenerator SHG = g.AddComponent(typeof(ShelfGenerator)) as ShelfGenerator;

            SHG.transform.SetParent(transform);
            SHG.Initialize(s.shelves[i], "shelf " + i);

            shelves[i] = SHG;

        }
        wall = new Vector2[s.wall_x.Length];
        for(int i =0; i < s.wall_x.Length; i++)
        {
            wall[i] = new Vector2(s.wall_x[i], s.wall_y[i]);
        }
        addWalls();

    }

    private void addWalls()
    {
        //Draw the backpanels
        //float lowest_y = this_stand.shelves[0].y_start;
        float lowest_y = 0;

        float highest_y = this_stand.shelves[this_stand.shelves.Length - 1].y_start;
        if (wall != null)
        {
            for (int i = 0; i < wall.Length - 1; i++)
            {
                GameObject w = GameObject.CreatePrimitive(PrimitiveType.Cube);

                Vector2 v = wall[i + 1] - wall[i];

                float magnitude = v.magnitude;
                float width = 0.1f;

                w.transform.parent = transform;

                w.GetComponent<Transform>().localPosition = new Vector3(wall[i].x + v.x / 2,
                                                                        (highest_y - lowest_y) / 2,
                                                                        wall[i].y + v.y / 2);

                float ang = Vector2.Angle(v, Vector2.right);

                if (v.x < 0 && v.y > 0)
                {
                    ang = ang * -1;
                }
                else if (v.x > 0 && v.y > 0)
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


    public override string ToString()
    {
        return name;
    }
}