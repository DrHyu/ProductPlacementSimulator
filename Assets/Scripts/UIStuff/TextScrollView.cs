using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextScrollView : CallBackRegisterableClass {

    private List<GameObject> texts;

    public GameObject prefab;

    public int current_index = -1;

    public void AddText(List<string> _new)
    {
        if(texts == null)
        {
            texts = new List<GameObject>();
            current_index = -1;
        }

        int prev_size = texts.Count;
        for (int i = 0; i < _new.Count; i++)
        {
            GameObject g = (GameObject) Instantiate(prefab);

            g.transform.SetParent(transform);
            g.GetComponent<Text>().text = _new[i];
            g.GetComponent<TextClickHandle>().setID(i+ prev_size);
            texts.Add(g);
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
            texts = new List<GameObject>();
            current_index =-1;
        }

        GameObject g = (GameObject)Instantiate(prefab);

        g.transform.SetParent(transform);
        g.GetComponent<Text>().text = _new;
        g.GetComponent<TextClickHandle>().setID(texts.Count);
        texts.Add(g);

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

        texts.Clear();
        current_index = -1;
    }


    public void TextWasClicked(int index)
    {
        if (current_index != -1) { 
            texts[current_index].GetComponent<Text>().color = Color.white;
        }

        texts[index].GetComponent<Text>().color = Color.cyan;
        current_index = index;

        if (indexChangedCallback != null)
        {
            for (int i = 0; i < indexChangedCallback.Count; i++)
            {
                indexChangedCallback[i](index);
            }
        }
    }
}
