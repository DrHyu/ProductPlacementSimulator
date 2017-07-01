using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;


public class DBHandler : MonoBehaviour
{

    // TODO: For now the DB will just be a simple JSON file with all the product descriptions in it.
    // Probabaly in the future it is worth to have proper MYSQL db of some kind. 

    public string DB_PATH;

    public DB db;

    // We have to load the DB before the rest of components Start() method
    void Awake()
    {
        DB_PATH = Path.Combine(Application.streamingAssetsPath, DB_PATH);

        Read_DB();
    }

    private void Read_DB()
    {
        if (File.Exists(DB_PATH))
        {
            // Read the json from the file into a string
            string dataAsJson = File.ReadAllText(DB_PATH);
            // Pass the json to JsonUtility, and tell it to create a GameData object from it
            db = JsonUtility.FromJson<DB>(dataAsJson);
        }
        else
        {
            Debug.LogError("Cannot load databse!");
        }
    }
    
    public BoxJSON FindItemByID(int ID)
    {
        return null;
    }

    public BoxJSON[] SearchItemByName(string search)
    {
        return null;
    }
}


[Serializable]
public class DB
{
    public DBItem[] contents;
}

[Serializable]
public class DBItem
{
    public int ID;
    public string name = "";

    // Dimensions of the product
    public float width;
    public float height;
    public float depth;

    public string img_path;
}