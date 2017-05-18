using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CallBackRegisterableClass : MonoBehaviour {


    public delegate void ClickCallback();
    public delegate void IndexChangedCallback(int index);

    protected  List<ClickCallback> clickCallback;
    protected List<IndexChangedCallback> indexChangedCallback;


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

}
