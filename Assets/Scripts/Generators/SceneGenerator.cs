using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;

[Serializable]
public class SceneGenerator : MonoBehaviour
{

    private SceneData sceneData;
    public string JSONPath = "data.json";
    public bool AUTOSTART = false;

    public List<StandGenerator> stands;

    public Dictionary<int, GameObject> id2stand;
    
    void Awake ()
    {
        if (AUTOSTART)
        {
            GenerateScene(JSONPath);
        }
    }

    public void GenerateScene(string JSONName)
    {
        JSONPath = Path.Combine(Application.streamingAssetsPath, JSONName);

        SceneData sc = LoadShelfData(JSONPath);
        GenerateScene(sc);
    }

    public void GenerateScene(SceneData sd)
    {
        sceneData = sd;

        stands = new List<StandGenerator>();
        id2stand = new Dictionary<int, GameObject>();

        for (int i = 0; i < sceneData.stands.Length; i++)
        {
            GameObject g = new GameObject(sceneData.stands[i].name);
            g.transform.SetParent(transform);

            StandGenerator STD = g.AddComponent(typeof(StandGenerator)) as StandGenerator;
            stands.Add(STD);
            id2stand.Add(STD.gameObject.GetInstanceID(), STD.gameObject);

            STD.Initialize(sceneData.stands[i]);
        }

        //if (AUTOSTART)
        //{
        //    GameObject UI = GameObject.Find("UIController");
        //    UIController uiController = UI.GetComponent<UIController>();
        //    uiController.SetStandList(stands);
        //}
    }

    private SceneData LoadShelfData(string JSONName)
    {
        SceneData sd = null;
        if (File.Exists(JSONName))
        {
            // Read the json from the file into a string
            string dataAsJson = File.ReadAllText(JSONName);
            // Pass the json to JsonUtility, and tell it to create a GameData object from it
            sd = JsonUtility.FromJson<SceneData>(dataAsJson);
        }
        else
        {
            Debug.LogError("Cannot load game data!");
        }
        return sd;
    }

    public void RegisterChild(StandGenerator s)
    {
        if(stands == null)
        {
            stands = new List<StandGenerator>();
        }

        stands.Add(s);
        id2stand.Add(s.gameObject.GetInstanceID(), s.gameObject);

    //    // TODO probably needs a less crappy style
    //    if (AUTOSTART)
    //    {
    //        GameObject UI = GameObject.Find("UIController");
    //        UIController uiController = UI.GetComponent<UIController>();
    //        uiController.SetStandList(stands);
    //    }
    }
}