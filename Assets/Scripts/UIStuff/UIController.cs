using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;


public class UIController : MonoBehaviour {


    public SceneGenerator _SceneGenerator;
    public UItoSimulation _UItoSimulation;
    public SimulationToUI _SimualtionToUI;
    public FloatingProducts _FloatingProducts;


    public Dropdown standDropDown;
    public Dropdown shelfDropDown;
    public TextScrollView productListerView;
    public Button addButton;
    public Button removeButton;

    public InputField searchField;
    public TextScrollView DBListerView;

    public DBHandler DBH;
    public DB myDB;

    public PreviewController previewController;



    private List<string> dbNames;
    private int dbIndex = 0;

    private int stack_x = 1;
    private int stack_y = 1;
    private int stack_z = 1;

    private bool initialized = false;

    private void Start()
    {
        Initialize();
    }

    public void Initialize()
    {
        standList = _SceneGenerator.stands;
        if (standList == null)
        {
            Debug.LogError("Stand list is null when loading UI Controller");
        }

        _UItoSimulation.Initialize(standList);
        _SimualtionToUI.Initialize(standList);

        standNames = new List<string>();
        for (int i = 0; i < standList.Count; i++)
        {
            standNames.Add(standList[i].ToString());
        }

        UpdateUIState(0, 0,null, false, true);
        initialized = true;

        InitializeDBStuff(DBH.ReadFullDB());
        DBListerView.RegisterIndexChangedCallback(OnDBListerIndexChanged);
        OnDBListerIndexChanged(dbIndex);
        productListerView.RegisterSelectedChangedCallback(BoxSlectedIndexChanged);
    }

    private void InitializeDBStuff(DB newDB)
    {
        myDB = newDB;

        DBListerView.Clear();
        dbNames = new List<string>();
        for (int i = 0; i < myDB.contents.Length; i++)
        {
            dbNames.Add(myDB.contents[i].name);
        }
        DBListerView.AddText(dbNames);
        dbIndex = 0;
    }


    private void Update()
    {
        if (Input.GetKey(KeyCode.Delete))
        {
            OnRemoveButonPressed();
        }


        if (Input.GetKey("escape")) {
            OnExitApplication();
        }

    }

    // Callbacks 
    // -----------------------------------------------------------------------------//

    public List<StandGenerator> standList;
    public List<string> standNames;
    public int stand_dropdown_index = 0;

    public void StandDropDownIndexChanged(int index)
    {
        if (stand_dropdown_index != index) { UpdateUIState(index); }
    }

    public List<string> shelfNames;
    public int shelf_dropdown_index = 0;

    public void ShelfDropDownIndexChanged(int index)
    {
        if (shelf_dropdown_index != index) { UpdateUIState(stand_dropdown_index, index); }
    }

    public List<string> productNames;
    public bool[] productIndexes = null;

    public void BoxSlectedIndexChanged(bool[] selected)
    {
        productIndexes = selected;

        _UItoSimulation.UISelectionChanged(stand_dropdown_index, shelf_dropdown_index, selected);
    }

    public void OnAddButtonPressed()
    {
        BoxJSON b = new BoxJSON(myDB.contents[dbIndex]);
        b.x_repeats = stack_x;
        b.y_repeats = stack_y;
        b.z_repeats = stack_z;

        Drag3D product =_UItoSimulation.AddProduct(stand_dropdown_index, shelf_dropdown_index, b);

        _SimualtionToUI.NotifyNewProductAdded(product);

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
        //UpdateUIState(stand_dropdown_index, shelf_dropdown_index, productIndexes, false, true);
    }

    public void OnRemoveButonPressed()
    {
        if (productIndexes != null && productIndexes.Length > 0)
        {
            _UItoSimulation.UISelectionChanged(stand_dropdown_index, shelf_dropdown_index, null);

            // Remove the products from the datastructure
            _UItoSimulation.RemoveProducts(stand_dropdown_index, shelf_dropdown_index, productIndexes);

            productIndexes = null;

            // Redraw UI
            UpdateUIState(stand_dropdown_index, shelf_dropdown_index, null, false, true);
        }
    }

    public void OnDBListerIndexChanged(int index)
    {
        dbIndex = index;
        stack_x = 1;
        stack_y = 1;
        stack_z = 1;
        UpdatePreview();
    }

    public void OnSearchFieldChanged(string search)
    {
        search = searchField.text;

        // search in db 
        if (search == "")
        {
            InitializeDBStuff(DBH.ReadFullDB());
        }
        else
        {
            InitializeDBStuff(DBH.SearchItemByName(search));
        }
        
    }

    // How to view the non-selected elements //

    public Button ViewAllButton;
    public Button AlphaChangeButton;
    public Button ViewNoneButton;

    public void OnViewAllButtonPressed()
    {
        ViewAllButton.interactable = false;
        AlphaChangeButton.interactable = true;
        ViewNoneButton.interactable = true;

        _UItoSimulation.ChangeViewMode(UItoSimulation.ALPHA_CHANGE);
    }

    public void OnAlphaChangeButtonPressed()
    {
        ViewAllButton.interactable = true;
        AlphaChangeButton.interactable = false;
        ViewNoneButton.interactable = true;

        _UItoSimulation.ChangeViewMode(UItoSimulation.NO_CHANGE);
    }

    public void OnViewNoneButtonPressed()
    {
        ViewAllButton.interactable = true;
        AlphaChangeButton.interactable = true;
        ViewNoneButton.interactable = false;

        _UItoSimulation.ChangeViewMode(UItoSimulation.ACTIVE_CHANGE);
    }

    public void OnExitApplication()
    {
        SaveToJSON();
        Application.Quit();
    }

    private void UpdatePreview()
    {
        BoxJSON b = new BoxJSON(myDB.contents[dbIndex]);
        b.x_repeats = stack_x;
        b.y_repeats = stack_y;
        b.z_repeats = stack_z;
        previewController.PreviewBox(b);
    }

    public void OnXPlusClicked()
    {
        stack_x++;
        UpdatePreview();
    }
    public void OnXMinusClicked()
    {
        stack_x--;
        UpdatePreview();
    }
    public void OnYPlusClicked()
    {
        stack_y++;
        UpdatePreview();
    }
    public void OnYMinusClicked()
    {
        stack_y--;
        UpdatePreview();
    }
    public void OnZPlusClicked()
    {
        stack_z++;
        UpdatePreview();
    }
    public void OnZMinusClicked()
    {
        stack_z--;
        UpdatePreview();
    }

    // Product select UI includes: stand dropdown, shelf dropdown and product selection text scrollview //
    public void UpdateUIState(int stand_index = 0 , int shelf_index = 0, bool[] products_selected = null, bool trigger_was_simulation = false, bool initialize = false)
    {
        bool stand_index_changed = stand_index != stand_dropdown_index;
        bool shelf_index_changed = shelf_index != shelf_dropdown_index;

        
        _UItoSimulation.UISelectionChanged(stand_index, shelf_index, products_selected);

        if (trigger_was_simulation)
        {
            //stand_dropdown_index = stand_index;
            //shelf_dropdown_index = shelf_index;
            productIndexes = products_selected;
        }

        if (initialize || trigger_was_simulation)
        {
            standDropDown.ClearOptions();
            standDropDown.AddOptions(standNames);
            stand_dropdown_index = stand_index;
            standDropDown.value = stand_index;
        }
        stand_dropdown_index = stand_index;

        if (stand_index_changed || initialize)
        {
            // Updathe the shelf dropdown to list the correct new shelf names
            shelfNames = new List<string>();
            for (int i = 0; i < standList[stand_index].shelves.Count; i++)
            {
                shelfNames.Add(standList[stand_index].shelves[i].name);
            }

            shelfDropDown.ClearOptions();
            shelfDropDown.AddOptions(shelfNames);
            shelf_dropdown_index = shelf_index;
            shelfDropDown.value = shelf_index;
        }
        if(stand_index_changed || shelf_index_changed || initialize)
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

    // Save to file logic //
    private void SaveToJSON()
    {
        // Path.Combine combines strings into a file path
        // Application.StreamingAssets points to Assets/StreamingAssets in the Editor, and the StreamingAssets folder in a build
        string filePath = Path.Combine(Application.streamingAssetsPath, "FarmaciaBaricentro.json");
        SceneData sd = FromSceneToJSON(standList);
        string json_data = JsonUtility.ToJson(sd);
        File.WriteAllText(filePath, json_data);
    }

    private SceneData FromSceneToJSON(List<StandGenerator> s)
    {
        StandJSON[] outData = new StandJSON[s.Count];

        for (int st = 0; st < s.Count; st++)
        {
            outData[st] = s[st].this_stand;

            for (int sh = 0; sh < s[st].shelves.Count; sh++)
            {
                // Each shelf has 1 array with the product data that was extracted from the JSON 
                // it also has 1 array list which is the one used and updated
                // TODO this is so confusing, it should be reworked
                outData[st].shelves[sh].boxes = s[st].shelves[sh].cubesJSON.ToArray();
            }

        }
        return new SceneData(outData);
    }

    // Utility, transforms from an array of indices to a bool array with the selected indices set as true and the rest set to false
    public static bool[] GetSelectedArray(int[] selected, int size)
    {
        bool[] sel = new bool[size];
        for (int i = 0; i < sel.Length; i++)
        {
            sel[i] = false;
        }
        for (int i = 0; i < selected.Length; i++)
        {
            sel[selected[i]] = true;
        }

        return sel;
    }

}
