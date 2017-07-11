using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UItoSimulation : MonoBehaviour
{
    // The purpose of this class is to enable the commminication between the UI and the simulation. 
    // It stands in the middle and passes all messages. 

    public DBHandler dbh;
    private DB myDB;
    
    private List<StandGenerator> standList;

    private int stand_sel = 0;
    private int shelf_sel = 0;
    private bool[] product_sel;

    private int view_mode = ALPHA_CHANGE;

    // This method should be called everytime the UI has changed it's selection so the simulation can update the selection accordingly
    public void UISelectionChanged(int stand_index =0, int shelf_index =0, bool[] product_index = null)
    {
        //Clear any previously selected products
        if(product_sel != null)
        {
            for (int i = 0; i < product_sel.Length; i++)
            {
                standList[stand_sel].shelves[shelf_sel].cubes[i].GetComponent<ProductAesthetics>().SetSelected(false);
            }
        }

        // Clear any previously selected stands
        standList[stand_sel].selected = false;
        standList[stand_sel].UpdateColor();
        if(view_mode == ACTIVE_CHANGE)
        {
            standList[stand_sel].gameObject.SetActive(false);
        }

        // Clear any previously selected shelfs
        standList[stand_sel].shelves[shelf_sel].selected = false;
        standList[stand_sel].shelves[shelf_sel].UpdateColor();

        // Highlight the selected stand
        standList[stand_index].selected = true;
        standList[stand_index].UpdateColor();
        if (view_mode == ACTIVE_CHANGE)
        {
            standList[stand_index].gameObject.SetActive(true);
        }

        // Highlight the shelf if no product is selected
        standList[stand_index].shelves[shelf_index].selected = true;
        standList[stand_index].shelves[shelf_index].UpdateColor();


        // Highlight the newly selected products
        if (product_index != null)
        {
            for (int i = 0; i < product_index.Length; i++)
            {
                standList[stand_index].shelves[shelf_index].cubes[i].GetComponent<ProductAesthetics>().SetSelected(product_index[i]);
            }
        }

        stand_sel = stand_index;
        shelf_sel = shelf_index;
        product_sel = product_index;
    }

    public const int ALPHA_CHANGE  = 0;
    public const int NO_CHANGE     = 1;
    public const int ACTIVE_CHANGE = 2;

    public void ChangeViewMode(int v_mode)
    {
        view_mode = v_mode;
        switch (view_mode)
        {
            case ALPHA_CHANGE:
                // Make sure all the stands are active
                for(int i = 0; i < standList.Count; i++)
                {
                    standList[i].gameObject.SetActive(true);
                    standList[i].ViewMode = ALPHA_CHANGE;
                    standList[i].UpdateColor();
                }
                break;
            case NO_CHANGE:
                for(int i = 0; i < standList.Count; i++)
                {
                    standList[i].gameObject.SetActive(true);
                    standList[i].ViewMode = NO_CHANGE;
                    standList[i].UpdateColor();
                }
                break;
            case ACTIVE_CHANGE:
                for (int i = 0; i < standList.Count; i++)
                {
                    standList[i].gameObject.SetActive(i == stand_sel);
                }
                break;
            default:
                break;
        }
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
                standList[stand_i].shelves[shelf_i].productList.RemoveAt(p);
                p--;
            }
            p++;
        }
    }

    public void AddProduct(int stand_i, int shelf_i, BoxJSON b)
    {

        b.actual_width = b.width * b.x_repeats + ProductAesthetics.BOX_STACK_X_SPACING * b.x_repeats;
        b.actual_height = b.height * b.y_repeats + ProductAesthetics.BOX_STACK_Y_SPACING * b.y_repeats;
        b.actual_depth = b.depth * b.z_repeats + ProductAesthetics.BOX_STACK_Z_SPACING * b.z_repeats;

        if (b != null)
        {
            GameObject go = standList[stand_i].shelves[shelf_i].GenerateProduct(b);
            standList[stand_i].shelves[shelf_i].AttatchProduct(b,go);
        }
        else
        {
            Debug.LogError("Item not found in database!");
        }
    }

    public void Initialize(List<StandGenerator> sg)
    {
        standList = sg;
    }

    private void Start()
    {
        myDB = dbh.ReadFullDB();
    }

}
