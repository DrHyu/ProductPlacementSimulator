using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour {

    public Dropdown standDropDown;
    public Dropdown shelfDropDown;
    public BoxSelector boxLister;

    private List<Stand> standList;
    private List<string> standNames;
    private int standIndex;

    private List<string> shelfNames;
    private int shelfIndex;

    private List<string> boxNames;
    private int boxIndex;


    public void Initialize()
    {


        standNames = new List<string>();
        for (int i = 0; i < standList.Count; i++)
        {
            standNames.Add(standList[i].ToString());
        }

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
        standIndex = index;
        updateShelfDropDown();
    }
    public void ShelfDropDownIndexChanged(int index)
    {
        shelfIndex = index;
        updateBoxLister();
    }




    private void updateStandDropDown()
    {
        standDropDown.ClearOptions();
        standDropDown.AddOptions(standNames);
        standIndex = 0;
    }
    private void updateShelfDropDown()
    {
        shelfNames = new List<string>();
        for (int i = 0; i < standList[standIndex].shelves.Length; i++)
        {
            shelfNames.Add(standList[standIndex].shelves[i].name);
        }

        shelfDropDown.ClearOptions();
        shelfDropDown.AddOptions(shelfNames);
        shelfIndex = 0;
    }
    private void updateBoxLister()
    {
        boxNames = new List<string>();
        for (int i = 0; i < standList[standIndex].shelves[shelfIndex].cubes.Length; i++)
        {
            boxNames.Add(standList[standIndex].shelves[shelfIndex].cubes[i].name);
        }

        boxLister.Clear();
        boxLister.AddText(boxNames);
        boxIndex = 0;
    }

}
