using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;

[Serializable]
public class StandGenerator : MonoBehaviour
{

    private Vector2[] wall;
    private GameObject[] wall_obj;

    public ShelfGenerator[] shelves;

    public StandJSON this_stand;

    private bool initialized = false;

    private void Start()
    {
        transform.parent.gameObject.GetComponent<SceneGenerator>().RegisterChild(this);

        if (!initialized)
        {
            Initialize();
        }
    }

    public void Initialize()
    {
        Initialize(this_stand);
    }

    public void Initialize(StandJSON s)
    {
        initialized = true;
        this_stand = s;

        transform.localPosition = Vector3.zero;

        transform.localRotation = Quaternion.identity;
        transform.RotateAround(FindStandCenter().to3DwY(0), Vector3.up, s.y_rotation);

        transform.localPosition += new Vector3(s.x_start, s.y_start, s.z_start);

        shelves = new ShelfGenerator[s.shelves.Length];

        for (int i = 0; i < s.shelves.Length; i++)
        {
            GameObject g = new GameObject("shelf " + i);

            ShelfGenerator SHG = g.AddComponent(typeof(ShelfGenerator)) as ShelfGenerator;

            SHG.transform.SetParent(transform);
            SHG.Initialize(s.shelves[i]);

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

        wall_obj = new GameObject[this_stand.shelves.Length];

        float highest_y = this_stand.shelves[this_stand.shelves.Length - 1].height;
        if (wall != null)
        {
            for (int i = 0; i < wall.Length - 1; i++)
            {
                GameObject w = GameObject.CreatePrimitive(PrimitiveType.Cube);
                w.name = "Wall " + i;

                Vector2 v = wall[i + 1] - wall[i];

                float magnitude = v.magnitude;
                float width = 0.01f;

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
                wall_obj[i] = w;
            }
        }
    }

    private void OnValidate()
    {
        // Redraw evcerything
        if (wall_obj != null)
        {
            for (int i = 0; i < wall_obj.Length; i++)
            {
                GameObject.Destroy(wall_obj[i]);
            }
            wall = null;
            wall_obj = null;
        }

        if (shelves != null)
        {
            for (int i = 0; i < shelves.Length; i++)
            {
                GameObject.Destroy(shelves[i].gameObject);
            }
            shelves = null;
        }
        Initialize();
    }

    public override string ToString()
    {
        return name;
    }

    public Vector2 FindStandCenter()
    {
        float biggest_x = 0;
        float smallest_x = 999999999;
        float biggest_y = 0;
        float smallest_y = 999999999;

        for (int i =0; i < this_stand.shelves[0].x_points.Length; i++)
        {
            if(this_stand.shelves[0].x_points[i] > biggest_x)
            {
                biggest_x = this_stand.shelves[0].x_points[i];
            }
            if (this_stand.shelves[0].x_points[i] < smallest_x)
            {
                smallest_x = this_stand.shelves[0].x_points[i];
            }
            if (this_stand.shelves[0].y_points[i] > biggest_y)
            {
                biggest_y = this_stand.shelves[0].y_points[i];
            }
            if (this_stand.shelves[0].y_points[i] < smallest_y)
            {
                smallest_y = this_stand.shelves[0].y_points[i];
            }
        }

        float width = biggest_x - smallest_x;
        float height = biggest_y - smallest_y;

        return new Vector2(smallest_x + width / 2, smallest_y + height / 2);
    }
}