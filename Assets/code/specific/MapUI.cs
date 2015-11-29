using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class MapUI : MonoBehaviour {

    // PUBLIC STATIC

    public static MapUI instance { get { return _instance; } }

    public static int GRID_WIDTH = 60; // width in cells (practical width: 50)
    public static int GRID_HEIGHT = 50; // height in cells (practical height: 42)
    public static int CELL_WIDTH = 9; // width of a cell in pixels
    public static int CELL_HEIGHT = 6; // height of a cell in pixels

    public enum Edge : int {
        NO_WALL = 0,
        OPEN = 1,
        WALL = 2
    }

    public static int makeCell(Edge leftEdge, Edge topEdge, Edge rightEdge, Edge bottomEdge) {
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
        return ret;
    }
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

    public enum Icon {
        NONE,
        CHAMBER,
        HEALTH_UPGRADE,
        BOOSTER, // wide upgrades like this should only be placed in the center of 2x2 rooms
        ORB0
    }

    ////////////
    // PUBLIC //
    ////////////

    public Color topEdgeColor = Color.white;
    public Color bottomEdgeColor = Color.black;
    public Color fillColor = Color.magenta;
    public int topOpenGapWidth = 3;
    public int leftOpenGapHeight = 2;
    public Vector2 positionOffset = new Vector2(-640, 360);
    public GameObject iconChamberGameObject;
    public GameObject iconHealthUpgradeGameObject;
    public GameObject iconBoosterGameObject;
    public GameObject iconOrb0GameObject;
    public GameObject playerPositionGameObject;

    public bool mapShowing { get { return mapRawImage.enabled; } }
    public bool mapFillShowing { get { return mapFillRawImage.enabled; } }

    // gets and sets the position of the center of the map relative to its parent
    public Vector2 position {
        get {
            return new Vector2(rectTransform.localPosition.x, rectTransform.localPosition.y)
                - positionOffset + (new Vector2(GRID_WIDTH * CELL_WIDTH, GRID_HEIGHT * CELL_HEIGHT));
        }
        set {
            rectTransform.localPosition = value + positionOffset - (new Vector2(GRID_WIDTH * CELL_WIDTH, GRID_HEIGHT * CELL_HEIGHT));
        }
    }

    ///////////////
    // FUNCTIONS //
    ///////////////

    public void setPlayerPosition(int x, int y) {
        if (playerPosition == null) return;
        playerPosition.transform.localPosition = new Vector2((x + .5f) * CELL_WIDTH * 2, (y + .5f - GRID_HEIGHT) * CELL_HEIGHT * 2);
    }

    public void showMap(bool showFill = true) {
        mapRawImage.enabled = true;
        mapFillRawImage.enabled = showFill;
        foreach (MapIcon mapIcon in icons) {
            mapIcon.GetComponent<Image>().enabled = true;
        }
        playerPosition.GetComponent<Image>().enabled = true;
    }
    public void hideMap() {
        mapRawImage.enabled = false;
        mapFillRawImage.enabled = false;
        foreach (MapIcon mapIcon in icons) {
            mapIcon.GetComponent<Image>().enabled = false;
        }
        playerPosition.GetComponent<Image>().enabled = false;
    }

    public int gridGetCell(int x, int y) {
        return grid[y, x];
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
                    continue;
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
                    continue;
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
                    continue;
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
                    continue;
                mapTexture.SetPixel(x * CELL_WIDTH + ix, y * CELL_HEIGHT, bottomEdgeColor);
            }
            break;
        }
        for (int iy=0; iy < CELL_HEIGHT; iy++){
            for (int ix=0; ix < CELL_WIDTH; ix++){
                mapFillTexture.SetPixel(x * CELL_WIDTH + ix, y * CELL_HEIGHT + iy, fillColor);
            }
        }
        
        if (applyDrawing) {
            mapTexture.Apply();
            mapFillTexture.Apply();
        }

    }
    
    public string gridToString() {
        string str = "";
        for (int i = 0; i < grid.Length; i++) {
            str = str + grid[i / GRID_HEIGHT, i % GRID_HEIGHT] + ",";
        }
        return str;
    }

    public void gridFromString(string str) {
        char[] delims = {','};
        string[] cells = str.Split(delims);
        clearMapTextures(false);
        for (int i = 0; i < grid.Length; i++) {
            gridSetCell(i % GRID_HEIGHT, i / GRID_HEIGHT, int.Parse(cells[i]), false);
        }
        mapTexture.Apply();
        mapFillTexture.Apply();
    }

    public void gridAddRoom(
        int x,
        int y,
        int roomWidth,
        int roomHeight,
        int[,] cells) {
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

    public void gridAddRoom(
        int x,
        int y,
        int roomWidth,
        int roomHeight,
        bool[] openLeftEdges,
        bool[] openTopEdges,
        bool[] openRightEdges,
        bool[] openBottomEdges) {
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
                    if (iy < openLeftEdges.Length && openLeftEdges[iy])
                        leftEdge = Edge.OPEN;
                    else leftEdge = Edge.WALL;
                }
                if (iy == 0) {
                    if (ix < openBottomEdges.Length && openBottomEdges[ix])
                        bottomEdge = Edge.OPEN;
                    else bottomEdge = Edge.WALL;
                }
                if (ix == roomWidth - 1) {
                    if (iy < openRightEdges.Length && openRightEdges[iy])
                        rightEdge = Edge.OPEN;
                    else rightEdge = Edge.WALL;
                }
                if (iy == roomHeight - 1) {
                    if (ix < openTopEdges.Length && openTopEdges[ix])
                        topEdge = Edge.OPEN;
                    else topEdge = Edge.WALL;
                }
                
                cells[iy, ix] = makeCell(leftEdge, topEdge, rightEdge, bottomEdge);
            }
        }
        gridAddRoom(x, y, roomWidth, roomHeight, cells);
    }

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
        if (playerPosition != null) {
            playerPosition.transform.SetAsLastSibling();
        }
        mapIcon.x = x;
        mapIcon.y = y;
        if (mapIcon.wideSprite) {
            mapIcon.transform.localPosition = new Vector2((x + 1) * CELL_WIDTH * 2, (y + 1 - GRID_HEIGHT) * CELL_HEIGHT * 2);
        } else {
            mapIcon.transform.localPosition = new Vector2((x + .5f) * CELL_WIDTH * 2, (y + .5f - GRID_HEIGHT) * CELL_HEIGHT * 2);
        }
        mapIcon.found = found;
        mapIcon.GetComponent<Image>().enabled = mapShowing;

        icons.Add(mapIcon);
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

    ////////////////////
    // EVENT MESSAGES //
    ////////////////////

    void Awake() {
        // singleton stuff
        if (_instance == null) {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        } else if (_instance != this) {
            Destroy(this);
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
        playerPosition = GameObject.Instantiate(playerPositionGameObject) as GameObject;
        playerPosition.transform.SetParent(transform, false);
        playerPosition.GetComponent<Image>().enabled = false;
        setPlayerPosition(0, 0);

    }

    void Start() {

        // all for testing

        gridAddRoom(30, 10, 1, 1,
            new int[,] { { makeCell(Edge.WALL, Edge.WALL, Edge.WALL, Edge.WALL) } });

        gridAddRoom(10, 10, 2, 1,
            new bool[] { false },
            new bool[] { false, true },
            new bool[] { false },
            new bool[] { false, false });

        gridAddRoom(30, 30, 2, 2,
            new bool[] { false, true },
            new bool[] { false, false },
            new bool[] { false, false },
            new bool[] { true, false });
        gridAddRoom(30, 28, 3, 2,
            new bool[] { false, false },
            new bool[] { true, false, false },
            new bool[] { false, false },
            new bool[] { false, false, false });

        gridAddRoom(59, 49, 1, 1,
            new int[,] { { makeCell(Edge.WALL, Edge.WALL, Edge.WALL, Edge.WALL) } });

        addIcon(Icon.CHAMBER, 10, 10);
        addIcon(Icon.BOOSTER, 30, 30, true);

        setPlayerPosition(11, 10);

        position = new Vector2(640, 360);

    }
	
	void Update() {
		
	}

    /////////////
    // PRIVATE //
    /////////////

    private static MapUI _instance = null;

    Texture2D mapTexture;
    Texture2D mapFillTexture;

    RawImage mapRawImage;
    RawImage mapFillRawImage;

    GameObject playerPosition;

    RectTransform rectTransform;

    int[,] grid = new int[GRID_HEIGHT, GRID_WIDTH];
    List<MapIcon> icons = new List<MapIcon>();

}
