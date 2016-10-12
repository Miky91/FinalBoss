using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;


public class TilePickerWindow : EditorWindow {

    public enum ENUM_SCALE
    {
        x1,
        x2,
        x3,
        x4,
        x5
    }
    ENUM_SCALE currentScale;


    private Vector2 currentSelectedTile = Vector2.zero;
    private Vector2 mainSelectedSprteID = Vector2.zero;
    List <Vector2> additionalcurrentHighlightTiles = new List<Vector2>();

    public Vector2 scrollPosition = Vector2.zero;


    [MenuItem("FinalBoss/Open Tile Picker Window",false,100)]
    public static void OpenTilePickerWindow()
    {
        var window = EditorWindow.GetWindow(typeof(TilePickerWindow));
        var title = new GUIContent();
        title.text = "Tile Picker";
        window.titleContent = title;
    }

    void OnSelectionChange()
    {
        Repaint();
    }

    void OnGUI()
    {
        if(Selection.activeObject == null)        
            return;

        //Con selection se recoje lo que tiene seleccionado el usuario
        GameObject seleccion =  Selection.activeGameObject;

        TileMap tileMapScript = null;


        //se comprueba que es un gameObject en la escena
        if (seleccion)
            tileMapScript = seleccion.GetComponent<TileMap>();        

        if (tileMapScript)
        {
            Texture2D tileAtlas = tileMapScript.tileAtlas;
            if (tileAtlas)
            {
               // currentScale = (ENUM_SCALE)EditorGUILayout.EnumPopup("Zoom", currentScale);
                //int selectedScale = ((int)currentScale) + 1;
                
                //harcodeado para eliminar por ahora el zoom
                int selectedScale = 1;
                var newAtlasSize = new Vector2(tileAtlas.width, tileAtlas.height) * selectedScale;

                //TODO: este offset está petando la selesccion
                var offset = new Vector2(10,0);
               // currentSortingLayer = (ENUM_SORT_LAYER)EditorGUILayout.EnumPopup("Sorting layer", currentSortingLayer);
                //tileMapScript.tileSortingLayer = currentSortingLayer.ToString();


                //Dibujar Atlas

                //se resta 5 para tener en cuenta el tamaño del scrollBar en si
                Rect viewPortPosition = new Rect(0f,0f,position.width-5f, position.height-5f);
                Rect contentSize = new Rect(0, 0, newAtlasSize.x + offset.x, newAtlasSize.y + offset.y);
                
                scrollPosition =  GUI.BeginScrollView(viewPortPosition, scrollPosition,contentSize);
                GUI.DrawTexture(new Rect(offset.x,offset.y,newAtlasSize.x,newAtlasSize.y), tileAtlas);

                //tamaño del tile en función del escalado
                Vector2 scaledTileSize = tileMapScript.TileSize * selectedScale;
                Vector2 grid = new Vector2(newAtlasSize.x / scaledTileSize.x, newAtlasSize.y / scaledTileSize.y);

                Vector2 selectionPos = new Vector2(scaledTileSize.x * currentSelectedTile.x + offset.x,
                                                    scaledTileSize.y * currentSelectedTile.y + offset.y);


                Texture2D texturaTransparente = new Texture2D(1, 1);
                texturaTransparente.SetPixel(0, 0, new Color(0, 0.5f, 1f, 0.4f));
                texturaTransparente.Apply();

                //se clona el estilo por defecto para no tener que rehacerlo todo
                GUIStyle myStyle = new GUIStyle(GUI.skin.customStyles[0]);

                myStyle.normal.background = texturaTransparente;

                foreach (Vector2 tile in additionalcurrentHighlightTiles)
                {
                    GUI.Box(new Rect(tile.x, tile.y, scaledTileSize.x, scaledTileSize.y), "", myStyle);
                }

                GUI.Box(new Rect(selectionPos.x, selectionPos.y, scaledTileSize.x, scaledTileSize.y), "", myStyle);

                //se pilla la posición del ratoón
                Event currentEvent = Event.current;
                Vector2 mousePos = new Vector2(currentEvent.mousePosition.x, currentEvent.mousePosition.y);

                //mira si está pulsado el botón y si es el click izquierdo
                if (currentEvent.type == EventType.mouseDown && currentEvent.button == 0)
                {

                    //currentSelectedTile.x = Mathf.Floor((mousePos.x - offset.x + scrollPosition.x) / scaledTileSize.x);
                    //currentSelectedTile.y = Mathf.Floor((mousePos.y - offset.y + scrollPosition.y) / scaledTileSize.y);

                    currentSelectedTile.x = Mathf.Floor((mousePos.x - offset.x ) / scaledTileSize.x);
                    currentSelectedTile.y = Mathf.Floor((mousePos.y - offset.y ) / scaledTileSize.y);


                    if (currentSelectedTile.x > grid.x - 1)
                        currentSelectedTile.x = grid.x - 1;

                    if (currentSelectedTile.y > grid.y - 1)
                        currentSelectedTile.y = grid.y - 1;

                    if(currentSelectedTile.y < 0)
                        currentSelectedTile.y = 0;

                    int selectedSpriteID = ((int)(currentSelectedTile.x + (currentSelectedTile.y * grid.x) + 1));

                    if (currentEvent.shift)
                    {
                        if (!additionalcurrentHighlightTiles.Contains(selectionPos))
                        {
                            additionalcurrentHighlightTiles.Add(selectionPos);
                            
                            tileMapScript.additionalSelectedTilesID.Add(new Tile(selectedSpriteID, currentSelectedTile - mainSelectedSprteID));

                            
                          //  Debug.Log(currentSelectedTile);
                            //Debug.Log(selectedSpriteID);
                            //Debug.Log(mainSelectedSprteID);
                        }
                    }
                    else
                    {
                        additionalcurrentHighlightTiles.Clear();
                        tileMapScript.additionalSelectedTilesID.Clear();
                        tileMapScript.selectedSpriteID = selectedSpriteID;

                        mainSelectedSprteID = currentSelectedTile;
                    }

                    //Calcula el id en función de tamaño del grid
                    //La ventana le dice al mapa todo lo que se ha seleccionado

                    Repaint();
                }

                GUI.EndScrollView();

            }
        }
    }    
}
