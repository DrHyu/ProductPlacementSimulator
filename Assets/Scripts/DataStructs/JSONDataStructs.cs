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

    [ShowOnly]
    public float absolute_height;
    //Relative to the previous shelf in the same stand
    public float relative_height; 

    public float thickness = 0.2f;

    public int[] front_index;

    public float[] x_points;
    public float[] y_points;

    public BoxJSON[] boxes;
}

[Serializable]
public class BoxJSON : DBItem
{
    public BoxJSON() { }

    public BoxJSON(DBItem ref_item)
    {
        this.width = ref_item.width;
        this.height = ref_item.height;
        this.depth = ref_item.depth;

        this.name = ref_item.name;
        this.img_path = ref_item.img_path;

        this.actual_width = ref_item.width;
        this.actual_height = ref_item.height;
        this.actual_depth = ref_item.depth;
    }

    public BoxJSON Copy()
    {
        BoxJSON copy = new BoxJSON();

        copy.width = this.width;
        copy.height = this.height;
        copy.depth = this.depth;

        copy.name = this.name;
        copy.img_path = this.img_path;

        copy.actual_width = this.actual_width;
        copy.actual_height = this.actual_height;
        copy.actual_depth = this.actual_depth;

        copy.x_repeats = this.x_repeats;
        copy.y_repeats = this.y_repeats;
        copy.z_repeats = this.z_repeats;

        return copy;
    }

    public float actual_width;
    public float actual_height;
    public float actual_depth;

    public int x_repeats = 1;
    public int y_repeats = 1;
    public int z_repeats = 1;


    public int cir;
    public float cpr;

    public int cil;
    public float cpl;
}


