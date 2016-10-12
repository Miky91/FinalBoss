using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;

public class NewTileMapMenu {

    [MenuItem("FinalBoss/New Tile Map",false,1)]
    public static void CreateTileMap()
    {

      
        var window = EditorWindow.GetWindow(typeof(NewTileMapOptions));

        window.ShowUtility();
        
    }
}

public class NewTileMapOptions : EditorWindow{

    public enum ENUM_SORT_LAYER
    {
        Background,
        Foreground
    }

    public ENUM_SORT_LAYER currentSortingLayer;
    public string mapName = "Nombre del mapa";
    public int sortingLayerSelected = 0;


    void OnGUI()
    {
        SortingLayer[] sortingLayers = SortingLayer.layers;
        List<string> sortingLayerList =  new List<string>();

        foreach (SortingLayer sl in sortingLayers)
        {

            sortingLayerList.Add(sl.name);

        }

        EditorGUILayout.LabelField("Map Name: ");
        mapName = EditorGUILayout.TextField(mapName);

        EditorGUILayout.Space();

        //sortingLayers.GetEnumerator() = (ENUM_SORT_LAYER)EditorGUILayout.EnumPopup("Sorting Layer: ", sortingLayers.GetEnumerator());
        sortingLayerSelected = EditorGUILayout.Popup(sortingLayerSelected, sortingLayerList.ToArray());
        //Debug.Log(sortingLayerList.ToArray()[1]);
        EditorGUILayout.Space();

        GUILayoutOption[] buttonOptions = { GUILayout.Width(100), GUILayout.Height(30) };

        if (GUILayout.Button("Create", buttonOptions))
        {

            GameObject go = new GameObject(mapName + "(" + sortingLayerList[sortingLayerSelected] + ")");
            go.AddComponent<TileMap>();
            go.GetComponent<TileMap>().tileSortingLayer = sortingLayerList[sortingLayerSelected];
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

            this.Close();
        }

    }
}
