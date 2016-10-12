using UnityEngine;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using UnityEditor;


[CustomEditor(typeof(TileMap))]
public class TileMapEditor : Editor
{

    public TileMap m_tileMap;

    private int mapX = 20;
    private int mapY = 10;

    List<int> NorthBorder = new List<int>();
    List<int> EastBorder = new List<int>();
    List<int> WestBorder = new List<int>();
    List<int> SouthBorder = new List<int>();

    private const int MAX_MAP_SIZE = 200;

    public struct TileCollider{
        public bool isCollider;
        public bool alreadyTakenIntoAccount;
        public GameObject tileGameObject;
    }

    private TileCollider[] colliderMatrix;

    private List<int> southTilesFromList;
    private List<int> eastTilesFromList;
    
    TileBrush m_brushInstance;
    Vector3 mouseHitPos;

    bool mouseOnMap{
        get
        {
            return mouseHitPos.x > 0 && mouseHitPos.x < m_tileMap.gridSize.x && mouseHitPos.y < 0 && mouseHitPos.y > -m_tileMap.gridSize.y;
        }

    }


    bool tileOnMap(float posX, float posY)
    {
        
        Vector2 gridGO = m_tileMap.getGridPosition();

        float rightBorder = gridGO.x + m_tileMap.gridSize.x;
        float upperBorder = gridGO.y;
        float leftBorder = gridGO.x;
        float lowerBorder = gridGO.y - m_tileMap.gridSize.y;

        if (!((posX > leftBorder && posX < rightBorder) && (posY > lowerBorder && posY < upperBorder)))
        {
            return false;
        }
        return true;

    }

    public override void OnInspectorGUI()
    {
        EditorGUILayout.BeginVertical();
        EditorGUILayout.Space();
        mapX = (int) m_tileMap.mapSize.x;
        mapY = (int) m_tileMap.mapSize.y;
        

        //se guarda el tamaño antiguo por si ha habido un cambio recalcularlo
        Vector2 oldSize = m_tileMap.mapSize;

        EditorGUILayout.LabelField("Map Size(Max size "+ MAX_MAP_SIZE +"x"+ MAX_MAP_SIZE +")");
        mapX = Mathf.Abs(EditorGUILayout.IntField("X", mapX));
        if (mapX > MAX_MAP_SIZE)
            mapX = MAX_MAP_SIZE;

        mapY = Mathf.Abs(EditorGUILayout.IntField("Y", mapY));
        if (mapY > MAX_MAP_SIZE)
            mapY = MAX_MAP_SIZE;

        m_tileMap.mapSize = new Vector2(mapX, mapY);

        //m_tileMap.mapSize = EditorGUILayout.Vector2Field("MapSize", m_tileMap.mapSize);

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Sorting layer:", m_tileMap.tileSortingLayer);

        EditorGUILayout.Space();



        if(m_tileMap.mapSize != oldSize)
        {
            updateCalculations();
        }

        var oldTexture = m_tileMap.tileAtlas;

        //el typeof es para filtar que solo acepte campos texture2d
        //el false es para que coja de los assets y no de la escena
        m_tileMap.tileAtlas = (Texture2D) EditorGUILayout.ObjectField("Tile Atlas:", m_tileMap.tileAtlas, typeof(Texture2D), false);

        if(oldTexture != m_tileMap.tileAtlas)
        {
            updateCalculations();
            m_tileMap.selectedSpriteID = 1;
            destroyBrush();
            createBrush();
            //TilePickerWindow.GetWindow<TilePickerWindow>().Repaint();

        }


        if (m_tileMap.tileAtlas == null)
        {
            EditorGUILayout.HelpBox("A texture is not selected", MessageType.Warning);
        }
        else
        {
            EditorGUILayout.LabelField("Tile Size:", m_tileMap.TileSize.x + "x" + m_tileMap.TileSize.y);
            EditorGUILayout.LabelField("Grid Size in units:", m_tileMap.gridSize.x + "x" + m_tileMap.gridSize.y);
            EditorGUILayout.LabelField("Pixels to units:", m_tileMap.pixelsToUnits.ToString());
            updateBrush(m_tileMap.getCurrentTileSprite(), m_tileMap.tileSortingLayer);


            if (GUILayout.Button("Change Sorting Layer"))
                OpenChangeSortingWindow();


            if (GUILayout.Button("Put colliders/effectors on this map"))
            {
                PutCollidersToMap();
            }

            if (GUILayout.Button("Remove colliders/effectors on this map"))
                RemoveCollidersOnMap();


            if(GUILayout.Button("Clear Tiles"))
            {
                if (EditorUtility.DisplayDialog("Erase tiles", "Do you want to ERASE all the tiles?", "Yes", "No"))
                    ClearMap();                
            }
        }

        EditorGUILayout.EndVertical();

    }

 

    void OnEnable()
    {
        //target es objeto que está en el inspector
        m_tileMap = target as TileMap;

        //cambia la herramienta que está utilizando la persona
        Tools.current = Tool.View;

        if(!m_tileMap.tilesGameObjectContainer)
        {
            var go = new GameObject("tilesGameObjectContainer");
            go.transform.SetParent(m_tileMap.transform);
            go.transform.position = Vector3.zero;

            m_tileMap.tilesGameObjectContainer = go;
        }

        if(m_tileMap.tileAtlas)
        {
            updateCalculations();
            newBrush();
        }

    }

    void OnDisable()
    {
        destroyBrush();
    }

 
    void OnSceneGUI()
    {
        if(m_brushInstance)
        {
            UpdateHitPosition();
            moveBrush();
            if(m_tileMap.tileAtlas && mouseOnMap)
            {
                Event current = Event.current;
                if (current.shift && EditorWindow.mouseOverWindow.ToString() == " (UnityEditor.SceneView)")
                {                 
                    drawTile(m_brushInstance.tileID.ToString());
                    drawTiles();
                }
                else if(current.alt)
                {
                    removeTile();
                }
            }
        }
    }

    private void updateCalculations()
    {
        //path donde está la textura
        var path = AssetDatabase.GetAssetPath(m_tileMap.tileAtlas);
        m_tileMap.spriteReferences = AssetDatabase.LoadAllAssetsAtPath(path);

        //el [0] siempre va a ser el atlas
        var sprite = (Sprite)m_tileMap.spriteReferences[1];
        var width = sprite.textureRect.width;
        var height = sprite.textureRect.height;

        m_tileMap.TileSize = new Vector2(width, height);

        //magia negra para calcular los pixelToUnits puestos al importar la textura
        m_tileMap.pixelsToUnits = (int)(sprite.rect.width / sprite.bounds.size.x);

        m_tileMap.gridSize = new Vector2((width / m_tileMap.pixelsToUnits) * m_tileMap.mapSize.x, (height / m_tileMap.pixelsToUnits) * m_tileMap.mapSize.y);

        //EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

        
        colliderMatrix = new TileCollider[(int)m_tileMap.mapSize.x * (int)m_tileMap.mapSize.y];
    
        updateBorders();


    }

    private void updateBorders()
    {
        NorthBorder.Clear();
        EastBorder.Clear();
        WestBorder.Clear();
        SouthBorder.Clear();
 
        //north border
        //0 hasta tamaño de X
        for(int i = 0; i < m_tileMap.mapSize.x;i++)
        {
            NorthBorder.Add(i);
        }

        //west border
        //0 hasta i * tamaño de x ---- 0 <= i < y

        for (int i = 0; i < m_tileMap.mapSize.y; i++)
        {
            WestBorder.Add(i * (int) m_tileMap.mapSize.x);
        }


        //East border
        // (i * tamaño de x) - 1 ------ 1<= i <= y
        for (int i = 1; i <= m_tileMap.mapSize.y; i++)
        {
            EastBorder.Add(i * (int) m_tileMap.mapSize.x - 1);
        }


        //South border
        //tamaño X * (y-1) hasta x*y - 1
        for (int i = (int) m_tileMap.mapSize.x * ((int)m_tileMap.mapSize.y - 1); i <= (int)m_tileMap.mapSize.x * (int)m_tileMap.mapSize.y - 1; i++)
        {
            SouthBorder.Add(i);
        }

    }

    private void createBrush()
    {
        Sprite currentTileBrush = m_tileMap.getCurrentTileSprite();
        if(currentTileBrush != null)
        {
            GameObject go = new GameObject("Brush");
            go.transform.SetParent(m_tileMap.transform);

            m_brushInstance = go.AddComponent<TileBrush>();

            m_brushInstance.m_spriteRenderer = go.AddComponent<SpriteRenderer>();
            m_brushInstance.m_spriteRenderer.sortingOrder = 1000;

            int pixelsToUnits = m_tileMap.pixelsToUnits;
            m_brushInstance.brushSize = new Vector2(currentTileBrush.textureRect.width / pixelsToUnits,
                                        currentTileBrush.textureRect.height / pixelsToUnits);
            m_brushInstance.updateBrush(currentTileBrush, m_tileMap.tileSortingLayer);
        }

           
    }
    private void newBrush()
    {
        if (!m_brushInstance)
            createBrush();
    }
    private void destroyBrush()
    {
        if (m_brushInstance)
            DestroyImmediate(m_brushInstance.gameObject);
    }

    public void updateBrush(Sprite tileToDraw, string newTileSortingLayer)
    {
        if(m_brushInstance)
        {
            m_brushInstance.updateBrush(tileToDraw, newTileSortingLayer);

        }

    }



    private void UpdateHitPosition()
    {
        Plane p = new Plane(m_tileMap.transform.TransformDirection(Vector3.forward), Vector3.zero);
        var ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

        var hit = Vector3.zero;
        float dist = 0f;

        if(p.Raycast(ray, out dist))
        {
         
            hit = ray.origin + ray.direction.normalized * dist;
        }
        mouseHitPos = m_tileMap.transform.InverseTransformPoint(hit);

    }
    private void moveBrush()
    {
        float tileSize = m_tileMap.TileSize.x / m_tileMap.pixelsToUnits;
        float x = Mathf.Floor(mouseHitPos.x / tileSize) * tileSize;
        float y = Mathf.Floor(mouseHitPos.y / tileSize) * tileSize;

        int row = (int)Mathf.Round(x / tileSize);
        int column = (int)Mathf.Round(Mathf.Abs(y / tileSize) - 1);

        if (!mouseOnMap)
            return;

        var id = (int)((column * m_tileMap.mapSize.x) + row);
        m_brushInstance.tileID = id;

        x += m_tileMap.transform.position.x + tileSize / 2;
        y += m_tileMap.transform.position.y + tileSize / 2;

        m_brushInstance.transform.position = new Vector3(x, y, m_tileMap.transform.position.z);
    }

    void drawTile(string id)
    {

        if (m_brushInstance.m_spriteRenderer.sprite == null)
            return;

        var posX = m_brushInstance.transform.position.x;
        var posY = m_brushInstance.transform.position.y;

        GameObject tile = GameObject.Find(m_tileMap.name + "/tilesGameObjectContainer/Tile_" + id);

        if(tile == null)
        {
            tile = new GameObject("Tile_" + id);
            tile.transform.SetParent(m_tileMap.tilesGameObjectContainer.transform);
            tile.transform.position = new Vector3(posX, posY, 0);
            tile.AddComponent<SpriteRenderer>();
            tile.GetComponent<SpriteRenderer>().sortingLayerName = m_brushInstance.tileSortingLayer;
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            //colliderMatrix[System.Int32.Parse(id)].tileGameObject = tile.gameObject;
    
        }
        tile.GetComponent<SpriteRenderer>().sprite = m_brushInstance.m_spriteRenderer.sprite;
    }

    void drawTiles()
    {
        int mainId = m_brushInstance.tileID;

        foreach (Tile mytile in m_tileMap.additionalSelectedTilesID)
        {

            var posX = m_brushInstance.transform.position.x + (mytile.m_offsetFromMain.x * m_tileMap.TileSize.x * 0.01f);
            var posY = m_brushInstance.transform.position.y + (mytile.m_offsetFromMain.y * m_tileMap.TileSize.y * -0.01f);

            if (tileOnMap(posX,posY))
            {
                 int id =  mainId + (int) mytile.m_offsetFromMain.x + ((int)mytile.m_offsetFromMain.y * (int)m_tileMap.mapSize.x);

                GameObject tile = GameObject.Find(m_tileMap.name + "/tilesGameObjectContainer/Tile_" + id);

                if (tile == null)
                {
                    tile = new GameObject("Tile_" + id);
                    tile.transform.SetParent(m_tileMap.tilesGameObjectContainer.transform);
                    tile.transform.position = new Vector3(posX, posY, 0);
                    tile.AddComponent<SpriteRenderer>();
                    tile.GetComponent<SpriteRenderer>().sortingLayerName = m_brushInstance.tileSortingLayer;
                    //colliderMatrix[id].tileGameObject = tile.gameObject;

                }

                //tile.GetComponent<SpriteRenderer>().sprite = m_brushInstance.m_spriteRenderer.sprite;
                tile.GetComponent<SpriteRenderer>().sprite = m_tileMap.getTileSpriteFromID(mytile.m_spriteID);
            }           
        }
    }

    void removeTile()
    {
        var id = m_brushInstance.tileID.ToString();
        GameObject tile = GameObject.Find(m_tileMap.name + "/tilesGameObjectContainer/Tile_" + id);

        if(tile != null)
        {
            DestroyImmediate(tile);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            int idInteger = System.Int32.Parse(id);
            colliderMatrix[idInteger].isCollider = false;
            colliderMatrix[idInteger].alreadyTakenIntoAccount = false;
            colliderMatrix[idInteger].tileGameObject = null;
        }

    }

    void ClearMap()
    {
        for (var i = 0; i < m_tileMap.tilesGameObjectContainer.transform.childCount; i++)
        {
            Transform t = m_tileMap.tilesGameObjectContainer.transform.GetChild(i);
            DestroyImmediate(t.gameObject);
            i--;
        }
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        resetColliderMatrix();
    }

    void PutCollidersToMap()
    {
        resetColliderMatrix();
        int tileId;

        //Guardar los gameObject de todos los tiles existentes.
        //Tarda más que pillarlos al momento de crearlos, pero permite poner los colliders a cualquier mapa sin 
        //tener que se creado desde que se añadió esto
        for (var i = 0; i < m_tileMap.tilesGameObjectContainer.transform.childCount; i++)
        {
            Transform t = m_tileMap.tilesGameObjectContainer.transform.GetChild(i);

            tileId = System.Int32.Parse(t.gameObject.name.Remove(0, 5));

            colliderMatrix[tileId].tileGameObject = t.gameObject;

        }
        for (var i = 0; i < m_tileMap.tilesGameObjectContainer.transform.childCount; i++)
        {
            Transform t = m_tileMap.tilesGameObjectContainer.transform.GetChild(i);

            tileId = System.Int32.Parse(t.gameObject.name.Remove(0, 5));

            //si no tiene effector o box collider 
            if (!t.gameObject.GetComponent<PlatformEffector2D>() || !t.gameObject.GetComponent<BoxCollider2D>())
            {
                //solo se pone si es un tile externo
                bool isWall = false;
                if(isAnOutsideTile(tileId, ref isWall))
                {
                    
                    if (!t.gameObject.GetComponent<PlatformEffector2D>())
                    {
                        t.gameObject.AddComponent<PlatformEffector2D>();
                        PlatformEffector2D effector = t.gameObject.GetComponent<PlatformEffector2D>();
                        effector.useOneWay = false;
                        effector.useColliderMask = false;
                        effector.surfaceArc = 180;
                        effector.sideArc = 70;
                    }

                    if (!t.gameObject.GetComponent<BoxCollider2D>())
                    {                        
                        t.gameObject.AddComponent<BoxCollider2D>();
                        t.gameObject.GetComponent<BoxCollider2D>().usedByEffector = true;           
                        
                    }
                    
                    //if (isWall)
                    //    t.tag = "Pared";

                    colliderMatrix[tileId].isCollider = true;
                }                
            }          
        }
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        ResizeColliders();

    }

    void RemoveCollidersOnMap()
    {
        for (var i = 0; i < m_tileMap.tilesGameObjectContainer.transform.childCount; i++)
        {
            Transform t = m_tileMap.tilesGameObjectContainer.transform.GetChild(i);

            t.tag = "Untagged";
            
            if (t.gameObject.GetComponent<PlatformEffector2D>())
                DestroyImmediate(t.gameObject.GetComponent<PlatformEffector2D>());

            if (t.gameObject.GetComponent<BoxCollider2D>())
                DestroyImmediate(t.gameObject.GetComponent<BoxCollider2D>());
        }
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

        resetColliderMatrix();
    }

    private List<int> getSouthTilesFrom(int id)
    {
        southTilesFromList.Clear();
        TileCollider tileCol;
        TileCollider southTileCol;

        tileCol.tileGameObject = null;
        southTileCol.tileGameObject = null;
        int k = id;
        bool finalTile = false;

        while (!finalTile)
        {
            tileCol = colliderMatrix[k];

            if (!tileCol.isCollider)
                break;

            if (tileCol.alreadyTakenIntoAccount)
                break;

            int southId = getSouthTileID(k);

            if (southId >= colliderMatrix.Length)
            {
                finalTile = true;
                southTilesFromList.Insert(southTilesFromList.Count, k);
                break;
            }

            southTileCol = colliderMatrix[getSouthTileID(k)];

            if (southTileCol.isCollider && !southTileCol.alreadyTakenIntoAccount)
            {
                southTilesFromList.Insert(southTilesFromList.Count, k);
            }
            else
            {
                southTilesFromList.Insert(southTilesFromList.Count, k);
                finalTile = true;

            }

            k = getSouthTileID(k);
        }
        return southTilesFromList;
    }


    private List<int> getEastTilesFrom(int id)
    {
        eastTilesFromList.Clear();
        TileCollider tileCol;
        TileCollider eastTileCol;

        tileCol.tileGameObject = null;
        eastTileCol.tileGameObject = null;
        int k = id;
        bool finalTile = false;
        
        while (!finalTile)
        {
            tileCol = colliderMatrix[k];

            if (!tileCol.isCollider)
                break;

            if (tileCol.alreadyTakenIntoAccount)
                break;

            if(EastBorder.Contains(k))
            {
                eastTilesFromList.Insert(eastTilesFromList.Count, k);
                break;
            }
            int eastId = getEastTileID(k);

            if (eastId >= colliderMatrix.Length)
            {
                finalTile = true;
                eastTilesFromList.Insert(eastTilesFromList.Count, k);

                break;
            }

            eastTileCol = colliderMatrix[getEastTileID(k)];

            if (eastTileCol.isCollider && !eastTileCol.alreadyTakenIntoAccount)
            {
                
                eastTilesFromList.Insert(eastTilesFromList.Count, k);                
            }
            else
            {
                eastTilesFromList.Insert(eastTilesFromList.Count, k);
                finalTile = true;

            }

            k = getEastTileID(k);
        }
        return eastTilesFromList;
    }

    private void StrechColliderList(List<int> colliderList,bool isSouthList)
    {
        int collidersDeleted = 0;
        BoxCollider2D coll2D = null;
        TileCollider tileCol;

        for (int n = 0; n < colliderList.Count; n++)
        {
            int tileId = colliderList[n];

            //Si ya estoy en el último no se elimna, sino se expande
            if (n == colliderList.Count - 1)
            {
                tileCol = colliderMatrix[tileId];
                if (tileCol.tileGameObject != null)
                {
                    coll2D = tileCol.tileGameObject.GetComponent<BoxCollider2D>();
                    colliderMatrix[tileId].alreadyTakenIntoAccount = true;
                    if (collidersDeleted != 0)
                    {
                        for (int j = 0; j < collidersDeleted; j++)
                        {
                            if (isSouthList)
                            {
                                coll2D.offset = new Vector2(coll2D.offset.x, coll2D.offset.y + 0.32f / 2f);
                                coll2D.size = new Vector2(coll2D.size.x, coll2D.size.y + 0.32f);
                                coll2D.tag = "Pared";
                            }
                            else
                            {
                                coll2D.offset = new Vector2(coll2D.offset.x - 0.32f / 2f, coll2D.offset.y);
                                coll2D.size = new Vector2(coll2D.size.x + 0.32f, coll2D.size.y);
                            }
                           
                        }
                    }
                }
            }
            else
            {
                collidersDeleted++;
                colliderMatrix[tileId].alreadyTakenIntoAccount = true;
                if (colliderMatrix[tileId].tileGameObject != null)
                {
                    DestroyImmediate(colliderMatrix[tileId].tileGameObject.GetComponent<BoxCollider2D>());
                    DestroyImmediate(colliderMatrix[tileId].tileGameObject.GetComponent<PlatformEffector2D>());

                }          
               
            }
        }           
    }

    private void ResizeColliders()
    {            
        southTilesFromList = new List<int>();
        eastTilesFromList = new List<int>();
       
        for(int i = 0; i < colliderMatrix.Length;i++)
        {
            if (colliderMatrix[i].isCollider)
            {
                southTilesFromList = getSouthTilesFrom(i);
                eastTilesFromList = getEastTilesFrom(i);

                if (southTilesFromList.Count >= eastTilesFromList.Count)
                    StrechColliderList(southTilesFromList, true);
                else
                    StrechColliderList(eastTilesFromList, false);
            }                            
        }    
    }

    bool isBorder(int tileId)
    {
        
        if (NorthBorder.Contains(tileId) || SouthBorder.Contains(tileId) || EastBorder.Contains(tileId) || WestBorder.Contains(tileId)) 
            return true;

        return false;
    }

    private int getNorthTileID(int tileId)
    {
        return tileId - (int)m_tileMap.mapSize.x;
    }
    private int getSouthTileID(int tileId)
    {
        return tileId + (int)m_tileMap.mapSize.x;
    }
    private int getEastTileID(int tileId)
    {
        return tileId + 1;
    }
    private int getWestTileID(int tileId)
    {
        return tileId - 1;
    }

    bool isAnOutsideTile(int tileId, ref bool isWall)
    {
        isWall = false;
        int norte = getNorthTileID(tileId);
        int sur = getSouthTileID(tileId);
        int este = getEastTileID(tileId);
        int oeste = getWestTileID(tileId);
        GameObject tileEste;
        GameObject tileOeste;
        GameObject tileNorte;
        GameObject tileSur;

        //Si pertence al borde es collider y pared seguro
        if (EastBorder.Contains(tileId) || WestBorder.Contains(tileId))
        {
            //isWall = true;
            return true;
        }

        //Si pertence al borde es collider y pared seguro
        if (NorthBorder.Contains(tileId) || SouthBorder.Contains(tileId))
        {
            //isWall = false;
            return true;
        }     

        //Como no están en los bordes podemos estar seguro que tileId no va a estár fuera del array
        tileNorte = colliderMatrix[norte].tileGameObject;
        tileEste = colliderMatrix[este].tileGameObject;
        tileOeste = colliderMatrix[oeste].tileGameObject;
        tileSur = colliderMatrix[sur].tileGameObject;
        


        //si alguno es null es que es el borde
        if(tileNorte == null || tileEste == null || tileOeste == null || tileSur == null )
        {
            if(tileEste == null || tileOeste == null)
            {
                //isWall = true;
            }
            return true;

        }

        return false;
    }

    void OpenChangeSortingWindow()
    {
        ChangeSortingWindow.CreateChangeLayoutOptions(this);
    }

    public void ChangeSortingLayerTo(string sortingLayer)
    {
        for (var i = 0; i < m_tileMap.tilesGameObjectContainer.transform.childCount; i++)
        {
            Transform t = m_tileMap.tilesGameObjectContainer.transform.GetChild(i);


            t.GetComponent<SpriteRenderer>().sortingLayerName = sortingLayer;
        }
        m_tileMap.tileSortingLayer = sortingLayer;
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
    }

    public void resetColliderMatrix()
    {
        for (int i = 0; i < colliderMatrix.Length; i++)
        {
            colliderMatrix[i].isCollider = false;
            colliderMatrix[i].alreadyTakenIntoAccount = false;
            colliderMatrix[i].tileGameObject = null;
        }
    }
}
