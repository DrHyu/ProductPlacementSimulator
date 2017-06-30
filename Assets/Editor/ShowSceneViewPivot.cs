using UnityEngine;
using UnityEditor;
using System.Collections;

public class ShowSceneViewPivot : EditorWindow
{
    [MenuItem("Tools/ShowSceneViewPivot")]
    static void Init()
    {
        GetWindow<ShowSceneViewPivot>();
    }
    public bool drawCube = true;
    public float size = 2.0f;

    void OnEnable()
    {
        SceneView.onSceneGUIDelegate += OnSceneGUI;
    }

    void OnDisable()
    {
        SceneView.onSceneGUIDelegate -= OnSceneGUI;
    }

    void OnSceneGUI(SceneView aView)
    {
        if (drawCube)
            Handles.CubeCap(0, aView.pivot, Quaternion.identity, size);
        Repaint();
    }

    void OnGUI()
    {
        GUILayout.Label("Just keep this window open");
        drawCube = GUILayout.Toggle(drawCube, "Draw SceneView pivot", "Button");
        size = GUILayout.HorizontalSlider(size, 0.1f, 10.0f);
        if (SceneView.lastActiveSceneView != null)
            GUILayout.Label("PivotPos: " + SceneView.lastActiveSceneView.pivot);
        if (GUI.changed)
        {
            SceneView.RepaintAll();
        }
    }
}