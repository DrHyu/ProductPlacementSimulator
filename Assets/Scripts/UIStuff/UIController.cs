using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;

public class UIController : MonoBehaviour {

    public Dropdown standDropDown;
    public Dropdown shelfDropDown;
    public TextScrollView productListerView;
    public Button addButton;
    public Button removeButton;

    public InputField searchField;
    public TextScrollView DBListerView;

    public DBHandler DBH;
    public DB myDB;

    public UItoSimulation _UItoSimulation;

    private List<StandGenerator> standList;
    private List<string> standNames;    
    private int stand_dropdown_index = 0;
    private List<string> shelfNames;
    private int shelf_dropdown_index = 0;

    private List<string> productNames;
    public bool[] productIndexes = null;

    private List<string> dbNames;
    private int dbIndex = 0;


    private bool callbacksSet = false;
    private bool initialized = false;

    public void Initialize()
    {


        standNames = new List<string>();
        for (int i = 0; i < standList.Count; i++)
        {
            standNames.Add(standList[i].ToString());
        }

        //TODO Crappy fix
        if (!callbacksSet)
        {
            productListerView.RegisterSelectedChangedCallback(BoxSlectedIndexChanged);
            addButton.GetComponent<ButtonClickCallback>().RegisterClickCallback(OnAddButtonPressed);
            removeButton.GetComponent<ButtonClickCallback>().RegisterClickCallback(OnRemoveButoonPressed);
            callbacksSet = true;
        }

        
        UpdateUIState(0, 0);
        initialized = true;
    }

    private void Start()
    {
        // TODO: Kinda crappy, fix when db rework is done
        myDB = DBH.db;

        DBListerView.RegisterIndexChangedCallback(OnDBListerIndexChanged);

        dbNames = new List<string>();
        for (int i = 0; i < myDB.contents.Length; i++)
        {
            dbNames.Add(myDB.contents[i].name);
        }
        DBListerView.AddText(dbNames);

    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.Delete))
        {
            OnRemoveButoonPressed();
        }
    }

    public void SetStandList(List<StandGenerator> _standList)
    {
        standList = _standList;
        initialized = false;
        _UItoSimulation.Initialize(_standList);
        Initialize();
    }

    // Callbacks 
    // -----------------------------------------------------------------------------//

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
        productIndexes = selected;

        _UItoSimulation.UISelectionChanged(stand_dropdown_index, shelf_dropdown_index, selected);
    }

    public void OnAddButtonPressed()
    {
        _UItoSimulation.AddProduct(stand_dropdown_index, shelf_dropdown_index, myDB.contents[dbIndex].ID);

        if (productIndexes != null && productIndexes.Length > 0)
        {
            bool[] new_index = new bool[productIndexes.Length + 1];

            for (int i = 0; i < productIndexes.Length; i++)
            {
                new_index[i] = productIndexes[i];
            }
            new_index[productIndexes.Length] = false;
            productIndexes = new_index;
        }

        // Redraw UI
        initialized = false;
        UpdateUIState(stand_dropdown_index, shelf_dropdown_index, productIndexes);
    }

    public void OnRemoveButoonPressed()
    {
        if (productIndexes != null && productIndexes.Length > 0)
        {
            _UItoSimulation.UISelectionChanged(stand_dropdown_index, shelf_dropdown_index, null);

            // Remove the products from the datastructure
            _UItoSimulation.RemoveProducts(stand_dropdown_index, shelf_dropdown_index, productIndexes);

            productIndexes = null;

            // Redraw UI
            initialized = false;
            UpdateUIState(stand_dropdown_index, shelf_dropdown_index, null);
        }
            

    }

    public void OnDBListerIndexChanged(int index)
    {
        dbIndex = index;
    }

    public void OnSearchFieldChanged(string search)
    {
        search = searchField.text;
        search = ".*" + search + ".*";
        //string pt = "@\"\\b(" + db.GetPattern + ")\\b\"";
        Regex regex = new Regex(search, RegexOptions.IgnoreCase);
        DBListerView.SearchFunction(regex);
    }

    // -----------------------------------------------------------------------------//


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
            productNames = new List<string>();
            for (int i = 0; i < standList[stand_index].shelves[shelf_index].cubes.Count; i++)
            {
                productNames.Add(standList[stand_index].shelves[shelf_index].cubes[i].GetComponent<Drag3D>().name);
            }

            productListerView.Clear();
            productListerView.AddText(productNames);
            shelf_dropdown_index = shelf_index;
            shelfDropDown.value = shelf_index;
        }

        if(products_selected != null && products_selected.Length > 0)
        {
            productListerView.SetSelected(products_selected);
        }
    }
}
