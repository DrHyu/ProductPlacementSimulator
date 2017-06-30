using UnityEngine;
using System;

[Serializable]
public class StandGenerator : MonoBehaviour
{
    private Vector2[] wall;
    private GameObject[] wall_obj;

    public ShelfGenerator[] shelves;
    public StandJSON this_stand;
    public Vector3 move_increment;

    public bool selected = false;

    private bool initialized = false;

    private Material transparent_m;
    private Material nonTransparent_m;

    public int ViewMode = UItoSimulation.ALPHA_CHANGE;


    private void Start()
    {
        // Load materials just once;
        transparent_m    = Resources.Load("Materials/StandardTransparent", typeof(Material)) as Material;
        nonTransparent_m = Resources.Load("Materials/StandardNonTransparent", typeof(Material)) as Material;

        UpdateColor();

        move_increment = Vector3.zero;

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

        float current_height = 0;

        for (int i = 0; i < s.shelves.Length; i++)
        {
            GameObject g = new GameObject("shelf " + i);

            ShelfGenerator SHG = g.AddComponent(typeof(ShelfGenerator)) as ShelfGenerator;

            SHG.transform.SetParent(transform);

            current_height += s.shelves[i].relative_height;
            s.shelves[i].absolute_height = current_height;
            SHG.Initialize(s.shelves[i]);

            shelves[i] = SHG;

        }
        wall = new Vector2[s.wall_x.Length];
        for(int i =0; i < s.wall_x.Length; i++)
        {
            wall[i] = new Vector2(s.wall_x[i], s.wall_y[i]);
        }

        AddWalls();
        //UpdateColor();
     }

    private void AddWalls()
    {
        //Draw the backpanels
        //float lowest_y = this_stand.shelves[0].y_start;
        float lowest_y = 0;

        wall_obj = new GameObject[this_stand.shelves.Length];

        float highest_y = 0;

        for (int i = 0; i < this_stand.shelves.Length; i++)
        {
            highest_y += this_stand.shelves[i].relative_height;
        }

        if (wall != null)
        {
            for (int i = 0; i < wall.Length - 1; i++)
            {
                GameObject w = GameObject.CreatePrimitive(PrimitiveType.Cube);
                w.GetComponent<Renderer>().material.color = Color.white;
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

        if(move_increment != Vector3.zero)
        {
            //transform.localPosition += move_increment;
            this_stand.x_start += move_increment.x;
            this_stand.y_start += move_increment.y;
            this_stand.z_start += move_increment.z;
            move_increment = Vector3.zero;
        }

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

    public void UpdateColor()
    {
        if (selected)
        {
            // Set the Alpha to high on child objects
            SetAlphaRecursively(transform, 1f, nonTransparent_m);
        }
        else
        {
            if (ViewMode == UItoSimulation.ALPHA_CHANGE)
            {
                // Set the Alpha to low on child objects
                SetAlphaRecursively(transform, 0.1f, transparent_m);
            }
            else
            {
                // Set the Alpha to low on child objects
                SetAlphaRecursively(transform, 1f, nonTransparent_m);
            }

        }
    }

    private void SetAlphaRecursively(Transform t, float a, Material m)
    {
        foreach(Transform child in t)
        {
            SetAlphaRecursively(child, a, m);

            if (child.GetComponent<Renderer>() != null)
            {
                Color c = child.GetComponent<Renderer>().material.color;
                c.a = a;
                child.GetComponent<Renderer>().material = m;
                child.GetComponent<Renderer>().material.color = c;

            }
        }
    }
}