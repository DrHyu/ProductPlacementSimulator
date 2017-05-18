using UnityEngine;
using System.Collections;
using System;


[Serializable]
public class SceneData
{
    public StandJSON[] stands;

    public SceneData(StandJSON[] _stands)
    {
        stands = _stands;
    }
}

[Serializable]
public class StandJSON
{
    public ShelfJSON shelf;

    public float[] shelf_heights;

    public float x_start = 0;
    public float y_start = 0;
    public float z_start = 0;

    public float y_rotation = 0;

    public float[] wall_x;
    public float[] wall_y;

    public string name;
}

[Serializable]
public class ShelfJSON
{
    public float x_start = 0;
    public float y_start = 0;
    public float z_start = 0;

    public float[] x_points;
    public float[] y_points;

    // List of points that are in the front or back of the shelf 
    public int[] front_index;

    public float thickness = 1f;
}


