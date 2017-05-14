using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour {

    public Dropdown standDropDown;
    public Dropdown shelfDropDown;
    public TextScrollView textScrollView;

    private List<Stand> standList;
    private List<string> standNames;
    public int standIndex;

    private List<string> shelfNames;
    public int shelfIndex;

    private List<string> boxNames;
    public int boxIndex;


    public void Initialize()
    {

        standNames = new List<string>();
        for (int i = 0; i < standList.Count; i++)
        {
            standNames.Add(standList[i].ToString());
        }

        GameObject SCRLLV = GameObject.Find("TextScrollViewContent");

        SCRLLV.GetComponent<TextScrollView>().RegisterClickCallback(BoxSlectedIndexChanged);

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
    public void BoxSlectedIndexChanged(int index)
    {
        boxIndex = index;

        standList[standIndex].shelves[shelfIndex].cubes[boxIndex].GetComponent<Renderer>().material.color = new Color(1, 1, 1, 0.1f);
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

        textScrollView.Clear();
        textScrollView.AddText(boxNames);
        boxIndex = 0;
    }

}
