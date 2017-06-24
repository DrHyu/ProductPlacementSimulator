using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour {

    public Dropdown standDropDown;
    public Dropdown shelfDropDown;
    public TextScrollView textscrollView;
    public Button addButton;
    public Button removeButton;

    public UItoSimulation _UItoSimulation;

    private List<StandGenerator> standList;
    private List<string> standNames;    
    private int stand_dropdown_index = 0;
    private List<string> shelfNames;
    private int shelf_dropdown_index = 0;

    private List<string> boxNames;
    public int boxIndex = -1;
    public bool[] boxIndexes;

    private bool initialized = false;

    public void Initialize()
    {

        standNames = new List<string>();
        for (int i = 0; i < standList.Count; i++)
        {
            standNames.Add(standList[i].ToString());
        }

        textscrollView.RegisterSelectedChangedCallback(BoxSlectedIndexChanged);
        addButton.GetComponent<ButtonClickCallback>().RegisterClickCallback(OnAddButtonPressed);
        removeButton.GetComponent<ButtonClickCallback>().RegisterClickCallback(OnRemoveButoonPressed);

        
        UpdateUIState(0, 0);
        initialized = true;
    }

    public void SetStandList(List<StandGenerator> _standList)
    {
        standList = _standList;
        initialized = false;
        _UItoSimulation.Initialize(_standList);
        Initialize();
    }

    public void StandDropDownIndexChanged(int index)
    {
        UpdateUIState(index);
    }

    public void ShelfDropDownIndexChanged(int index)
    {
        UpdateUIState(stand_dropdown_index, index);
    }

    public void BoxSlectedIndexChanged(bool[] selected)
    {
        boxIndexes = selected;

        _UItoSimulation.UISelectionChanged(stand_dropdown_index, shelf_dropdown_index, selected);
    }

    public void OnAddButtonPressed()
    {
        
    }

    public void OnRemoveButoonPressed()
    {
        if (standList[stand_dropdown_index].shelves[shelf_dropdown_index].cubes[boxIndex] != null )
        {
            GameObject.Destroy(standList[stand_dropdown_index].shelves[shelf_dropdown_index].cubes[boxIndex]);

            standList[stand_dropdown_index].shelves[shelf_dropdown_index].cubes.RemoveAt(boxIndex);
            boxIndex--;

            // Clear and fill up again the txt scroll view
            // Keep the same id 
            updateBoxLister(false);
        }
    }

    private void updateStandDropDown()
    {

    }

    private void updateBoxLister(bool resetIndex = true)
    {
        if (resetIndex) {
            boxIndex = -1;
        }



    }

    // Product select UI includes: stand dropdown, shelf dropdown and product selection text scrollview
    private void UpdateUIState(int stand_index = 0 , int shelf_index = 0, bool[] products_selected = null)
    {

        bool stand_index_changed = stand_index != stand_dropdown_index;
        bool shelf_index_changed = shelf_index != shelf_dropdown_index;


        _UItoSimulation.UISelectionChanged(stand_index, shelf_index, products_selected);

        if (!initialized)
        {
            standDropDown.ClearOptions();
            standDropDown.AddOptions(standNames);
            standDropDown.value = stand_index;
        }
        stand_dropdown_index = stand_index;

        if (stand_index_changed || !initialized)
        {
            // Updathe the shelf dropdown to list the correct new shelf names
            shelfNames = new List<string>();
            for (int i = 0; i < standList[stand_index].shelves.Length; i++)
            {
                shelfNames.Add(standList[stand_index].shelves[i].name);
            }

            shelfDropDown.ClearOptions();
            shelfDropDown.AddOptions(shelfNames);
            shelf_dropdown_index = shelf_index;
            shelfDropDown.value = shelf_index;
        }
        if(stand_index_changed || shelf_index_changed || !initialized)
        {
            boxNames = new List<string>();
            for (int i = 0; i < standList[stand_index].shelves[shelf_index].cubes.Count; i++)
            {
                boxNames.Add(standList[stand_index].shelves[shelf_index].cubes[i].name);
            }

            textscrollView.Clear();
            textscrollView.AddText(boxNames);
            shelf_dropdown_index = shelf_index;
            shelfDropDown.value = shelf_index;

        }
    }
}
