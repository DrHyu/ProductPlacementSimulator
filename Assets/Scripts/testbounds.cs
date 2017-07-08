using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testbounds : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnValidate()
    {
        GameObject b = transform.GetChild(0).gameObject;



        b.transform.position = transform.position;
        b.transform.localScale = transform.localScale;
    }
}
