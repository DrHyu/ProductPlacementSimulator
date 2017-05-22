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

    
    void Start()
    {
        JSONPath = Path.Combine(Application.streamingAssetsPath, JSONPath);

        if (AUTOSTART)
        {
            GenerateScene(JSONPath);
        }
    }

    public void GenerateScene(string JSONName)
    {
        LoadShelfData(JSONName);

        stands = new List<StandGenerator>();

        for (int i = 0; i < sceneData.stands.Length; i++)
        {
            GameObject g = new GameObject(sceneData.stands[i].name);
            g.transform.SetParent(transform);

            StandGenerator STD = g.AddComponent(typeof(StandGenerator)) as StandGenerator;
            STD.Initialize(sceneData.stands[i]);
        }
    }

    private void LoadShelfData(string JSONName)
    {
        if (File.Exists(JSONName))
        {
            // Read the json from the file into a string
            string dataAsJson = File.ReadAllText(JSONName);
            // Pass the json to JsonUtility, and tell it to create a GameData object from it
            sceneData = JsonUtility.FromJson<SceneData>(dataAsJson);
        }
        else
        {
            Debug.LogError("Cannot load game data!");
        }
    }
    
    public void RegisterChild(StandGenerator s)
    {
        if(stands == null)
        {
            stands = new List<StandGenerator>();
        }

        stands.Add(s);

        GameObject UI = GameObject.Find("UIController");
        UIController uiController = UI.GetComponent<UIController>();
        uiController.SetStandList(stands);
    }
}