using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreviewController : MonoBehaviour {

    public SceneGenerator SG;

    private GameObject go;

	// Use this for initialization
	void Start () {
        go = new GameObject();
        //go.transform.parent = transform;
        //go.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);


        //SG.GenerateScene("werid_shape.json");

	}

    public void previewBox(BoxJSON box)
    {
        Destroy(go);
        go = new GameObject();

        ShelfJSON shelf_json = new ShelfJSON();
        shelf_json.front_index = new int[] { 0, 1, 2, 3, 0 };
        shelf_json.x_points = new float[] { -1.0f, -1.0f, 1.0f, 1.0f };
        shelf_json.y_points = new float[] { -1.0f, 1.0f, 1.0f, -1.0f };
        shelf_json.boxes = new BoxJSON[] { box };

        ShelfJSON[] shj_array = new ShelfJSON[] { shelf_json };
        StandJSON sj = new StandJSON();
        sj.shelves = shj_array;
        sj.y_start = -1.0f;
        sj.wall_x = new float[0];
        sj.wall_y = new float[0];
        StandJSON[] sj_array = new StandJSON[] {sj};
        SceneData sd = new SceneData(sj_array);

        SG = go.AddComponent<SceneGenerator>();
        SG.transform.parent = this.transform;
        SG.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
        SG.GenerateScene(sd);
    }



	// Update is called once per frame
	void Update () {
		
	}
}
