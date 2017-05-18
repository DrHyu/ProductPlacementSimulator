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

    private List<Stand> standList;
    private List<string> standNames;
    public int standIndex;

    private List<string> shelfNames;
    public int shelfIndex;

    private List<string> boxNames;
    public int boxIndex = -1;

    private GameObject scrollView;

    public void Initialize()
    {

        standNames = new List<string>();
        for (int i = 0; i < standList.Count; i++)
        {
            standNames.Add(standList[i].ToString());
        }

        textscrollView.RegisterIndexChangedCallback(BoxSlectedIndexChanged);
        addButton.GetComponent<ButtonClickCallback>().RegisterClickCallback(OnAddButtonPressed);
        removeButton.GetComponent<ButtonClickCallback>().RegisterClickCallback(OnRemoveButoonPressed);

        updateStandDropDown();
        updateShelfDropDown();
        updateBoxLister();
    }

    public void SetStandList(List<Stand> _standList)
    {
        standList = _standList;
        Initialize();
    }

    public void StandDropDownIndexChanged(int index)
    {
        if (boxIndex != -1)
        {
            standList[standIndex].shelves[shelfIndex].cubes[boxIndex].GetComponent<Drag3D>().selected = false;
            standList[standIndex].shelves[shelfIndex].cubes[boxIndex].GetComponent<Drag3D>().updateColor();
        }

        standIndex = index;
        updateShelfDropDown();
    }
    public void ShelfDropDownIndexChanged(int index)
    {
        if (boxIndex != -1)
        {
            standList[standIndex].shelves[shelfIndex].cubes[boxIndex].GetComponent<Drag3D>().selected = false;
            standList[standIndex].shelves[shelfIndex].cubes[boxIndex].GetComponent<Drag3D>().updateColor();
        }

        shelfIndex = index;
        updateBoxLister();
    }
    public void BoxSlectedIndexChanged(int index)
    {
        if (boxIndex != -1)
        {
            standList[standIndex].shelves[shelfIndex].cubes[boxIndex].GetComponent<Drag3D>().selected = false;
            standList[standIndex].shelves[shelfIndex].cubes[boxIndex].GetComponent<Drag3D>().updateColor();
        }

        standList[standIndex].shelves[shelfIndex].cubes[index].GetComponent<Drag3D>().selected = true;
        standList[standIndex].shelves[shelfIndex].cubes[index].GetComponent<Drag3D>().updateColor();

        boxIndex = index;
    }

    public void OnAddButtonPressed()
    {
        
    }

    public void OnRemoveButoonPressed()
    {
        if (standList[standIndex].shelves[shelfIndex].cubes[boxIndex] != null )
        {
            GameObject.Destroy(standList[standIndex].shelves[shelfIndex].cubes[boxIndex]);

            standList[standIndex].shelves[shelfIndex].cubes.RemoveAt(boxIndex);
            boxIndex--;

            // Clear and fill up again the txt scroll view
            // Keep the same id 
            updateBoxLister(false);
        }
    }



    private void updateStandDropDown()
    {
        standDropDown.ClearOptions();
        standDropDown.AddOptions(standNames);
        standIndex = 0;
    }
    private void updateShelfDropDown()
    {
        shelfIndex = 0;

        shelfNames = new List<string>();
        for (int i = 0; i < standList[standIndex].shelves.Length; i++)
        {
            shelfNames.Add(standList[standIndex].shelves[i].name);
        }

        shelfDropDown.ClearOptions();
        shelfDropDown.AddOptions(shelfNames);
    }
    private void updateBoxLister(bool resetIndex = true)
    {
        if (resetIndex) {
            boxIndex = -1;
        }

        boxNames = new List<string>();
        for (int i = 0; i < standList[standIndex].shelves[shelfIndex].cubes.Count; i++)
        {
            boxNames.Add(standList[standIndex].shelves[shelfIndex].cubes[i].name);
        }

        textscrollView.Clear();
        textscrollView.AddText(boxNames);

        if (boxIndex != -1)
        {
            standList[standIndex].shelves[shelfIndex].cubes[boxIndex].GetComponent<Drag3D>().selected = true;
            standList[standIndex].shelves[shelfIndex].cubes[boxIndex].GetComponent<Drag3D>().updateColor();
        }
    }

}
