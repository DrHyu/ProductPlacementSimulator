using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextScrollView : CallBackRegisterableClass {

    private List<GameObject> texts;

    public GameObject prefab;

    public int current_index = -1;

    private bool shift_pressed = false;
    private bool ctrl_pressed = false;

    private List<bool> entry_selected;
    private int last_entry_selected; 


    private void Initialize()
    {
        texts = new List<GameObject>();
        current_index = -1;
        entry_selected = new List<bool>();
        last_entry_selected = -1;
    }

    public void AddText(List<string> _new)
    {
        if(texts == null)
        {
            Initialize();
        }

        for (int i = 0; i < _new.Count; i++)
        {
            GameObject g = (GameObject) Instantiate(prefab);

            g.transform.SetParent(transform);
            g.GetComponent<Text>().text = _new[i];
            g.GetComponent<TextClickHandle>().setID(i);
            texts.Add(g);
            entry_selected.Add(false);
        }
        if (current_index != -1)
        {
            texts[current_index].GetComponent<Text>().color = Color.cyan;
        }
    }

    public void AddText(string _new)
    {
        if (texts == null)
        {
            Initialize();
        }

        GameObject g = (GameObject)Instantiate(prefab);

        g.transform.SetParent(transform);
        g.GetComponent<Text>().text = _new;
        g.GetComponent<TextClickHandle>().setID(texts.Count);
        texts.Add(g);
        entry_selected.Add(false);

        if (current_index != -1)
        {
            texts[current_index].GetComponent<Text>().color = Color.cyan;
        }
    }

    public void Clear()
    {
        if (texts == null)
        {
            return;
        }

        for (int i = 0; i < texts.Count; i++)
        {
            GameObject.Destroy(texts[i]);
        }

        Initialize();
    }


    public void ProcessClick(int textClickedID)
    {

        // Support for CTRL | SHIFT to select multiple products at once
        // Determine what needs to be selected
        if (shift_pressed)
        {
            if(last_entry_selected == -1)
            {
                // First entry selected, here shift has no effect
                entry_selected[textClickedID] = true;
            }
            else
            {
                // There is a previous text selected, select everything from the last selected index to the current click index (inclusive)
                int from = textClickedID < last_entry_selected ? textClickedID : last_entry_selected;
                int to = textClickedID < last_entry_selected ? last_entry_selected : textClickedID;

                for (int i = 0; i < entry_selected.Count; i++)
                {
                    if (i >= from && i <= to)
                    {
                        entry_selected[i] = true;
                    }
                    else
                    {
                        entry_selected[i] = false;
                    }
                }
            }
        }
        // Shift has priortity
        else if (ctrl_pressed)
        {
            entry_selected[textClickedID] = !entry_selected[textClickedID];
        }
        // Normal click, clear the rest and only leave the clicked one selected
        else
        {
            for(int i =0; i < entry_selected.Count; i++)
            {
                entry_selected[i] = false;
            }
            entry_selected[textClickedID] = true;
        }


        // Set the color status accordingly
        for(int i =0; i < texts.Count; i++)
        {
            texts[i].GetComponent<Text>().color = entry_selected[i] ?  Color.cyan : Color.white;
        }

        if (selectedChangedCallback != null)
        {
            for (int i = 0; i < selectedChangedCallback.Count; i++)
            {
                selectedChangedCallback[i](entry_selected.ToArray());
            }
        }

        last_entry_selected = textClickedID;
    }

    private void Update()
    {
        shift_pressed = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        ctrl_pressed = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
    }
}
