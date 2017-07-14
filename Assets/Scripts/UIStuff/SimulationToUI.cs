using UnityEngine;
using System.Collections.Generic;

public class SimulationToUI : MonoBehaviour
{

    public UIController _UIController;

    public List<StandGenerator> standList;

    //TODO Not the way it should work ... Initialize should asume a clean state
    public void Initialize(List<StandGenerator> standList)
    {
        if (standList != null)
        {
            foreach (StandGenerator sg in standList)
            {
                if(sg != null)
                {
                    foreach (Transform sh in sg.transform)
                    {
                        if(sh != null)
                        {
                            foreach (Transform box in sh.transform)
                            {
                                Drag3D d3d= box.gameObject.GetComponent<Drag3D>();
                                if (d3d != null) { d3d.UnregisterOnClickCallback(ProductClickedInSimulation); }
                            }
                        }
                    }
                }
            }
        }
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

                    foreach (Transform box in sh.transform)
                    {
                        Drag3D d3d = box.gameObject.GetComponent<Drag3D>();
                        if (d3d != null) { d3d.RegisterOnClickCallback(ProductClickedInSimulation); }
                    }
                }
            }
        }
    }

    public int p = 0;

    public void ProductClickedInSimulation(StandGenerator stand , ShelfGenerator shelf, Drag3D box)
    {
        int stand_index = standList.IndexOf(stand);
        int shelf_index = standList[stand_index].shelves.IndexOf(shelf);
        int box_index = standList[stand_index].shelves[shelf_index].cubes.IndexOf(box);

        bool[] sel = UIController.GetSelectedArray(new int[] { box_index }, standList[stand_index].shelves[shelf_index].cubes.Count);

        _UIController.UpdateUIState(stand_index, shelf_index, sel, true);
    }

    public void NotifyNewProductAdded(Drag3D new_product)
    {
        new_product.RegisterOnClickCallback(ProductClickedInSimulation);
    }

    public void NotifyProductRemoved(Drag3D old_product)
    {
        old_product.UnregisterOnClickCallback(ProductClickedInSimulation);
    }

    public void OnProdcutRemovedFromShelf(StandGenerator stand, ShelfGenerator shelf, Drag3D cube)
    {
        // When a product is added or removed from a shelf we should update the UI 
        // To avoid any (out of bounds) errors and dislpaying incorrect information

        if(stand == _UIController.standList[_UIController.stand_dropdown_index] &&
                shelf == _UIController.standList[_UIController.stand_dropdown_index].shelves[_UIController.shelf_dropdown_index])
        {
            _UIController.UpdateUIState(_UIController.stand_dropdown_index, _UIController.shelf_dropdown_index, null, true, true);
        }
    }

    public void OnProductAddedToShelf(StandGenerator stand, ShelfGenerator shelf, Drag3D cube)
    {
        if (stand == _UIController.standList[_UIController.stand_dropdown_index] &&
        shelf == _UIController.standList[_UIController.stand_dropdown_index].shelves[_UIController.shelf_dropdown_index])
        {
            int len = _UIController.standList[_UIController.stand_dropdown_index].shelves[_UIController.shelf_dropdown_index].cubes.Count;

            bool[] sel = UIController.GetSelectedArray( new int[]{len-1}, len);
            _UIController.UpdateUIState(_UIController.stand_dropdown_index, _UIController.shelf_dropdown_index, sel, true, true);
        }
    }
}