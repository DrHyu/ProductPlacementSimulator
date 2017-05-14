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
    public ShelfJSON[] shelves;

    public float x_start;
    public float y_start;
    public float z_start;

    public float[] wall_x;
    public float[] wall_y;

    public string name;

    public StandJSON( ShelfJSON[] _shelves)
    {
        shelves = _shelves;
    }
}

[Serializable]
public class ShelfJSON
{
    public float x_start;
    public float y_start;
    public float z_start;

    public float[] x_points;
    public float[] y_points;

    // List of points that are in the front or back of the shelf 
    public int[] front_index;

    public float thickness;
}


