using UnityEngine;
using System.Collections;
using System;


[Serializable]
public class SceneData
{
    public StandJSON[] stands;

    public SceneData(StandJSON[] s)
    {
        stands = s;
    }
}

[Serializable]
public class StandJSON
{
    public string name = "Stand";

    public float x_start = 0;
    public float y_start = 0;
    public float z_start = 0;

    public float y_rotation = 0;

    public ShelfJSON[] shelves;

    public float[] wall_x;
    public float[] wall_y;

}

[Serializable]
public class ShelfJSON
{
    public string name = "Shelf";

    public float height;

    public float thickness = 0.2f;

    public int[] front_index;

    public float[] x_points;
    public float[] y_points;

    public BoxJSON[] boxes;
}

[Serializable]
public class BoxJSON
{
    public string name = "Box";

    // Dimensions of the product
    public float width;
    public float height;
    public float depth;

    public int current_index;
    public float current_pos_relative;

    public string texture_path = "";
    
}


