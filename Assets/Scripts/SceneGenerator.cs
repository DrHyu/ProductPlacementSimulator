using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class SceneGenerator : MonoBehaviour
{
    //store gameObject reference
    GameObject objToSpawn;

    private SceneData sceneData;
    public string dataJson = "data.json";

    public GameObject[] stands;
    
    void Start()
    {
        LoadShelfData();

        stands = new GameObject[sceneData.shelves.Length];

        for(int i = 0; i < sceneData.shelves.Length; i++)
        {
            GameObject ss = new GameObject("Stand_" + i);

            StandfGenerator SG =  ss.AddComponent<StandfGenerator>();
            SG.initialize(sceneData.shelves[i]);
        }

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