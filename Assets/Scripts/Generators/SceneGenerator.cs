using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;

[Serializable]
public class SceneGenerator : MonoBehaviour
{
    //store gameObject reference
    GameObject objToSpawn;

    private SceneData sceneData;
    public string dataJson = "data.json";

    public List<Stand> stands;

    
    void Start()
    {
        LoadShelfData();

        stands = new List<Stand>();

        for (int i = 0; i < sceneData.stands.Length; i++)
        {
            GameObject g = new GameObject(sceneData.stands[i].name);
            g.transform.SetParent(transform);

            Stand STD = g.AddComponent(typeof(Stand)) as Stand;
            stands.Add(STD);
            STD.Initialize(sceneData.stands[i]);
        }

        GameObject UI = GameObject.Find("UIController");

        // Link the UI cotroller to the stand objects
        UIController uiController = UI.GetComponent<UIController>();
        uiController.SetStandList(stands);
    }

    private void LoadShelfData()
    {
        // Path.Combine combines strings into a file path
        // Application.StreamingAssets points to Assets/StreamingAssets in the Editor, and the StreamingAssets folder in a build
        string filePath = Path.Combine(Application.streamingAssetsPath, dataJson);

        if (File.Exists(filePath))
        {
            // Read the json from the file into a string
            string dataAsJson = File.ReadAllText(filePath);
            // Pass the json to JsonUtility, and tell it to create a GameData object from it
            sceneData = JsonUtility.FromJson<SceneData>(dataAsJson);
        }
        else
        {
            Debug.LogError("Cannot load game data!");
        }
    }
    

}