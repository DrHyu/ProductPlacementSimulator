using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;

using Mono.Data.Sqlite;
using System.Data;


public class DBHandler : MonoBehaviour
{

    // TODO: For now the DB will just be a simple JSON file with all the product descriptions in it.
    // Probabaly in the future it is worth to have proper MYSQL db of some kind. 

    public string DB_PATH;
    private DB full_db;

    private const string SELECT_ALL = "SELECT ID, NAME, WIDTH, HEIGHT, DEPTH ";

    private IDbConnection dbconn;

    // We have to load the DB before the rest of components Start() method
    void Awake()
    {
        DB_PATH = Application.dataPath + "/productDB.db"; //Path to database.
        full_db = new DB();
        Read_DB();
    }

    private void Read_DB()
    {

        if (File.Exists(Application.dataPath + "/productDB.db"))
        {

            dbconn = (IDbConnection)new SqliteConnection("URI=file:" + DB_PATH); // the connection requires that prefix
            dbconn.Open(); //Open connection to the database.
            IDbCommand dbcmd = dbconn.CreateCommand();

            // Straightforward query to get all elements
            string sqlQuery = "SELECT ID, NAME, WIDTH, HEIGHT, DEPTH " + "FROM Product";
            dbcmd.CommandText = sqlQuery;

            List<DBItem> items = new List<DBItem>();

            IDataReader reader = dbcmd.ExecuteReader();
            while (reader.Read())
            {
                // Get the variables form the entry of the reader
                int id = reader.GetInt32(0);
                string name = reader.GetString(1);
                float width = reader.GetFloat(2);
                float height = reader.GetFloat(3);
                float depth = reader.GetFloat(4);

                Debug.Log("id = " + id + "  with name =" + name + "  and dimensions(w,h,d) = (" + width + ", "+height+", "+depth+")");

                // Set DB item and push it
                DBItem aux = new DBItem();
                aux.ID = id;
                aux.name = name;
                aux.width = width;
                aux.height = height;
                aux.depth = depth;

                items.Add(aux);
            }

            // Convert accumulated List into array
            full_db.contents = items.ToArray();

            // DB Closing
            reader.Close();
            reader = null;
            dbcmd.Dispose();
            dbcmd = null;
            dbconn.Close();
            dbconn = null;

        }
        else
        {
            Debug.LogError("Cannot load databse!");
        }
    }
    
    public BoxJSON SearchItemByID(int ID)
    {

        dbconn = (IDbConnection)new SqliteConnection("URI=file:" + DB_PATH); // the connection requires that prefix
        dbconn.Open(); //Open connection to the database.
        IDbCommand dbcmd = dbconn.CreateCommand();

        // Partial coincidence query
        string sqlQuery = SELECT_ALL + "FROM Product" + " WHERE ID = @param";
        dbcmd.CommandText = sqlQuery;
        // Requires % around search word to look for it in any substring of the products' names
        dbcmd.Parameters.Add(new SqliteParameter("@param", ID));

        List<String> return_items = new List<String>();

        DBItem aux = new DBItem();

        IDataReader reader = dbcmd.ExecuteReader();
        while (reader.Read())
        {
            // Get the variables form the entry of the reader
            int id = reader.GetInt32(0);
            string name = reader.GetString(1);
            float width = reader.GetFloat(2);
            float height = reader.GetFloat(3);
            float depth = reader.GetFloat(4);

            Debug.Log("found = " + name);

            aux.ID = id;
            aux.name = name;
            aux.width = width;
            aux.height = height;
            aux.depth = depth;
            
        }

        BoxJSON found = new BoxJSON(aux);

        // DB Closing
        reader.Close();
        reader = null;
        dbcmd.Dispose();
        dbcmd = null;
        dbconn.Close();
        dbconn = null;


        return found;
    }

    public DB SearchItemByName(string search)
    {
        // ';UPDATE Product SET NAME = 'Troll1' WHERE ID = 1;-- // ';DROP TABLE;-- // SQL Injection tests

        dbconn = (IDbConnection)new SqliteConnection("URI=file:" + DB_PATH); // the connection requires that prefix
        dbconn.Open(); //Open connection to the database.
        IDbCommand dbcmd = dbconn.CreateCommand();
        
        // Partial coincidence query
        string sqlQuery = SELECT_ALL + "FROM Product" + " WHERE NAME LIKE @param";
        dbcmd.CommandText = sqlQuery;
        // Requires % around search word to look for it in any substring of the products' names
        dbcmd.Parameters.Add(new SqliteParameter("@param", "%"+search+"%"));

        DB returnDB = new DB();
        List<DBItem> cntents = new List<DBItem>();

        IDataReader reader = dbcmd.ExecuteReader();
        while (reader.Read())
        {
            // Get the variables form the entry of the reader
            int id = reader.GetInt32(0);
            string name = reader.GetString(1);
            float width = reader.GetFloat(2);
            float height = reader.GetFloat(3);
            float depth = reader.GetFloat(4);

            DBItem temp = new DBItem();

            Debug.Log("found = " + name);

            temp.ID = id;
            temp.name = name;
            temp.width = width;
            temp.height = height;
            temp.depth = depth;

            cntents.Add(temp);
        }

        returnDB.contents = cntents.ToArray();

        // DB Closing
        reader.Close();
        reader = null;
        dbcmd.Dispose();
        dbcmd = null;
        dbconn.Close();
        dbconn = null;


        return returnDB;
    }

    public DB ReadFullDB()
    {
        if (full_db != null)
        {
            return full_db;
        }
        else
        {
            Debug.LogError("Attempted to fetch empty full db");
            return null;
        }
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