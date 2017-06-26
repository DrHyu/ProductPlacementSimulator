using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UItoSimulation : MonoBehaviour
{
    // The purpose of this class is to enable the commminication between the UI and the simulation. 
    // It stands in the middle and passes all messages. 
    
    private List<StandGenerator> standList;

    private int stand_sel = 0;
    private int shelf_sel = 0;
    private bool[] product_sel;


    // This method should be called everytime the UI has changed it's selection so the simulation can update the selection accordingly
    public void UISelectionChanged(int stand_index =0, int shelf_index =0, bool[] product_index = null)
    {
        //Clear any previously selected products
        if(product_sel != null)
        {
            for (int i = 0; i < product_sel.Length; i++)
            {
                standList[stand_sel].shelves[shelf_sel].cubes[i].GetComponent<Drag3D>().selected = false;
                standList[stand_sel].shelves[shelf_sel].cubes[i].GetComponent<Drag3D>().updateColor();
            }
        }

        // Clear any previously selected stands

        // Clear any previously selected shelfs
        standList[stand_sel].shelves[shelf_sel].selected = false;
        standList[stand_sel].shelves[shelf_sel].UpdateColor();


        // Highlight the shelf if no product is selected
        standList[stand_index].shelves[shelf_index].selected = true;
        standList[stand_index].shelves[shelf_index].UpdateColor();


        // Highlight the newly selected products
        if (product_index != null)
        {
            for (int i = 0; i < product_index.Length; i++)
            {
                standList[stand_index].shelves[shelf_index].cubes[i].GetComponent<Drag3D>().selected = product_index[i];
                standList[stand_index].shelves[shelf_index].cubes[i].GetComponent<Drag3D>().updateColor();
            }
        }

        stand_sel = stand_index;
        shelf_sel = shelf_index;
        product_sel = product_index;
    }
    
    
    public void RemoveProducts(int stand_i, int shelf_i, bool[] to_remove)
    {
        int p = 0;
        for (int i = 0; i < to_remove.Length; i++)
        {   
            if (to_remove[i])
            {
                GameObject.Destroy(standList[stand_i].shelves[shelf_i].cubes[p]);
                standList[stand_i].shelves[shelf_i].cubes.RemoveAt(p);
                p--;
            }
            p++;
        }
    }

    public void Initialize(List<StandGenerator> sg)
    {
        standList = sg;
    }
    

}
