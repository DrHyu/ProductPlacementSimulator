using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class OnClickPassUp : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {

    public void OnPointerDown(PointerEventData pointerEventData)
    {
        ShelfGenerator shg = transform.GetComponentInParent<ShelfGenerator>();

        if(shg != null)
        {
            shg.OnMeshClicked();
        }
        else
        {
            Debug.LogError("Could not find shelf generator attached to this mesh");
        }
    }

    public void OnPointerUp(PointerEventData pointerEventData)
    {
        /* Dummy */
    }

}
