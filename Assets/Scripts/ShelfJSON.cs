using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class ShelfJSON : Shelf {

    public int n_levels = 1;
    public float lvl_offset = 1.0f;


    public ShelfJSON(float _x, float _y, float _z, float[] _x_points, float[] _y_points, int _n_levels, float _lvl_offset, int[] _front_index = null) :
                     base (_x,_y,_z,_x_points,_y_points,_front_index)
    {

        n_levels = _n_levels;
        lvl_offset = _lvl_offset;
    }
}
