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

            string filePath = Path.Combine(Application.streamingAssetsPath, "testsJSON.json");


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

    // Validated menu item.
    // Add a menu item named "Log Selected Transform Name" to MyMenu in the menu bar.
    // We use a second function to validate the menu item
    // so it will only be enabled if we have a transform selected.
    [MenuItem("MyMenu/Log Selected Transform Name")]
    static void LogSelectedTransformName()
    {
        Debug.Log("Selected Transform is on " + Selection.activeTransform.gameObject.name + ".");
    }

    // Validate the menu item defined by the function above.
    // The menu item will be disabled if this function returns false.
    [MenuItem("MyMenu/Log Selected Transform Name", true)]
    static bool ValidateLogSelectedTransformName()
    {
        // Return false if no transform is selected.
        return Selection.activeTransform != null;
    }

    // Add a menu item named "Do Something with a Shortcut Key" to MyMenu in the menu bar
    // and give it a shortcut (ctrl-g on Windows, cmd-g on macOS).
    [MenuItem("MyMenu/Do Something with a Shortcut Key %g")]
    static void DoSomethingWithAShortcutKey()
    {
        Debug.Log("Doing something with a Shortcut Key...");
    }

    // Add a menu item called "Double Mass" to a Rigidbody's context menu.
    [MenuItem("CONTEXT/Rigidbody/Double Mass")]
    static void DoubleMass(MenuCommand command)
    {
        Rigidbody body = (Rigidbody)command.context;
        body.mass = body.mass * 2;
        Debug.Log("Doubled Rigidbody's Mass to " + body.mass + " from Context Menu.");
    }

    // Add a menu item to create custom GameObjects.
    // Priority 1 ensures it is grouped with the other menu items of the same kind
    // and propagated to the hierarchy dropdown and hierarch context menus.
    [MenuItem("GameObject/MyCategory/Custom Game Object", false, 10)]
    static void CreateCustomGameObject(MenuCommand menuCommand)
    {
        // Create a custom game object
        GameObject go = new GameObject("Custom Game Object");
        // Ensure it gets reparented if this was a context click (otherwise does nothing)
        GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
        // Register the creation in the undo system
        Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
        Selection.activeObject = go;
    }




    private static SceneData FromSceneToJSON (List<StandGenerator> s)
    {
        StandJSON[] sJ = new StandJSON[s.Count];

        for(int st = 0; st < s.Count; st++)
        {
            sJ[st] = s[st].this_stand;


        }
        return new SceneData(sJ);
    }

}