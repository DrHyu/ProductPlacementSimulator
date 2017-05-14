using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoxSelector : MonoBehaviour {


    private List<string> text;
    private List<GameObject> UIText;


    private void OnValidate()
    {
    }

    public void AddText(List<string> _new)
    {
        if(text == null)
        {
            text = new List<string>();
            UIText = new List<GameObject>();
        }

        for (int i = 0; i < _new.Count; i++)
        {
            Text t = new GameObject().AddComponent<Text>();
            t.transform.SetParent(transform);
            t.gameObject.layer = 5;
            t.text = _new[i];
            t.font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
            UIText.Add(t.gameObject);
            text.Add(_new[i]);
        }
    }
    public void AddText(string _new)
    {
        if (text == null)
        {
            text = new List<string>();
            UIText = new List<GameObject>();
        }

        Text t  = new GameObject().AddComponent<Text>();
        t.transform.SetParent(transform);
        t.gameObject.layer = 5;
        t.text = _new;
        t.font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
        UIText.Add(t.gameObject);
        text.Add(_new);
    }

    public void Clear()
    {
        if (text == null)
        {
            return;
        }

        for (int i = 0; i < UIText.Count; i++)
        {
            GameObject.Destroy(UIText[i]);
        }

        UIText.Clear();
        text.Clear();
    }
}
