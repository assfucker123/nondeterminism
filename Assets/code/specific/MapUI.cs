using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class MapUI : MonoBehaviour {

    /* Mask positioning:
     * 
     * Normal: (shows entire map, center of screen)
     * UI: x=0, y=0, width=1080, height=600, scale=1
     * textures: x=-540, y=-300, width=540, height=300
     * 
     * Only showing middle:
     * UI: changed width to 400 and height to 200.
     * textures: stayed the same
     * 
     * Moving the map on the HUD:
     * UI: change x and y.  Don't need to change anything else
     * 
     * Only showing left center:
     * UI: changed width to 400 and height to 200.  width0 = 1080, width1 = 400
     * textures: changed x to -200.  x0 = -540, x1 = -200
     * to make x', y' of the texture the center, position x = -x', y = -y'
     * 
     * 
     * */

    #region Public Static Properties/Functions

    public static MapUI instance { get { return _instance; } }

    public static int GRID_WIDTH = 60; // width in cells (practical width: 50)
    public static int GRID_HEIGHT = 50; // height in cells (practical height: 42)
    public static int CELL_WIDTH = 9; // width of a cell in pixels
    public static int CELL_HEIGHT = 6; // height of a cell in pixels

    public static string tempGridString = ""; // if Vars is being loaded and MapUI.instance doesn't exist, stored grid string will be placed here instead.  Will be automatically used in Awake();
    public static string tempIconString = "";
    public static bool canDisplayMapInHUD = true;
    
    // will automatically determine if there should be a fill or not
    public static int makeCell(Edge leftEdge, Edge topEdge, Edge rightEdge, Edge bottomEdge) {
        return makeCell(leftEdge, topEdge, rightEdge, bottomEdge,
            leftEdge != Edge.NO_WALL || topEdge != Edge.NO_WALL || rightEdge != Edge.NO_WALL || bottomEdge != Edge.NO_WALL);
    }
    public static int makeCell(Edge leftEdge, Edge topEdge, Edge rightEdge, Edge bottomEdge, bool fill) {
        int ret = 0;
        if (leftEdge != Edge.NO_WALL) {
            ret |= 1 << ((int)leftEdge - 1);
        }
        if (topEdge != Edge.NO_WALL) {
            ret |= 1 << 2 + ((int)topEdge - 1);
        }
        if (rightEdge != Edge.NO_WALL) {
            ret |= 1 << 4 + ((int)rightEdge - 1);
        }
        if (bottomEdge != Edge.NO_WALL) {
            ret |= 1 << 6 + ((int)bottomEdge - 1);
        }
        if (fill) {
            ret |= 1 << 8;
        }
        return ret;
    }
    
    // extracting properties from an int cell
    public static Edge leftEdge(int cell) {
        if ((cell & 1) != 0) return Edge.OPEN;
        if ((cell & 2) != 0) return Edge.WALL;
        return Edge.NO_WALL;
    }
    public static Edge topEdge(int cell) {
        if ((cell & 4) != 0) return Edge.OPEN;
        if ((cell & 8) != 0) return Edge.WALL;
        return Edge.NO_WALL;
    }
    public static Edge rightEdge(int cell) {
        if ((cell & 16) != 0) return Edge.OPEN;
        if ((cell & 32) != 0) return Edge.WALL;
        return Edge.NO_WALL;
    }
    public static Edge bottomEdge(int cell) {
        if ((cell & 64) != 0) return Edge.OPEN;
        if ((cell & 128) != 0) return Edge.WALL;
        return Edge.NO_WALL;
    }
    public static bool fill(int cell) {
        return (cell & 256) != 0;
    }
    
    #endregion

    #region Public Enums

    public enum Edge : int {
        NO_WALL = 0,
        OPEN = 1,
        WALL = 2
    }

    public enum Icon {
        NONE,
        CHAMBER,
        HEALTH_UPGRADE,
        BOOSTER, // wide upgrades like this should only be placed in the center of 2x2 rooms
        ORB0
    }

    public enum DisplayMode {
        MAP_PAGE,
        HUD_SLIDE_IN,
        HUD,
        HUD_SLIDE_OUT
    }

    #endregion

    #region Inspector Properties

    public Color topEdgeColor = Color.white;
    public Color bottomEdgeColor = Color.black;
    public Color fillColor = Color.magenta;
    public int topOpenGapWidth = 3;
    public int leftOpenGapHeight = 2;
    public Vector2 HUDPos = new Vector2(0, 0);
    public Vector2 HUDSize = new Vector2(400, 200);
    public float slideDuration = .5f;
    public GameObject iconChamberGameObject;
    public GameObject iconHealthUpgradeGameObject;
    public GameObject iconBoosterGameObject;
    public GameObject iconOrb0GameObject;
    public GameObject playerPositionGameObject;
    public GameObject selectorGreenGameObject;
    public AudioClip chamberSwitchSound;

    #endregion

    #region Public Properties/Functions

    public bool mapShowing { get { return mapRawImage.enabled; } }
    public bool mapFillShowing { get { return mapFillRawImage.enabled; } }

    // gets and sets the position of the center of the map.  (0, 0) is center of the screen
    public Vector2 position {
        get {
            return new Vector2(rectTransform.localPosition.x, rectTransform.localPosition.y);
        }
        set {
            rectTransform.localPosition = value;
        }
    }

    // gets and sets how big the mask is
    public Vector2 maskSize {
        get {
            return rectTransform.sizeDelta;
        }
        set {
            rectTransform.sizeDelta = value;
        }
    }
    // sets the mask to show everything
    public void fullMask() {
        maskSize = new Vector2(GRID_WIDTH * CELL_WIDTH * 2, GRID_HEIGHT * CELL_HEIGHT * 2);
    }

    public string iconsStr { get; private set; }

    // sets which cell coordinates are at the center of the map (using floats for easing)
    public void setMapCenter(float x, float y) {
        Vector2 pos0 = mapRawImage.GetComponent<RectTransform>().localPosition;
        Vector2 pos1 = new Vector2(x * -CELL_WIDTH * 2, y * -CELL_HEIGHT * 2);
        Vector2 diff = pos1 - pos0;
        Vector3 diff3 = new Vector3(diff.x, diff.y);

        mapRawImage.GetComponent<RectTransform>().localPosition += diff3;
        mapFillRawImage.GetComponent<RectTransform>().localPosition += diff3;
        if (playerPosition != null)
            playerPosition.GetComponent<RectTransform>().localPosition += diff3;
        foreach (MapIcon icon in icons) {
            icon.GetComponent<RectTransform>().localPosition += diff3;
        }
        if (selectorGreen != null) {
            selectorGreen.GetComponent<RectTransform>().localPosition += diff3;
        }

    }
    
    public void setPlayerPosition(int x, int y) {
        playerPositionGridX = x;
        playerPositionGridY = y;
        if (playerPosition == null) return;
        playerPosition.transform.localPosition = 
            (new Vector3((x + .5f) * CELL_WIDTH * 2, (y + .5f) * CELL_HEIGHT * 2)) +
            mapRawImage.GetComponent<RectTransform>().localPosition;
    }

    public void showMap(bool showFill = true) {
        mapRawImage.enabled = true;
        //mapFillRawImage.enabled = showFill;
        mapFillRawImage.enabled = true;
        if (showFill) {
            mapFillRawImage.color = new Color(1, 1, 1, 1);
        } else {
            mapFillRawImage.color = new Color(1, 1, 1, .3f);
        }
        
        playerPosition.GetComponent<Image>().enabled = true;
        iconsFromString(iconsStr);
    }
    public void hideMap() {
        if (!mapRawImage.enabled) return;
        mapRawImage.enabled = false;
        mapFillRawImage.enabled = false;
        iconsStr = iconsToString();
        clearIcons();
        playerPosition.GetComponent<Image>().enabled = false;
        selectorGreen.GetComponent<Image>().enabled = false;
    }
    public bool mapShown {  get { return mapRawImage.enabled; } }

    public void setMapPagePosition() {
        position = new Vector2(0, 0);
        fullMask();
        setMapCenter(GRID_WIDTH / 2, GRID_HEIGHT / 2);
        displayMode = DisplayMode.MAP_PAGE;
    }

    public void setTimeTreePagePosition(Vector2 pos, Vector2 size) {
        position = pos;
        maskSize = size;
        setMapCenter(GRID_WIDTH / 2, GRID_HEIGHT / 2);
        displayMode = DisplayMode.MAP_PAGE;
    }

    public DisplayMode displayMode { get; private set; }

    public bool inputEnabled = true;

    /// <summary>
    /// From a position in a room, returns what its position would be on the grid (returns int Vector2)
    /// </summary>
    public Vector2 gridPositionFromWorldPosition(int roomX, int roomY, Vector2 worldPos, float mapBoundsXMin, float mapBoundsYMin) {
        Vector2 posDiff = worldPos - new Vector2(mapBoundsXMin, mapBoundsYMin);
        Vector2 ret = new Vector2(roomX, roomY);
        ret.x += Mathf.FloorToInt(posDiff.x / CameraControl.ROOM_UNIT_WIDTH);
        ret.y += Mathf.FloorToInt(posDiff.y / CameraControl.ROOM_UNIT_HEIGHT);
        return ret;
    }
    public Vector2 gridPositionFromWorldPosition(int roomX, int roomY, Vector2 worldPos) {
        Rect bounds = CameraControl.getMapBounds();
        return gridPositionFromWorldPosition(roomX, roomY, worldPos, bounds.xMin, bounds.yMin);
    }

    public int gridGetCell(int x, int y) {
        return grid[y, x];
    }
    public bool gridIsEmpty(int x, int y) {
        return gridGetCell(x, y) == 0;
    }
    public bool gridIsEmpty(int x, int y, int roomWidth, int roomHeight) {
        for (int iy = 0; iy < roomHeight; iy++) {
            for (int ix = 0; ix < roomWidth; ix++) {
                if (!gridIsEmpty(x+ix, y+iy)) return false;
            }
        }
        return true;
    }
    public bool gridIsEmpty() {
        return gridIsEmpty(0, 0, CELL_WIDTH, CELL_HEIGHT);
    }
    public void gridSetCell(int x, int y, int cell, bool applyDrawing = true) {
        grid[y, x] = cell;

        // draw cell
        if (cell == 0) return;
        switch (leftEdge(cell)) {
        case Edge.WALL:
            for (int iy=0; iy < CELL_HEIGHT; iy++){
                mapTexture.SetPixel(x * CELL_WIDTH, y * CELL_HEIGHT + iy, topEdgeColor);
            }
            break;
        case Edge.OPEN:
            for (int iy = 0; iy < CELL_HEIGHT; iy++) {
                if ((CELL_HEIGHT - leftOpenGapHeight) / 2 <= iy && iy < (CELL_HEIGHT + leftOpenGapHeight) / 2)
                    mapTexture.SetPixel(x * CELL_WIDTH, y * CELL_HEIGHT + iy, Color.clear);
                else
                    mapTexture.SetPixel(x * CELL_WIDTH, y * CELL_HEIGHT + iy, topEdgeColor);
            }
            break;
        }
        switch (topEdge(cell)) {
        case Edge.WALL:
            for (int ix = 0; ix < CELL_WIDTH; ix++) {
                mapTexture.SetPixel(x * CELL_WIDTH + ix, (y+1) * CELL_HEIGHT - 1, topEdgeColor);
            }
            break;
        case Edge.OPEN:
            for (int ix = 0; ix < CELL_WIDTH; ix++) {
                if ((CELL_WIDTH - topOpenGapWidth) / 2 <= ix && ix < (CELL_WIDTH + topOpenGapWidth) / 2)
                    mapTexture.SetPixel(x * CELL_WIDTH + ix, (y + 1) * CELL_HEIGHT - 1, Color.clear);
                else
                    mapTexture.SetPixel(x * CELL_WIDTH + ix, (y + 1) * CELL_HEIGHT - 1, topEdgeColor);
            }
            break;
        }
        switch (rightEdge(cell)) {
        case Edge.WALL:
            for (int iy = 0; iy < CELL_HEIGHT; iy++) {
                mapTexture.SetPixel((x+1) * CELL_WIDTH - 1, y * CELL_HEIGHT + iy, bottomEdgeColor);
            }
            break;
        case Edge.OPEN:
            for (int iy = 0; iy < CELL_HEIGHT; iy++) {
                if ((CELL_HEIGHT - leftOpenGapHeight) / 2 <= iy && iy < (CELL_HEIGHT + leftOpenGapHeight) / 2)
                    mapTexture.SetPixel((x + 1) * CELL_WIDTH - 1, y * CELL_HEIGHT + iy, Color.clear);
                else
                    mapTexture.SetPixel((x + 1) * CELL_WIDTH - 1, y * CELL_HEIGHT + iy, bottomEdgeColor);
            }
            break;
        }
        switch (bottomEdge(cell)) {
        case Edge.WALL:
            for (int ix = 0; ix < CELL_WIDTH; ix++) {
                mapTexture.SetPixel(x * CELL_WIDTH + ix, y * CELL_HEIGHT, bottomEdgeColor);
            }
            break;
        case Edge.OPEN:
            for (int ix = 0; ix < CELL_WIDTH; ix++) {
                if ((CELL_WIDTH - topOpenGapWidth) / 2 <= ix && ix < (CELL_WIDTH + topOpenGapWidth) / 2)
                    mapTexture.SetPixel(x * CELL_WIDTH + ix, y * CELL_HEIGHT, Color.clear);
                else
                    mapTexture.SetPixel(x * CELL_WIDTH + ix, y * CELL_HEIGHT, bottomEdgeColor);
            }
            break;
        }
        if (fill(cell)) {
            for (int iy = 0; iy < CELL_HEIGHT; iy++) {
                for (int ix = 0; ix < CELL_WIDTH; ix++) {
                    mapFillTexture.SetPixel(x * CELL_WIDTH + ix, y * CELL_HEIGHT + iy, fillColor);
                }
            }
        }
        
        if (applyDrawing) {
            mapTexture.Apply();
            mapFillTexture.Apply();
        }

    }

    public void gridSetOpenLeftEdge(int x, int y, bool applyDrawing = false) {
        int cell = gridGetCell(x, y);
        int newCell = makeCell(Edge.OPEN, topEdge(cell), rightEdge(cell), bottomEdge(cell));
        gridSetCell(x, y, newCell, applyDrawing);
    }
    public void gridSetOpenTopEdge(int x, int y, bool applyDrawing = false) {
        int cell = gridGetCell(x, y);
        int newCell = makeCell(leftEdge(cell), Edge.OPEN, rightEdge(cell), bottomEdge(cell));
        gridSetCell(x, y, newCell, applyDrawing);
    }
    public void gridSetOpenRightEdge(int x, int y, bool applyDrawing = false) {
        int cell = gridGetCell(x, y);
        int newCell = makeCell(leftEdge(cell), topEdge(cell), Edge.OPEN, bottomEdge(cell));
        gridSetCell(x, y, newCell, applyDrawing);
    }
    public void gridSetOpenBottomEdge(int x, int y, bool applyDrawing = false) {
        int cell = gridGetCell(x, y);
        int newCell = makeCell(leftEdge(cell), topEdge(cell), rightEdge(cell), Edge.OPEN);
        gridSetCell(x, y, newCell, applyDrawing);
    }

    public string gridToString() {
        string str = "";
        for (int i = 0; i < grid.Length; i++) {
            str = str + grid[i / GRID_WIDTH, i % GRID_WIDTH] + ",";
        }
        return str;
    }

    public void gridFromString(string str) {
        clearMapTextures(false);
        if (str == "") {
            for (int i = 0; i < grid.Length; i++) {
                gridSetCell(i % GRID_WIDTH, i / GRID_WIDTH, 0, false);
            }
        } else {
            char[] delims = {','};
            string[] cells = str.Split(delims);
            for (int i = 0; i < grid.Length; i++) {
                gridSetCell(i % GRID_WIDTH, i / GRID_WIDTH, int.Parse(cells[i]), false);
            }
        }
        mapTexture.Apply();
        mapFillTexture.Apply();
    }

    public string iconsToString() {
        string str = "";
        for (int i = 0; i < icons.Count; i++) {
            MapIcon icon = icons[i];
            str += icon.toString();
            if (i != icons.Count - 1)
                str += ",";
        }
        return str;
    }
    public string iconsAppendToString(string str, MapIcon icon) {
        if (str == "")
            return icon.toString();
        return str + "," + icon.toString();
    }
    public void iconsFromString(string str) {
        clearIcons();
        iconsStr = ""; // since addIcon also adds to string
        if (str == "") return;
        char[] delims = { ',' };
        string[] iconStrs = str.Split(delims);
        for (int i = 0; i < iconStrs.Length; i++) {
            string iStr = iconStrs[i];
            int index0 = iStr.IndexOf("i") + 1;
            int index1 = iStr.IndexOf("x");
            Icon iconID = (Icon)(int.Parse(iStr.Substring(index0, index1 - index0)));
            index0 = iStr.IndexOf("x") + 1;
            index1 = iStr.IndexOf("y");
            int x = int.Parse(iStr.Substring(index0, index1 - index0));
            index0 = iStr.IndexOf("y") + 1;
            index1 = iStr.IndexOf("f");
            int y = int.Parse(iStr.Substring(index0, index1 - index0));
            index0 = iStr.IndexOf("f") + 1;
            int f = int.Parse(iStr.Substring(index0));
            addIcon(iconID, x, y, (f == 1));
        }
    }
    public void clearIcons() {
        foreach (MapIcon icon in icons) {
            Destroy(icon.gameObject);
        }
        icons.Clear();
    }

    /// <summary>
    /// Simply makes an empty room with walls on the border
    /// </summary>
    public void gridAddRoom(int x, int y, int roomWidth, int roomHeight) {
        if (roomWidth < 1 || roomHeight < 1) {
            Debug.LogError("Room invalid dimensions");
            return;
        }
        int[,] cells = new int[roomHeight, roomWidth];
        Edge le;
        Edge te;
        Edge re;
        Edge be;
        for (int iy = 0; iy < roomHeight; iy++) {
            for (int ix = 0; ix < roomWidth; ix++) {
                le = ix == 0 ? Edge.WALL : Edge.NO_WALL;
                te = iy == roomHeight - 1 ? Edge.WALL : Edge.NO_WALL;
                re = ix == roomWidth - 1 ? Edge.WALL : Edge.NO_WALL;
                be = iy == 0 ? Edge.WALL : Edge.NO_WALL;
                cells[iy, ix] = makeCell(le, te, re, be, true);
            }
        }
        gridAddRoom(x, y, roomWidth, roomHeight, cells);
    }

    public void gridAddRoom(int x, int y, int roomWidth, int roomHeight, int[,] cells) {
        for (int iy = 0; iy < roomHeight; iy++) {
            for (int ix = 0; ix < roomWidth; ix++) {
                gridSetCell(x + ix, y + iy, cells[iy, ix], false);
            }
        }
        mapTexture.Apply();
        mapFillTexture.Apply();
    }

    public void clearMapTextures(bool applyDrawing = true) {
        for (int x = 0; x < GRID_WIDTH * CELL_WIDTH; x++) {
            for (int y = 0; y < GRID_HEIGHT * CELL_HEIGHT; y++) {
                mapTexture.SetPixel(x, y, Color.clear);
                mapFillTexture.SetPixel(x, y, Color.clear);
            }
        }
        if (applyDrawing) {
            mapTexture.Apply();
            mapFillTexture.Apply();
        }
    }

    /// <summary>
    /// Adds a rectangular room to the map.
    /// </summary>
    /// <param name="x">Grid x coordinate of the lower left of the room.</param>
    /// <param name="y">Grid y coordinate of the lower left of the room.</param>
    /// <param name="roomWidth">width of the room, must be >0</param>
    /// <param name="roomHeight">height of the room, must be >0</param>
    /// <param name="openLeftEdges">for each iy in this array, cell (x, y+iy) will have an open left edge.</param>
    /// <param name="openTopEdges">for each ix in this array, cell (x+ix, y+roomHeight-1) will have an open top edge.</param>
    /// <param name="openRightEdges">for each iy in this array, cell (x+roomWidth-1, y+iy) will have an open right edge.</param>
    /// <param name="openBottomEdges">for each ix in this array, cell (x+ix, y) will have an open bottom edge.</param>
    public void gridAddRoom(int x, int y, int roomWidth, int roomHeight,
        int[] openLeftEdges, int[] openTopEdges, int[] openRightEdges, int[] openBottomEdges) {
        if (roomWidth < 1 || roomHeight < 1) {
            Debug.LogError("Room invalid dimensions");
            return;
        }
        int[,] cells = new int[roomHeight, roomWidth];
        for (int iy = 0; iy < roomHeight; iy++) {
            for (int ix = 0; ix < roomWidth; ix++) {
                Edge leftEdge = Edge.NO_WALL;
                Edge topEdge = Edge.NO_WALL;
                Edge rightEdge = Edge.NO_WALL;
                Edge bottomEdge = Edge.NO_WALL;
                if (ix == 0) {
                    if (contains(openLeftEdges, iy))
                        leftEdge = Edge.OPEN;
                    else leftEdge = Edge.WALL;
                }
                if (iy == 0) {
                    if (contains(openBottomEdges, ix))
                        bottomEdge = Edge.OPEN;
                    else bottomEdge = Edge.WALL;
                }
                if (ix == roomWidth - 1) {
                    if (contains(openRightEdges, iy))
                        rightEdge = Edge.OPEN;
                    else rightEdge = Edge.WALL;
                }
                if (iy == roomHeight - 1) {
                    if (contains(openTopEdges, ix))
                        topEdge = Edge.OPEN;
                    else topEdge = Edge.WALL;
                }
                
                cells[iy, ix] = makeCell(leftEdge, topEdge, rightEdge, bottomEdge, true);
            }
        }
        gridAddRoom(x, y, roomWidth, roomHeight, cells);
    }

    // adds icon to the map and appends it to iconsStr
    public MapIcon addIcon(Icon icon, int x, int y, bool found = false) {
        MapIcon mapIcon = null;
        switch (icon) {
        case Icon.CHAMBER:
            mapIcon = GameObject.Instantiate(iconChamberGameObject).GetComponent<MapIcon>();
            break;
        case Icon.HEALTH_UPGRADE:
            mapIcon = GameObject.Instantiate(iconHealthUpgradeGameObject).GetComponent<MapIcon>();
            break;
        case Icon.BOOSTER:
            mapIcon = GameObject.Instantiate(iconBoosterGameObject).GetComponent<MapIcon>();
            break;
        case Icon.ORB0:
            mapIcon = GameObject.Instantiate(iconOrb0GameObject).GetComponent<MapIcon>();
            break;
        }
        if (mapIcon == null) {
            return mapIcon;
        }
        mapIcon.transform.SetParent(transform, false);
        if (icon == Icon.CHAMBER) {
            mapIcon.transform.SetAsFirstSibling();
            transform.Find("mapFillRawImage").SetAsFirstSibling();
        } else {
            mapIcon.transform.SetAsLastSibling();
            if (playerPosition != null) {
                playerPosition.transform.SetAsLastSibling();
            }
        }
        mapIcon.x = x;
        mapIcon.y = y;
        mapIcon.transform.localPosition = iconLocalPosition(x, y, mapIcon.wideSprite);
        mapIcon.found = found;
        mapIcon.GetComponent<Image>().enabled = mapShowing;
        icons.Add(mapIcon);
        iconsStr = iconsAppendToString(iconsStr, mapIcon);
        return mapIcon;
    }
    public MapIcon getIcon(Icon icon) {
        foreach (MapIcon mapIcon in icons) {
            if (mapIcon.icon == icon)
                return mapIcon;
        }
        return null;
    }
    public MapIcon getIcon(Icon icon, int x, int y) {
        foreach (MapIcon mapIcon in icons) {
            if (mapIcon.icon == icon &&
                mapIcon.x == x &&
                mapIcon.y == y)
                return mapIcon;
        }
        return null;
    }
    public bool iconInPosition(Icon icon, int x, int y, bool checkIconsStr = true) {

        if (getIcon(icon, x, y) != null)
            return true;

        if (!checkIconsStr) return false;

        string str = iconsStr;
        if (str == "") return false;
        char[] delims = { ',' };
        string[] iconStrs = str.Split(delims);
        for (int i = 0; i < iconStrs.Length; i++) {
            string iStr = iconStrs[i];
            int index0 = iStr.IndexOf("i") + 1;
            int index1 = iStr.IndexOf("x");
            Icon iconID = (Icon)(int.Parse(iStr.Substring(index0, index1 - index0)));
            index0 = iStr.IndexOf("x") + 1;
            index1 = iStr.IndexOf("y");
            int iconX = int.Parse(iStr.Substring(index0, index1 - index0));
            index0 = iStr.IndexOf("y") + 1;
            index1 = iStr.IndexOf("f");
            int iconY = int.Parse(iStr.Substring(index0, index1 - index0));
            index0 = iStr.IndexOf("f") + 1;
            int iconFound = int.Parse(iStr.Substring(index0));

            if (icon == iconID && x == iconX && y == iconY)
                return true;
        }

        return false;
    }

    #endregion




    public void setSlectorGreenPosition(int gridX, int gridY) {
        selectorGreenGridX = gridX;
        selectorGreenGridY = gridY;
        selectorGreen.transform.localPosition = iconLocalPosition(selectorGreenGridX, selectorGreenGridY);
        selectorGreen.GetComponent<Image>().enabled = true;
    }

    float slideTime = 0;



    #region Private Event Funtions

    void Awake() {
        // singleton stuff
        if (_instance == null) {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        } else if (_instance != this) {
            Destroy(this);
            return;
        }
        rectTransform = GetComponent<RectTransform>();
        // create map textures
        mapTexture = new Texture2D(GRID_WIDTH * CELL_WIDTH, GRID_HEIGHT * CELL_HEIGHT);
        mapTexture.filterMode = FilterMode.Point;
        mapFillTexture = new Texture2D(GRID_WIDTH * CELL_WIDTH, GRID_HEIGHT * CELL_HEIGHT);
        mapFillTexture.filterMode = FilterMode.Point;
        clearMapTextures(true);
        //
        mapRawImage = transform.Find("mapRawImage").GetComponent<RawImage>();
        mapRawImage.texture = mapTexture;
        mapFillRawImage = transform.Find("mapFillRawImage").GetComponent<RawImage>();
        mapFillRawImage.texture = mapFillTexture;
        // creating player position
        if (playerPosition != null) {
            GameObject.Destroy(playerPosition);
            playerPosition = null;
        }
        playerPosition = GameObject.Instantiate(playerPositionGameObject) as GameObject;
        playerPosition.transform.SetParent(transform, false);
        playerPosition.GetComponent<Image>().enabled = false;
        setPlayerPosition(0, 0);
        // creating selector green
        if (selectorGreen != null) {
            GameObject.Destroy(selectorGreen);
            selectorGreen = null;
        }
        selectorGreen = GameObject.Instantiate(selectorGreenGameObject) as GameObject;
        selectorGreen.transform.SetParent(transform, false);
        selectorGreen.GetComponent<Image>().enabled = false;

        // set map from temp strings
        gridFromString(tempGridString);
        iconsStr = tempIconString;
        iconsFromString(iconsStr);
    }

    void Start() {
        
        // saves icon locations in iconsStr
        hideMap();

    }
	
	void Update() {
        

        slideTime += Time.unscaledDeltaTime;

        switch (displayMode) {
        case DisplayMode.MAP_PAGE:
            break;
        case DisplayMode.HUD_SLIDE_IN:
            maskSize = Utilities.easeLinearClamp(slideTime, Vector2.zero, HUDSize, slideDuration);
            if (slideTime >= slideDuration) {
                displayMode = DisplayMode.HUD;
            }
            break;
        case DisplayMode.HUD:
            break;
        case DisplayMode.HUD_SLIDE_OUT:
            maskSize = Utilities.easeLinearClamp(slideTime, HUDSize, -HUDSize, slideDuration);
            if (slideTime >= slideDuration) {
                hideMap();
                displayMode = DisplayMode.MAP_PAGE;
            }
            break;
        }

        if (mapShown && displayMode == DisplayMode.MAP_PAGE && inputEnabled) {
            
            // moving selector green
            if (selectorGreen.GetComponent<Image>().enabled) {
                MapIcon nextIcon = null;
                if (Keys.instance.upPressed) {
                    nextIcon = getNextChamberIcon(selectorGreenGridX, selectorGreenGridY, Direction.UP);
                } else if (Keys.instance.rightPressed) {
                    nextIcon = getNextChamberIcon(selectorGreenGridX, selectorGreenGridY, Direction.RIGHT);
                } else if (Keys.instance.downPressed) {
                    nextIcon = getNextChamberIcon(selectorGreenGridX, selectorGreenGridY, Direction.DOWN);
                } else if (Keys.instance.leftPressed) {
                    nextIcon = getNextChamberIcon(selectorGreenGridX, selectorGreenGridY, Direction.LEFT);
                }
                if (nextIcon != null) {
                    setSlectorGreenPosition(nextIcon.x, nextIcon.y);
                    SoundManager.instance.playSFXIgnoreVolumeScale(chamberSwitchSound);
                    mapPageSetChamberText(nextIcon);
                }

            } else {
                // if isn't visible, pressing any button will make it visible
                if (Keys.instance.upPressed || Keys.instance.rightPressed || Keys.instance.downPressed || Keys.instance.leftPressed ||
                    Keys.instance.confirmPressed) {
                    
                    // position where the chamber icon closest to the player is
                    MapIcon closestIcon = getClosestChamberIcon(playerPositionGridX, playerPositionGridY);
                    if (closestIcon != null) {
                        selectorGreen.GetComponent<Image>().enabled = true;
                        setSlectorGreenPosition(closestIcon.x, closestIcon.y);
                        mapPageSetChamberText(closestIcon);
                    }
                    
                }
            }

        }

	}

    // not working right.  possible future feature
    //public void setMapSlideIn() {
    //    if (!canDisplayMapInHUD) return;
    //    if (displayMode != DisplayMode.MAP_PAGE) return;

    //    displayMode = DisplayMode.HUD_SLIDE_IN;
    //    slideTime = 0;
    //    showMap(false);
    //    position = HUDPos;
    //    maskSize = new Vector2(0, 0);

    //}

    //public void setMapSlideOut() {
    //    if (!canDisplayMapInHUD) return;
    //    if (displayMode != DisplayMode.HUD) return;

    //    displayMode = DisplayMode.HUD_SLIDE_OUT;
    //    slideTime = 0;

    //}

    #endregion

    #region Private Misc.

    private static MapUI _instance = null;

    Texture2D mapTexture;
    Texture2D mapFillTexture;

    RawImage mapRawImage;
    RawImage mapFillRawImage;

    GameObject playerPosition;
    int playerPositionGridX = 0;
    int playerPositionGridY = 0;
    GameObject selectorGreen;
    int selectorGreenGridX = 0;
    int selectorGreenGridY = 0;

    RectTransform rectTransform;

    int[,] grid = new int[GRID_HEIGHT, GRID_WIDTH];
    List<MapIcon> icons = new List<MapIcon>();
    
    private bool contains(int[] intArr, int val) {
        foreach (int i in intArr) {
            if (i == val) return true;
        }
        return false;
    }

    enum Direction {
        LEFT, UP, RIGHT, DOWN
    }
    
    MapIcon getNextChamberIcon(int startGridX, int startGridY, Direction direction) {
        // get all chamber icons
        List<MapIcon> cIcons = new List<MapIcon>();
        foreach (MapIcon mapIcon in icons) {
            if (mapIcon.icon == Icon.CHAMBER &&
                (mapIcon.x != startGridX || mapIcon.y != startGridY))
                cIcons.Add(mapIcon);
        }
        if (cIcons.Count == 0)
            return null;

        Vector2 startGrid = new Vector2(startGridX, startGridY);
        MapIcon ret = null;
        float dist = 99999;
        float d;

        switch (direction) {
        case Direction.LEFT:
            foreach (MapIcon mapIcon in cIcons) {
                if (Utilities.pointInSector(new Vector2(mapIcon.x, mapIcon.y), startGrid, 99999, Mathf.PI, Mathf.PI / 2)) {
                    d = startGridX - mapIcon.x;
                    if (d < dist) {
                        dist = d;
                        ret = mapIcon;
                    }
                }
            }
            break;
        case Direction.DOWN:
            foreach (MapIcon mapIcon in cIcons) {
                if (Utilities.pointInSector(new Vector2(mapIcon.x, mapIcon.y), startGrid, 99999, -Mathf.PI/2, Mathf.PI / 2)) {
                    d = startGridY - mapIcon.y;
                    if (d < dist) {
                        dist = d;
                        ret = mapIcon;
                    }
                }
            }
            break;
        case Direction.RIGHT:
            foreach (MapIcon mapIcon in cIcons) {
                if (Utilities.pointInSector(new Vector2(mapIcon.x, mapIcon.y), startGrid, 99999, 0, Mathf.PI / 2)) {
                    d = mapIcon.x - startGridX;
                    if (d < dist) {
                        dist = d;
                        ret = mapIcon;
                    }
                }
            }
            break;
        case Direction.UP:
            foreach (MapIcon mapIcon in cIcons) {
                if (Utilities.pointInSector(new Vector2(mapIcon.x, mapIcon.y), startGrid, 99999, Mathf.PI / 2, Mathf.PI / 2)) {
                    d = mapIcon.y - startGridY;
                    if (d < dist) {
                        dist = d;
                        ret = mapIcon;
                    }
                }
            }
            break;
        }

        return ret;
    }

    MapIcon getClosestChamberIcon(int startGridX, int startGridY) {
        // get all chamber icons
        List<MapIcon> cIcons = new List<MapIcon>();
        foreach (MapIcon mapIcon in icons) {
            if (mapIcon.icon == Icon.CHAMBER)
                cIcons.Add(mapIcon);
        }
        if (cIcons.Count == 0)
            return null;

        float dist = 9999;
        float d = 0;
        MapIcon ret = null;
        foreach (MapIcon icon in cIcons) {
            d = Mathf.Abs(icon.x - startGridX) + Mathf.Abs(icon.y - startGridY);
            if (d < dist) {
                ret = icon;
                dist = d;
            }
        }
        return ret;
    }

    Vector3 iconLocalPosition(int gridX, int gridY, bool wideSprite = false) {
        if (wideSprite) {
            return (new Vector3((gridX + 1) * CELL_WIDTH * 2, (gridY + 1) * CELL_HEIGHT * 2)) +
                mapRawImage.GetComponent<RectTransform>().localPosition;
        } else {
            return (new Vector3((gridX + .5f) * CELL_WIDTH * 2, (gridY + .5f) * CELL_HEIGHT * 2)) +
                mapRawImage.GetComponent<RectTransform>().localPosition;
        }
    }

    void mapPageSetChamberText(MapIcon chamberIcon) {
        // get map page
        MapPage mapPage = HUD.instance.pauseScreen.mapPage;
        if (mapPage == null) return;

        // search through all the nodes to get information about the chamber
        string positionCode = ChamberPlatform.positionCodeFromMapPosition(chamberIcon.x, chamberIcon.y);
        float firstTime = 99999;
        float lastTime = -1;
        foreach (NodeData node in NodeData.allNodes) {
            if (node.temporary) continue;
            if (node.chamberPositionCode == positionCode) {
                firstTime = Mathf.Min(firstTime, node.time);
                lastTime = Mathf.Max(lastTime, node.time);
            }
        }

        // set text
        if (lastTime == -1) {
            mapPage.setChamberText(positionCode);
        } else if (firstTime == lastTime) {
            mapPage.setChamberText(positionCode, firstTime);
        } else {
            mapPage.setChamberText(positionCode, firstTime, lastTime);
        }

    }

    #endregion

}
