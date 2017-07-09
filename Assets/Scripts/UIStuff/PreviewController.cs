using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreviewController : MonoBehaviour {

    public SceneGenerator SG;

    private GameObject go;
    private bool initialized = false;

	// Use this for initialization
	void Start () {

        PreviewBox(null);
	}

    public void PreviewBox(BoxJSON box)
    {
        if (go != null) { GameObject.Destroy(go); }

        go = new GameObject();

        float shelf_size = box == null ? 1 : box.width > box.depth ? box.width : box.depth;
        if(shelf_size < 1) { shelf_size = 1; }

        GameObject camera = transform.Find("PreviewCamera").gameObject;

        float sum = box == null ? 0 : box.depth + box.height + box.depth;
        camera.transform.localPosition = new Vector3(0, 0, -sum/2 -3);

        ShelfJSON shelf_json = new ShelfJSON();
        shelf_json.front_index = new int[] { 0, 1, 2, 3, 0 };
        shelf_json.x_points = new float[] { -shelf_size, -shelf_size, shelf_size, shelf_size };
        shelf_json.y_points = new float[] { -shelf_size, shelf_size, shelf_size, -shelf_size };
        shelf_json.boxes = box == null ? new BoxJSON[0] : new BoxJSON[] { box };

        ShelfJSON[] shj_array = new ShelfJSON[] { shelf_json };
        StandJSON sj = new StandJSON();
        sj.shelves = shj_array;
        sj.y_start = -1.0f;
        sj.wall_x = new float[0];
        sj.wall_y = new float[0];
        StandJSON[] sj_array = new StandJSON[] {sj};
        SceneData sd = new SceneData(sj_array);

        SG = go.AddComponent<SceneGenerator>() as SceneGenerator;
        SG.transform.parent = this.transform;
        SG.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
        SG.GenerateScene(sd);
    }



	// Update is called once per frame
	void Update ()
    {

    }
}
