using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BezierLines 
{



    public BezierLines(Vector2[] _v, int _order, int _resolution)
    {

    }


    public static Vector2[] doBezier(Vector2[] v, int _order, int _resolution)
    {

        List<Vector2> result = new List<Vector2>();

        Ray2D[] r1 = new Ray2D[_order];
        Ray2D[] r2 = new Ray2D[_order];

        Vector2[] v1 = new Vector2[_order+1];
        Vector2[] v2 = new Vector2[_order+1];

        for (int i = 0; i < v.Length - _order; i+= _order)
        {
            for (int x = 0; x < _resolution; x++)
            {
                // Calculate for the initial "order" iteration
                for (int o = 0; o < _order; o++)
                {
                    r1[o] = new Ray2D(v[o + i], v[o + i + 1] - v[o]);
                }
                for (int o = 0; o < _order + 1; o++)
                {
                    v1[o] = v[i + o];
                }


                for (int order = _order; order > 1; order--)
                {
                    for (int p = 0; p < order; p++)
                    {
                        v2[p] = r1[p].GetPoint((v1[p + 1] - v1[p]).magnitude * x / _resolution);
                    }
                    for (int p = 0; p < order - 1; p++)
                    {
                        r2[p] = new Ray2D(v2[p], v2[p + 1] - v2[p]);
                    }

                    //Clean up for next iteration
                    for (int p = 0; p < order; p++) { v1[p] = v2[p]; }
                    for (int p = 0; p < order - 1; p++) { r1[p] = r2[p]; }
                }

                result.Add(v[0]);
            }
        }

        return result.ToArray();
        
    }

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
