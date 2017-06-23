using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CallBackRegisterableClass : MonoBehaviour {


    public delegate void ClickCallback();
    public delegate void IndexChangedCallback(int index);
    public delegate void SelectedChangedCallback(bool[] selections);

    protected  List<ClickCallback> clickCallback;
    protected  List<IndexChangedCallback> indexChangedCallback;
    protected List<SelectedChangedCallback> selectedChangedCallback;



    public void RegisterClickCallback(ClickCallback f)
    {
        if (clickCallback == null)
        {
            clickCallback = new List<ClickCallback>();
        }
        clickCallback.Add(f);
    }

    public void RegisterIndexChangedCallback(IndexChangedCallback f)
    {
        if (indexChangedCallback == null)
        {
            indexChangedCallback = new List<IndexChangedCallback>();
        }
        indexChangedCallback.Add(f);
    }

    public void RegisterSelectedChangedCallback(SelectedChangedCallback f)
    {
        if (selectedChangedCallback == null)
        {
            selectedChangedCallback = new List<SelectedChangedCallback>();
        }
        selectedChangedCallback.Add(f);
    }

}
