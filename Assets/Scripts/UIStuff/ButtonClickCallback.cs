using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonClickCallback : CallBackRegisterableClass, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        for(int i = 0; i < clickCallback.Count; i++)
        {
            clickCallback[i]();
        }
    }
}
