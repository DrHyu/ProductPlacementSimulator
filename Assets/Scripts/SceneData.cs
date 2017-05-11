using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


[Serializable]
public class SceneData {

    public ShelfJSON[] shelves;

    public SceneData(ShelfJSON[] _shelves)
    {
        shelves = _shelves;
    }
}
