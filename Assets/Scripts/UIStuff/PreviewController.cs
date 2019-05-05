using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreviewController : MonoBehaviour {

    public SceneGenerator SG;

    private GameObject go;
    private bool initialized = false;
    private bool camera_pos_needs_update = false;
    private Camera cam;

	// Use this for initialization
	void Start () {
        cam = transform.Find("PreviewCamera").GetComponent<Camera>();
        cam.transform.localPosition = new Vector3(0, 0, 0);
        PreviewBox(null);
	}

    public void PreviewBox(BoxJSON box)
    {
        if (go != null) { GameObject.Destroy(go); }

        go = new GameObject();

        float shelf_size = box == null ? 1 : box.width;
        //if (shelf_size < 1) { shelf_size = 1; }

        ShelfJSON shelf_json = new ShelfJSON();
        //shelf_json.front_index = new int[] { 3, 0, 1, 2, 3 };
        shelf_json.front_index = new int[] { 3, 0 };
        shelf_json.x_points = new float[] { -shelf_size, -shelf_size, shelf_size, shelf_size };
        shelf_json.y_points = new float[] { -shelf_size, shelf_size, shelf_size, -shelf_size };
        shelf_json.boxes = box == null ? new BoxJSON[0] : new BoxJSON[] { box };


        shelf_json.thickness = shelf_size / 5.0f;


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

        shelf_json.boxes[0].cpr = 0.25f;
        shelf_json.boxes[0].cir = 0;
        //shelf_json.boxes[0].cil = 0;

        ShelfGenerator shg = SG.stands[0].shelves[0];

        shg.cubes[0].SetStartingPosition(0, 0.25f);
    
        /* Camera needs to focus the new object, the position of the new object is not defined till the next
            update cycle. Do the camera position update in the next lateupdate */
        camera_pos_needs_update = true;
    }



    Bounds CalculateBounds(GameObject go)
    {
        Bounds b = new Bounds(go.transform.position, Vector3.zero);
        Object[] rList = go.GetComponentsInChildren(typeof(Renderer));
        foreach (Renderer r in rList)
        {
            b.Encapsulate(r.bounds);
        }
        return b;
    }

    void FocusCameraOnGameObject(Camera c, GameObject go)
    {
        Bounds b = CalculateBounds(go);

        Vector3 max = b.size;
        float radius = Mathf.Max(max.x, Mathf.Max(max.y, max.z));
        float dist = radius / (Mathf.Sin(c.fieldOfView * Mathf.Deg2Rad / 2f));
        c.transform.position = go.transform.position + transform.rotation * Vector3.forward * -dist;

        /* If we are focusing a product, position the camera on the front size of the product, not only zoom */
    }

	// Update is called once per frame
	void LateUpdate ()
    {
        if (camera_pos_needs_update)
        {
            camera_pos_needs_update = false;
            FocusCameraOnGameObject(cam, SG.stands[0].shelves[0].cubes[0].gameObject);
        }
    }
}
