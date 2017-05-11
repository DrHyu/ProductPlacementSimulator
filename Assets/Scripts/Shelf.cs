using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


[Serializable]
public class Shelf {

    public float x_start;
    public float y_start;
    public float z_start;

    public float[] x_points;
    public float[] y_points;

    // List of points that are in the front or back of the shelf 
    public int[] front_index;
    public int[] back_index;

    public Shelf (float _x, float _y, float _z, float[] _x_points, float[] _y_points, int[] _front_index = null)
    {
        x_start = _x;
        y_start = _y;
        z_start = _z;

        x_points = _x_points;
        y_points = _y_points;

        front_index = _front_index;
    }
}
