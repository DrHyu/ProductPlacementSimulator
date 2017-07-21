using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnClickPassUp : MonoBehaviour {


    private void OnMouseDown()
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

}
