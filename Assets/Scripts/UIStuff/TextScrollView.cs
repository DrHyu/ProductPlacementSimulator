using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextScrollView : MonoBehaviour {

    private List<GameObject> texts;

    public GameObject prefab;

    public int current_index;

    public delegate void ClickHandler(int index);
    private List<ClickHandler> callBacks;

    public void AddText(List<string> _new)
    {
        if(texts == null)
        {
            texts = new List<GameObject>();
            current_index = 0;
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
    }

    public void AddText(string _new)
    {
        if (texts == null)
        {
            texts = new List<GameObject>();
            current_index = 0;
        }

        GameObject g = (GameObject)Instantiate(prefab);

        g.transform.SetParent(transform);
        g.GetComponent<Text>().text = _new;
        g.GetComponent<TextClickHandle>().setID(texts.Count);
        texts.Add(g);
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
        current_index = 0;
    }

    public void RegisterClickCallback(ClickHandler f)
    {
        if(callBacks == null)
        {
            callBacks = new List<ClickHandler>();
        }
        callBacks.Add(f);
    }

    public void TextWasClicked(int index)
    {
        texts[current_index].GetComponent<Text>().color = Color.white;
        texts[index].GetComponent<Text>().color = Color.cyan;
        current_index = index;

        if (callBacks != null)
        {
            for (int i = 0; i < callBacks.Count; i++)
            {
                callBacks[i](index);
            }
        }
    }
}
