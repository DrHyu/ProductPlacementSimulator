using UnityEngine;
using System.Collections.Generic;

public class SimulationToUI : MonoBehaviour
{

    public UIController _UIController;

    public List<StandGenerator> standList;

    //TODO Not the way it should work ... Initialize should asume a clean state
    public void Initialize(List<StandGenerator> standList)
    {

        this.standList = standList;
        if (standList != null)
        {
            foreach (StandGenerator sg in standList)
            {
                foreach (Transform sh in sg.transform)
                {
                    ShelfGenerator shg = sh.gameObject.GetComponent<ShelfGenerator>();
                    if (shg == null) { continue; }
                    shg.RegisterOnItemAttachedCallback(OnProductAddedToShelf);
                    shg.RegisterOnItemDeattachedCallback(OnProdcutRemovedFromShelf);
                    shg.RegisterOnShelfClickedCallback(OnShelfClicked);
                    shg.RegisterOnChildProductClickedCallback(OnProductClickedInSimulation);
                }
            }
        }
    }

    public int p = 0;

    public void OnProductClickedInSimulation(StandGenerator stand, ShelfGenerator shelf, bool[] selected)
    {
        int stand_index = standList.IndexOf(stand);
        int shelf_index = standList[stand_index].shelves.IndexOf(shelf);

        _UIController.UpdateUIState(stand_index, shelf_index, selected, true);
    }

    public void NotifyNewProductAdded(Drag3D new_product)
    {
       // new_product.RegisterOnClickCallback(ProductClickedInSimulation);
    }

    public void NotifyProductRemoved(Drag3D old_product)
    {
       // old_product.UnregisterOnClickCallback(ProductClickedInSimulation);
    }

    public void OnProdcutRemovedFromShelf(StandGenerator stand, ShelfGenerator shelf, Drag3D cube, bool[] selected)
    {
        // When a product is added or removed from a shelf we should update the UI 
        // To avoid any (out of bounds) errors and dislpaying incorrect information

        if(stand == _UIController.standList[_UIController.stand_dropdown_index] &&
                shelf == _UIController.standList[_UIController.stand_dropdown_index].shelves[_UIController.shelf_dropdown_index])
        {
            _UIController.UpdateUIState(_UIController.stand_dropdown_index, _UIController.shelf_dropdown_index, selected, true, true);
        }
    }

    public void OnProductAddedToShelf(StandGenerator stand, ShelfGenerator shelf, Drag3D cube, bool[] selected)
    {
        //if (stand == _UIController.standList[_UIController.stand_dropdown_index] &&
        //shelf == _UIController.standList[_UIController.stand_dropdown_index].shelves[_UIController.shelf_dropdown_index])
        {
            int stand_idx = _UIController.standList.IndexOf(stand);
            int shelf_idx = _UIController.standList[stand_idx].shelves.IndexOf(shelf);

            //int len = _UIController.standList[stand_idx].shelves[shelf_idx].cubes.Count;
            //bool[] sel = UIController.GetSelectedArray( new int[]{len-1}, len);

            _UIController.UpdateUIState(stand_idx, shelf_idx, selected, true, true);
        }
    }

    public void OnShelfClicked(ShelfGenerator shg, StandGenerator stg)
    {
        int standIndex = _UIController.standList.IndexOf(stg);
        int shelfIndex = _UIController.standList[standIndex].shelves.IndexOf(shg);

        int len = _UIController.standList[standIndex].shelves[shelfIndex].cubes.Count;
        bool[] sel = UIController.GetSelectedArray(new int[] {}, len);

        _UIController.UpdateUIState(standIndex, shelfIndex, sel, true, false);
    }
}