using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using System.Collections;

public class ChangeSortingWindow : EditorWindow {

  
    public int sortingLayerSelected = 0;
    public static TileMapEditor m_requestingMap;

    public static void CreateChangeLayoutOptions(TileMapEditor map)
    {
        m_requestingMap = map;

        var window = GetWindow(typeof(ChangeSortingWindow));

        window.ShowUtility();

    }

    void OnGUI()
    {
        SortingLayer[] sortingLayers = SortingLayer.layers;
        List<string> sortingLayerList = new List<string>();

        foreach (SortingLayer sl in sortingLayers)
        {

            sortingLayerList.Add(sl.name);

        }

        //sortingLayers.GetEnumerator() = (ENUM_SORT_LAYER)EditorGUILayout.EnumPopup("Sorting Layer: ", sortingLayers.GetEnumerator());
        sortingLayerSelected = EditorGUILayout.Popup(sortingLayerSelected, sortingLayerList.ToArray());
        //Debug.Log(sortingLayerList.ToArray()[1]);
        EditorGUILayout.Space();

        GUILayoutOption[] buttonOptions = { GUILayout.Width(100), GUILayout.Height(30) };

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Change", buttonOptions))
        {

            m_requestingMap.ChangeSortingLayerTo(sortingLayerList[sortingLayerSelected]);
          
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

            this.Close();
        }
        if (GUILayout.Button("Cancel", buttonOptions))
        {

            this.Close();
        }
        EditorGUILayout.EndHorizontal();

    }
    
}
