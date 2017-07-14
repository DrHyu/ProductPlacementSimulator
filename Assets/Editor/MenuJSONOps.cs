using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.IO;


public class MenuJSONOps : MonoBehaviour
{
    // Add a menu item named "Do Something" to MyMenu in the menu bar.
    [MenuItem("JSON/Save To JSON")]
    static void SaveToJSON()
    {
        // Path.Combine combines strings into a file path
        // Application.StreamingAssets points to Assets/StreamingAssets in the Editor, and the StreamingAssets folder in a build


        GameObject[] selected_objects = Selection.gameObjects;

        if(selected_objects != null && selected_objects.Length > 0) {

            List<StandGenerator> sg = new List<StandGenerator>();

            foreach (Transform child in selected_objects[0].transform)
            {
                sg.Add(child.gameObject.GetComponent<StandGenerator>());
            }

            string filePath = Path.Combine(Application.streamingAssetsPath, "FarmaciaBaricentro.json");


            SceneData sd = FromSceneToJSON(sg);
            string json_data = JsonUtility.ToJson(sd);
            File.WriteAllText(filePath, json_data);
        }
    }

    [MenuItem("JSON/Load From JSON")]
    static void LoadFromJSON()
    {
        // Path.Combine combines strings into a file path
        // Application.StreamingAssets points to Assets/StreamingAssets in the Editor, and the StreamingAssets folder in a build

        string file_name = EditorUtility.OpenFilePanel("Select JSON to import", Application.streamingAssetsPath, "json");

        if (file_name != null && file_name != "" )
        {
            SceneGenerator sg = new GameObject("SceneGenerator").AddComponent<SceneGenerator>();
            sg.GenerateScene(file_name);
        }
    }


    private static SceneData FromSceneToJSON (List<StandGenerator> s)
    {
        StandJSON[] outData = new StandJSON[s.Count];

        for(int st = 0; st < s.Count; st++)
        {
            outData[st] = s[st].this_stand;

            for(int sh = 0; sh < s[st].shelves.Count; sh ++)
            {
                // Each shelf has 1 array with the product data that was extracted from the JSON 
                // it also has 1 array list which is the one used and updated
                // TODO this is so confusing, it should be reworked
                outData[st].shelves[sh].boxes = s[st].shelves[sh].cubesJSON.ToArray();
            }

        }
        return new SceneData(outData);
    }

}